namespace MAGES.DeviceManager
{
    using System;
    using System.Collections.Generic;
    using Unity.Collections.LowLevel.Unsafe;
    using UnityEngine;
    using UnityEngine.EventSystems;
    using UnityEngine.Experimental.Rendering;
    using UnityEngine.UI;

    /// <summary>
    /// MAGES joystick. Primarily used for moving and rotating the camera.
    /// </summary>
    public class MAGESScreenStick : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private const string KDynamicOriginClickable = "DynamicOriginClickable";

        [SerializeField]
        [Min(0)]
        private float movementRange = 25;

        [SerializeField]
        [Tooltip("Defines how this control will apply the user's input to the camera.")]
        private MovementType movementType;

        [SerializeField]
        [Tooltip("Choose how the onscreen stick will move relative to it's origin and the press position.\n\n" +
            "RelativePositionWithStaticOrigin: The control's center of origin is fixed. " +
            "The control will begin un-actuated at it's centered position and then move relative to the pointer or finger motion.\n\n" +
            "ExactPositionWithStaticOrigin: The control's center of origin is fixed. The stick will immediately jump to the " +
            "exact position of the click or touch and begin tracking motion from there.\n\n" +
            "ExactPositionWithDynamicOrigin: The control's center of origin is determined by the initial press position. " +
            "The stick will begin un-actuated at this center position and then track the current pointer or finger position.")]
        private BehaviourTypes behaviour;

        private Vector3 startPos;
        private Vector2 pointerDownPos;

        [NonSerialized]
        private List<RaycastResult> raycastResults;

        [NonSerialized]
        private PointerEventData pointerEventData;

        /// <summary>Defines how the onscreen stick will move relative to it's center of origin and the press position.</summary>
        public enum BehaviourTypes
        {
            /// <summary>The control's center of origin is fixed in the scene.
            /// The control will begin un-actuated at it's centered position and then move relative to the press motion.</summary>
            RelativePositionWithStaticOrigin,

            /// <summary>The control's center of origin is fixed in the scene.
            /// The control may begin from an actuated position to ensure it is always tracking the current press position.</summary>
            ExactPositionWithStaticOrigin,

            /// <summary>The control's center of origin is determined by the initial press position.
            /// The control will begin un-actuated at this center position and then track the current press position.</summary>
            ExactPositionWithDynamicOrigin,
        }

        /// <summary>
        /// Defines how the input will be applied.
        /// </summary>
        private enum MovementType
        {
            /// <summary>
            /// Moves the camera.
            /// </summary>
            MoveCamera,

            /// <summary>
            /// Rotates the camera smoothly.
            /// </summary>
            RotateCamera,
        }

        /// <summary>Gets or sets how the onscreen stick will move relative to it's origin and the press position.</summary>
        public BehaviourTypes Behaviour
        {
            get => behaviour;
            set => behaviour = value;
        }

        /// <summary>
        /// Gets or sets the distance from the onscreen control's center of origin, around which the control can move.
        /// </summary>
        public float MovementRange
        {
            get => movementRange;
            set => movementRange = value;
        }

        /*
        /// <summary>
        /// Gets or sets defines the circular region where the onscreen control may have it's origin placed.
        /// </summary>
        /// <remarks>
        /// This only applies if <see cref="behaviour"/> is set to <see cref="BehaviourTypes.ExactPositionWithDynamicOrigin"/>.
        /// When the first press is within this region, then the control will appear at that position and have it's origin of motion placed there.
        /// Otherwise, if pressed outside of this region the control will ignore it.
        /// This property defines the radius of the circular region. The center point being defined by the component position in the scene.
        /// </remarks>
        public float DynamicOriginRange
        {
            get => dynamicOriginRange;
            set
            {
                // ReSharper disable once CompareOfFloatsByEqualityOperator
                if (dynamicOriginRange == value)
                {
                    return;
                }

                dynamicOriginRange = value;
                UpdateDynamicOriginClickableArea();
            }
        }
        */

