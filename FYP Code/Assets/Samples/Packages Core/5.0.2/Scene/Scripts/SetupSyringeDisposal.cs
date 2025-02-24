namespace MAGES.ExampleScene
{
    using MAGES;
    using MAGES.SceneGraph;
    using UnityEngine;

    /// <summary>
    /// Script that configures the syringe for the Use action.
    /// </summary>
    public class SetupSyringeDisposal : MonoBehaviour
    {
        private GameObject syringe;

        /// <summary>
        /// Called when the action is initialized.
        /// </summary>
        /// <param name="data">The ACtion data.</param>
        public void ActionInitialized(BaseActionData data)
        {
            UseActionData useData = data as UseActionData;
            syringe = useData.UseObjectSpawned;

            syringe.GetComponent<Rigidbody>().isKinematic = false;
            Hub.Instance.Get<InteractionSystemModule>().GetGrabbable(syringe).EnableKinematicOnSelectExit = false;
        }
    }
}