using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using TDTS.Data;
using TDTS.Managers;
using static TDTS.UI.SundownArt;

namespace TDTS.UI
{
    /// <summary>
    /// The wireframe v0.1 pieces layered over the game table, all driven by IGameView so they work
    /// offline, on the host and on online clients:
    /// - 03-7 Kinds tracker: which Supply kinds you KNOW you hold (from peeks), and the 3/3 goal.
    /// - 03-11 Your portrait: your puppet in a gold ring; eyes closed at Night until it's your turn.
    /// - 05-1 Whose turn banner: while someone else plays a Day turn, who it is and what just happened.
    /// - 05-4 Your hand dims a little while it isn't your turn (you can still read every card).
    /// - 05-7 Night blackout: during other players' Night turns everything is hidden.
    /// - 08 Morning recap: from Day 2, a "night log" of what happened to YOUR hand while you slept.
    /// HUDController adds this at the start of a game.
    /// </summary>
    public class TableHud : MonoBehaviour
    {
        private IGameView view;
        private Transform canvas;

        // Kinds tracker
        private RectTransform tracker;
        private Image trackerEdge, trackerFill;
        private TextMeshProUGUI trackerCount, trackerHint;
        private readonly Dictionary<SupplyKind, (Image box, TextMeshProUGUI name, TextMeshProUGUI tick)> kindRows =
            new Dictionary<SupplyKind, (Image, TextMeshProUGUI, TextMeshProUGUI)>();
        private string trackerShown = "";

        // Portrait
        private Puppet portraitPuppet;
        private RectTransform portrait;

        // Waiting banner
        private CanvasGroup waiting;
        private TextMeshProUGUI waitingName, waitingStatus;

        // Blackout
        private CanvasGroup blackout;
        private RectTransform blackoutPulse;

        // Hand dimming
        private CanvasGroup handGroup;

        // Morning recap bookkeeping (front names; the client view rebuilds card objects)
        private List<string> lastHand = new List<string>();
        private readonly List<string> lostToOthers = new List<string>();
        private readonly List<string> gainedFromOthers = new List<string>();
        private bool watchingNight;

        public static readonly Dictionary<SupplyKind, string> KindNames = new Dictionary<SupplyKind, string>
        {
            { SupplyKind.Food, "Food" }, { SupplyKind.Water, "Water" }, { SupplyKind.Medicine, "Medicine" }, { SupplyKind.Entertainment, "Entertainment" },
        };

        /// <summary>The kind colours shared with the cards (wireframe 06-2).</summary>
        public static Color KindColor(SupplyKind kind) => kind switch
        {
            SupplyKind.Food => UITheme.Orange,
            SupplyKind.Water => UITheme.Teal,
            SupplyKind.Medicine => UITheme.Brick,
            _ => UITheme.Gold,
        };

        /// <summary>The kind symbols (circle, drop, plus, diamond), so colour is never the only clue.</summary>
        public static string KindSymbol(SupplyKind kind) => kind switch
        {
            SupplyKind.Food => "\u25CF",          // circle
            SupplyKind.Water => "\u25BC",         // drop
            SupplyKind.Medicine => "+",
            _ => "\u2666",                        // diamond (in the fallback font, unlike U+25C6)
        };

        private void Start()
        {
            view = GameView.Current;
            Canvas c = GetComponentInParent<Canvas>();
            if (view == null || c == null) return;
            canvas = c.rootCanvas.transform;

            BuildTracker();
            BuildWaiting();
            BuildBlackout(); // built now, so pop-ups made later (pickers, banners) sit above it

            HandController hand = FindAnyObjectByType<HandController>();
            if (hand != null) handGroup = hand.GetComponent<CanvasGroup>() ?? hand.gameObject.AddComponent<CanvasGroup>();

            view.OnStateChanged += HandleStateChanged;
            lastHand = HandNames();
        }

        private void OnDestroy()
        {
            if (view != null) view.OnStateChanged -= HandleStateChanged;
        }

