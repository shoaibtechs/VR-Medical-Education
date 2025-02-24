namespace MAGES.DeviceManager.Editor
{
    using UnityEditor;
    using UnityEngine.UIElements;

    /// <summary>
    /// Custom editor for MAGES InputController.
    /// </summary>
    [CustomEditor(typeof(InputController))]
    [CanEditMultipleObjects]
    public class InputControllerEditor : Editor
    {
        private InputController content;
        private VisualElement root;

        private VisualElement twoDoF;
        private VisualElement streams;
        private VisualElement events;

        /// <summary>
        /// Creates a visual element that represents the InputControllerEditor.
        /// </summary>
        /// <returns>A copy of the new visual element.</returns>
        public override VisualElement CreateInspectorGUI()
        {
            base.CreateInspectorGUI();
            root = new VisualElement();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Packages/com.oramavr.mages.core/DeviceManager/Editor/InputControllerEditor.uxml");
            visualTree.CloneTree(root);
            content = target as InputController;

            root.Q<Toggle>("EventsToggle")?.RegisterValueChangedCallback(ValueChangedGenericEvents);
            root.Q<Toggle>("InteractionToggle")?.RegisterValueChangedCallback(ValueChangedInteraction);
            root.Q<Toggle>("LocomotionToggle")?.RegisterValueChangedCallback(ValueChangedLocomotion);
            return root;
        }

        private void ValueChangedGenericEvents(ChangeEvent<bool> ve)
        {
            events = root.Q<VisualElement>("Generic_Events");
            events.style.display = content != null && content.GenericEvents ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void ValueChangedInteraction(ChangeEvent<bool> ve)
        {
            events = root.Q<VisualElement>("Interaction_Events");
            events.style.display = content != null && content.Interaction ? DisplayStyle.Flex : DisplayStyle.None;
        }

        private void ValueChangedLocomotion(ChangeEvent<bool> ve)
        {
            events = root.Q<VisualElement>("Locomotion_Events");
            events.style.display = content != null && content.Locomotion ? DisplayStyle.Flex : DisplayStyle.None;
        }
    }
}