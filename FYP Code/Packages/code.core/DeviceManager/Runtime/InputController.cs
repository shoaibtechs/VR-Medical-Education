namespace MAGES.DeviceManager
{
    using System;
    using UnityEngine;
    using UnityEngine.InputSystem;

    /// <summary>
    /// MAGES InputController.Implements IInteractionActions ILocomotionActions and IGenericActions.
    /// Wraps Unity's input actions to generic events that can be used by the rest of MAGES.
    /// </summary>
    [CreateAssetMenu(fileName = "InputController", menuName = "MAGES/DeviceManager/MAGESInputController")]
    [HelpURL(HelpUrls.InputController)]
    public class InputController : ScriptableObject, IInteractionActions, ILocomotionActions, IGenericInputActions
    {
        [SerializeReference]
        [Tooltip("Input bindings on how user can select with the left hand.")]
        private InputActionReference selectLeft;

        [SerializeReference]
        [Tooltip("Input bindings on how user can activate objects with the left hand.")]
        private InputActionReference activateLeft;

        [SerializeReference]
        [Tooltip("Gets the value of the user's grip on his left hand.")]
        private InputActionReference selectionIntensityLeft;

        [SerializeReference]
        [Tooltip("Gets the value of the user's trigger on his left hand.")]
        private InputActionReference activationIntensityLeft;

        [SerializeReference]
        [Tooltip("Gets the value of the button that will be used for left click on UIs for the left hand")]
#pragma warning disable SA1306 // Field names should begin with lower-case letter
        private InputActionReference UISelectLeft;
#pragma warning restore SA1306 // Field names should begin with lower-case letter

        [SerializeReference]
        [Tooltip("Input bindings on how user can select with the right hand.")]
        private InputActionReference selectRight;

        [SerializeReference]
        [Tooltip("Input bindings on how user can activate objects with the right hand.")]
        private InputActionReference activateRight;

        [SerializeReference]
        [Tooltip("Gets the value of the user's grip on his right hand.")]
        private InputActionReference selectionIntensityRight;

        [SerializeReference]
        [Tooltip("Gets the value of the user's trigger on his right hand.")]
        private InputActionReference activationIntensityRight;

        [SerializeReference]
        [Tooltip("Gets the value of the button that will be used for left click on UIs for the right hand")]
#pragma warning disable SA1306 // Field names should begin with lower-case letter
        private InputActionReference UISelectRight;
#pragma warning restore SA1306 // Field names should begin with lower-case letter

        [SerializeReference]
        [Tooltip("Input bindings on how user can perform to the next action.")]
        private InputActionReference perform;

        [SerializeReference]
        [Tooltip("Input bindings on how user can undo to the previous action.")]
        private InputActionReference undo;

        [SerializeReference]
        [Tooltip("Input bindings for enabling and disabling movement.")]
        private InputActionReference enableMovement;

        [SerializeReference]
        [Tooltip("Input bindings for showing options at runtime.")]
        private InputActionReference options;

        [SerializeReference]
        [Tooltip("Input binding for toggling the raycast activation.")]
        private InputActionReference toggleRaycastActivation;

        [SerializeReference]
        [Tooltip("Input bindings for user movement on X and Z axis.")]
        private InputActionReference move;

        [SerializeReference]
        [Tooltip("Input bindings for user rotation around the Y axis.")]
        private InputActionReference snapTurn;

        [SerializeReference]
        [Tooltip("Input bindings for user smooth rotation.")]
        private InputActionReference smoothTurn;

        [SerializeReference]
        [Tooltip("Input bindings for user movement on the Y axis.")]
        private InputActionReference changeHeight;

        [SerializeReference]
        [Tooltip("Shows and enables the generic events.")]
        private bool genericEvents;

        [SerializeReference]
        [Tooltip("Shows and enables the interaction events.")]
        private bool interaction;

        [SerializeReference]
        [Tooltip("Shows and enables the locomotion events.")]
        private bool locomotion;

        [SerializeReference]
        [Tooltip("Shows and enables the events of the XR Simulator.")]
        private bool xrsimulator;

        private Action<Vector2> moveActionRef;
        private Action<Vector2> rotateActionRef;

        /// <summary>
        /// Gets or sets a value indicating whether the generic input actions of MAGES are enabled.
        /// </summary>
        public bool GenericEvents
        {
            get => genericEvents;
            set => genericEvents = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the actions for interaction are enabled.
        /// </summary>
        public bool Interaction
        {
            get => interaction;
            set => interaction = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the actions for desktop mode are enabled.
        /// </summary>
        public bool XRSimulator
        {
            get => xrsimulator;
            set => xrsimulator = value;
        }

        /// <summary>
        /// Gets or sets a value indicating whether the actions for locomotion are enabled.
        /// </summary>
        public bool Locomotion
        {
            get => locomotion;
            set => locomotion = value;
        }

        /// <inheritdoc cref="IInteractionActions.Startup"/>
        /// Enables the actions of the Unity Input System.
        public void Startup()
        {
            if (GenericEvents)
            {
                EnableActionIfNotNull(options);
                EnableActionIfNotNull(enableMovement);
                EnableActionIfNotNull(perform);
                EnableActionIfNotNull(undo);
                EnableActionIfNotNull(toggleRaycastActivation);
            }

            if (XRSimulator)
            {
            }

            if (Locomotion)
            {
                EnableActionIfNotNull(move);
                EnableActionIfNotNull(snapTurn);
                EnableActionIfNotNull(changeHeight);
                EnableActionIfNotNull(smoothTurn);
            }

            if (Interaction)
            {
                EnableActionIfNotNull(selectLeft);
                EnableActionIfNotNull(activateLeft);
                EnableActionIfNotNull(selectionIntensityLeft);
                EnableActionIfNotNull(activationIntensityLeft);
                EnableActionIfNotNull(UISelectLeft);

                EnableActionIfNotNull(selectRight);
                EnableActionIfNotNull(activateRight);
                EnableActionIfNotNull(selectionIntensityRight);
                EnableActionIfNotNull(activationIntensityRight);
                EnableActionIfNotNull(UISelectRight);
            }
        }

        /// <inheritdoc cref="IInteractionActions.AddActionOnSelectLeft"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public void AddActionOnSelectLeft(Action action)
        {
            selectLeft.action.performed += (_) => action();
        }

        /// <inheritdoc cref="IInteractionActions.AddActionOnDeselectLeft"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public void AddActionOnDeselectLeft(Action action)
        {
            selectLeft.action.canceled += (_) => action();
        }

        /// <inheritdoc cref="IInteractionActions.AddActionOnActivateLeft"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public void AddActionOnActivateLeft(Action action)
        {
            activateLeft.action.performed += (_) => action();
        }

        /// <inheritdoc cref="IInteractionActions.AddActionOnDeactivateLeft"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public void AddActionOnDeactivateLeft(Action action)
        {
            activateLeft.action.canceled += (_) => action();
        }

        /// <inheritdoc cref="IInteractionActions.AddActionOnSelectRight"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public void AddActionOnSelectRight(Action action)
        {
            selectRight.action.performed += (_) => action();
        }

        /// <inheritdoc cref="IInteractionActions.AddActionOnDeselectRight"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public void AddActionOnDeselectRight(Action action)
        {
            selectRight.action.canceled += (_) => action();
        }

        /// <inheritdoc cref="IInteractionActions.AddActionOnActivateRight"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public void AddActionOnActivateRight(Action action)
        {
            activateRight.action.performed += (_) => action();
        }

        /// <inheritdoc cref="IInteractionActions.AddActionOnDeactivateRight"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public void AddActionOnDeactivateRight(Action action)
        {
            activateRight.action.canceled += (_) => action();
        }

        /// <inheritdoc cref="IInteractionActions.GetSelectionIntensityLeft"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public float GetSelectionIntensityLeft()
        {
            return selectionIntensityLeft.action.ReadValue<float>();
        }

        /// <inheritdoc cref="IInteractionActions.GetSelectionIntensityRight"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public float GetSelectionIntensityRight()
        {
            return selectionIntensityRight.action.ReadValue<float>();
        }

        /// <inheritdoc cref="IInteractionActions.GetActivationIntensityLeft"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public float GetActivationIntensityLeft()
        {
            return activationIntensityLeft.action.ReadValue<float>();
        }

        /// <inheritdoc cref="IInteractionActions.GetActivationIntensityRight"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public float GetActivationIntensityRight()
        {
            return activationIntensityRight.action.ReadValue<float>();
        }

        /// <inheritdoc cref="IGenericInputActions.AddActionOnPerform"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public void AddActionOnPerform(Action action)
        {
            perform.action.performed += (_) => action();
        }

        /// <inheritdoc cref="IGenericInputActions.AddActionOnUndo"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public void AddActionOnUndo(Action action)
        {
            undo.action.performed += (_) => action();
        }

        /// <inheritdoc cref="IGenericInputActions.AddActionOnEnableMovement"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public void AddActionOnEnableMovement(Action action)
        {
            enableMovement.action.performed += (_) => action();
        }

        /// <inheritdoc cref="IGenericInputActions.AddActionOnEnableOptions"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public void AddActionOnEnableOptions(Action action)
        {
            options.action.performed += (_) => action();
        }

        /// <inheritdoc cref="ILocomotionActions.AddActionOnMove"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public void AddActionOnMove(Action<Vector2> moveAction)
        {
            moveActionRef = moveAction;
            move.action.performed += (context) => moveAction(context.ReadValue<Vector2>());
        }

        /// <inheritdoc cref="ILocomotionActions.AddActionOnSnapTurn"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public void AddActionOnSnapTurn(Action<float> snapRotateAction)
        {
            snapTurn.action.performed += (context) => snapRotateAction(context.ReadValue<float>());
        }

        /// <inheritdoc cref="ILocomotionActions.AddActionOnSmoothTurn"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public void AddActionOnSmoothTurn(Action<Vector2> smoothRotateAction)
        {
            rotateActionRef = smoothRotateAction;
            smoothTurn.action.performed += (context) => smoothRotateAction(context.ReadValue<Vector2>());
        }

        /// <inheritdoc cref="ILocomotionActions.AddActionOnChangeHeight"/>
        /// <remarks>Uses Unity Input System. </remarks>
        public void AddActionOnChangeHeight(Action<float> changeHeightAction)
        {
            changeHeight.action.performed += (context) => changeHeightAction(context.ReadValue<float>());
        }

#pragma warning disable SA1305 // Field names should not use Hungarian notation
        /// <inheritdoc cref="IInteractionActions.AddActionOnUISelectLeft(Action)"/>
        public void AddActionOnUISelectLeft(Action uiSelectLeft)
        {
            UISelectLeft.action.performed += (_) => uiSelectLeft();
        }

        /// <inheritdoc cref="IInteractionActions.AddActionOnUISelectCancelLeft(Action)"/>
        public void AddActionOnUISelectCancelLeft(Action uiDeselectLeft)
        {
            UISelectLeft.action.canceled += (_) => uiDeselectLeft();
        }

        /// <inheritdoc cref="IInteractionActions.AddActionOnUISelectRight(Action)"/>
        public void AddActionOnUISelectRight(Action uiSelectLeft)
        {
            UISelectRight.action.performed += (_) => uiSelectLeft();
        }

        /// <inheritdoc cref="IInteractionActions.AddActionOnUISelectCancelRight(Action)"/>
        public void AddActionOnUISelectCancelRight(Action uiDeselectRight)
        {
            UISelectRight.action.canceled += (_) => uiDeselectRight();
        }
#pragma warning restore SA1305 // Field names should not use Hungarian notation

        /// <inheritdoc cref="IGenericInputActions.AddActionOnToggleRaycastActivation(Action)"/>
        public void AddActionOnToggleRaycastActivation(Action action)
        {
            toggleRaycastActivation.action.performed += (_) => action();
        }

        /// <summary>
        /// Moves the camera directly from the passed input.
        /// Does not use unity's bindings.
        /// </summary>
        /// <param name="moveInput">The 2D vector that defines how the camera should be moved.</param>
        public void CustomMoveCamera(Vector2 moveInput)
        {
            moveActionRef?.Invoke(moveInput);
        }

        /// <summary>
        /// Rotates the camera directly from the passed input.
        /// Does not use unity's bindings.
        /// </summary>
        /// <param name="rotateInput">The 2D vector that defines how the camera will rotate.</param>
        public void CustomRotateCamera(Vector2 rotateInput)
        {
            rotateActionRef?.Invoke(rotateInput);
        }

        private void EnableActionIfNotNull(InputActionReference actionReference)
        {
            if (actionReference != null)
            {
                actionReference.action.Enable();
            }
        }
    }
}