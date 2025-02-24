namespace Packages.Scene
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
        /// Gets or sets a value indicating whether the cotton ball looks wet.
        /// </summary>
        public bool IsWet
        {
            get => isWet;
            set
            {
                isWet = value;
                UpdateMaterial();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the cotton ball is grabbed.
        /// </summary>
        [SerializeField]
        public bool IsGrabbed
        {
            get => isGrabbed;
            set
            {
                isGrabbed = value;
                UpdateRendererState();
            }
        }

        /// <summary>
        /// Updates the renderer state of the cotton ball.
        /// </summary>
        private void UpdateRendererState()
        {
            ungrabbedRenderer.enabled = !isGrabbed;
            grabbedRenderer.enabled = isGrabbed;
        }

        /// <summary>
        /// Updates the material state of the cotton ball.
        /// </summary>
        private void UpdateMaterial()
        {
            var material = isWet ? wetMaterial : dryMaterial;
            ungrabbedRenderer.material = material;
            grabbedRenderer.material = material;
        }

        private void Start()
        {
            ungrabbedRenderer = transform.GetChild(0).GetComponent<Renderer>();
            grabbedRenderer = transform.GetChild(1).GetComponent<Renderer>();

            dryMaterial = ungrabbedRenderer.material;
            wetMaterial = grabbedRenderer.material;

            UpdateRendererState();
            UpdateMaterial();
        }
    }
}