        /// <summary>
        /// Callback to handle OnPointerDown UI events.
        /// </summary>
        /// <param name="eventData">The data for this interaction.</param>
        public void OnPointerDown(PointerEventData eventData)
        {
            if (eventData == null)
            {
                throw new ArgumentNullException(nameof(eventData));
            }

            BeginInteraction(eventData.position, eventData.pressEventCamera);
        }

        /// <summary>
        /// Callback to handle OnDrag UI events.
        /// </summary>
        /// <param name="eventData">The data for this interaction.</param>
        public void OnDrag(PointerEventData eventData)
        {
            if (eventData == null)
            {
                throw new ArgumentNullException(nameof(eventData));
            }

            MoveStick(eventData.position, eventData.pressEventCamera);
        }

        /// <summary>
        /// Callback to handle OnPointerUp UI events.
        /// </summary>
        /// <param name="eventData">The data for this interaction.</param>;
        public void OnPointerUp(PointerEventData eventData)
        {
            EndInteraction();
            ApplyValue(Vector2.zero);
        }

        private void Start()
        {
            startPos = ((RectTransform)transform).anchoredPosition;

            if (behaviour != BehaviourTypes.ExactPositionWithDynamicOrigin)
            {
                return;
            }

            pointerDownPos = startPos;

            var dynamicOrigin = new GameObject(KDynamicOriginClickable, typeof(Image));
            dynamicOrigin.transform.SetParent(transform);
            var image = dynamicOrigin.GetComponent<Image>();
            image.color = new Color(1, 1, 1, 0);
            var rectTransform = (RectTransform)dynamicOrigin.transform;
            rectTransform.localScale = new Vector3(1, 1, 0);
            rectTransform.anchoredPosition3D = Vector3.zero;

            image.sprite = CreateCircleSprite(16, new Color32(255, 255, 255, 255));
            image.alphaHitTestMinimumThreshold = 0.5f;
        }

