namespace MAGES.ExampleScene
{
    using UnityEngine;

    /// <summary>
    /// Controls the visual state of the Cotton ball.
    /// </summary>
    public class CottonBallStateManager : MonoBehaviour
    {
        private bool isWet = false;
        private bool isGrabbed = false;
        private Material dryMaterial;
        private Material wetMaterial;
        private Renderer ungrabbedRenderer;
        private Renderer grabbedRenderer;

        /// <summary>
        /// Gets or Sets a value indicating whether the cotton ball looks wet.
        /// </summary>
        public bool IsWet { get => isWet; set => isWet = value; }

        /// <summary>
        /// Gets or Sets a value indicating whether the cotton ball is grabbed.
        /// </summary>
        [SerializeField]
        public bool IsGrabbed { get => isGrabbed; set => isGrabbed = value; }

        /// <summary>
        /// Sets the grabbed state of the cotton ball.
        /// </summary>
        /// <param name="isGrabbed">State to set to.</param>
        public void SetGrabbedState(bool isGrabbed)
        {
            ungrabbedRenderer.enabled = !isGrabbed;
            grabbedRenderer.enabled = isGrabbed;
        }

        /// <summary>
        /// Sets the wet state of the cotton ball.
        /// </summary>
        /// <param name="isWet">State to set to.</param>
        public void SetWetState(bool isWet)
        {
            ungrabbedRenderer.material = isWet ? wetMaterial : dryMaterial;
            grabbedRenderer.material = isWet ? wetMaterial : dryMaterial;
        }

        private void Start()
        {
            ungrabbedRenderer = transform.GetChild(0).GetComponent<Renderer>();
            grabbedRenderer = transform.GetChild(1).GetComponent<Renderer>();

            dryMaterial = ungrabbedRenderer.material;
            wetMaterial = grabbedRenderer.material;

            SetGrabbedState(false);
            SetWetState(false);
        }
    }
}