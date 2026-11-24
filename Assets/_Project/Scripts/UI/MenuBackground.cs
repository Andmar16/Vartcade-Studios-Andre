using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TDTS.Managers;
using static TDTS.UI.SundownArt;

namespace TDTS.UI
{
    /// <summary>
    /// Wireframe 01-1 and 01-2: the Main Menu's animated environment, drawn in code (no texture files).
    /// Layers back to front: breathing sky, stars, a sun that slowly sinks and rises, drifting clouds,
    /// far ruins, the ground with a dead tree and the bunker hatch with its flickering bulb, then ash.
    /// On PC the mouse adds a little parallax. Reduce motion stops everything and keeps the bulb on.
    /// </summary>
    public class MenuBackground : MonoBehaviour
    {
        private class Layer
        {
            public RectTransform rect;
            public float parallax; // pixels at the edge of the screen
        }

        private readonly List<Layer> layers = new List<Layer>();
        private readonly List<RectTransform> clouds = new List<RectTransform>();
        private readonly List<(RectTransform rect, Vector2 speed)> ash = new List<(RectTransform, Vector2)>();

        private RawImage sky;
        private RectTransform sun;
        private Vector2 sunHome;
        private RectTransform hatchLid;
        private Image bulb;
        private RawImage bulbGlow;
        private float nextFlicker;
        private float flickerOffUntil;
        private float nextRattle;
        private float rattleUntil;
        private Vector2 parallaxNow;

        /// <summary>Where the puppet and anything else standing on the ground goes.</summary>
        public RectTransform Foreground { get; private set; }

        private static readonly Color Ruins = new Color32(0x1C, 0x14, 0x1C, 255);
        private static readonly Color Ground = new Color32(0x0E, 0x0A, 0x0C, 255);
        private static readonly Color HatchColor = new Color32(0x2C, 0x37, 0x3C, 255);
        private static readonly Color HatchLidColor = new Color32(0x45, 0x55, 0x5C, 255);
        private static readonly Color BulbOn = new Color32(0xFF, 0xD5, 0x8A, 255);

        private static bool Still => GameSettings.ReduceMotion;

