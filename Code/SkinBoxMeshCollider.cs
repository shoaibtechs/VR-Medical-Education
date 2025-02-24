namespace Packages.Scene
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// A Class that manages the skin collider of the SkinBox.
    /// </summary>
    public class SkinBoxMeshCollider : MonoBehaviour
    {
        [SerializeField]
        private GameObject meshColliderObject = null;

        [SerializeField]
        private GameObject toolObject = null;

        private SkinnedMeshRenderer skinnedMesh = null;
        private MeshCollider meshCollider = null;

        private bool isInteracting = false;

        /// <summary>
        /// Start function.
        /// </summary>
        public void Start()
        {
            UpdateMeshCollider();
        }

        /// <summary>
        /// Start is called before the first frame update.
        /// </summary>
        public void Update()
        {
            if (isInteracting)
            {
                UpdateMeshCollider();
            }

            isInteracting = false;
        }

        private void OnCollisionStay(Collision collision)
        {
            if (collision.transform.root.name == toolObject.name + "(Clone)")
            {
                isInteracting = true;
            }
            else
            {
                isInteracting = false;
            }
        }

        private void UpdateMeshCollider()
        {
            Mesh bakeMesh = new Mesh();
            skinnedMesh = meshColliderObject.GetComponent<SkinnedMeshRenderer>();
            skinnedMesh.BakeMesh(bakeMesh);

            meshCollider = meshColliderObject.GetComponent<MeshCollider>();
            meshCollider.sharedMesh = bakeMesh;
        }
    }
}