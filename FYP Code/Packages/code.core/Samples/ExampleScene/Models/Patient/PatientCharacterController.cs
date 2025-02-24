namespace MAGES.ExampleScene
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Controller that manages the patient's model.
    /// </summary>
    public class PatientCharacterController : MonoBehaviour
    {
        [SerializeField]
        private List<GameObject> patientMeshes = new List<GameObject>();

        /// <summary>
        /// Start is called before the first frame update.
        /// </summary>
        public void Start()
        {
            foreach (var mesh in patientMeshes)
            {
                Mesh bakeMesh = new Mesh();
                mesh.GetComponent<SkinnedMeshRenderer>().BakeMesh(bakeMesh);

                var collider = mesh.GetComponent<MeshCollider>();
                collider.sharedMesh = bakeMesh;
            }
        }
    }
}