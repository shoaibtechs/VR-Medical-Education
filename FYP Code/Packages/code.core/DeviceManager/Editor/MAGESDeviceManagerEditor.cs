namespace MAGES.DeviceManager.Editor
{
    using System;
    using MAGES.Editor;
    using UnityEditor;
    using UnityEditor.PackageManager;
    using UnityEditor.UIElements;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom editor for MAGES DeviceManager.
    /// </summary>
    [CustomEditor(typeof(MAGESDeviceManager))]
    [CanEditMultipleObjects]
    public class MAGESDeviceManagerEditor : Editor
    {
        private VisualElement root;
        private Button pluginInstallButtonXR;
        private FloatField cameraOffset;

        /// <summary>
        /// Creates a visual element that represents the InputControllerEditor.
        /// </summary>
        /// <returns>A copy of the new visual element.</returns>
        public override VisualElement CreateInspectorGUI()
        {
            base.CreateInspectorGUI();
            root = new VisualElement();
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Packages/com.oramavr.mages.core/DeviceManager/Editor/MagesDeviceManagerEditor.uxml");
            visualTree.CloneTree(root);
            pluginInstallButtonXR = root.Q<Button>("XRPlugin");
#if XR_MANAGEMENT
            pluginInstallButtonXR.style.display = DisplayStyle.None;
#endif
            var xr_ModeEnum = root.Q<EnumField>("XR_Mode");
            cameraOffset = root.Q<FloatField>("CamHeight");
            cameraOffset.style.display = DisplayStyle.None;
            xr_ModeEnum.RegisterValueChangedCallback(CheckXR_Management);
            UpdateXR_Management(((MAGESDeviceManager)target).CurrentMode);
            SetupXRButton();
            return root;
        }

        private void CheckXR_Management(ChangeEvent<Enum> eventXR)
        {
            DeviceManagerModule.CameraMode newValue = (DeviceManagerModule.CameraMode)eventXR.newValue;
            UpdateXR_Management(newValue);
        }

        private void SetupXRButton()
        {
            pluginInstallButtonXR.text = "Install XR Plugin";
            pluginInstallButtonXR.clickable.clicked += InstallXRPlugin;
        }

        private void UpdateXR_Management(DeviceManagerModule.CameraMode newValue)
        {
            if (newValue == DeviceManagerModule.CameraMode.UniversalXR)
            {
                cameraOffset.style.display = DisplayStyle.None;
#if !XR_MANAGEMENT
                pluginInstallButtonXR.style.display = DisplayStyle.Flex;
                pluginInstallButtonXR.style.flexDirection = FlexDirection.Column;

#else
                pluginInstallButtonXR.style.display = DisplayStyle.None;
#endif
            }
            else
            {
                pluginInstallButtonXR.style.display = DisplayStyle.None;
                cameraOffset.style.display = DisplayStyle.Flex;
            }
        }

        private void InstallXRPlugin()
        {
            bool windowResult =
                EditorUtility.DisplayDialog(
                    "XR Devices Installation/Selection",
                    "After XR installation is finished, please select the supported devices for your project",
                    "Ok",
                    "Cancel");

            if (!windowResult)
            {
                return;
            }

            PackageManagerUtility pm = new PackageManagerUtility();
            pm.InstallPackage("com.unity.xr.management", (_, success) =>
            {
                if (success)
                {
                    SettingsService.OpenProjectSettings("Project/XR Plug-in Management");
                }
            });
        }
    }
}
