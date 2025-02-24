#if PHOTON_UNITY_NETWORKING
namespace MAGES.Networking.Editor
{
    using System;
    using System.Collections;
    using System.Linq;
    using MAGES.Networking;
    using MAGES.Utilities;
    using Photon.Pun;
    using UnityEditor;
    using UnityEditor.SceneManagement;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Updates the current scene to always have photon views correctly.
    /// </summary>
    [InitializeOnLoad]
    public static class NetworkSceneUpdater
    {
        static NetworkSceneUpdater()
        {
            EditorSceneManager.sceneDirtied += AddPhotonViews;
            EditorSceneManager.sceneLoaded += AddPhotonViewsOnLoad;
            EditorSceneManager.sceneSaved += AddPhotonViews;
            EditorSceneManager.sceneOpened += AddPhotonViewsOnOpen;
        }

        /// <summary>
        /// Adds photon view component to each grabbable object in the scene.
        /// </summary>
        /// <param name="sceneToExamine">Reference to the scene that will be updated.</param>
        public static void AddPhotonViews(Scene sceneToExamine)
        {
            if (Hub.Instance == null || Hub.Instance.Get<NetworkingModule>() == null || !Hub.Instance.Get<NetworkingModule>().AutomaticSceneSetup)
            {
                return;
            }

            var gameObjects = sceneToExamine.GetRootGameObjects();
            Array.Sort(gameObjects, new SortByName());
            foreach (var gameObject in gameObjects)
            {
                var grabbable = gameObject.GetComponent<IInteractable>();
                if (grabbable != null && gameObject.transform.parent == null)
                {
                    var photonView = gameObject.GetOrAddComponent<PhotonView>();
                    photonView.OwnershipTransfer = OwnershipOption.Takeover;
                    gameObject.GetOrAddComponent<MAGESObject>();
                    gameObject.GetOrAddComponent<SyncTransformPhoton>();
                }

                var childrenPhotonViews = gameObject.GetComponentsInChildren<PhotonView>().Where(go => go.gameObject != gameObject);;
                foreach (var childrenPhotonView in childrenPhotonViews)
                {
                    if (childrenPhotonView == null || childrenPhotonView.gameObject == null)
                    {
                        continue;
                    }

                    UnityEngine.Object.DestroyImmediate(childrenPhotonView.gameObject.GetComponent<SyncTransformPhoton>());
                    UnityEngine.Object.DestroyImmediate(childrenPhotonView.gameObject.GetComponent<MAGESObject>());
                    UnityEngine.Object.DestroyImmediate(childrenPhotonView);
                }
            }
        }

        private static void AddPhotonViewsOnLoad(Scene sceneToExamine, LoadSceneMode mode)
        {
            AddPhotonViews(sceneToExamine);
        }

        private static void AddPhotonViewsOnOpen(Scene sceneToExamine, OpenSceneMode mode)
        {
            AddPhotonViews(sceneToExamine);
        }

        private class SortByName : IComparer
        {
            /// <inheritdoc cref="IComparer.Compare"/>
            int IComparer.Compare(object x, object y)
            {
                return new CaseInsensitiveComparer().Compare(((GameObject)x)?.name, ((GameObject)y)?.name);
            }
        }
    }
}
#endif
