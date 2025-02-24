 namespace MAGES.DeviceManager
{
    using System.Linq;
    using MAGES;
    using UnityEngine;

    /// <summary>
    /// Responsible for offering public methods to be called from external buttons regarding the scenegraph flow.
    /// </summary>
    public class MobileActionsHandler : MonoBehaviour
    {
        private GameObject nextAction;
        private GameObject prevAction;

        /// <summary>
        /// Used for performing actions from MobileNavigation Perform button.
        /// </summary>
        public void ActionPerform()
        {
            if (Hub.Instance != null)
            {
                Hub.Instance?.Get<SceneGraphModule>().Skip();
            }
        }

        /// <summary>
        /// Used for undoing actions from MobileNavigation Undo button.
        /// </summary>
        public void ActionUndo()
        {
            if (Hub.Instance != null)
            {
                Hub.Instance?.Get<SceneGraphModule>().Undo();
            }
        }

        private void Start()
        {
            nextAction = GameObject.Find("NextAction");
            prevAction = GameObject.Find("PreviousAction");

            nextAction?.SetActive(false);
            prevAction?.SetActive(false);

            Hub.Instance.Get<SceneGraphModule>().ActionPerformed(
            (action, skipped) =>
            {
                nextAction?.SetActive(true);
                prevAction?.SetActive(true);
            });

            Hub.Instance.Get<SceneGraphModule>().ActionInitialized(
            (action) =>
            {
                if (action.IsStartAction)
                {
                    nextAction?.SetActive(false);

                    if (!Hub.Instance.Get<NetworkingModule>().IsInitialized)
                    {
                        prevAction?.SetActive(false);
                    }
                }
            });
        }
    }
}