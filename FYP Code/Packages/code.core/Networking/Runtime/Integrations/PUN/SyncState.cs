#if PHOTON_UNITY_NETWORKING
namespace MAGES.Networking
{
    using Photon.Pun;
    using Hashtable = ExitGames.Client.Photon.Hashtable;

    /// <summary>
    /// Synchronizes the state of the MAGES scenegraph.
    /// </summary>
    public class SyncState : MonoBehaviourPunCallbacks
    {
        private Hashtable customRoomProperties;

        private void Awake()
        {
            customRoomProperties = new Hashtable();
            Hub.Instance.Get<SceneGraphModule>().ForEachAction(AddActionToHashTable);
            Hub.Instance.Get<SceneGraphModule>().ActionInitialized(UpdateCustomPropertiesOnInit);
            Hub.Instance.Get<SceneGraphModule>().ActionPerformed(UpdateCustomPropertiesOnPerform);

            PhotonNetwork.CurrentRoom.SetCustomProperties(customRoomProperties);
        }

        private void UpdateCustomPropertiesOnPerform(BaseActionData data, bool test)
        {
            Hashtable changes = new Hashtable { { data.ID, (int)data.State } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(changes);
        }

        private void UpdateCustomPropertiesOnInit(BaseActionData data)
        {
            Hashtable changes = new Hashtable { { data.ID, (int)data.State } };
            PhotonNetwork.CurrentRoom.SetCustomProperties(changes);
        }

        private void AddActionToHashTable(BaseActionData actionData)
        {
            customRoomProperties.Add(actionData.ID, (int)actionData.State);
        }
    }
}
#endif