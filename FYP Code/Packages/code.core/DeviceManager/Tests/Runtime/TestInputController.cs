namespace MAGES.Tests
{
    using MAGES;
    using MAGES.DeviceManager;
    using MAGES.SceneGraph;
    using NUnit.Framework;
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// Test initialization of input controller and keyboard input.
    /// </summary>
    public class TestInputController : InputTestFixture
    {
        private GameObject go;

        /// <summary>
        /// Testing the initialization of Device Manager and input controller.
        /// </summary>
        [Test]
        public void Initialization()
        {
            var deviceManagerModule = SetupHub();
            Assert.IsNotNull(deviceManagerModule);
            Assert.IsNotNull(deviceManagerModule.ControllerActions);
            GameObject.Destroy(GameObject.Find("go"));
        }

        /// <summary>
        /// Testing the keyboard inputs.
        /// </summary>
        [Test]
        public void Keyboard()
        {
            bool x_pressed = false, z_pressed = false;
            var deviceManagerModule = SetupHub();
            var keyboard = InputSystem.AddDevice<Keyboard>();
            deviceManagerModule.ControllerActions.AddActionOnPerform(() => x_pressed = true);
            deviceManagerModule.ControllerActions.AddActionOnUndo(() => z_pressed = true);

            Press(keyboard.xKey);
            Assert.IsTrue(x_pressed);
            Press(keyboard.zKey);
            Assert.IsTrue(z_pressed);
        }

        private Bundle BuildStubBundle()
        {
            DataContainerModule dataContainerModule = ScriptableObject.CreateInstance<StubDataContainer>();
            InteractionSystemModule interactionSystemModule = ScriptableObject.CreateInstance<StubInteractionSystem>();
            NetworkingModule networkingModule = ScriptableObject.CreateInstance<StubNetworking>();
            AnalyticsModule analyticsModule = ScriptableObject.CreateInstance<StubAnalytics>();
            MAGESDeviceManager deviceManagerModule = ScriptableObject.CreateInstance<MAGESDeviceManager>();
            SceneGraphModule sceneGraphModule = ScriptableObject.CreateInstance<StubSceneGraph>();

            Bundle bundle = ScriptableObject.CreateInstance<Bundle>();
            bundle.DataContainer = dataContainerModule;
            bundle.InteractionSystem = interactionSystemModule;
            bundle.Networking = networkingModule;
            bundle.Analytics = analyticsModule;
            bundle.DeviceManager = deviceManagerModule;
            bundle.SceneGraph = sceneGraphModule;
            deviceManagerModule.ControllerActions = Resources.Load<InputController>("MAGESInputController");
            return bundle;
        }

        private MAGESDeviceManager SetupHub()
        {
            go = new GameObject("Test_Hub_GO");

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            hub.StartSystems();

            return Hub.Instance.Get<DeviceManagerModule>() as MAGESDeviceManager;
        }
    }
}