        private void Update()
        {
            if (view == null || tracker == null) return;
            bool sundown = view.CurrentState == GameState.Sundown;
            bool night = view.CurrentState == GameState.NightPhase;
            bool offeredToMe = view.HasPendingTrade && view.TradeTo == view.LocalPlayer;

            RefreshTracker();
            tracker.gameObject.SetActive(!sundown);
            portrait.gameObject.SetActive(!sundown);
            if (portraitPuppet != null) portraitPuppet.EyesClosed = view.EyesClosed && !view.IsLocalTurn;

            // Waiting banner: someone else's Day turn
            bool showWaiting = !sundown && !view.IsLocalTurn && view.ActivePlayer != null
                               && view.CurrentState == GameState.DayPhase && !view.EyesClosed && !offeredToMe;
            Fade(waiting, showWaiting, 6f);
            if (showWaiting) RefreshWaiting();

            // Blackout: other players' turns with eyes closed (Night, or a Blackout Day), unless paused
            bool paused = PauseManager.Instance != null && PauseManager.Instance.IsPaused;
            bool showBlackout = !sundown && view.EyesClosed && !view.IsLocalTurn && !offeredToMe && !paused;
            Fade(blackout, showBlackout, 2.5f);
            blackout.blocksRaycasts = showBlackout;
            if (blackout.alpha > 0f && !GameSettings.ReduceMotion)
                blackoutPulse.localScale = new Vector3(0.35f + 0.65f * Mathf.PingPong(Time.unscaledTime * 0.35f, 1f), 1f, 1f);

            if (handGroup != null)
                handGroup.alpha = Mathf.MoveTowards(handGroup.alpha, view.IsLocalTurn || offeredToMe || sundown ? 1f : 0.72f, Time.unscaledDeltaTime * 3f);

            WatchNight(night);
        }

        private static void Fade(CanvasGroup group, bool show, float speed)
        {
            group.alpha = Mathf.MoveTowards(group.alpha, show ? 1f : 0f, Time.unscaledDeltaTime * speed);
            group.gameObject.SetActive(group.alpha > 0.001f || show);
        }

        private List<string> HandNames()
        {
            var hand = view.LocalPlayer?.hand;
            return hand == null ? new List<string>() : hand.Select(c => c.FrontName).ToList();
        }

        // ---------- 03-7 Kinds tracker ----------

