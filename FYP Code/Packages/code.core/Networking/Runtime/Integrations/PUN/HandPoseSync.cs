#if PHOTON_UNITY_NETWORKING

namespace MAGES.Networking
{
    using MAGES.Interaction.Interactors;
    using MAGES.Networking;
    using UnityEngine;

    /// <summary>
    /// Synchronization of hand pose transforms over network.
    /// </summary>
    public class HandPoseSync : SyncTransformPhoton
    {
        /// <summary>
        /// Initialises The transform sync of hands.
        /// </summary>
        /// <param name="handInteractor">The hand interactor that will be synced across network.</param>
        public void Initialise(HandInteractor handInteractor)
        {
            if (PhotonViewRef.IsMine)
            {
                var handVisual = handInteractor.transform.Find("HandVisual").Find("Root");
                InitTransforms(handVisual.gameObject);
            }
            else
            {
                InitTransforms(transform.Find("HandVisual").Find("Root").gameObject);
            }
        }

        private void Start()
        {
            if (!PhotonViewRef.IsMine)
            {
                Initialise(null);
            }
        }

        private void InitTransforms(GameObject handVisual)
        {
            foreach (Transform child in handVisual.transform)
            {
                Transforms.Add(child);
                InitTransforms(child.gameObject);
            }

            Initialise();
        }
    }
}

#endif