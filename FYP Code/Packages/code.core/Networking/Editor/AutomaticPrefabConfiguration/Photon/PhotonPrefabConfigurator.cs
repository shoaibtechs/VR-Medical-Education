namespace MAGES.Networking.Editor.PrefabConfiguration
{
    using System;
    using MAGES.Networking.PrefabConfiguration;
    using MAGES.Utilities;
    using UnityEditor;
    using UnityEngine;
    using static MAGES.Networking.PrefabConfiguration.PrefabConfigurationData;

    /// <summary>
    /// Automatically configures all in project assets and attaches the required Photon scripts.
    /// </summary>
    [InitializeOnLoad]
    [DefaultExecutionOrder(1000)]
    public class PhotonPrefabConfigurator : AssetPostprocessor
    {
        static PhotonPrefabConfigurator()
        {
            EditorApplication.delayCall += () =>
            {
                ApplyPhotonConfigurationInPrefabs();
            };
        }

        /// <summary>
        /// Finds all prefabs in the project and applies the Photon Networking configuration to them.
        /// </summary>
        public static void ApplyPhotonConfigurationInPrefabs()
        {
            string[] guids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets" });

            foreach (var guid in guids)
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(guid);

                if (!assetPath.StartsWith("Packages/"))
                {
                    ApplyPendingConfigurationsIfExistingInAsset(assetPath);
                }
            }

            AssetDatabase.Refresh();
        }

        /// <summary>
        /// Called by unity when a prefab is done importing.
        /// </summary>
        /// <param name="g">The prefab.</param>
        public void OnPostprocessPrefab(GameObject g)
        {
            var editorNetworkingConfiguration = g.GetComponent<PrefabConfigurationData>();
            if (editorNetworkingConfiguration != null)
            {
                var configurationsToApply = editorNetworkingConfiguration.GetPendingConfigurationsForSpecificConfigurator(typeof(PhotonPrefabConfigurator));
                foreach (var configuration in configurationsToApply)
                {
                    ApplyConfiguration(g, configuration);
                }
            }
        }

        private static void ApplyPendingConfigurationsIfExistingInAsset(string assetPath)
        {
            GameObject prefab = PrefabUtility.LoadPrefabContents(assetPath);
            bool changed = false;

            // Check if the asset is a prefab
            if (prefab != null)
            {
                var editorNetworkingConfiguration = prefab.GetComponent<PrefabConfigurationData>();
                if (editorNetworkingConfiguration != null)
                {
                    var configurationsToApply = editorNetworkingConfiguration.GetPendingConfigurationsForSpecificConfigurator(typeof(PhotonPrefabConfigurator));
                    foreach (var configuration in configurationsToApply)
                    {
                        ApplyConfiguration(prefab, configuration);
                        changed = true;
                    }

                    if (changed)
                    {
                        // Ignore exceptions when saving the prefab for prefabs that are immutable.
                        try
                        {
                            GameObjectUtility.RemoveMonoBehavioursWithMissingScript(prefab);

                            PrefabUtility.SaveAsPrefabAsset(prefab, assetPath);
                        }
                        catch (ArgumentException)
                        {
                        }
                    }
                }
            }

            PrefabUtility.UnloadPrefabContents(prefab);
        }

        private static void ApplyConfiguration(GameObject prefab, Configuration configuration)
        {
            foreach (var componentType in configuration.ComponentsToAdd)
            {
#nullable enable
                Type? t = Type.GetType(componentType, false);
#nullable disable

                if (t != null)
                {
                    prefab.GetOrAddComponent(t);
                }
                else
                {
                    Debug.LogWarning($"[PhotonConfigurator]: Type {componentType} for {prefab.name} configuration was not found.");
                }
            }

            foreach (var componentType in configuration.ComponentsToRemove)
            {
#nullable enable
                Type? t = Type.GetType(componentType, false);
#nullable disable

                if (t != null)
                {
                    Component component = prefab.GetComponent(t);
                    if (component != null)
                    {
                        Component.DestroyImmediate(component);
                    }
                }
                else
                {
                    Debug.LogWarning($"[PhotonConfigurator]: Type {componentType} for {prefab.name} configuration was not found.");
                }
            }
        }
    }
}