        /// <summary>
        /// One strip just above your hand (the space the wireframe's corner panel needs is taken by the
        /// left player's zone at this table): your portrait, YOUR KINDS with the n/3 goal, and the four
        /// kinds, lit when you know you hold one. A thin gold bar along the bottom fills toward 3.
        /// </summary>
        private void BuildTracker()
        {
            // In the room layout (wireframe 03) it sits top-left under the Day counter; otherwise above your hand
            bool room = TableRoom.Current != null;
            tracker = room
                ? NewRect("KindsTracker", canvas, new Vector2(0f, 1f), new Vector2(0f, 1f), new Vector2(24f, -208f), new Vector2(924f, -134f))
                : NewRect("KindsTracker", canvas, Vector2.zero, Vector2.zero, new Vector2(250f, 266f), new Vector2(1150f, 340f));
            var edge = NewRect("Edge", tracker, Vector2.zero, Vector2.one, new Vector2(-3f, -3f), new Vector2(3f, 3f));
            trackerEdge = Rounded(edge.gameObject.AddComponent<Image>(), UITheme.Gold, 14);
            trackerEdge.raycastTarget = false;
            // The face is a child drawn after the edge, so the edge only shows as a border
            var face = NewRect("Face", tracker, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Rounded(face.gameObject.AddComponent<Image>(), UITheme.WithAlpha(UITheme.Charcoal, 0.96f), 14).raycastTarget = false;

            BuildPortrait(room ? null : tracker);

            var title = NewText("Title", tracker, 18, FontStyles.Bold, TextAlignmentOptions.BottomLeft,
                new Vector2(0f, 0.5f), new Vector2(0f, 1f), new Vector2(84f, 0f), new Vector2(250f, -6f));
            title.text = "YOUR KINDS";
            title.characterSpacing = 8f;
            title.color = UITheme.Gold;
            trackerHint = NewText("Hint", tracker, 15, FontStyles.Italic, TextAlignmentOptions.TopLeft,
                new Vector2(0f, 0f), new Vector2(0f, 0.5f), new Vector2(84f, 12f), new Vector2(250f, -2f));
            trackerHint.color = UITheme.WithAlpha(UITheme.Cream, 0.65f);
            trackerCount = SundownFonts.UseTitle(NewText("Count", tracker, 44, FontStyles.Normal, TextAlignmentOptions.Center,
                Vector2.zero, new Vector2(0f, 1f), new Vector2(252f, 6f), new Vector2(326f, 0f)));
            trackerCount.color = UITheme.Cream;

            var bar = NewRect("Bar", tracker, Vector2.zero, new Vector2(1f, 0f), new Vector2(84f, 6f), new Vector2(-14f, 11f));
            Rounded(bar.gameObject.AddComponent<Image>(), UITheme.WithAlpha(UITheme.Cream, 0.12f), 3).raycastTarget = false;
            var fill = NewRect("Fill", bar, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            trackerFill = Rounded(fill.gameObject.AddComponent<Image>(), UITheme.Gold, 3);
            trackerFill.raycastTarget = false;

            float x = 336f;
            foreach (SupplyKind kind in KindNames.Keys)
            {
                var box = NewRect(kind + "Box", tracker, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(x, -13f), new Vector2(x + 34f, 21f));
                Image boxImage = Rounded(box.gameObject.AddComponent<Image>(), KindColor(kind), 6);
                boxImage.raycastTarget = false;
                var symbol = NewText("Symbol", box, 20, FontStyles.Bold, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                symbol.text = KindSymbol(kind);
                symbol.color = UITheme.Charcoal;

                var name = NewText(kind + "Name", tracker, 21, FontStyles.Bold, TextAlignmentOptions.MidlineLeft,
                    new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(x + 42f, -14f), new Vector2(x + 140f, 22f));
                name.text = KindNames[kind];
                name.enableAutoSizing = true;
                name.fontSizeMin = 12f;
                name.fontSizeMax = 21f;
                var tick = NewText(kind + "Tick", box, 1, FontStyles.Normal, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
                kindRows[kind] = (boxImage, name, tick);
                x += 140f;
            }
        }

        /// <summary>Only cards you've peeked count: the tracker never shows a back you don't know.</summary>
        private void RefreshTracker()
        {
            var hand = view.LocalPlayer?.hand;
            var known = new HashSet<SupplyKind>();
            bool key = false;
            int unknown = 0;
            if (hand != null)
            {
                foreach (CardData card in hand)
                {
                    if (!card.peeked) { unknown++; continue; }
                    if (card.back == BackType.Supply) known.Add(card.backSupply);
                    if (card.back == BackType.BunkerKey) key = true;
                }
            }

            string summary = $"{string.Join(",", known)}|{key}|{unknown}";
            if (summary == trackerShown) return;
            trackerShown = summary;

            int count = Mathf.Min(3, known.Count);
            bool done = key || known.Count >= 3;
            trackerCount.text = key ? "<size=70%>KEY</size>" : $"{count}/3";
            trackerCount.color = done ? UITheme.Gold : UITheme.Cream;
            trackerFill.rectTransform.anchorMax = new Vector2(key ? 1f : count / 3f, 1f);
            trackerEdge.color = done ? UITheme.Gold : UITheme.WithAlpha(UITheme.Cream, 0.18f);

            foreach (var pair in kindRows)
            {
                bool have = known.Contains(pair.Key);
                pair.Value.box.color = have ? KindColor(pair.Key) : UITheme.WithAlpha(KindColor(pair.Key), 0.3f);
                pair.Value.name.color = have ? UITheme.Cream : UITheme.WithAlpha(UITheme.Cream, 0.45f);
            }

            trackerHint.text = key ? "You hold the Bunker Key"
                : unknown == 0 ? "From cards you've peeked"
                : unknown == 1 ? "1 card not peeked yet"
                : $"{unknown} cards not peeked yet";
        }

        // ---------- 03-11 Portrait ----------

        /// <summary>03-11: your puppet's head in a gold ring, bottom-left (or small, at the left end of the strip).</summary>
        private void BuildPortrait(RectTransform strip)
        {
            bool big = strip == null;
            portrait = big
                ? NewRect("Portrait", canvas, Vector2.zero, Vector2.zero, new Vector2(44f, 44f), new Vector2(214f, 214f))
                : NewRect("Portrait", strip, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f), new Vector2(10f, -30f), new Vector2(70f, 30f));
            var ring = NewRect("Ring", portrait, Vector2.zero, Vector2.one, new Vector2(-4f, -4f), new Vector2(4f, 4f));
            Image ringImage = ring.gameObject.AddComponent<Image>();
            ringImage.sprite = CircleSprite();
            ringImage.color = UITheme.Gold;
            ringImage.raycastTarget = false;

            var window = NewRect("Window", portrait, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Image windowImage = window.gameObject.AddComponent<Image>();
            windowImage.sprite = CircleSprite();
            windowImage.color = UITheme.SkyMid;
            windowImage.raycastTarget = false;
            window.gameObject.AddComponent<Mask>().showMaskGraphic = true;

            int seat = 1;
            var players = view.Players;
            for (int i = 0; i < players.Count; i++) if (players[i] == view.LocalPlayer) seat = i + 1;
            portraitPuppet = Puppet.Create(window, new Vector2(0.5f, 0f), seat, big ? 360f : 128f);
            ((RectTransform)portraitPuppet.transform).anchoredPosition = new Vector2(0f, big ? -190f : -67f);
            if (!big) return;

            var tag = NewRect("You", portrait, new Vector2(0.5f, 0f), new Vector2(0.5f, 0f), new Vector2(-40f, -16f), new Vector2(40f, 18f));
            Rounded(tag.gameObject.AddComponent<Image>(), UITheme.Gold, 6).raycastTarget = false;
            var you = SundownFonts.UseTitle(NewText("Label", tag, 22, FontStyles.Normal, TextAlignmentOptions.Center, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero));
            you.text = "YOU";
            you.color = UITheme.Charcoal;
        }

        // ---------- 05-1 Whose turn banner ----------

        private void BuildWaiting()
        {
            // 05-1: top centre under the Day bar in the room layout; otherwise over the idle action bar
            var rect = TableRoom.Current != null
                ? NewRect("WaitingBanner", canvas, new Vector2(0.5f, 1f), new Vector2(0.5f, 1f), new Vector2(-400f, -228f), new Vector2(400f, -128f))
                : NewRect("WaitingBanner", canvas, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-740f, 122f), new Vector2(-96f, 232f));
            waiting = rect.gameObject.AddComponent<CanvasGroup>();
            waiting.blocksRaycasts = false;
            waiting.alpha = 0f;
            Rounded(rect.gameObject.AddComponent<Image>(), UITheme.WithAlpha(UITheme.Charcoal, 0.92f), 16).raycastTarget = false;
            var stripe = NewRect("Stripe", rect, Vector2.zero, new Vector2(0f, 1f), new Vector2(0f, 12f), new Vector2(8f, -12f));
            Rounded(stripe.gameObject.AddComponent<Image>(), UITheme.Orange, 4).raycastTarget = false;

            waitingName = SundownFonts.UseTitle(NewText("Name", rect, 38, FontStyles.Normal, TextAlignmentOptions.TopLeft,
                Vector2.zero, Vector2.one, new Vector2(30f, 40f), new Vector2(-24f, -10f)));
            waitingName.color = UITheme.Cream;
            waitingStatus = NewText("Status", rect, 21, FontStyles.Normal, TextAlignmentOptions.BottomLeft,
                Vector2.zero, Vector2.one, new Vector2(30f, 12f), new Vector2(-24f, -50f));
            waitingStatus.color = UITheme.WithAlpha(UITheme.Cream, 0.75f);
            waitingStatus.textWrappingMode = TextWrappingModes.NoWrap;
            waitingStatus.overflowMode = TextOverflowModes.Ellipsis;
            rect.gameObject.SetActive(false);
        }

        private void RefreshWaiting()
        {
            PlayerData active = view.ActivePlayer;
            string bot = BotController.Current != null ? BotController.Current.TagFor(active) : "";
            waitingName.text = $"<color={UITheme.GoldHex}>{active.playerName}</color>'s turn<size=55%><color=#F4E9D8AA>{bot}</color></size>";
            var log = view.PlayLog;
            string last = log.Count > 0 ? Regex.Replace(log[log.Count - 1], "<.*?>", "") : "";
            waitingStatus.text = string.IsNullOrEmpty(last) ? "Thinking..." : last;
        }

        // ---------- 05-7 Night blackout ----------

        private void BuildBlackout()
        {
            var rect = NewRect("NightBlackout", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            blackout = rect.gameObject.AddComponent<CanvasGroup>();
            blackout.alpha = 0f;
            Image dark = rect.gameObject.AddComponent<Image>();
            dark.color = new Color32(0x05, 0x06, 0x0C, 255);

            // Moon and two closed eyes
            MenuBackground.Shape(rect, new Vector2(0.5f, 0.66f), new Vector2(110f, 110f), UITheme.WithAlpha(UITheme.Cream, 0.85f), circle: true);
            RectTransform bite = MenuBackground.Shape(rect, new Vector2(0.5f, 0.66f), new Vector2(100f, 100f), new Color32(0x05, 0x06, 0x0C, 255), circle: true);
            bite.anchoredPosition = new Vector2(28f, 14f);
            for (int side = -1; side <= 1; side += 2)
            {
                RectTransform eye = MenuBackground.Shape(rect, new Vector2(0.5f, 0.5f), new Vector2(90f, 10f), UITheme.WithAlpha(UITheme.Cream, 0.7f), cornerScale: 5f);
                eye.anchoredPosition = new Vector2(side * 70f, 30f);
            }

            var title = SundownFonts.UseTitle(NewText("Title", rect, 60, FontStyles.Normal, TextAlignmentOptions.Center,
                new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, -70f), new Vector2(0f, 0f)));
            title.text = "EYES CLOSED";
            title.color = UITheme.Cream;
            title.characterSpacing = 10f;
            var line = NewText("Line", rect, 28, FontStyles.Italic, TextAlignmentOptions.Center,
                new Vector2(0f, 0.5f), new Vector2(1f, 0.5f), new Vector2(0f, -120f), new Vector2(0f, -76f));
            line.text = "Someone is moving in the dark...";
            line.color = UITheme.WithAlpha(UITheme.Cream, 0.65f);

            var track = NewRect("Track", rect, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-220f, -160f), new Vector2(220f, -152f));
            Rounded(track.gameObject.AddComponent<Image>(), UITheme.WithAlpha(UITheme.Cream, 0.12f), 4).raycastTarget = false;
            blackoutPulse = NewRect("Pulse", track, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            blackoutPulse.pivot = new Vector2(0f, 0.5f);
            Rounded(blackoutPulse.gameObject.AddComponent<Image>(), UITheme.Teal, 4).raycastTarget = false;

            var hint = NewText("Hint", rect, 20, FontStyles.Normal, TextAlignmentOptions.Center,
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(0f, 40f), new Vector2(0f, 80f));
            hint.text = "Your screen opens when it's your Night turn. Esc pauses.";
            hint.color = UITheme.WithAlpha(UITheme.Cream, 0.4f);
            rect.gameObject.SetActive(false);
        }

        // ---------- 08 Morning recap ----------

        /// <summary>
        /// Through the Night, changes to your hand during OTHER players' turns are noted (your own
        /// moves aren't news). Front names only, so it works on an online client too.
        /// </summary>
        private void WatchNight(bool night)
        {
            List<string> now = HandNames();
            if (night)
            {
                if (!watchingNight)
                {
                    watchingNight = true;
                    lostToOthers.Clear();
                    gainedFromOthers.Clear();
                }
                else if (!view.IsLocalTurn)
                {
                    lostToOthers.AddRange(Minus(lastHand, now));
                    gainedFromOthers.AddRange(Minus(now, lastHand));
                }
            }
            lastHand = now;
        }

        private void HandleStateChanged(GameState previous, GameState next)
        {
            if (next != GameState.DayPhase || previous != GameState.NightPhase || view.CurrentDay < 2) return;

            // The Day just started: anything new since the end of the Night came from the Rondo or the catch-up draw
            List<string> now = HandNames();
            List<string> rondoTook = Minus(lastHand, now);
            List<string> newToday = Minus(now, lastHand);
            bool caughtUp = view.PlayLog.Any(l => l.StartsWith("Catch-up"));

            var lines = new List<string>();
            var lost = new List<string>(lostToOthers);
            var gained = new List<string>(gainedFromOthers);
            while (lost.Count > 0 && gained.Count > 0)
            {
                lines.Add($"<b>Swapped:</b> your {lost[0]} left; a {gained[0]} arrived.");
                lost.RemoveAt(0);
                gained.RemoveAt(0);
            }
            foreach (string card in lost) lines.Add($"<b>Stolen:</b> your {card} is gone. You didn't see who took it.");
            foreach (string card in gained) lines.Add($"<b>New card:</b> a {card} turned up in your hand.");
            // Only these Rondo results touch hands; otherwise a loss here was the night's last Steal
            bool handRondo = view.TodaysRondo is RondoResult.HungerDisaster or RondoResult.Outbreak or RondoResult.Tornado;
            foreach (string card in rondoTook)
                lines.Add(handRondo ? $"<b>The Rondo</b> took your {card}." : $"<b>Stolen:</b> your {card} is gone. You didn't see who took it.");
            if (caughtUp && newToday.Count > 0)
                lines.Add($"<b>Catch-up:</b> you drew a {newToday[0]} because you're the furthest from the bunker.");

            watchingNight = false;
            lastHand = now;
            StartCoroutine(ShowRecap(view.CurrentDay, lines));
        }

        private static List<string> Minus(List<string> from, List<string> take)
        {
            var left = new List<string>(from);
            foreach (string t in take) left.Remove(t);
            return left;
        }

        private IEnumerator ShowRecap(int day, List<string> lines)
        {
            // Let the Rondo spin and announce first
            yield return new WaitForSeconds(0.2f);
            RondoDial rondo = FindAnyObjectByType<RondoDial>();
            while ((rondo != null && rondo.IsAnnouncing) || GameObject.Find("RondoBanner") != null) yield return null;
            if (view.CurrentState != GameState.DayPhase || view.CurrentDay != day) yield break;

            bool quiet = lines.Count == 0;
            if (quiet) lines.Add("Quiet night. Nothing missing.");

            float height = 150f + lines.Count * 46f;
            var root = NewRect("MorningRecap", canvas, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f),
                new Vector2(-380f, -height / 2f), new Vector2(380f, height / 2f));
            root.localEulerAngles = new Vector3(0f, 0f, -1.5f);
            var group = root.gameObject.AddComponent<CanvasGroup>();
            var shadow = NewRect("Shadow", root, Vector2.zero, Vector2.one, new Vector2(8f, -10f), new Vector2(8f, -10f));
            Rounded(shadow.gameObject.AddComponent<Image>(), new Color(0f, 0f, 0f, 0.45f), 6).raycastTarget = false;
            var paper = NewRect("Paper", root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            Rounded(paper.gameObject.AddComponent<Image>(), UITheme.Cream, 6);

            var title = SundownFonts.UseTitle(NewText("Title", paper, 34, FontStyles.Normal, TextAlignmentOptions.TopLeft,
                new Vector2(0f, 1f), Vector2.one, new Vector2(36f, -70f), new Vector2(-36f, -22f)));
            title.text = $"NIGHT LOG  <size=60%>·  MORNING OF DAY {day}</size>";
            title.color = UITheme.Charcoal;

            var texts = new List<TextMeshProUGUI>();
            float y = -84f;
            foreach (string line in lines)
            {
                var text = NewText("Line", paper, 23, FontStyles.Italic, TextAlignmentOptions.TopLeft,
                    new Vector2(0f, 1f), Vector2.one, new Vector2(36f, y - 44f), new Vector2(-36f, y));
                text.text = line;
                text.color = UITheme.Charcoal;
                text.alpha = 0f;
                texts.Add(text);
                y -= 46f;
            }

            float seconds = quiet ? 2f : 5f;
            var buttonRect = NewRect("Continue", paper, new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(-250f, 22f), new Vector2(-30f, 74f));
            Button button = MakeButton(buttonRect, "Continue", UITheme.Brick, () => { if (root != null) Destroy(root.gameObject); }, 22);
            TextMeshProUGUI label = button.GetComponentInChildren<TextMeshProUGUI>();
            label.color = UITheme.Cream;

            // Slide up and settle, then write the lines in one after another
            for (float t = 0f; t < 1f && root != null; t += Time.unscaledDeltaTime / 0.35f)
            {
                group.alpha = t;
                root.anchoredPosition = new Vector2(0f, Mathf.Lerp(-120f, 0f, 1f - (1f - t) * (1f - t)));
                yield return null;
            }
            if (root == null) yield break;
            group.alpha = 1f;
            root.anchoredPosition = Vector2.zero;
            foreach (TextMeshProUGUI text in texts)
            {
                if (root == null) yield break;
                text.alpha = 1f;
                yield return new WaitForSecondsRealtime(0.15f);
            }

            // Never wait on one player: it closes by itself
            for (float left = seconds; left > 0f && root != null; left -= Time.unscaledDeltaTime)
            {
                label.text = $"Continue ({Mathf.CeilToInt(left)})";
                yield return null;
            }
            if (root != null) Destroy(root.gameObject);
        }
    }
}
