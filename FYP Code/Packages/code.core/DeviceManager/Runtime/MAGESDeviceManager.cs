namespace MAGES.DeviceManager
{
    using System;
    using System.Linq;
    using MAGES.DeviceManager;
    using MAGES.Utilities;
    using UnityEngine;
    using UnityEngine.EventSystems;
#if XR_MANAGEMENT
    using System.Collections;
    using UnityEditor.XR.LegacyInputHelpers;
    using UnityEngine.XR.Management;
#endif

    /// <summary>
    /// MAGES implementation for Device Management.
    /// </summary>
    [CreateAssetMenu(fileName = "MAGESDeviceManager", menuName = "MAGES/DeviceManager/MAGESDeviceManager")]
    public class MAGESDeviceManager : DeviceManagerModule
    {
        /// <summary>
        /// Reference to the MAGES InputController.
        /// </summary>
        [SerializeField]
        private InputController inputActions;

        /// <summary>
        /// Gets or sets the MAGES InputController.
        /// </summary>
        public InputController ControllerActions
        {
            get => inputActions;
            set => inputActions = value;
        }

        /// <summary>
        /// Initializes the DeviceManager and InputController of MAGES.
        /// </summary>
        public override void Startup()
        {
            InteractionActions = inputActions;
            LocomotionActions = inputActions;
            GenericActions = inputActions;
            inputActions.Startup();

            SetupInputDevices(CurrentMode);

            SetupXR();
        }

        /// <summary>
        /// Shutdowns the DeviceManager and InputController of MAGES.
        /// </summary>
        public override void Shutdown()
        {
            InteractionActions = null;
            LocomotionActions = null;
        }

        private void SetupInputDevices(CameraMode mode)
        {
            switch (mode)
            {
                case CameraMode.UniversalXR:
                    UnityEngine.Object controllerXR = Resources.Load("XRRig", typeof(GameObject));
                    Camera mainCamera = Camera.main;
                    Camera[] cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);

                    if (mainCamera)
                    {
                        SetupCamera(mainCamera.gameObject);
                        SetupControllers();
                    }
                    else if (cameras != null && cameras.Any())
                    {
                        SetupCamera(cameras[0].gameObject);
                        SetupControllers();
                    }
                    else
                    {
#if XR_MANAGEMENT
                        if (controllerXR != null)
                        {
                            (CameraRig = Instantiate(controllerXR) as GameObject) !.name = "XRRig";

                            // CameraRig.SetActive(false); // Set inactive to prevent Awake() from being called and throwing warnings.
                            CameraRef = CameraRig.GetComponentInChildren<Camera>(true);
                            if (CameraRef == null)
                            {
                                CameraRef = new GameObject("Camera").AddComponent<Camera>();
                                CameraRef.transform.SetParent(CameraRig.transform);
                            }

                            var cameraPoseDriver = CameraRef.gameObject.GetOrAddComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
                            cameraPoseDriver.SetPoseSource(
                                UnityEngine.SpatialTracking.TrackedPoseDriver.DeviceType.GenericXRDevice,
                                UnityEngine.SpatialTracking.TrackedPoseDriver.TrackedPose.Center);
                            cameraPoseDriver.trackingType = UnityEngine.SpatialTracking.TrackedPoseDriver.TrackingType.RotationAndPosition;
                            cameraPoseDriver.updateType = UnityEngine.SpatialTracking.TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;

                            CameraGameObject = CameraRef.gameObject;

                            CameraOffset offset = CameraRig.gameObject.GetOrAddComponent<CameraOffset>();

                            GameObject offsetGO = CameraRef.transform.parent?.gameObject;
                            if (offsetGO == null || offsetGO.transform == CameraRig.transform)
                            {
                                offsetGO = new GameObject("Camera Offset");
                                offsetGO.transform.SetParent(CameraRig.transform);
                                offsetGO.transform.localPosition = Vector3.zero;
                                offsetGO.transform.localRotation = Quaternion.identity;
                            }

                            offset.cameraFloorOffsetObject = offsetGO;

                            // CameraRig.SetActive(true);
                            LeftController = CameraRig.transform.Find("LController")?.gameObject;
                            if (LeftController != null)
                            {
                                LeftController.GetOrAddComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>()
                                .SetPoseSource(
                                    UnityEngine.SpatialTracking.TrackedPoseDriver.DeviceType.GenericXRController,
                                    UnityEngine.SpatialTracking.TrackedPoseDriver.TrackedPose.LeftPose);
                            }

                            RightController = CameraRig.transform.Find("RController")?.gameObject;
                            if (RightController != null)
                            {
                                RightController.GetOrAddComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>()
                            .SetPoseSource(
                            UnityEngine.SpatialTracking.TrackedPoseDriver.DeviceType.GenericXRController,
                            UnityEngine.SpatialTracking.TrackedPoseDriver.TrackedPose.RightPose);
                            }
                        }
                        else
                        {
                            Debug.LogError("The camera prefab for Universal XR Controller was not found.");
                        }
#endif
                    }

                    break;

                case DeviceManagerModule.CameraMode.XREmulator:
                    break;

                case CameraMode.Mobile3D:
                    UnityEngine.Object controllerMobile = Resources.Load("MobileRig", typeof(GameObject));
                    mainCamera = Camera.main;
                    cameras = FindObjectsByType<Camera>(FindObjectsSortMode.None);

                    if (mainCamera)
                    {
                        SetupCamera(mainCamera.gameObject);

                        SetupControllers();
                    }

                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
            }
        }

        private void SetupCamera(GameObject cameraGO)
        {
            switch (CurrentMode)
            {
                case CameraMode.UniversalXR:
#if XR_MANAGEMENT
                    CameraGameObject = cameraGO;
                    var cameraPoseDriver = cameraGO.GetOrAddComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>();
                    cameraPoseDriver.SetPoseSource(
                        UnityEngine.SpatialTracking.TrackedPoseDriver.DeviceType.GenericXRDevice,
                        UnityEngine.SpatialTracking.TrackedPoseDriver.TrackedPose.Center);
                    cameraPoseDriver.trackingType = UnityEngine.SpatialTracking.TrackedPoseDriver.TrackingType.RotationAndPosition;
                    cameraPoseDriver.updateType = UnityEngine.SpatialTracking.TrackedPoseDriver.UpdateType.UpdateAndBeforeRender;
                    CameraRig = CameraGameObject.transform.root.gameObject;
                    CameraRef = CameraGameObject.GetComponent<Camera>();
                    CameraRef.gameObject.GetOrAddComponent<MAGESAudioController>();
                    if (CameraRig == CameraGameObject)
                    {
                        GameObject rig = new GameObject("XRRig");
                        rig.transform.position = CameraGameObject.transform.position;

                        UnityEditor.XR.LegacyInputHelpers.CameraOffset offset = rig.AddComponent<UnityEditor.XR.LegacyInputHelpers.CameraOffset>();

                        GameObject cameraOffset = new GameObject("Camera Offset");
                        cameraOffset.transform.SetParent(rig.transform);
                        cameraOffset.transform.localPosition = Vector3.zero;
                        cameraOffset.transform.localRotation = Quaternion.identity;

                        offset.cameraFloorOffsetObject = cameraOffset;

                        CameraGameObject.transform.SetParent(cameraOffset.transform);
                        CameraGameObject.transform.localPosition = Vector3.zero;
                        CameraGameObject.transform.localRotation = Quaternion.identity;

                        CameraRig = rig;
                    }

#endif
                    break;

                case CameraMode.XREmulator:
                    break;

                case CameraMode.Mobile3D:
                    CameraGameObject = cameraGO;
                    CameraRig = CameraGameObject.transform.root.gameObject;
                    CameraRef = CameraGameObject.GetComponent<Camera>();
                    CameraRef.gameObject.GetOrAddComponent<MAGESAudioController>();
                    if (CameraRig == CameraGameObject)
                    {
                        GameObject rig = new GameObject("MobileRig");

                        CameraGameObject.transform.SetParent(rig.transform);

                        Transform cameraTRS = CameraGameObject.transform;
                        rig.transform.position = cameraTRS.position + new Vector3(0, CameraHeight, 0);

                        cameraTRS.localPosition = Vector3.zero;

                        CameraRig = rig;
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(CurrentMode), CurrentMode, null);
            }
        }

        private void SetupControllers()
        {
            switch (CurrentMode)
            {
                case CameraMode.UniversalXR:
#if XR_MANAGEMENT
                    var leftControllerDriver = CameraRig.transform.Find("LController");

                    if (!leftControllerDriver)
                    {
                        var newLController = new GameObject("LController");
                        newLController.transform.SetParent(CameraRig.transform);
                        LeftController = newLController;
                    }
                    else
                    {
                        LeftController = leftControllerDriver.gameObject;
                    }

                    LeftController.AddComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>()
                        .SetPoseSource(
                            UnityEngine.SpatialTracking.TrackedPoseDriver.DeviceType.GenericXRController,
                            UnityEngine.SpatialTracking.TrackedPoseDriver.TrackedPose.LeftPose);

                    var rightControllerDriver = CameraRig.transform.Find("RController");

                    if (!rightControllerDriver)
                    {
                        var newRController = new GameObject("RController");
                        newRController.transform.SetParent(CameraRig.transform);
                        RightController = newRController;
                    }
                    else
                    {
                        RightController = rightControllerDriver.gameObject;
                    }

                    RightController.AddComponent<UnityEngine.SpatialTracking.TrackedPoseDriver>()
                        .SetPoseSource(
                            UnityEngine.SpatialTracking.TrackedPoseDriver.DeviceType.GenericXRController,
                            UnityEngine.SpatialTracking.TrackedPoseDriver.TrackedPose.RightPose);

#endif
                    break;

                case CameraMode.XREmulator:
                    break;

                case CameraMode.Mobile3D:
                    GameObject existingEventSystem = GameObject.FindAnyObjectByType<EventSystem>().gameObject;
                    if (existingEventSystem != null)
                    {
                        DestroyImmediate(existingEventSystem);
                    }

                    UnityEngine.Object navigationMobile = Resources.Load("MobileNavigation", typeof(GameObject));
                    navigationMobile = Instantiate(navigationMobile) as GameObject;
                    navigationMobile.name = "MobileNavigation";

                    ((GameObject)navigationMobile).transform.SetParent(CameraRig.transform);

                    var prev = FindFirstObjectByType<EventSystem>();
                    if (prev != null)
                    {
                        prev.name = "EventSystemMobile";
                        var prevInput = prev.GetComponent<BaseInputModule>();
                        if (prevInput != null)
                        {
                            Destroy(prevInput);
                        }

                        prev.gameObject.AddComponent<StandaloneInputModule>();
                    }
                    else
                    {
                        new GameObject("EventSystemMobile", typeof(EventSystem), typeof(StandaloneInputModule));
                    }

                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(CurrentMode), CurrentMode, null);
            }
        }

        private void SetupXR()
        {
#if XR_MANAGEMENT
            switch (CurrentMode)
            {
                case CameraMode.UniversalXR:
                    Hub.Instance.StartCoroutine(StartXRCoroutine());
                    break;
                default:
                    Hub.Instance.StartCoroutine(StopXRCoroutine());
                    break;
            }
#endif
        }

