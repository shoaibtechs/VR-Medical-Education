#if PHOTON_UNITY_NETWORKING
namespace MAGES.Networking
{
    using System.Collections.Generic;
    using MAGES.Utilities;
    using Photon.Pun;
    using UnityEngine;

    /// <summary>
    /// Synchronization of transform over network.
    /// </summary>
    /// <remarks>Uses PUN plugin.</remarks>
    [RequireComponent(typeof(MAGESObject))]
    public class SyncTransformPhoton : SyncTransform, IPunObservable
    {
        /// <summary>
        /// Gets reference to photon view.
        /// </summary>
        protected PhotonView PhotonViewRef { get; private set; }

        /// <inheritdoc cref="SyncTransform.Initialise"/>
        public override void Initialise()
        {
            base.Initialise();
            PhotonViewRef = PhotonView.Get(this);
            if (PhotonViewRef == null)
            {
                return;
            }

            PhotonViewRef.ObservedComponents ??= new List<Component>();

            var hasSyncTransform = false;
            foreach (var observedComponent in PhotonViewRef.ObservedComponents)
            {
                if (observedComponent.GetType().IsSubclassOf(typeof(SyncTransformPhoton)) || observedComponent is SyncTransformPhoton)
                {
                    hasSyncTransform = true;
                }
            }

            if (!hasSyncTransform)
            {
                PhotonViewRef.ObservedComponents.Add(this);
            }
        }

        /// <inheritdoc cref="IPunObservable.OnPhotonSerializeView"/>
        public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
        {
            if (stream.IsWriting)
            {
                SendTransforms(stream);
            }
            else
            {
               ApplyTransforms(stream);
            }
        }

        /// <inheritdoc/>
        protected override void Awake()
        {
            SyncRate = 1.0f / PhotonNetwork.SendRate;
            base.Awake();
            Initialise();
        }

        private void SendTransforms(PhotonStream stream)
        {
            var sendData = PrepareSendData(Transforms.Count);
            stream.SendNext(sendData.Count);
            foreach (var chunk in sendData)
            {
                if (chunk.Flags != TransformFlags.None)
                {
                    stream.SendNext((byte)chunk.Flags);
                    stream.SendNext(chunk.ID);

                    if ((chunk.Flags & TransformFlags.Position) != 0)
                    {
                        stream.SendNext(chunk.Pos);
                    }

                    if ((chunk.Flags & TransformFlags.Rotation) != 0)
                    {
                        stream.SendNext(chunk.Rot);
                    }
                }
            }
        }

        private void ApplyTransforms(PhotonStream stream)
        {
            int transformsCount = (int)stream.ReceiveNext();
            for (int i = 0; i < transformsCount; i++)
            {
                TransformFlags transformFlags = (TransformFlags)stream.ReceiveNext();
                int currentID = (int)stream.ReceiveNext();
                if (transformFlags != TransformFlags.None)
                {
                    if ((transformFlags & TransformFlags.Position) != 0)
                    {
                        LastPos[currentID] = (Vector3)stream.ReceiveNext();
                    }

                    if ((transformFlags & TransformFlags.Rotation) != 0)
                    {
                        LastRot[currentID] = (Quaternion)stream.ReceiveNext();
                    }

                    RestartInterpolator(currentID);
                }
            }
        }

        private List<TransformSendData> PrepareSendData(int transformSyncCount)
        {
            List<TransformSendData> sendData = new List<TransformSendData>();

            for (int i = 0; i < transformSyncCount; ++i)
            {
                TransformFlags transfromFlags = TransformFlags.None;
                var position = GetCurrentPosition(i);
                if (Vector3.Distance(position, LastPos[i]) > MovementThreshold)
                {
                    transfromFlags = transfromFlags | TransformFlags.Position;
                }

                var rotation = GetCurrentRotation(i);
                if (Quaternion.Angle(rotation, LastRot[i]) > RotationThreshold)
                {
                    transfromFlags = transfromFlags | TransformFlags.Rotation;
                }

                if (transfromFlags != TransformFlags.None)
                {
                    if ((transfromFlags & TransformFlags.Position) != 0)
                    {
                        LastPos[i] = position;
                    }

                    if ((transfromFlags & TransformFlags.Rotation) != 0)
                    {
                        LastRot[i] = rotation;
                    }

                    sendData.Add(new TransformSendData()
                        { Flags = transfromFlags, ID = i, Pos = LastPos[i], Rot = LastRot[i] });
                }
            }

            return sendData;
        }

        private Vector3 GetCurrentPosition(int i)
        {
            return i == 0 ? Transforms[i].position : Transforms[i].localPosition;
        }

        private Quaternion GetCurrentRotation(int i)
        {
            return i == 0 ? Transforms[i].rotation : Transforms[i].localRotation;
        }

        private struct TransformSendData
        {
            /// <summary>
            /// Changes in the current transform serialization.
            /// </summary>
            public TransformFlags Flags;

            /// <summary>
            /// The ID of the transform that these data refer to.
            /// </summary>
            public int ID;

            /// <summary>
            /// The position data that need to be synchronized.
            /// </summary>
            public Vector3 Pos;

            /// <summary>
            /// The rotation data that need to be synchronized.
            /// </summary>
            public Quaternion Rot;
        }
    }
}
#endif