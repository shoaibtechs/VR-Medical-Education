namespace Packages.Scene
{
    using UnityEngine;

    /// <summary>
    /// Simulates a liquid effect on a mesh.
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    [RequireComponent(typeof(MeshFilter))]
    [ExecuteInEditMode]
    public class LiquidRenderer : MonoBehaviour
    {
        [Range(0, 1)]
        [SerializeField]
        private float fill = 0.5f;

        [Tooltip("How thick the liquid is. Affects the movement of the simulation")]
        [Range(0, 1)]
        [SerializeField]
        private float liquidThickness = 0.5f;

        [Tooltip("Is the liquid inverted. (Invert gravity).")]
        [SerializeField]
        private bool isInverted = false;

        private float maxDisturbance;
        private float disturbanceSpeed;
        private float recovery;
        private float density;

        private Mesh mesh;
        private Renderer rend;
        private Rigidbody rb;
        private Vector2 addVariance;
        private float currPhase;
        private float currTime;
        private float thicknessScaled;
        private Vector3 velocity;
        private Vector3 angularVelocity;
        private float deltaTime;
        private Vector2 variance;
        private Vector3 finalPos;

        private void Awake()
        {
            mesh = GetComponent<MeshFilter>().sharedMesh;
            rend = GetComponent<Renderer>();
            rb = transform.root.GetComponent<Rigidbody>();

            if (mesh == null || rend == null || rb == null)
            {
                Debug.LogError(
                    "LiquidRenderer: Missing required components.", transform);
                Destroy(this);
            }
        }

        private void Update()
        {
            if (rend == null || rend.sharedMaterial == null)
            {
                return;
            }

            if (rb == null)
            {
                Debug.LogWarning(
                    "LiquidRenderer: No rigidbody placed on the root object" +
                    " of the Liquid Renderer");
                return;
            }

            thicknessScaled = liquidThickness * 0.3f;
            density = thicknessScaled * 10;
            disturbanceSpeed = 1 - thicknessScaled;
            maxDisturbance = (1 - thicknessScaled) * 0.03f;
            recovery = 1 - thicknessScaled;

            velocity = rb.velocity;
            angularVelocity = rb.angularVelocity;

            deltaTime = Time.deltaTime;
            variance = Vector2.zero;

            currTime += deltaTime;

            if (deltaTime != 0)
            {
                // Calculate variation for the current frame
                addVariance.x = Mathf.Lerp(addVariance.x, 0, deltaTime * recovery);
                addVariance.y = Mathf.Lerp(addVariance.y, 0, deltaTime * recovery);

                currPhase = Mathf.Lerp(
                    currPhase,
                    Mathf.Sin(2 * Mathf.PI * disturbanceSpeed * currTime),
                    deltaTime * Mathf.Clamp(velocity.magnitude + angularVelocity.magnitude, density, 10));

                variance.x = addVariance.x * currPhase;
                variance.y = addVariance.y * currPhase;

                // Prepare variation for next frame based on current
                addVariance.x += Mathf.Clamp(
                    (velocity.x + (velocity.y * 0.2f) + angularVelocity.z + angularVelocity.y) * maxDisturbance,
                    -maxDisturbance,
                    maxDisturbance);
                addVariance.y += Mathf.Clamp(
                    (velocity.z + (velocity.y * 0.2f) + angularVelocity.x + angularVelocity.y) * maxDisturbance,
                    -maxDisturbance,
                    maxDisturbance);
            }

            rend.sharedMaterial.SetFloat("_VarianceX", variance.x);
            rend.sharedMaterial.SetFloat("_VarianceY", variance.y);
            rend.sharedMaterial.SetFloat("_InvertLiquid", isInverted ? 1 : 0);

            finalPos = transform.TransformPoint(mesh.bounds.center) - transform.position;

            if (isInverted)
            {
                finalPos.y = finalPos.y - fill;
            }
            else
            {
                finalPos.y = finalPos.y - (1 - fill);
            }

            rend.sharedMaterial.SetVector("_FillAmount", finalPos);
        }
    }
}