#if XR_MANAGEMENT
        private UnityEngine.SpatialTracking.TrackedPoseDriver FindLeftController(GameObject cameraRoot)
        {
            var poseDrivers = cameraRoot.GetComponentsInChildren<UnityEngine.SpatialTracking.TrackedPoseDriver>();
            return poseDrivers.FirstOrDefault(driver => driver.deviceType == UnityEngine.SpatialTracking.TrackedPoseDriver.DeviceType.GenericXRController && driver.poseSource == UnityEngine.SpatialTracking.TrackedPoseDriver.TrackedPose.LeftPose);
        }

        private UnityEngine.SpatialTracking.TrackedPoseDriver FindRightController(GameObject cameraRoot)
        {
            var poseDrivers = cameraRoot.GetComponentsInChildren<UnityEngine.SpatialTracking.TrackedPoseDriver>();
            return poseDrivers.FirstOrDefault(driver => driver.deviceType == UnityEngine.SpatialTracking.TrackedPoseDriver.DeviceType.GenericXRController && driver.poseSource == UnityEngine.SpatialTracking.TrackedPoseDriver.TrackedPose.RightPose);
        }

        private IEnumerator StartXRCoroutine()
        {
            XRLoader m_SelectedXRLoader;
            if (XRGeneralSettings.Instance?.Manager?.activeLoader != null)
            {
                yield break;
            }
            else
            {
                if (XRGeneralSettings.Instance.Manager.activeLoaders.Count == 0)
                {
                    Debug.LogError("No XR Loader is active. Please make sure that you have one or more XR Loaders active in the XR Plugin Management section of the Project Settings.");
                    yield break;
                }

                m_SelectedXRLoader = XRGeneralSettings.Instance.Manager.activeLoaders[0];
            }

            var initSuccess = m_SelectedXRLoader.Initialize();
            if (initSuccess)
            {
                yield return null;
                var startSuccess = m_SelectedXRLoader.Start();
                if (!startSuccess)
                {
                    yield return null;
                    m_SelectedXRLoader.Deinitialize();
                }
            }
        }

        private IEnumerator StopXRCoroutine()
        {
            yield return null;

            var activeLoader = XRGeneralSettings.Instance?.Manager?.activeLoader;
            if (activeLoader == null)
            {
                yield break;
            }

            activeLoader.Stop();
            activeLoader.Deinitialize();

            yield break;
        }
#endif
    }
}