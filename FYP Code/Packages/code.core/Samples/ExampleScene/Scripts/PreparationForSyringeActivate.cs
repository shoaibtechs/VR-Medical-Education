namespace Packages.Scene
{
    using Packages;
    using Packages.SceneGraph;
    using UnityEngine;

    /// <summary>
    /// Script to prepare the syringe for the injection.
    /// </summary>
    public class PreparationForSyringeActivate : MonoBehaviour
    {
        private GameObject syringe;
        private ActivateActionData activateActionData;
        private Rigidbody syringeRigidbody;

        /// <summary>
        /// Function that is being called when the action is initialized.
        /// </summary>
        /// <param name="data">The Action data.</param>
        public void ActionInitialized(BaseActionData data)
        {
            // Restrict syringe movement
            activateActionData = (ActivateActionData)data;
            syringe = activateActionData.ActivateObjectSpawned;

            syringeRigidbody = syringe.GetComponent<Rigidbody>();
            syringeRigidbody.isKinematic = true;
            syringeRigidbody.constraints = RigidbodyConstraints.FreezeAll;

            Hub.Instance.Get<InteractionSystemModule>().GetGrabbable(syringe).DisableKinematicOnSelectEnter = false;
        }

        /// <summary>
        /// Function that is being called when the action is performed.
        /// </summary>
        /// <param name="data">The ActionData.</param>
        public void ActionPerformed(BaseActionData data)
        {
            // UnRestrict syringe movement
            syringeRigidbody.isKinematic = false;
            syringeRigidbody.constraints = RigidbodyConstraints.None;

            Hub.Instance.Get<InteractionSystemModule>().GetGrabbable(syringe).DisableKinematicOnSelectEnter = true;
        }
    }
}