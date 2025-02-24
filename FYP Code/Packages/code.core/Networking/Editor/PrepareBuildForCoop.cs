namespace MAGES.Networking.Editor
{
    using MAGES.Editor;
    using UnityEditor;
    using UnityEditor.Build;
    using UnityEditor.Build.Reporting;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    /// <summary>
    /// Creates and updates the reference cache before build.
    /// </summary>
    [InitializeOnLoad]
    public class PrepareBuildForCoop : IPreprocessBuildWithReport
    {
        private static bool sceneHasBeenInitialized;

        static PrepareBuildForCoop()
        {
            EditorApplication.update += OnEditorUpdate;
        }

#pragma warning disable SA1300
        /// <inheritdoc cref="IOrderedCallback.callbackOrder"/>
        public int callbackOrder => 0;
#pragma warning restore SA1300

        /// <inheritdoc cref="IPostprocessBuildWithReport.OnPreprocessBuild"/>
        public void OnPreprocessBuild(BuildReport report)
        {
#if PHOTON_UNITY_NETWORKING
            NetworkSceneUpdater.AddPhotonViews(SceneManager.GetActiveScene());
#endif
            CreatePathReferences.CreatePathReferencesCache();
        }

        private static void OnEditorUpdate()
        {
#if PHOTON_UNITY_NETWORKING
            if (!sceneHasBeenInitialized)
            {
                NetworkSceneUpdater.AddPhotonViews(SceneManager.GetActiveScene());
                sceneHasBeenInitialized = true;
            }
#else
            sceneHasBeenInitialized = false;
            if (!sceneHasBeenInitialized)
            {
                return;
            }
#endif
        }
    }
}