#if PHOTON_UNITY_NETWORKING
namespace MAGES.Networking
{
    using System.Collections.Generic;
    using System.Linq;
    using ExitGames.Client.Photon;
    using MAGES.Interaction.Interactables;
    using MAGES.Networking;
    using MAGES.SceneGraph;
    using MAGES.Utilities;
    using Photon.Pun;
    using Photon.Realtime;
    using UnityEngine;

    /// <summary>
    /// The network integration module of photon for unity.
    /// </summary>
    public class PUNIntegration : MonoBehaviourPunCallbacks, IMAGESNetworkIntegration, IOnEventCallback
    {
        private bool isConnectedToServer;
        private List<RoomInfo> allRoomsInfo;
        private MAGESNetworking networking;
        private PunMessageHandler networkMessageHandler;
        private short? returnCode = null;

        /// <summary>
        /// Gets or sets the Pun message handler, responsible for sending MAGES RPC calls over the network.
        /// </summary>
        public PunMessageHandler NetworkMessageHandler
        {
            get => networkMessageHandler;
            set => networkMessageHandler = value;
        }

        /// <summary>
        /// Called when the object is enabled.
        /// </summary>
        public override void OnEnable()
        {
            PhotonNetwork.NetworkingClient.OpResponseReceived += (operationResponse) =>
            {
                returnCode = operationResponse.ReturnCode;
            };
            base.OnEnable();
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.IsConnectedToServer"/>
        /// <remarks>Uses PUN plugin. </remarks>
        public bool IsConnectedToServer()
        {
            return isConnectedToServer;
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.CreateRoom"/>
        /// <remarks>Uses PUN plugin. </remarks>
        public bool CreateRoom(string roomName)
        {
            return PhotonNetwork.CreateRoom(roomName);
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.JoinRoom"/>
        /// <remarks>Uses PUN plugin. </remarks>
        public bool JoinRoom(string roomName)
        {
            return PhotonNetwork.JoinRoom(roomName);
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.ValidateClientNetworkingSetup"/>
        public short? ValidateClientNetworkingSetup()
        {
            return returnCode;
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.GetAvailableRooms"/>
        /// <remarks>Uses PUN plugin. </remarks>
        public List<string> GetAvailableRooms()
        {
            if (!IsConnectedToServer())
            {
                return null;
            }

            return (from room in allRoomsInfo where room != null select room.Name).ToList();
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.GetCurrentConnectedRoom"/>
        /// <remarks>Uses PUN plugin. </remarks>
        public string GetCurrentConnectedRoom()
        {
            if (PhotonNetwork.CurrentRoom != null)
            {
                return PhotonNetwork.CurrentRoom.Name;
            }

            return null;
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.EstablishConnectionToMainServer"/>
        /// <remarks>Uses PUN plugin. </remarks>
        public bool EstablishConnectionToMainServer(string args)
        {
            var result = PhotonNetwork.ConnectUsingSettings();
            return result;
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.EstablishConnectionToMainServer"/>
        /// <remarks>Uses PUN plugin. </remarks>
        public void Disconnect()
        {
            isConnectedToServer = false;
            returnCode = null;
            PhotonNetwork.Disconnect();
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.SpawnObject"/>
        /// <remarks>Uses PUN plugin. </remarks>
        public GameObject SpawnObject(GameObject prefab, bool isUnique = true)
        {
            if (prefab == null)
            {
                Debug.LogError("Network spawn called with null prefab.");
                return null;
            }

            GameObject spawnedObject;
            if (prefab.GetComponent<PhotonView>())
            {
                spawnedObject = PhotonNetwork.Instantiate(
                    networking.PathReferences[prefab],
                    prefab.transform.position,
                    prefab.transform.rotation,
                    0,
                    new object[] { networking.PathReferences[prefab], true, isUnique });
            }
            else
            {
                spawnedObject = Instantiate(prefab);
                var newPhotonView = spawnedObject.AddComponent<PhotonView>();
                PhotonNetwork.AllocateViewID(newPhotonView);
                newPhotonView.OwnershipTransfer = OwnershipOption.Takeover;
                RaiseEventOptions raiseEventOptions = new RaiseEventOptions
                {
                    Receivers = ReceiverGroup.Others,
                    CachingOption = EventCaching.AddToRoomCache,
                };
                PhotonNetwork.RaiseEvent(
                    1,
                    new object[] { networking.PathReferences[prefab], newPhotonView.ViewID },
                    raiseEventOptions,
                    new SendOptions { Reliability = true });
            }

            if (spawnedObject.GetComponent<Rigidbody>())
            {
                spawnedObject.GetOrAddComponent<SyncTransformPhoton>();
            }

            return spawnedObject;
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.DestroyObject"/>
        /// <remarks>Uses PUN plugin. </remarks>
        public bool DestroyObject(GameObject objectToBeDestroyed)
        {
            if (objectToBeDestroyed == null)
            {
                Debug.LogError("The object you are trying to destroy is null.");
                return false;
            }

            if (!networking.IsHost)
            {
                return true;
            }

            var view = objectToBeDestroyed.GetComponent<PhotonView>();
            if (view != null && view.IsMine)
            {
                PhotonNetwork.Destroy(objectToBeDestroyed);
            }
            else if (view != null)
            {
                view.AddCallbackTarget(new DeleteOnOwnershipChange(objectToBeDestroyed));
                view.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                Debug.LogError("The object you are trying to network-destroy is not a network object.");
                return false;
            }

            return true;
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.GetPrefabIDFromNetwork"/>
        /// <remarks>Uses PUN plugin. </remarks>
        public int GetPrefabIDFromNetwork(GameObject prefab, out bool isUnique)
        {
            var networkComponent = prefab.GetComponent<PhotonView>();
            if (networkComponent == null)
            {
                isUnique = true;
                return -1;
            }

            if (networkComponent.InstantiationData == null)
            {
                isUnique = true;
                return -1;
            }

            var path = (string)networkComponent.InstantiationData[0];
            var isNetworkObject = (bool)networkComponent.InstantiationData[1];
            isUnique = (bool)networkComponent.InstantiationData[2];

            if (isNetworkObject)
            {
                return networking.PathToPrefabID[path];
            }

            return -1;
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.LinkNetworkObject"/>
        /// <remarks>Uses PUN plugin. </remarks>
        public GameObject LinkNetworkObject(GameObject remotePrefab, GameObject localPrefab)
        {
            if (remotePrefab.name != localPrefab.name)
            {
                return null;
            }

            var remotePview = remotePrefab.GetComponent<PhotonView>();
            var viewID = remotePview.ViewID;
            var newPhotonView = localPrefab.GetOrAddComponent<PhotonView>();
            PhotonNetwork.LocalCleanPhotonView(remotePview);
            remotePrefab.SetActive(false);
            PhotonNetwork.PrefabPool.Destroy(remotePrefab);
            Destroy(remotePrefab);
            newPhotonView.ViewID = viewID;
            localPrefab.GetOrAddComponent<SyncTransform>().Initialise();

            return localPrefab;
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.RequestAuthority"/>
        /// <remarks>Uses PUN plugin.</remarks>
        public void RequestAuthority(GameObject networkObject)
        {
            var networkComponent = networkObject.GetComponent<PhotonView>();
            if (networkComponent != null)
            {
                networkComponent.TransferOwnership(PhotonNetwork.LocalPlayer.ActorNumber);
            }
            else
            {
                Debug.LogError($"Network Request Ownership called for object {gameObject.name}, which does not have photonView component.");
            }
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.GetNetworkID"/>
        /// <remarks>Uses PUN plugin.</remarks>
        public int GetNetworkID(GameObject networkObject)
        {
            if (networkObject == null)
            {
                return -1;
            }

            var view = networkObject.GetComponent<PhotonView>();
            if (view == null)
            {
                return -1;
            }

            return view.ViewID;
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.RequestChangeState"/>
        /// <remarks>Uses PUN plugin.</remarks>
        public void RequestChangeState(byte changeState, string actionID)
        {
            if (NetworkMessageHandler == null)
            {
                var messageHandlerGo = GameObject.Find("PunMessageHandler(Clone)");
                NetworkMessageHandler = messageHandlerGo.GetComponent<PunMessageHandler>();
                if (!networkMessageHandler)
                {
                    return;
                }
            }

            networkMessageHandler.RequestStateChange(changeState, actionID);
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.HasAuthority"/>
        /// <remarks>Uses PUN plugin.</remarks>
        public bool HasAuthority(GameObject networkObject)
        {
            var newtrowkView = networkObject.GetComponent<PhotonView>();
            if (newtrowkView == null)
            {
                Debug.LogError("Has authority called for object " + gameObject.name + " which is not a network object.");
            }

            return newtrowkView.IsMine;
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.DestroyComponent"/>
        /// <remarks>Uses PUN plugin.</remarks>
        public bool DestroyComponent(GameObject networkObject, string componentType)
        {
            var objectPhotonView = networkObject.GetComponent<PhotonView>();
            if (objectPhotonView)
            {
                networkMessageHandler.DestroyComponent(objectPhotonView, componentType);
                return true;
            }

            Debug.LogError("The component you are trying to destroy is not in a valid network gameobject.");
            return false;
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.GetConnectedUsersToRoom"/>
        /// <remarks>Uses PUN plugin.</remarks>
        public int GetConnectedUsersToRoom(string roomID)
        {
            RoomInfo roomInfo = null;
            foreach (var room in allRoomsInfo)
            {
                if (roomID == room.Name)
                {
                    roomInfo = room;
                }
            }

            if (roomInfo != null)
            {
                return roomInfo.PlayerCount;
            }

            return -1;
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.GetConnectedUsersToCurrentRoom"/>
        public int GetConnectedUsersToCurrentRoom()
        {
            if (PhotonNetwork.CurrentRoom != null)
            {
                return PhotonNetwork.CurrentRoom.PlayerCount;
            }

            return -1;
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.AddSyncTransform"/>
        public IMAGESObjectSynchronization AddSyncTransform(GameObject networkObject)
        {
            return networkObject.GetOrAddComponent<SyncTransformPhoton>();
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.InitSpawnedObjectForNetwork"/>
        public void InitSpawnedObjectForNetwork(GameObject prefab, bool syncTransform)
        {
            var newPhotonView = prefab.AddComponent<PhotonView>();
            PhotonNetwork.AllocateViewID(newPhotonView);
            newPhotonView.OwnershipTransfer = OwnershipOption.Takeover;
            RaiseEventOptions raiseEventOptions = new RaiseEventOptions
            {
                Receivers = ReceiverGroup.Others,
                CachingOption = EventCaching.AddToRoomCache,
            };
            PhotonNetwork.RaiseEvent(
                1,
                new object[] { networking.PathReferences[prefab], newPhotonView.ViewID },
                raiseEventOptions,
                new SendOptions { Reliability = true });

            if (syncTransform)
            {
                prefab.GetOrAddComponent<SyncTransformPhoton>();
            }
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.AddQuestionSyncScript"/>
        public void AddQuestionSyncScript(GameObject questionPrefab)
        {
            if (questionPrefab)
            {
                questionPrefab.AddComponent<SyncQuestionState>();
            }
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.OnStartup"/>
        /// <remarks>Uses PUN plugin.</remarks>
        public void OnStartup()
        {
            Hub.Instance.Get<NetworkingModule>().NetworkIdType = typeof(PhotonView);
            networking = Hub.Instance.Get<MAGESNetworking>();

            PhotonNetwork.SendRate = 30;
            PhotonNetwork.SerializationRate = 30;
        }

        /// <inheritdoc cref="IMAGESNetworkIntegration.GetPing"/>
        /// <remarks>Uses PUN plugin.</remarks>
        public int GetPing()
        {
            return PhotonNetwork.GetPing();
        }

        /// <summary>
        /// Event called every time the list of available rooms is updated.
        /// </summary>
        /// <param name="roomList">The list of rooms.</param>
        public override void OnRoomListUpdate(List<RoomInfo> roomList)
        {
            base.OnRoomListUpdate(roomList);
            allRoomsInfo = roomList;
        }

        /// <summary>
        /// Event called when connection to server is established.
        /// </summary>
        public override void OnConnectedToMaster()
        {
            base.OnConnectedToMaster();

            var photonPool = ((DefaultPool)PhotonNetwork.PrefabPool).ResourceCache;
            foreach (var gameObjectPath in GameObjectPathReferences.Instance.GetGameObjectsPaths())
            {
                if (photonPool.ContainsKey(gameObjectPath.Value))
                {
                    continue;
                }

                photonPool.Add(gameObjectPath.Value, gameObjectPath.Key);
            }

            PhotonNetwork.JoinLobby();
        }

        /// <summary>
        /// Event called when the user joins a lobby.
        /// </summary>
        public override void OnJoinedLobby()
        {
            base.OnJoinedLobby();
            isConnectedToServer = true;
        }

        /// <summary>
        /// Event called when a user creates a room.
        /// </summary>
        public override void OnCreatedRoom()
        {
            base.OnCreatedRoom();
            gameObject.AddComponent<SyncState>();

            GameObject chatObject = GameObjectSpawner.Instance.SpawnObject(networking.MessageHandlerPrefab, isUnique: false);
            networkMessageHandler = chatObject.GetComponent<PunMessageHandler>();
        }

        /// <summary>
        /// Event called when a user joins the current room.
        /// Also, called when a room is created.
        /// </summary>
        public override void OnJoinedRoom()
        {
            base.OnJoinedRoom();
            networking.OnConnected(string.Empty);
        }

        /// <inheritdoc/>
        public override void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            var sceneGraphModule = Hub.Instance.Get<MAGESSceneGraph>();
            foreach (var stateChange in propertiesThatChanged)
            {
                var action = sceneGraphModule.Runner.RuntimeGraph.Find(stateChange.Key.ToString());
                if (action.State == (ActionState)stateChange.Value)
                {
                    return;
                }

                switch ((int)stateChange.Value)
                {
                    case 2:
                    {
                        sceneGraphModule.Runner.PerformAction(action);
                        break;
                    }

                    case 1:
                    {
                        sceneGraphModule.Runner.InitializeAction(action);
                        break;
                    }

                    default:
                    {
                        sceneGraphModule.Runner.UndoAction(action);
                        break;
                    }
                }
            }
        }

        /// <inheritdoc cref="IOnEventCallback.OnEvent"/>
        public void OnEvent(EventData photonEvent)
        {
            if (photonEvent.Code == 1)
            {
                /*
                    1 is custom pun code for our own network instantiation.
                    Client receives this event when the server network spawns using the MAGES spawn function.
                    This is used when an object is spawned and it is not configured.
                */
                object[] data = (object[])photonEvent.CustomData;
                var localPrefabID = networking.PathToPrefabID[(string)data[0]];
                GameObjectSpawner.Instance.TryGetInstantiatedGameObject(localPrefabID, out GameObject spawnedObject);
                if (spawnedObject == null)
                {
                    spawnedObject = GameObjectSpawner.Instance.SpawnObject(networking.PathToGameObject[(string)data[0]]);
                }

                var spawnedPhotonView = spawnedObject.AddComponent<PhotonView>();
                spawnedPhotonView.ViewID = (int)data[1];
                spawnedPhotonView.OwnershipTransfer = OwnershipOption.Takeover;

                if (spawnedObject.GetComponent<Rigidbody>() && spawnedObject.GetComponent<Grabbable>())
                {
                    var syncTransform = spawnedObject.GetOrAddComponent<SyncTransformPhoton>();
                    syncTransform.Initialise();
                }
            }
            else if (photonEvent.Code == 202)
            {
                /*
                    202 is pun code for network instantiation.
                    Client receives this event when the server network spawns an object.
                    This is used when an object is spawned and it is already configured.
                */
                object keyByteFive = (byte)5;
                object keyByteSeven = (byte)7;
                Hashtable networkInstantiationData = (Hashtable)photonEvent.CustomData;
                var data = (object[])networkInstantiationData[keyByteFive];
                var viewID = (int)networkInstantiationData[keyByteSeven];

                // This is not an object that other users should not be linking for such as the user's other hands.
                if (!(bool)data[2])
                {
                    return;
                }

                var viewFromPhoton = PhotonNetwork.GetPhotonView(viewID);
                if (viewFromPhoton != null)
                {
                    PhotonNetwork.LocalCleanPhotonView(viewFromPhoton);
                    Destroy(viewFromPhoton.gameObject);
                }

                var localPrefabID = networking.PathToPrefabID[(string)data[0]];
                var spawnedObject = GameObjectSpawner.Instance.GetDummyGameObject(localPrefabID);
                if (spawnedObject != null)
                {
                    var spawnedPhotonView = spawnedObject.GetOrAddComponent<PhotonView>();
                    spawnedPhotonView.ViewID = viewID;
                    spawnedPhotonView.OwnershipTransfer = OwnershipOption.Takeover;
                }
            }
        }

        private class DeleteOnOwnershipChange : IOnPhotonViewControllerChange
        {
            private readonly GameObject networkObject;

            public DeleteOnOwnershipChange(GameObject gameObject)
            {
                networkObject = gameObject;
            }

            public void OnControllerChange(Player newOwner, Player previousOwner)
            {
                if (networkObject == null)
                {
                    Debug.LogError("GameObject is null. Aborting Ownership transfer for deletion.");
                    return;
                }

                PhotonNetwork.Destroy(networkObject);

                networkObject.GetComponent<PhotonView>().RemoveCallbackTarget(this);
            }
        }

        public void AddQuestionSyncScript()
        {
            
        }
    }
}
#endif