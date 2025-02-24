namespace MAGES.Tests
{
    using MAGES;
    using NUnit.Framework;
    using UnityEngine;

    /// <summary>
    /// Tests various aspects of the Hub class, at runtime.
    /// </summary>
    public class TestHub
    {
        /// <summary>
        /// Tests that the hub module's Startup() is called correctly.
        /// </summary>
        [Test]
        public void TestHubStartupIsCalled()
        {
            GameObject go = new GameObject("__test_go__");

            go.SetActive(false);

            Bundle bundle = Hub.CreateStubBundle();

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = bundle;
            hub.BaseBundle.ExtraModules = new HubModule[] { ScriptableObject.CreateInstance<CustomHubModule>() };

            go.SetActive(true);

            hub.StartSystems();

            Assert.IsTrue(hub.Get<CustomHubModule>().IsInitialized);

            GameObject.Destroy(go);
        }

        /// <summary>
        /// Tests that a bundle's extra modules can be retrieved properly.
        /// </summary>
        [Test]
        public void TestHubExtraModules()
        {
            GameObject go = new GameObject("__test_go__");

            go.SetActive(false);

            Bundle bundle = Hub.CreateStubBundle();

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = bundle;
            hub.BaseBundle.ExtraModules = new HubModule[] { ScriptableObject.CreateInstance<CustomHubModule>() };

            go.SetActive(true);

            hub.StartSystems();

            Assert.IsNotNull(hub.Get<DataContainerModule>());
            Assert.IsNotNull(hub.Get<InteractionSystemModule>());
            Assert.IsNotNull(hub.Get<NetworkingModule>());
            Assert.IsNotNull(hub.Get<AnalyticsModule>());
            Assert.IsNotNull(hub.Get<DeviceManagerModule>());
            Assert.IsNotNull(hub.Get<SceneGraphModule>());
            Assert.IsNotNull(hub.Get<CustomHubModule>());

            GameObject.Destroy(go);
        }

        /// <summary>
        /// Tests that all the core modules can be retrieved properly.
        /// </summary>
        [Test]
        public void TestHubCoreModules()
        {
            GameObject go = new GameObject("__test_go__");

            go.SetActive(false);

            Bundle bundle = Hub.CreateStubBundle();

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = bundle;

            go.SetActive(true);

            hub.StartSystems();

            Assert.IsNotNull(hub.Get<DataContainerModule>());
            Assert.IsNotNull(hub.Get<InteractionSystemModule>());
            Assert.IsNotNull(hub.Get<NetworkingModule>());
            Assert.IsNotNull(hub.Get<AnalyticsModule>());
            Assert.IsNotNull(hub.Get<DeviceManagerModule>());
            Assert.IsNotNull(hub.Get<SceneGraphModule>());

            GameObject.Destroy(go);
        }

        private class CustomHubModule : HubModule
        {
            private bool isInitialized;

            /// <summary>
            /// Gets a value indicating whether this module is initialized.
            /// </summary>
            public bool IsInitialized { get => isInitialized; }

            public override void Shutdown()
            {
            }

            public override void Startup()
            {
                isInitialized = true;
            }
        }
    }
}