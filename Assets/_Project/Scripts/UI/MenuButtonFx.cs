using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TDTS.UI
{
    /// <summary>
    /// Wireframe 01 button states: hover tilts 2 degrees and grows to 1.03, press moves the face
    /// down 4 px onto its hard shadow. Put it on the button's face; the shadow sits behind it.
    /// </summary>
    public class MenuButtonFx : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        private RectTransform face;
        private Selectable selectable;
        private bool hover, pressed;
        private float tilt = 2f;

        private void Awake()
        {
            face = (RectTransform)transform;
            selectable = GetComponent<Selectable>();
            tilt = Random.value < 0.5f ? 2f : -2f;
        }

        private bool Active => selectable == null || selectable.IsInteractable();

        public void OnPointerEnter(PointerEventData e) => hover = Active;
        public void OnPointerExit(PointerEventData e) => hover = pressed = false;
        public void OnPointerDown(PointerEventData e) => pressed = Active;
        public void OnPointerUp(PointerEventData e) => pressed = false;

        private void Update()
        {
            float k = 1f - Mathf.Exp(-18f * Time.unscaledDeltaTime);
            float scale = hover && !pressed ? 1.03f : 1f;
            face.localScale = Vector3.Lerp(face.localScale, new Vector3(scale, scale, 1f), k);
            float angle = Mathf.LerpAngle(face.localEulerAngles.z, hover && !pressed ? tilt : 0f, k);
            face.localEulerAngles = new Vector3(0f, 0f, angle);
            face.anchoredPosition = Vector2.Lerp(face.anchoredPosition, new Vector2(0f, pressed ? -4f : 0f), k);
        }
    }
}
