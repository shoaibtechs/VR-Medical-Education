namespace MAGES.Networking.PrefabConfiguration
{
    using System.Collections.Generic;
    using UnityEngine;

    /// <summary>
    /// Utility to add/remove components from prefabs based on a configuration.
    /// </summary>
    public class PrefabConfigurationData : MonoBehaviour
    {
        [SerializeField]
        private List<Configuration> configurations = new List<Configuration>();

        /// <summary>
        /// Gets a list of configurations for different configurators to apply.
        /// </summary>
        public List<Configuration> Configurations => configurations;

        /// <summary>
        /// Gets the configuration for a specific configurator.
        /// </summary>
        /// <param name="configurator">The configurator to get the configuration for.</param>
        /// <returns>The configuration.</returns>
        public List<Configuration> GetPendingConfigurationsForSpecificConfigurator(System.Type configurator)
        {
            var configurationsPending = new List<Configuration>();

            for (int i = 0; i < Configurations.Count; i++)
            {
                Configuration configuration = Configurations[i];
                if (configurator.AssemblyQualifiedName == configuration.Configurator)
                {
                    // See if the configuration is empty
                    if ((configuration.ComponentsToAdd == null || configuration.ComponentsToAdd.Count <= 0) &&
                        (configuration.ComponentsToRemove == null || configuration.ComponentsToRemove.Count <= 0))
                    {
                        continue;
                    }

                    // Or already done.
                    bool configurationAlreadyApplied = true;
                    foreach (var component in configuration.ComponentsToAdd)
                    {
                        if (gameObject.GetComponent(component) != null)
                        {
                            continue;
                        }

                        configurationAlreadyApplied = false;
                    }

                    if (!configurationAlreadyApplied)
                    {
                        configurationsPending.Add(configuration);
                        continue;
                    }

                    foreach (var component in configuration.ComponentsToRemove)
                    {
                        if (gameObject.GetComponent(component) == null)
                        {
                            continue;
                        }

                        configurationAlreadyApplied = false;
                    }

                    if (!configurationAlreadyApplied)
                    {
                        configurationsPending.Add(configuration);
                        continue;
                    }
                }
            }

            return configurationsPending;
        }

        /// <summary>
        /// Holds the configuration for a specific configurator.
        /// </summary>
        [System.Serializable]
        public class Configuration
        {
            [SerializeField]
            private string configurator = string.Empty;
            [SerializeField]
            private List<string> componentsToAdd = new List<string>();
            [SerializeField]
            private List<string> componentsToRemove = new List<string>();

            /// <summary>
            /// Gets the name of the configurator that will use this configuration.
            /// </summary>
            [Tooltip("Use Assembly Qualified Name of Configurator")]
            public string Configurator => configurator;

            /// <summary>
            /// Gets the components that will be added to the prefab.
            /// </summary>
            [Tooltip("Use Assembly Qualified Name of Components")]
            public List<string> ComponentsToAdd => componentsToAdd;

            /// <summary>
            /// Gets the components that will be removed from the prefab.
            /// </summary>
            [Tooltip("Use Assembly Qualified Name of Components")]
            public List<string> ComponentsToRemove => componentsToRemove;
        }
    }
}
