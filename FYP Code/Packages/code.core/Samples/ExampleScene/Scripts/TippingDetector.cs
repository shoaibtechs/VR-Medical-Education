namespace MAGES.ExampleScene
{
    using UnityEngine;
    using UnityEngine.Events;

    /// <summary>
    /// Detects if an object has tipped passed a certain threshold angle.
    /// </summary>
    public class TippingDetector : MonoBehaviour
    {
        /// <summary>
        /// The tipping threshold angle in degrees.
        /// </summary>
        [Range(10f, 170f)]
        [SerializeField]
        [Tooltip("The tipping threshold angle in degrees")]
        private float tippingThreshold = 30.0f;

        /// <summary>
        /// Event triggered when tipping starts.
        /// </summary>
        [SerializeField]
        [Tooltip("Event triggered when tipping starts")]
        private UnityEvent onTipStart;

        /// <summary>
        /// Event triggered when tipping stops.
        /// </summary>
        [SerializeField]
        [Tooltip("Event triggered when tipping stops")]
        private UnityEvent onTipStopped;

        private bool isTipping; // Indicates if the object is currently tipping

        /// <summary>
        /// Gets a value indicating whether the object is currently tipping.
        /// </summary>
        public bool IsTipping { get => isTipping; private set => isTipping = value; }

        private void FixedUpdate()
        {
            // Calculate the angle between the object's up vector and the world up vector
            float angle = Vector3.Angle(transform.up, Vector3.up);

            // Check if the angle exceeds the tipping threshold
            if (angle > tippingThreshold)
            {
                // If the object wasn't previously tipping, trigger the start event
                if (!IsTipping)
                {
                    IsTipping = true;
                    if (onTipStart != null)
                    {
                        onTipStart.Invoke();
                    }
                }
            }
            else
            {
                // If the object was previously tipping, trigger the stop event
                if (IsTipping)
                {
                    IsTipping = false;
                    if (onTipStopped != null)
                    {
                        onTipStopped.Invoke();
                    }
                }
            }
        }
    }
}