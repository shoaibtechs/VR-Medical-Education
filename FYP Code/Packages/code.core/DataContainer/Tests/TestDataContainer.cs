namespace MAGES.Tests
{
    using System.Collections.Generic;
    using MAGES;
    using MAGES.DataContainer;
    using MAGES.SceneGraph;
    using NUnit.Framework;
    using UnityEngine;

    /// <summary>
    /// Test various aspects of datacontainer at runtime.
    /// </summary>
    public class TestDataContainer
    {
        /// <summary>
        /// Test data container instance.
        /// </summary>
        [Test]
        public void TestContainerInitialization()
        {
            GameObject go = new GameObject("__test_go__");

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            hub.StartSystems();

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);
            GameObject.Destroy(go);
        }

        /// <summary>
        /// Testing storing retrieving and deleting data from Count Schema.
        /// </summary>
        [Test]
        public void TestContainerCountSchema()
        {
            GameObject go = new GameObject("__test_go__");

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            hub.StartSystems();

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);
            dataContainer.SetSchema("countSchema.*", typeof(CountSchema));
            dataContainer.StoreData("countSchema.number", 1);
            dataContainer.StoreData("countSchema.number", 1.5);
            dataContainer.StoreData("countSchema.anothernumber", 2);
            dataContainer.StoreData("countSchema.anothernumber", 5.5);
            Assert.AreEqual(7.5, dataContainer.GetData("countSchema.anothernumber"));
            Assert.AreEqual(2.5, dataContainer.GetData("countSchema.number"));
            Dictionary<string, double> dict = new Dictionary<string, double>
        {
            { "countSchema.number", 2.5 },
            { "countSchema.anothernumber", 7.5 },
        };
            Assert.AreEqual(dict, dataContainer.GetSchemaData("countSchema.*"));
            dataContainer.DeleteData("countSchema.number");
            dict = new Dictionary<string, double>
        {
            { "countSchema.anothernumber", 7.5 },
        };
            Assert.Catch(() => dataContainer.GetData("countSchema.number"));
            Assert.AreEqual(dict, dataContainer.GetSchemaData("countSchema.*"));
            dataContainer.DeleteSchema("countSchema.*");
            Assert.Catch(() => dataContainer.GetSchemaData("countSchema.*"));
            GameObject.Destroy(go);
        }

        /// <summary>
        /// Testing storing retrieving and deleting data from Event Schema.
        /// </summary>
        [Test]
        public void TestContainerEventSchema()
        {
            GameObject go = new GameObject("__test_go__");

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            hub.StartSystems();

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);
            dataContainer.SetSchema("eventSchema.*", typeof(EventSchema));
            dataContainer.StoreData("eventSchema.number", 1);
            dataContainer.StoreData("eventSchema.number", 1.5);
            dataContainer.StoreData("eventSchema.anothernumber", 2);
            dataContainer.StoreData("eventSchema.anothernumber", 5.5);
            Assert.AreEqual(5.5, dataContainer.GetData("eventSchema.anothernumber"));
            Assert.AreEqual(1.5, dataContainer.GetData("eventSchema.number"));
            Dictionary<string, double> dict = new Dictionary<string, double>
        {
            { "eventSchema.number", 1.5 },
            { "eventSchema.anothernumber", 5.5 },
        };
            Assert.AreEqual(dict, dataContainer.GetSchemaData("eventSchema.*"));
            dataContainer.DeleteData("eventSchema.number");
            dict = new Dictionary<string, double>
        {
            { "eventSchema.anothernumber", 5.5 },
        };
            Assert.Catch(() => dataContainer.GetData("eventSchema.number"));
            Assert.AreEqual(dict, dataContainer.GetSchemaData("eventSchema.*"));
            dataContainer.DeleteSchema("eventSchema.*");
            Assert.Catch(() => dataContainer.GetSchemaData("eventSchema.*"));
            GameObject.Destroy(go);
        }

        /// <summary>
        /// Testing storing retrieving and deleting data from Event Schema.
        /// </summary>
        [Test]
        public void TestContainerListSchema()
        {
            GameObject go = new GameObject("__test_go__");

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            hub.StartSystems();

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);
            dataContainer.SetSchema("ListSchema.*", typeof(ListSchema));
            dataContainer.StoreData("ListSchema.number", 1);
            dataContainer.StoreData("ListSchema.number", 1.5);
            dataContainer.StoreData("ListSchema.anothernumber", 2);
            dataContainer.StoreData("ListSchema.anothernumber", 5.5);
            Dictionary<string, List<double>> dict = new Dictionary<string, List<double>>
        {
            { "ListSchema.number", new List<double> { 1.0, 1.5 } },
            { "ListSchema.anothernumber", new List<double> { 2.0, 5.5 } },
        };
            Assert.AreEqual(dict, dataContainer.GetSchemaData("ListSchema.*"));
            dataContainer.DeleteData("ListSchema.number");
            dict = new Dictionary<string, List<double>>
        {
            { "ListSchema.anothernumber", new List<double> { 2.0, 5.5 } },
        };
            Assert.Catch(() => dataContainer.GetData("ListSchema.number"));
            Assert.AreEqual(dict, dataContainer.GetSchemaData("ListSchema.*"));
            dataContainer.DeleteSchema("ListSchema.*");
            Assert.Catch(() => dataContainer.GetSchemaData("ListSchema.*"));
            GameObject.Destroy(go);
        }

        /// <summary>
        /// Testing initialization with already existing key pattern.
        /// </summary>
        [Test]
        public void TestSchemaInitializationWithAlreadyExistingKey()
        {
            GameObject go = new GameObject("__test_go__");

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            hub.StartSystems();

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);
            dataContainer.SetSchema("aSchema.*", typeof(CountSchema));
            Assert.Catch(() => dataContainer.SetSchema("aSchema.test*", typeof(ListSchema)));
            GameObject.Destroy(go);
        }

        /// <summary>
        /// Testing the function GetSpecificSchemaData, which returns a dictionary with all the data under a subconvention under a certain schema.
        /// </summary>
        [Test]
        public void TestSchemaGetSpecificData()
        {
            GameObject go = new GameObject("__test_go__");

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            hub.StartSystems();

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);
            dataContainer.SetSchema("aSchema1.*", typeof(CountSchema));
            dataContainer.StoreData("aSchema1.number.1", 1);
            dataContainer.StoreData("aSchema1.number.2", 2);
            List<KeyValuePair<string, double>> list = new List<KeyValuePair<string, double>>
        {
            new KeyValuePair<string, double>("aSchema1.number.2", 2),
            new KeyValuePair<string, double>("aSchema1.number.1", 1),
        };
            Assert.AreEqual(list, dataContainer.GetSpecificSchemaData("aSchema1.number.*"));
            GameObject.Destroy(go);
        }

        /// <summary>
        /// Testing the function DeleteSpecificSchemaData, which deletes all the data under a subconvention under a certain schema.
        /// </summary>
        [Test]
        public void TestSchemaDeleteSpecificData()
        {
            GameObject go = new GameObject("__test_go__");

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            hub.StartSystems();

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);
            dataContainer.SetSchema("aSchema2.*", typeof(CountSchema));
            dataContainer.StoreData("aSchema2.number.1", 1);
            dataContainer.StoreData("aSchema2.number.2", 2);
            dataContainer.StoreData("aSchema2.othernumber.3", 3);
            Dictionary<string, double> dict = new Dictionary<string, double>
        {
            { "aSchema2.othernumber.3", 3 },
        };
            dataContainer.DeleteSpecificSchemaData("aSchema2.number.*");
            Dictionary<string, object> schemaDict = dataContainer.GetSchemaData("aSchema2.*");
            Assert.AreEqual(dict, dataContainer.GetSchemaData("aSchema2.*"));
            Assert.Catch(() => dataContainer.GetSpecificSchemaData("aSchema2.number.*"));
            GameObject.Destroy(go);
        }

        /// <summary>
        /// Testing the function GetSchemaData, which returns a dictionary with all the data under a certain schema when given a keyPattern that doesn't match
        /// a schema.
        /// </summary>
        [Test]
        public void TestGetAllSchemaDataException()
        {
            GameObject go = new GameObject("__test_go__");

            go.SetActive(false);

            Hub hub = go.AddComponent<Hub>();
            hub.AutoStart = false;
            hub.BaseBundle = BuildStubBundle();

            go.SetActive(true);

            hub.StartSystems();

            DataContainerModule dataContainer = Hub.Instance.Get<DataContainerModule>();
            Assert.IsNotNull(dataContainer);
            dataContainer.SetSchema("aSchema3.*", typeof(CountSchema));
            dataContainer.StoreData("aSchema3.number.1", 1);
            dataContainer.StoreData("aSchema3.number.2", 2);
            dataContainer.StoreData("aSchema3.othernumber.3", 3);
            Assert.Catch(() => dataContainer.GetSchemaData("aSchema3.number.*"));
            GameObject.Destroy(go);
        }

        private Bundle BuildStubBundle()
        {
            DataContainerModule dataContainerModule = ScriptableObject.CreateInstance<MAGESDataContainer>();
            InteractionSystemModule interactionSystemModule = ScriptableObject.CreateInstance<StubInteractionSystem>();
            NetworkingModule networkingModule = ScriptableObject.CreateInstance<StubNetworking>();
            AnalyticsModule analyticsModule = ScriptableObject.CreateInstance<StubAnalytics>();
            DeviceManagerModule deviceManagerModule = ScriptableObject.CreateInstance<StubDeviceManager>();
            SceneGraphModule sceneGraphModule = ScriptableObject.CreateInstance<StubSceneGraph>();

            Bundle bundle = ScriptableObject.CreateInstance<Bundle>();
            bundle.DataContainer = dataContainerModule;
            bundle.InteractionSystem = interactionSystemModule;
            bundle.Networking = networkingModule;
            bundle.Analytics = analyticsModule;
            bundle.DeviceManager = deviceManagerModule;
            bundle.SceneGraph = sceneGraphModule;
            return bundle;
        }
    }
}