        public static MenuBackground Create(Transform canvas)
        {
            var root = NewRect("MenuBackground", canvas, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            var bg = root.gameObject.AddComponent<MenuBackground>();
            bg.Build(root);
            return bg;
        }

        private void Build(RectTransform root)
        {
            // Sky (fills a little past the edges so parallax never shows a gap)
            RectTransform skyLayer = AddLayer(root, "Sky", 4f);
            sky = NewRect("Gradient", skyLayer, Vector2.zero, Vector2.one, new Vector2(-30f, -30f), new Vector2(30f, 30f))
                .gameObject.AddComponent<RawImage>();
            sky.texture = SkyTexture(UITheme.SkyGlow, UITheme.SkyMid, UITheme.SkyTop);
            sky.raycastTarget = false;

            var rng = new System.Random(11);
            for (int i = 0; i < 26; i++)
            {
                float size = 2f + (float)rng.NextDouble() * 3f;
                Shape(skyLayer, new Vector2((float)rng.NextDouble(), 0.45f + (float)rng.NextDouble() * 0.55f),
                    new Vector2(size, size), UITheme.WithAlpha(UITheme.Cream, 0.25f + (float)rng.NextDouble() * 0.4f), circle: true);
            }

            // Sun low on the left, sitting in its own glow
            RectTransform sunLayer = AddLayer(root, "Sun", 6f);
            sun = NewRect("Sun", sunLayer, new Vector2(0.13f, 0.155f), new Vector2(0.13f, 0.155f), Vector2.zero, Vector2.zero);
            sun.sizeDelta = new Vector2(1f, 1f);
            sunHome = sun.anchoredPosition;
            var glow = NewRect("Glow", sun, new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, Vector2.zero);
            glow.sizeDelta = new Vector2(760f, 760f);
            var glowImage = glow.gameObject.AddComponent<RawImage>();
            glowImage.texture = PoolTexture();
            glowImage.color = UITheme.WithAlpha(UITheme.Sun, 0.35f);
            glowImage.raycastTarget = false;
            Shape(sun, new Vector2(0.5f, 0.5f), new Vector2(170f, 170f), UITheme.Sun, circle: true);

            // Clouds drift left in a 90 s loop
            RectTransform cloudLayer = AddLayer(root, "Clouds", 8f);
            for (int i = 0; i < 5; i++)
            {
                var cloud = NewRect("Cloud", cloudLayer, new Vector2(i / 5f, 0.66f + 0.07f * (i % 3)), new Vector2(i / 5f, 0.66f + 0.07f * (i % 3)),
                    Vector2.zero, Vector2.zero);
                cloud.sizeDelta = new Vector2(420f + 90f * (i % 2), 70f + 20f * (i % 3));
                var img = cloud.gameObject.AddComponent<RawImage>();
                img.texture = PoolTexture();
                img.color = UITheme.WithAlpha(new Color32(0x3A, 0x30, 0x4A, 255), 0.55f);
                img.raycastTarget = false;
                clouds.Add(cloud);
            }

            // Horizon glow band, then the far ruins: uneven hills and broken towers
            RectTransform ruinsLayer = AddLayer(root, "Ruins", 10f);
            var band = NewRect("Horizon", ruinsLayer, new Vector2(0f, 0.105f), new Vector2(1f, 0.135f), new Vector2(-30f, 0f), new Vector2(30f, 0f));
            band.gameObject.AddComponent<Image>().color = UITheme.WithAlpha(UITheme.Sun, 0.85f);
            band.GetComponent<Image>().raycastTarget = false;
            (float x, float w, float h, float rot)[] hills =
            {
                (0.05f, 700f, 150f, 4f), (0.30f, 640f, 120f, -3f), (0.55f, 820f, 170f, 5f), (0.82f, 760f, 140f, -4f),
            };
            foreach (var hill in hills)
                Shape(ruinsLayer, new Vector2(hill.x, 0.12f), new Vector2(hill.w, hill.h), Ruins, rotation: hill.rot, cornerScale: 60f);
            (float x, float w, float h)[] towers = { (0.36f, 46f, 130f), (0.395f, 30f, 90f), (0.62f, 54f, 150f), (0.66f, 26f, 70f), (0.70f, 40f, 110f) };
            foreach (var tower in towers)
                Shape(ruinsLayer, new Vector2(tower.x, 0.17f), new Vector2(tower.w, tower.h), Ruins, rotation: (tower.x * 97f % 6f) - 3f, cornerScale: 2f);

            // Ground, dead tree, bunker hatch and its bulb
            RectTransform groundLayer = AddLayer(root, "Ground", 14f);
            Foreground = groundLayer;
            var ground = NewRect("Ground", groundLayer, Vector2.zero, new Vector2(1f, 0.115f), new Vector2(-40f, -40f), new Vector2(40f, 0f));
            ground.gameObject.AddComponent<Image>().color = Ground;
            ground.GetComponent<Image>().raycastTarget = false;
            Shape(groundLayer, new Vector2(0.25f, 0.11f), new Vector2(1300f, 60f), Ground, rotation: 2f, cornerScale: 40f);
            Shape(groundLayer, new Vector2(0.78f, 0.11f), new Vector2(1200f, 70f), Ground, rotation: -2.5f, cornerScale: 40f);

            // Crooked dead tree
            RectTransform trunk = Shape(groundLayer, new Vector2(0.425f, 0.11f), new Vector2(16f, 300f), Ground, rotation: 8f, pivot: new Vector2(0.5f, 0f));
            Shape(trunk, new Vector2(0.5f, 0.78f), new Vector2(120f, 10f), Ground, rotation: 24f, pivot: new Vector2(0f, 0.5f));
            Shape(trunk, new Vector2(0.5f, 0.62f), new Vector2(150f, 9f), Ground, rotation: -12f, pivot: new Vector2(0f, 0.5f));
            Shape(trunk, new Vector2(0.5f, 0.88f), new Vector2(70f, 8f), Ground, rotation: 150f, pivot: new Vector2(0f, 0.5f));

            // The bunker hatch: the brightest point on the right, because the bunker is the goal
            Shape(groundLayer, new Vector2(0.865f, 0.105f), new Vector2(330f, 120f), HatchColor, cornerScale: 120f, pivot: new Vector2(0.5f, 0f));
            hatchLid = Shape(groundLayer, new Vector2(0.86f, 0.205f), new Vector2(150f, 30f), HatchLidColor, cornerScale: 6f);
            Shape(groundLayer, new Vector2(0.852f, 0.215f), new Vector2(6f, 120f), Ground, pivot: new Vector2(0.5f, 0f));
            var bulbGlowRect = NewRect("BulbGlow", groundLayer, new Vector2(0.852f, 0.33f), new Vector2(0.852f, 0.33f), Vector2.zero, Vector2.zero);
            bulbGlowRect.sizeDelta = new Vector2(320f, 320f);
            bulbGlow = bulbGlowRect.gameObject.AddComponent<RawImage>();
            bulbGlow.texture = PoolTexture();
            bulbGlow.color = UITheme.WithAlpha(BulbOn, 0.45f);
            bulbGlow.raycastTarget = false;
            bulb = Shape(groundLayer, new Vector2(0.852f, 0.33f), new Vector2(34f, 34f), BulbOn, circle: true).GetComponent<Image>();

            // Ash drifting up and left in front of everything
            RectTransform ashLayer = AddLayer(root, "Ash", 18f);
            for (int i = 0; i < 40; i++)
            {
                float size = 3f + (float)rng.NextDouble() * 4f;
                RectTransform flake = Shape(ashLayer, new Vector2((float)rng.NextDouble(), (float)rng.NextDouble()),
                    new Vector2(size, size), UITheme.WithAlpha(new Color(0.75f, 0.72f, 0.7f), 0.25f + (float)rng.NextDouble() * 0.3f));
                ash.Add((flake, new Vector2(-0.004f - (float)rng.NextDouble() * 0.008f, 0.006f + (float)rng.NextDouble() * 0.01f)));
            }

            ScheduleFlicker();
            nextRattle = Time.unscaledTime + Random.Range(8f, 12f);
        }

        private void Update()
        {
            float t = Time.unscaledTime;
            bool still = Still;

            foreach (var (rect, _) in ash) rect.gameObject.SetActive(!still);
            if (still)
            {
                bulb.color = BulbOn;
                bulbGlow.color = UITheme.WithAlpha(BulbOn, 0.45f);
                return;
            }

            // Sky breathes between cooler and warmer every 20 s
            sky.color = Color.Lerp(Color.white, new Color(1f, 0.9f, 0.85f), (Mathf.Sin(t * Mathf.PI * 2f / 20f) + 1f) * 0.5f);

            // Sun sinks 20 px and rises again over 60 s
            sun.anchoredPosition = sunHome + new Vector2(0f, -10f + Mathf.Cos(t * Mathf.PI * 2f / 60f) * 10f);

            // Clouds: one full screen width (plus their size) every 90 s, wrapping round
            for (int i = 0; i < clouds.Count; i++)
            {
                float x = Mathf.Repeat(i / (float)clouds.Count - t / 90f, 1.4f) - 0.2f;
                var anchor = new Vector2(x, clouds[i].anchorMin.y);
                clouds[i].anchorMin = clouds[i].anchorMax = anchor;
            }

            foreach (var (rect, speed) in ash)
            {
                Vector2 a = rect.anchorMin + speed * Time.unscaledDeltaTime;
                if (a.y > 1.02f) a.y = -0.02f;
                if (a.x < -0.02f) a.x = 1.02f;
                rect.anchorMin = rect.anchorMax = a;
            }

            UpdateBulb(t);
            UpdateHatch(t);
            UpdateParallax();
        }

        // Random flickers, never faster than 3 flashes a second (photosensitive players)
        private void UpdateBulb(float t)
        {
            if (t >= nextFlicker)
            {
                flickerOffUntil = t + Random.Range(0.06f, 0.14f);
                ScheduleFlicker();
            }
            bool on = t >= flickerOffUntil;
            bulb.color = on ? BulbOn : new Color32(0x6B, 0x5A, 0x3A, 255);
            bulbGlow.color = UITheme.WithAlpha(BulbOn, on ? 0.45f : 0.08f);
        }

        private void ScheduleFlicker()
        {
            // Mostly calm, sometimes a quick double flicker (still at least 0.34 s apart)
            nextFlicker = Time.unscaledTime + (Random.value < 0.3f ? Random.Range(0.34f, 0.6f) : Random.Range(2f, 6f));
        }

        // The hatch lid rattles slightly every 8 to 12 s
        private void UpdateHatch(float t)
        {
            if (t >= nextRattle)
            {
                rattleUntil = t + 0.5f;
                nextRattle = t + Random.Range(8f, 12f);
            }
            hatchLid.localEulerAngles = new Vector3(0f, 0f, t < rattleUntil ? Mathf.Sin(t * 60f) * 2.5f : 0f);
        }

        private void UpdateParallax()
        {
            Vector2 target = Vector2.zero;
            if (Mouse.current != null && Screen.width > 0)
            {
                Vector2 p = Mouse.current.position.ReadValue();
                target = new Vector2(p.x / Screen.width - 0.5f, p.y / Screen.height - 0.5f) * 2f;
                target = Vector2.ClampMagnitude(target, 1.4f);
            }
            parallaxNow = Vector2.Lerp(parallaxNow, target, 1f - Mathf.Exp(-4f * Time.unscaledDeltaTime));
            foreach (Layer layer in layers) layer.rect.anchoredPosition = -parallaxNow * layer.parallax;
        }

        // ---------- Building helpers ----------

        private RectTransform AddLayer(RectTransform root, string name, float parallax)
        {
            var rect = NewRect(name, root, Vector2.zero, Vector2.one, Vector2.zero, Vector2.zero);
            layers.Add(new Layer { rect = rect, parallax = parallax });
            return rect;
        }

        /// <summary>A flat shape placed by an anchor point (fraction of the parent) with a size in pixels.</summary>
        public static RectTransform Shape(Transform parent, Vector2 anchor, Vector2 size, Color color, float rotation = 0f,
            bool circle = false, float cornerScale = 0f, Vector2? pivot = null)
        {
            var rect = NewRect("Shape", parent, anchor, anchor, Vector2.zero, Vector2.zero);
            rect.pivot = pivot ?? new Vector2(0.5f, 0.5f);
            rect.sizeDelta = size;
            rect.anchoredPosition = Vector2.zero;
            rect.localEulerAngles = new Vector3(0f, 0f, rotation);
            Image image = rect.gameObject.AddComponent<Image>();
            if (circle) image.sprite = CircleSprite();
            else if (cornerScale > 0f) Rounded(image, color, cornerScale);
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }
    }
}
