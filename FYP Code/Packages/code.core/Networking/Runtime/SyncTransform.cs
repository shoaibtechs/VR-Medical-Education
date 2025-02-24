namespace MAGES.Networking
{
    using System;
    using System.Collections.Generic;
    using MAGES.RigidBodyAnimation;
    using MAGES.Utilities;
    using UnityEngine;

    /// <summary>
    /// Synchronization of transform over network.
    /// </summary>
    [RequireComponent(typeof(MAGESObject))]
    public class SyncTransform : MonoBehaviour, IMAGESObjectSynchronization
    {
        [SerializeField]
        private float movementThreshold = 0.005f;

        [SerializeField]
        private float rotationThreshold = 1.0f;

        [SerializeField]
        private MAGESSyncTransform syncTransformMode = MAGESSyncTransform.All;

        [SerializeField]
        private MAGESSyncChildren syncObjectsMode = MAGESSyncChildren.All;

        private List<IRigidBodyAnimation> interpolators;

        private float syncRate = 0.001f;

        /// <summary>
        /// Enum that defines type of received changed from other users.
        /// </summary>
        [Flags]
        protected enum TransformFlags : byte
        {
            /// <summary>
            /// Neither rotation, neither position.
            /// </summary>
            None = 1,

            /// <summary>
            /// Rotation change.
            /// </summary>
            Rotation = 2,

            /// <summary>
            /// Position change.
            /// </summary>
            Position = 4,
        }

        /// <inheritdoc cref="IMAGESObjectSynchronization.RotationThreshold"/>
        public float RotationThreshold
        {
            get => rotationThreshold;

            set => rotationThreshold = value;
        }

        /// <inheritdoc cref="IMAGESObjectSynchronization.MovementThreshold"/>
        public float MovementThreshold
        {
            get => movementThreshold;

            set => movementThreshold = value;
        }

        /// <inheritdoc cref="IMAGESObjectSynchronization.SyncTransformMode"/>
        public MAGESSyncTransform SyncTransformMode
        {
            get => syncTransformMode;

            set => syncTransformMode = value;
        }

        /// <inheritdoc cref="IMAGESObjectSynchronization.SyncChildrenMode"/>
        public MAGESSyncChildren SyncChildrenMode
        {
            get => syncObjectsMode;

            set => syncObjectsMode = value;
        }

        /// <summary>
        /// Gets or Sets Rate of synchronization.
        /// </summary>
        protected float SyncRate { get => syncRate; set => syncRate = value; }

        /// <summary>
        /// Gets list of transforms that are synchronized.
        /// </summary>
        protected List<Transform> Transforms { get; private set; }

        /// <summary>
        /// Gets vector of last positions that were synchronized.
        /// </summary>
        protected Vector3[] LastPos { get; private set; }

        /// <summary>
        /// Gets vector of last rotations that were synchronized.
        /// </summary>
        protected Quaternion[] LastRot { get; private set; }

        /// <summary>
        /// Initialises the component.
        /// </summary>
        public virtual void Initialise()
        {
            LastPos = new Vector3[Transforms.Count];
            LastRot = new Quaternion[Transforms.Count];
            interpolators = new List<IRigidBodyAnimation>();
            for (int i = 0; i < Transforms.Count; ++i)
            {
                LastPos[i] = Transforms[i].localPosition;
                LastRot[i] = Transforms[i].localRotation;

                if (i == 0)
                {
                    interpolators.Add(new RigidbodyMoveAndRotateDualQuat(
                        Vector3.zero,
                        Quaternion.identity,
                        Transforms[i].gameObject,
                        SyncRate));
                }
                else
                {
                    interpolators.Add(new RigidbodyMoveAndRotateDualQuatLocal(
                        Vector3.zero,
                        Quaternion.identity,
                        Transforms[i].gameObject,
                        SyncRate));
                }

                RigidbodyAnimationController.AddRigidbodyAnimation(interpolators[i]);
            }
        }

        /// <summary>
        /// Add a transform that will be synchronized.
        /// </summary>
        /// <param name="t">The new transform that will be added.</param>
        public void AddSynchronizedTransform(Transform t)
        {
            Transforms ??= new List<Transform>();
            Transforms.Add(t);
        }

        /// <summary>
        /// Clears all transforms that are synchronized from this object.
        /// </summary>
        /// <remarks>This function needs to be called in all connected users, it is not synchronized.</remarks>
        public void ClearSynchronizedTransforms()
        {
            Transforms.Clear();
        }

        /// <summary>
        /// Updates all interpolators to apply new transforms.
        /// </summary>
        /// <param name="currentID">The id of the interpolator that will be restarted.</param>
        protected void RestartInterpolator(int currentID)
        {
            interpolators[currentID].Restart(LastPos[currentID], LastRot[currentID]);
        }

        /// <inheritdoc cref="MonoBehaviour.Awake"/>
        protected virtual void Awake()
        {
            try
            {
                Transforms = new List<Transform> { transform };
            }
            catch (System.Exception e)
            {
                Debug.LogError(e.StackTrace);
            }
        }
    }
}