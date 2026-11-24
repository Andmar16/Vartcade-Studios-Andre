using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TDTS.Managers;
using static TDTS.UI.SundownArt;

namespace TDTS.UI
{
    /// <summary>
    /// The shared placeholder puppet from the wireframes: a cutout figure in a teal jumpsuit with its
    /// seat number on the chest. Thin or big body. It breathes, blinks, looks toward the pointer and
    /// now and then checks its wristwatch. Drawn in code; final avatar art replaces it later.
    /// </summary>
    public class Puppet : MonoBehaviour
    {
        private RectTransform body;
        private RectTransform head;
        private RectTransform eyes;
        private RectTransform leftEye, rightEye;
        private RectTransform watchArm;
        private float nextBlink;
        private float blinkUntil;
        private float nextWatch;
        private float watchUntil;
        private float phase;

        public bool EyesClosed { get; set; }

        private static readonly Color[] Skins =
        {
            new Color32(0xE8, 0xB0, 0x86, 255), new Color32(0xC6, 0x8A, 0x5E, 255), new Color32(0x8D, 0x5A, 0x3B, 255),
        };
        private static readonly Color[] Hair =
        {
            new Color32(0x4A, 0x2E, 0x1E, 255), new Color32(0x22, 0x1A, 0x16, 255), new Color32(0x7A, 0x4E, 0x2A, 255),
        };

        /// <summary>Builds a puppet standing on its feet at the given anchor point. Height is in reference pixels.</summary>
        public static Puppet Create(Transform parent, Vector2 anchor, int seat, float height = 300f, bool big = false)
        {
            var root = NewRect($"Puppet{seat:00}", parent, anchor, anchor, Vector2.zero, Vector2.zero);
            root.pivot = new Vector2(0.5f, 0f);
            float width = height * (big ? 0.62f : 0.46f);
            root.sizeDelta = new Vector2(width, height);
            var puppet = root.gameObject.AddComponent<Puppet>();
            puppet.Build(root, seat, width, height, big);
            return puppet;
        }

        private void Build(RectTransform root, int seat, float width, float height, bool big)
        {
            Color skin = Skins[(seat - 1 + Skins.Length * 10) % Skins.Length];
            Color hair = Hair[(seat + 1) % Hair.Length];
            phase = seat * 1.7f;

            // Body: a rounded jumpsuit with a darker zip seam and the seat number
            body = Part(root, "Body", new Vector2(0.5f, 0f), new Vector2(width, height * 0.62f), UITheme.Teal, width * 0.45f, new Vector2(0.5f, 0f));
            Part(body, "Seam", new Vector2(0.5f, 0f), new Vector2(Mathf.Max(3f, width * 0.03f), height * 0.56f), UITheme.TealDark, 0f, new Vector2(0.5f, 0f));
            var number = NewText("Seat", body, height * 0.075f, FontStyles.Bold, TextAlignmentOptions.Center,
                new Vector2(0.08f, 0.55f), new Vector2(0.48f, 0.8f), Vector2.zero, Vector2.zero);
            number.text = seat.ToString("00");
            number.color = UITheme.Brick;

            // Arm that lifts to check the watch
            watchArm = Part(body, "Arm", new Vector2(big ? 0.9f : 0.86f, 0.78f), new Vector2(width * 0.16f, height * 0.36f),
                UITheme.TealDark, width * 0.1f, new Vector2(0.5f, 1f));
            Part(watchArm, "Watch", new Vector2(0.5f, 0.08f), new Vector2(width * 0.15f, width * 0.1f), UITheme.Gold, 3f, new Vector2(0.5f, 0.5f));

            // Head with hair, eyes and a flat mouth
            float headSize = width * (big ? 0.62f : 0.78f);
            head = Part(root, "Head", new Vector2(0.5f, 0.58f), new Vector2(headSize, headSize), skin, 0f, new Vector2(0.5f, 0f), circle: true);
            Part(head, "Hair", new Vector2(0.5f, 1f), new Vector2(headSize * 1.04f, headSize * 0.42f), hair, headSize * 0.4f, new Vector2(0.5f, 1f));
            eyes = NewRect("Eyes", head, new Vector2(0.5f, 0.48f), new Vector2(0.5f, 0.48f), Vector2.zero, Vector2.zero);
            float eye = headSize * 0.11f;
            leftEye = Part(eyes, "EyeL", new Vector2(0.5f, 0.5f), new Vector2(eye, eye), UITheme.Charcoal, 0f, new Vector2(0.5f, 0.5f), circle: true);
            leftEye.anchoredPosition = new Vector2(-headSize * 0.17f, 0f);
            rightEye = Part(eyes, "EyeR", new Vector2(0.5f, 0.5f), new Vector2(eye, eye), UITheme.Charcoal, 0f, new Vector2(0.5f, 0.5f), circle: true);
            rightEye.anchoredPosition = new Vector2(headSize * 0.17f, 0f);
            Part(head, "Mouth", new Vector2(0.5f, 0.26f), new Vector2(headSize * 0.22f, Mathf.Max(2f, headSize * 0.035f)), UITheme.Charcoal, 0f, new Vector2(0.5f, 0.5f));

            nextBlink = Time.unscaledTime + Random.Range(1.5f, 4f);
            nextWatch = Time.unscaledTime + Random.Range(10f, 18f);
        }