        private void BeginInteraction(Vector2 pointerPosition, Camera interfaceCamera)
        {
            var canvasRect = transform.parent != null ? transform.parent.GetComponentInParent<RectTransform>() : null;
            if (canvasRect == null)
            {
                Debug.LogError("OnScreenStick needs to be attached as a child to a UI Canvas to function properly.");
                return;
            }

            switch (behaviour)
            {
                case BehaviourTypes.RelativePositionWithStaticOrigin:
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pointerPosition, interfaceCamera, out pointerDownPos);
                    break;
                case BehaviourTypes.ExactPositionWithStaticOrigin:
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pointerPosition, interfaceCamera, out pointerDownPos);
                    MoveStick(pointerPosition, interfaceCamera);
                    break;
                case BehaviourTypes.ExactPositionWithDynamicOrigin:
                    RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pointerPosition, interfaceCamera, out var pointerDown);
                    pointerDownPos = ((RectTransform)transform).anchoredPosition = pointerDown;
                    break;
            }
        }

        private void MoveStick(Vector2 pointerPosition, Camera interfaceCamera)
        {
            var canvasRect = transform.parent != null ? transform.parent.GetComponentInParent<RectTransform>() : null;
            if (canvasRect == null)
            {
                Debug.LogError("OnScreenStick needs to be attached as a child to a UI Canvas to function properly.");
                return;
            }

            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvasRect, pointerPosition, interfaceCamera, out var position);
            var delta = position - pointerDownPos;

            switch (behaviour)
            {
                case BehaviourTypes.RelativePositionWithStaticOrigin:
                    delta = Vector2.ClampMagnitude(delta, MovementRange);
                    ((RectTransform)transform).anchoredPosition = (Vector2)startPos + delta;
                    break;

                case BehaviourTypes.ExactPositionWithStaticOrigin:
                    delta = position - (Vector2)startPos;
                    delta = Vector2.ClampMagnitude(delta, MovementRange);
                    ((RectTransform)transform).anchoredPosition = (Vector2)startPos + delta;
                    break;

                case BehaviourTypes.ExactPositionWithDynamicOrigin:
                    delta = Vector2.ClampMagnitude(delta, MovementRange);
                    ((RectTransform)transform).anchoredPosition = pointerDownPos + delta;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            var newPos = new Vector2(delta.x / MovementRange, delta.y / MovementRange);
            ApplyValue(newPos);
        }

        private void EndInteraction()
        {
            ((RectTransform)transform).anchoredPosition = pointerDownPos = startPos;
        }

        private void ApplyValue(Vector2 value)
        {
            if (Hub.Instance == null)
            {
                return;
            }

            var deviceManager = (MAGESDeviceManager)Hub.Instance.Get<DeviceManagerModule>();
            if (deviceManager != null)
            {
                switch (movementType)
                {
                    case MovementType.MoveCamera:
                        deviceManager.ControllerActions.CustomMoveCamera(value);
                        break;
                    case MovementType.RotateCamera:
                        deviceManager.ControllerActions.CustomRotateCamera(value);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        private void OnDrawGizmosSelected()
        {
            var transform1 = transform;
            Gizmos.matrix = ((RectTransform)transform1.parent).localToWorldMatrix;

            var tmpStartPos = ((RectTransform)transform1).anchoredPosition;
            if (Application.isPlaying)
            {
                tmpStartPos = startPos;
            }

            Gizmos.color = new Color32(84, 173, 219, 255);

            var center = tmpStartPos;
            if (Application.isPlaying && behaviour == BehaviourTypes.ExactPositionWithDynamicOrigin)
            {
                center = pointerDownPos;
            }

            DrawGizmoCircle(center, movementRange);

            if (behaviour != BehaviourTypes.ExactPositionWithDynamicOrigin)
            {
                return;
            }

            Gizmos.color = new Color32(158, 84, 219, 255);
        }

        private void DrawGizmoCircle(Vector2 center, float radius)
        {
            for (var i = 0; i < 32; i++)
            {
                var radians = i / 32f * Mathf.PI * 2;
                var nextRadian = (i + 1) / 32f * Mathf.PI * 2;
                Gizmos.DrawLine(
                    new Vector3(center.x + (Mathf.Cos(radians) * radius), center.y + (Mathf.Sin(radians) * radius), 0),
                    new Vector3(center.x + (Mathf.Cos(nextRadian) * radius), center.y + (Mathf.Sin(nextRadian) * radius), 0));
            }
        }

        private unsafe Sprite CreateCircleSprite(int radius, Color32 colour)
        {
            // cache the diameter
            var d = radius * 2;

            var texture = new Texture2D(d, d, DefaultFormat.LDR, TextureCreationFlags.None);
            var colours = texture.GetRawTextureData<Color32>();
            var coloursPtr = (Color32*)colours.GetUnsafePtr();
            UnsafeUtility.MemSet(coloursPtr, 0, colours.Length * UnsafeUtility.SizeOf<Color32>());

            // pack the colour into a ulong so we can write two pixels at a time to the texture data
            var colorPtr = (uint*)UnsafeUtility.AddressOf(ref colour);
            var colourAsULong = *(ulong*)colorPtr << 32 | *colorPtr;

            float squared = radius * radius;

            // loop over the texture memory one column at a time filling in a line between the two x coordinates
            // of the circle at each column
            for (var y = -radius; y < radius; y++)
            {
                // for the current column, calculate what the x coordinate of the circle would be
                // using x^2 + y^2 = r^2, or x^2 = r^2 - y^2. The square root of the value of the
                // x coordinate will equal half the width of the circle at the current y coordinate
                var halfWidth = (int)Mathf.Sqrt(squared - (y * y));

                // position the pointer so it points at the memory where we should start filling in
                // the current line
                var ptr = coloursPtr
                    + ((y + radius) * d) // the position of the memory at the start of the row at the current y coordinate
                    + radius - halfWidth;   // the position along the row where we should start inserting colours

                // fill in two pixels at a time
                for (var x = 0; x < halfWidth; x++)
                {
                    *(ulong*)ptr = colourAsULong;
                    ptr += 2;
                }
            }

            texture.Apply();

            var sprite = Sprite.Create(texture, new Rect(0, 0, d, d), new Vector2(radius, radius), 1, 0, SpriteMeshType.FullRect);
            return sprite;
        }
    }
}
