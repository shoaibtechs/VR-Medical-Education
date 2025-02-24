#if PHOTON_UNITY_NETWORKING
namespace MAGES.Networking
{
    using MAGES;
    using MAGES.Interaction.Interactables;
    using MAGES.Interaction.Interactors;
    using MAGES.SceneGraph;
    using Photon.Pun;
    using UnityEngine;

    /// <summary>
    /// Responsible for MAGES RPC calls.
    /// </summary>
    [RequireComponent(typeof(PhotonView))]
    public class PunMessageHandler : MonoBehaviour
    {
        private PhotonView photonView;

        /// <summary>
        /// Called when a client requests a change in the scenegraph.
        /// </summary>
        /// <param name="code">The keycode for the specific range that was requested.</param>
        /// <param name="actionID">The ID of the action that will change.</param>
        [PunRPC]
        public void StateChangeFromClient(byte code, string actionID)
        {
            var baseActionData = Hub.Instance.Get<MAGESSceneGraph>().Runner.RuntimeGraph.Find(actionID);
            switch (code)
            {
                case 0:
                    Hub.Instance.Get<MAGESSceneGraph>().Runner.PerformAction(baseActionData);
                    break;
                case 1:
                    Hub.Instance.Get<MAGESSceneGraph>().Runner.UndoAction(baseActionData);
                    break;
            }
        }

        /// <summary>
        /// Called when another user requests a destroy of a component to all other users.
        /// </summary>
        /// <param name="viewID">The keycode for the specific range that was requested.</param>
        /// <param name="componentType">The ID of the action that will change.</param>
        [PunRPC]
        public void DestroyComponent(int viewID, string componentType)
        {
            var objectPhotonView = PhotonNetwork.GetPhotonView(viewID);
            if (!objectPhotonView)
            {
                Debug.LogError("Network destroy component called for invalid viewID: " + viewID + ".");
                return;
            }

            var component = objectPhotonView.gameObject.GetComponent(componentType);
            if (component != null)
            {
                Destroy(component);
            }
            else
            {
                Debug.LogWarning("[NetworkDestroyComponent] Could not find component " + componentType + " in object " + objectPhotonView.gameObject.name + ". It may be already destroyed.");
            }
        }

        /// <summary>
        /// Sends an RPC call to Host to request a change in the scenegraph.
        /// </summary>
        /// <param name="code">The keycode for the specific range that was requested.</param>
        /// <param name="actionID">The ID of the action that will change.</param>
        public void RequestStateChange(byte code, string actionID)
        {
            if (photonView == null)
            {
                photonView = PhotonView.Get(this);
            }

            photonView.RPC("StateChangeFromClient", RpcTarget.MasterClient, code, actionID);
        }

        /// <summary>
        /// Sends an RPC call to all users to destroy a component.
        /// </summary>
        /// <param name="objectPhotonView">The photonView of the gameobject that has the component. </param>
        /// <param name="componentType"> The type of component that will be destroyed.</param>
        /// <returns>False if the objectPhotonView is invalid.</returns>
        public bool DestroyComponent(PhotonView objectPhotonView, string componentType)
        {
            if (objectPhotonView != null)
            {
                photonView.RPC("DestroyComponent", RpcTarget.All, objectPhotonView.ViewID, componentType);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Called when a grabbable is activated. Propagates the activation to all users.
        /// </summary>
        /// <param name="eventArgs">The args that have the reference of the interactable.</param>
        public void ActivatedGrabbable(ActivateEnterInteractionEventArgs eventArgs)
        {
            var grabbableObject = (Grabbable)eventArgs.Interactable;
            if (grabbableObject != null)
            {
                var objectPhotonView = grabbableObject.GetComponent<PhotonView>();
                photonView.RPC("RegisterActivation", RpcTarget.All, objectPhotonView.ViewID, true);
            }
        }

        /// <summary>
        /// Called when a grabbable is deactivated.
        /// </summary>
        /// <param name="eventArgs">The args that have the reference of the interactable.</param>
        public void DeActivatedGrabbable(ActivateExitInteractionEventArgs eventArgs)
        {
            var grabbableObject = (Grabbable)eventArgs.Interactable;
            if (grabbableObject != null)
            {
                var objectPhotonView = grabbableObject.GetComponent<PhotonView>();
                photonView.RPC("RegisterActivation", RpcTarget.All, objectPhotonView.ViewID, false);
            }
        }

        /// <summary>
        /// Registers an object that was remotely activated.
        /// </summary>
        /// <param name="viewID">The network ID of the object.</param>
        /// <param name="activated">Whether the object is activated or deactivated.</param>
        [PunRPC]
        public void RegisterActivation(int viewID, bool activated)
        {
            var view = PhotonView.Find(viewID);
            if (!view)
            {
                Debug.LogError("Could not register activation for viewID " + viewID + ". ViewID not found.");
            }

            var activatedObject = view.gameObject;
            Hub.Instance.Get<NetworkingModule>().RegisterActivatedObject(activatedObject, activated);
        }

        /// <summary>
        /// Adds activation listeners to a hand interactor.
        /// </summary>
        /// <param name="handInteractor">The hand interactor in which to add listeners.</param>
        public void AddActivationListeners(HandInteractor handInteractor)
        {
            handInteractor.ActivateEntered.AddListener(ActivatedGrabbable);
            handInteractor.ActivateExited.AddListener(DeActivatedGrabbable);
        }

        private void Start()
        {
            photonView = PhotonView.Get(this);
            var integration = (PUNIntegration)Hub.Instance.Get<NetworkingModule>().Integration;
            integration.NetworkMessageHandler = this;
            var interactionSystem = Hub.Instance.Get<InteractionSystemModule>();
            AddActivationListeners(interactionSystem.LeftHand.GetComponent<HandInteractor>());
            AddActivationListeners(interactionSystem.RightHand.GetComponent<HandInteractor>());
        }
    }
}
#endif