        private void Update()
        {
            float t = Time.unscaledTime;
            bool still = GameSettings.ReduceMotion;

            // Breathe: the body swells a little, the head rides on top
            float breath = still ? 0f : Mathf.Sin(t * 1.6f + phase);
            body.localScale = new Vector3(1f, 1f + breath * 0.015f, 1f);
            head.anchoredPosition = new Vector2(0f, breath * 2f);

            // Blink every 2 to 5 s (eyes stay shut while EyesClosed)
            if (t >= nextBlink)
            {
                blinkUntil = t + 0.12f;
                nextBlink = t + Random.Range(2f, 5f);
            }
            float open = EyesClosed || t < blinkUntil ? 0.12f : 1f;
            leftEye.localScale = rightEye.localScale = new Vector3(1f, open, 1f);

            // Look toward the pointer
            Vector2 look = Vector2.zero;
            if (Mouse.current != null && Screen.width > 0 && !EyesClosed)
            {
                Vector2 screen = RectTransformUtility.WorldToScreenPoint(null, head.position);
                look = Vector2.ClampMagnitude((Mouse.current.position.ReadValue() - screen) / 300f, 1f);
            }
            eyes.anchoredPosition = Vector2.Lerp(eyes.anchoredPosition, look * head.rect.width * 0.06f, 1f - Mathf.Exp(-8f * Time.unscaledDeltaTime));

            // Now and then, lift the arm and glance at the watch
            if (!still && t >= nextWatch)
            {
                watchUntil = t + 1.6f;
                nextWatch = t + Random.Range(12f, 20f);
            }
            float lift = t < watchUntil ? Mathf.Sin(Mathf.InverseLerp(watchUntil - 1.6f, watchUntil, t) * Mathf.PI) : 0f;
            watchArm.localEulerAngles = new Vector3(0f, 0f, lift * 110f);
            if (lift > 0.3f) eyes.anchoredPosition = new Vector2(head.rect.width * 0.05f, -head.rect.width * 0.04f);
        }

        private static RectTransform Part(Transform parent, string name, Vector2 anchor, Vector2 size, Color color,
            float cornerScale, Vector2 pivot, bool circle = false)
        {
            var rect = NewRect(name, parent, anchor, anchor, Vector2.zero, Vector2.zero);
            rect.pivot = pivot;
            rect.sizeDelta = size;
            Image image = rect.gameObject.AddComponent<Image>();
            if (circle) image.sprite = CircleSprite();
            else if (cornerScale > 0f) Rounded(image, color, cornerScale);
            image.color = color;
            image.raycastTarget = false;
            return rect;
        }
    }
}
