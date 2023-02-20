using System;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
#if UNITY_EDITOR
using UnityEditor;
using Rhinox.GUIUtils.Editor;
#endif
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

namespace Rhinox.XR.UnityXR.Simulator
{
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_DeviceSimulator)]
    public class XRDeviceSimulatorControls : MonoBehaviour
    {
        [SerializeField]
        [Tooltip("The Input System Action used to translate in the x-axis (left/right) while held. Must be a Value Axis Control.")]
        InputActionReference m_KeyboardXTranslateAction;
        /// <summary>
        /// The Input System Action used to translate in the x-axis (left/right) while held.
        /// Must be a <see cref="InputActionType.Value"/> <see cref="AxisControl"/>.
        /// </summary>
        public InputActionReference keyboardXTranslateAction
        {
            get => m_KeyboardXTranslateAction;
            set
            {
                UnsubscribeKeyboardXTranslateAction();
                m_KeyboardXTranslateAction = value;
                SubscribeKeyboardXTranslateAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to translate in the y-axis (up/down) while held. Must be a Value Axis Control.")]
        InputActionReference m_KeyboardYTranslateAction;
        /// <summary>
        /// The Input System Action used to translate in the y-axis (up/down) while held.
        /// Must be a <see cref="InputActionType.Value"/> <see cref="AxisControl"/>.
        /// </summary>
        public InputActionReference keyboardYTranslateAction
        {
            get => m_KeyboardYTranslateAction;
            set
            {
                UnsubscribeKeyboardYTranslateAction();
                m_KeyboardYTranslateAction = value;
                SubscribeKeyboardYTranslateAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to translate in the z-axis (forward/back) while held. Must be a Value Axis Control.")]
        InputActionReference m_KeyboardZTranslateAction;
        /// <summary>
        /// The Input System Action used to translate in the z-axis (forward/back) while held.
        /// Must be a <see cref="InputActionType.Value"/> <see cref="AxisControl"/>.
        /// </summary>
        public InputActionReference keyboardZTranslateAction
        {
            get => m_KeyboardZTranslateAction;
            set
            {
                UnsubscribeKeyboardZTranslateAction();
                m_KeyboardZTranslateAction = value;
                SubscribeKeyboardZTranslateAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to toggle enable manipulation of each controller/head individually when pressed each time. Must be a Button Control.")]
        InputActionReference m_ToggleManipulateAction;
        /// <summary>
        /// The Input System Action used to toggle enable manipulation of the left-hand controller when pressed.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <seealso cref="manipulateLeftAction"/>
        /// <seealso cref="toggleManipulateRightAction"/>
        public InputActionReference ToggleManipulateAction
        {
            get => m_ToggleManipulateAction;
            set
            {
                UnsubscribeToggleManipulateAction();
                m_ToggleManipulateAction = value;
                SubscribeToggleManipulateAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to toggle keyboard space.")]
        InputActionReference m_ToggleKeyboardSpaceAction;
        public InputActionReference ToggleKeyboardSpaceAction
        {
            get => m_ToggleKeyboardSpaceAction;
            set
            {
                UnsubscribeToggleKeyboardSpaceAction();
                m_ToggleKeyboardSpaceAction = value;
                SubscribeToggleKeyboardSpaceAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to translate or rotate by a scaled amount along or about the x- and y-axes. Must be a Value Vector2 Control.")]
        InputActionReference m_MouseDeltaAction;
        /// <summary>
        /// The Input System Action used to translate or rotate by a scaled amount along or about the x- and y-axes.
        /// Must be a <see cref="InputActionType.Value"/> <see cref="Vector2Control"/>.
        /// </summary>
        /// <remarks>
        /// Typically bound to the screen-space motion delta of the mouse in pixels.
        /// </remarks>
        /// <seealso cref="mouseScrollAction"/>
        public InputActionReference mouseDeltaAction
        {
            get => m_MouseDeltaAction;
            set
            {
                UnsubscribeMouseDeltaAction();
                m_MouseDeltaAction = value;
                SubscribeMouseDeltaAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to translate or rotate by a scaled amount along or about the z-axis. Must be a Value Vector2 Control.")]
        InputActionReference m_MouseScrollAction;
        /// <summary>
        /// The Input System Action used to translate or rotate by a scaled amount along or about the z-axis.
        /// Must be a <see cref="InputActionType.Value"/> <see cref="Vector2Control"/>.
        /// </summary>
        /// <remarks>
        /// Typically bound to the horizontal and vertical scroll wheels, though only the vertical is used.
        /// </remarks>
        /// <seealso cref="mouseDeltaAction"/>
        public InputActionReference mouseScrollAction
        {
            get => m_MouseScrollAction;
            set
            {
                UnsubscribeMouseScrollAction();
                m_MouseScrollAction = value;
                SubscribeMouseScrollAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to constrain the translation or rotation to the x-axis when moving the mouse or resetting. May be combined with another axis constraint to constrain to a plane. Must be a Button Control.")]
        InputActionReference m_XConstraintAction;
        /// <summary>
        /// The Input System Action used to constrain the translation or rotation to the x-axis when moving the mouse or resetting.
        /// May be combined with another axis constraint to constrain to a plane.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <seealso cref="yConstraintAction"/>
        /// <seealso cref="zConstraintAction"/>
        public InputActionReference xConstraintAction
        {
            get => m_XConstraintAction;
            set
            {
                UnsubscribeXConstraintAction();
                m_XConstraintAction = value;
                SubscribeXConstraintAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to constrain the translation or rotation to the y-axis when moving the mouse or resetting. May be combined with another axis constraint to constrain to a plane. Must be a Button Control.")]
        InputActionReference m_YConstraintAction;
        /// <summary>
        /// The Input System Action used to constrain the translation or rotation to the y-axis when moving the mouse or resetting.
        /// May be combined with another axis constraint to constrain to a plane.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <seealso cref="xConstraintAction"/>
        /// <seealso cref="zConstraintAction"/>
        public InputActionReference yConstraintAction
        {
            get => m_YConstraintAction;
            set
            {
                UnsubscribeYConstraintAction();
                m_YConstraintAction = value;
                SubscribeYConstraintAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to constrain the translation or rotation to the z-axis when moving the mouse or resetting. May be combined with another axis constraint to constrain to a plane. Must be a Button Control.")]
        InputActionReference m_ZConstraintAction;
        /// <summary>
        /// The Input System Action used to constrain the translation or rotation to the z-axis when moving the mouse or resetting.
        /// May be combined with another axis constraint to constrain to a plane.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <seealso cref="xConstraintAction"/>
        /// <seealso cref="yConstraintAction"/>
        public InputActionReference zConstraintAction
        {
            get => m_ZConstraintAction;
            set
            {
                UnsubscribeZConstraintAction();
                m_ZConstraintAction = value;
                SubscribeZConstraintAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to cause the manipulated device(s) to reset position or rotation (depending on the effective manipulation mode). Must be a Button Control.")]
        InputActionReference m_ResetAction;
        /// <summary>
        /// The Input System Action used to cause the manipulated device(s) to reset position or rotation
        /// (depending on the effective manipulation mode).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <remarks>
        /// Resets position to <see cref="Vector3.zero"/> and rotation to <see cref="Quaternion.identity"/>.
        /// May be combined with axis constraints (<see cref="xConstraintAction"/>, <see cref="yConstraintAction"/>, and <see cref="zConstraintAction"/>).
        /// </remarks>
        public InputActionReference resetAction
        {
            get => m_ResetAction;
            set
            {
                UnsubscribeResetAction();
                m_ResetAction = value;
                SubscribeResetAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to toggle the cursor lock mode for the game window when pressed. Must be a Button Control.")]
        InputActionReference m_ToggleCursorLockAction;
        /// <summary>
        /// The Input System Action used to toggle the cursor lock mode for the game window when pressed.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <seealso cref="Cursor.lockState"/>
        /// <seealso cref="desiredCursorLockMode"/>
        public InputActionReference toggleCursorLockAction
        {
            get => m_ToggleCursorLockAction;
            set
            {
                UnsubscribeToggleCursorLockAction();
                m_ToggleCursorLockAction = value;
                SubscribeToggleCursorLockAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to toggle enable translation from keyboard inputs when pressed. Must be a Button Control.")]
        InputActionReference m_ToggleDevicePositionTargetAction;
        /// <summary>
        /// The Input System Action used to toggle enable translation from keyboard inputs when pressed.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <seealso cref="keyboardXTranslateAction"/>
        /// <seealso cref="keyboardYTranslateAction"/>
        /// <seealso cref="keyboardZTranslateAction"/>
        public InputActionReference toggleDevicePositionTargetAction
        {
            get => m_ToggleDevicePositionTargetAction;
            set
            {
                UnsubscribeToggleDevicePositionTargetAction();
                m_ToggleDevicePositionTargetAction = value;
                SubscribeToggleDevicePositionTargetAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to toggle enable manipulation of the Primary2DAxis of the controllers when pressed. Must be a Button Control.")]
        InputActionReference m_TogglePrimary2DAxisTargetAction;
        /// <summary>
        /// The Input System action used to toggle enable manipulation of the <see cref="Axis2DTargets.Primary2DAxis"/> of the controllers when pressed.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <seealso cref="toggleSecondary2DAxisTargetAction"/>
        /// <seealso cref="toggleDevicePositionTargetAction"/>
        /// <seealso cref="axis2DAction"/>
        public InputActionReference togglePrimary2DAxisTargetAction
        {
            get => m_TogglePrimary2DAxisTargetAction;
            set
            {
                UnsubscribeTogglePrimary2DAxisTargetAction();
                m_TogglePrimary2DAxisTargetAction = value;
                SubscribeTogglePrimary2DAxisTargetAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to toggle enable manipulation of the Secondary2DAxis of the controllers when pressed. Must be a Button Control.")]
        InputActionReference m_ToggleSecondary2DAxisTargetAction;
        /// <summary>
        /// The Input System action used to toggle enable manipulation of the <see cref="Axis2DTargets.Secondary2DAxis"/> of the controllers when pressed.
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        /// <seealso cref="togglePrimary2DAxisTargetAction"/>
        /// <seealso cref="toggleDevicePositionTargetAction"/>
        /// <seealso cref="axis2DAction"/>
        public InputActionReference toggleSecondary2DAxisTargetAction
        {
            get => m_ToggleSecondary2DAxisTargetAction;
            set
            {
                UnsubscribeToggleSecondary2DAxisTargetAction();
                m_ToggleSecondary2DAxisTargetAction = value;
                SubscribeToggleSecondary2DAxisTargetAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the value of one or more 2D Axis controls on the manipulated controller device(s). Must be a Value Vector2 Control.")]
        InputActionReference m_Axis2DAction;
        /// <summary>
        /// The Input System Action used to control the value of one or more 2D Axis controls on the manipulated controller device(s).
        /// Must be a <see cref="InputActionType.Value"/> <see cref="Vector2Control"/>.
        /// </summary>
        /// <remarks>
        /// <see cref="togglePrimary2DAxisTargetAction"/> and <see cref="toggleSecondary2DAxisTargetAction"/> toggle enables
        /// the ability to manipulate 2D Axis controls on the simulated controllers, and this <see cref="axis2DAction"/>
        /// actually controls the value of them while those controller devices are being manipulated.
        /// <br />
        /// Typically bound to WASD on a keyboard, and controls the primary and/or secondary 2D Axis controls on them.
        /// </remarks>
        public InputActionReference axis2DAction
        {
            get => m_Axis2DAction;
            set
            {
                UnsubscribeAxis2DAction();
                m_Axis2DAction = value;
                SubscribeAxis2DAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control one or more 2D Axis controls on the opposite hand of the exclusively manipulated controller device. Must be a Value Vector2 Control.")]
        InputActionReference m_RestingHandAxis2DAction;
        /// <summary>
        /// The Input System Action used to control one or more 2D Axis controls on the opposite hand
        /// of the exclusively manipulated controller device.
        /// Must be a <see cref="InputActionType.Value"/> <see cref="Vector2Control"/>.
        /// </summary>
        /// <remarks>
        /// Typically bound to Q and E on a keyboard for the horizontal component, and controls the opposite hand's
        /// 2D Axis controls when manipulating one (and only one) controller. Can be used to quickly and simultaneously
        /// control the 2D Axis on the other hand's controller. In a typical setup of continuous movement bound on the left-hand
        /// controller stick, and turning bound on the right-hand controller stick, while exclusively manipulating the left-hand
        /// controller to move, this action can be used to trigger turning.
        /// </remarks>
        /// <seealso cref="axis2DAction"/>
        public InputActionReference restingHandAxis2DAction
        {
            get => m_RestingHandAxis2DAction;
            set
            {
                UnsubscribeRestingHandAxis2DAction();
                m_RestingHandAxis2DAction = value;
                SubscribeRestingHandAxis2DAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the Grip control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_GripAction;
        /// <summary>
        /// The Input System Action used to control the Grip control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference gripAction
        {
            get => m_GripAction;
            set
            {
                UnsubscribeGripAction();
                m_GripAction = value;
                SubscribeGripAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the Trigger control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_TriggerAction;
        /// <summary>
        /// The Input System Action used to control the Trigger control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference triggerAction
        {
            get => m_TriggerAction;
            set
            {
                UnsubscribeTriggerAction();
                m_TriggerAction = value;
                SubscribeTriggerAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the PrimaryButton control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_PrimaryButtonAction;
        /// <summary>
        /// The Input System Action used to control the PrimaryButton control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference primaryButtonAction
        {
            get => m_PrimaryButtonAction;
            set
            {
                UnsubscribePrimaryButtonAction();
                m_PrimaryButtonAction = value;
                SubscribePrimaryButtonAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the SecondaryButton control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_SecondaryButtonAction;
        /// <summary>
        /// The Input System Action used to control the SecondaryButton control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference secondaryButtonAction
        {
            get => m_SecondaryButtonAction;
            set
            {
                UnsubscribeSecondaryButtonAction();
                m_SecondaryButtonAction = value;
                SubscribeSecondaryButtonAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the Menu control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_MenuAction;
        /// <summary>
        /// The Input System Action used to control the Menu control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference menuAction
        {
            get => m_MenuAction;
            set
            {
                UnsubscribeMenuAction();
                m_MenuAction = value;
                SubscribeMenuAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the Primary2DAxisClick control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_Primary2DAxisClickAction;
        /// <summary>
        /// The Input System Action used to control the Primary2DAxisClick control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference primary2DAxisClickAction
        {
            get => m_Primary2DAxisClickAction;
            set
            {
                UnsubscribePrimary2DAxisClickAction();
                m_Primary2DAxisClickAction = value;
                SubscribePrimary2DAxisClickAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the Secondary2DAxisClick control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_Secondary2DAxisClickAction;
        /// <summary>
        /// The Input System Action used to control the Secondary2DAxisClick control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference secondary2DAxisClickAction
        {
            get => m_Secondary2DAxisClickAction;
            set
            {
                UnsubscribeSecondary2DAxisClickAction();
                m_Secondary2DAxisClickAction = value;
                SubscribeSecondary2DAxisClickAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the Primary2DAxisTouch control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_Primary2DAxisTouchAction;
        /// <summary>
        /// The Input System Action used to control the Primary2DAxisTouch control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference primary2DAxisTouchAction
        {
            get => m_Primary2DAxisTouchAction;
            set
            {
                UnsubscribePrimary2DAxisTouchAction();
                m_Primary2DAxisTouchAction = value;
                SubscribePrimary2DAxisTouchAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the Secondary2DAxisTouch control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_Secondary2DAxisTouchAction;
        /// <summary>
        /// The Input System Action used to control the Secondary2DAxisTouch control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference secondary2DAxisTouchAction
        {
            get => m_Secondary2DAxisTouchAction;
            set
            {
                UnsubscribeSecondary2DAxisTouchAction();
                m_Secondary2DAxisTouchAction = value;
                SubscribeSecondary2DAxisTouchAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the PrimaryTouch control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_PrimaryTouchAction;
        /// <summary>
        /// The Input System Action used to control the PrimaryTouch control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference primaryTouchAction
        {
            get => m_PrimaryTouchAction;
            set
            {
                UnsubscribePrimaryTouchAction();
                m_PrimaryTouchAction = value;
                SubscribePrimaryTouchAction();
            }
        }

        [SerializeField]
        [Tooltip("The Input System Action used to control the SecondaryTouch control of the manipulated controller device(s). Must be a Button Control.")]
        InputActionReference m_SecondaryTouchAction;
        /// <summary>
        /// The Input System Action used to control the SecondaryTouch control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference secondaryTouchAction
        {
            get => m_SecondaryTouchAction;
            set
            {
                UnsubscribeSecondaryTouchAction();
                m_SecondaryTouchAction = value;
                SubscribeSecondaryTouchAction();
            }
        }

        [SerializeField]
        private InputActionReference m_ToggleButtonControlTargetAction;
        /// <summary>
        /// The Input System Action used to control the SecondaryTouch control of the manipulated controller device(s).
        /// Must be a <see cref="ButtonControl"/>.
        /// </summary>
        public InputActionReference ToggleButtonControlTargetAction
        {
            get => m_ToggleButtonControlTargetAction;
            set
            {
                UnsubscribeToggleButtonControlTargetAction();
                m_ToggleButtonControlTargetAction = value;
                SubscribeToggleButtonControlTargetAction();
            }
        }

        [Tooltip("The desired cursor lock mode to toggle to from None (either Locked or Confined).")]
        public CursorLockMode DesiredCursorLockMode = CursorLockMode.Locked;

        [Tooltip("Speed of translation in the x-axis (left/right) when triggered by keyboard input.")]
        public float KeyboardXTranslateSpeed = 0.2f;

        [Tooltip("Speed of translation in the y-axis (up/down) when triggered by keyboard input.")]
        public float KeyboardYTranslateSpeed = 0.2f;

        [Tooltip("Speed of translation in the z-axis (forward/back) when triggered by keyboard input.")]
        public float KeyboardZTranslateSpeed = 0.2f;

        [Tooltip("Sensitivity of translation in the x-axis (left/right) when triggered by mouse input.")]
        public float MouseXTranslateSensitivity = 0.0004f;

        [Tooltip("Sensitivity of translation in the y-axis (up/down) when triggered by mouse input.")]
        public float MouseYTranslateSensitivity = 0.0004f;

        [Tooltip("Sensitivity of translation in the z-axis (forward/back) when triggered by mouse scroll input.")]
        public float MouseScrollTranslateSensitivity = 0.0002f;

        [Tooltip("Sensitivity of rotation along the x-axis (pitch) when triggered by mouse input.")]
        public float MouseXRotateSensitivity = 0.1f;

        [Tooltip("Sensitivity of rotation along the y-axis (yaw) when triggered by mouse input.")]
        public float MouseYRotateSensitivity = 0.1f;

        [Tooltip("Sensitivity of rotation along the z-axis (roll) when triggered by mouse scroll input.")]
        public float MouseScrollRotateSensitivity = 0.05f;

        [Tooltip("A boolean value of whether to invert the y-axis of mouse input when rotating by mouse input." +
                 "\nA false value (default) means typical FPS style where moving the mouse up/down pitches up/down." +
                 "\nA true value means flight control style where moving the mouse up/down pitches down/up.")]
        public bool MouseYRotateInvert;

        [Tooltip("The coordinate space in which keyboard translation should operate.")]
        public Space KeyboardTranslateSpace = Space.Local;

        [NonSerialized]
        public bool ManipulateRightControllerButtons = true;

        /// <summary>
        /// One or more 2D Axis controls that keyboard input should apply to (or none).
        /// </summary>
        /// <remarks>
        /// Used to control a combination of the position (<see cref="Axis2DTargets.Position"/>),
        /// primary 2D axis (<see cref="Axis2DTargets.Primary2DAxis"/>), or
        /// secondary 2D axis (<see cref="Axis2DTargets.Secondary2DAxis"/>) of manipulated device(s).
        /// </remarks>
        /// <seealso cref="keyboardXTranslateAction"/>
        /// <seealso cref="keyboardYTranslateAction"/>
        /// <seealso cref="keyboardZTranslateAction"/>
        /// <seealso cref="axis2DAction"/>
        /// <seealso cref="restingHandAxis2DAction"/>
        public Axis2DTargets axis2DTargets { get; set; } = Axis2DTargets.Primary2DAxis;

        float m_KeyboardXTranslateInput;
        float m_KeyboardYTranslateInput;
        float m_KeyboardZTranslateInput;

        public ManipulationTarget ManipulationTarget;

        Vector2 m_MouseDeltaInput;
        Vector2 m_MouseScrollInput;

        bool m_XConstraintInput;
        bool m_YConstraintInput;
        bool m_ZConstraintInput;

        bool m_ResetInput;

        public Vector2 Axis2DInput;
        public Vector2 RestingHandAxis2DInput;

        public bool GripInput { get; set; }
        public bool TriggerInput { get; set; }
        public bool PrimaryButtonInput { get; set; }
        public bool SecondaryButtonInput { get; set; }
        public bool MenuInput { get; set; }
        public bool Primary2DAxisClickInput { get; private set; }
        public bool Secondary2DAxisClickInput { get; private set; }
        public bool Primary2DAxisTouchInput { get; private set; }
        public bool Secondary2DAxisTouchInput { get; private set; }
        public bool PrimaryTouchInput { get; private set; }
        public bool SecondaryTouchInput { get; private set; }

        bool m_ManipulatedRestingHandAxis2D;

        Vector3 m_LeftControllerEuler;
        Vector3 m_RightControllerEuler;
        Vector3 m_CenterEyeEuler;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnEnable()
        {
            ManipulationTarget = ManipulationTarget.All;

            SubscribeKeyboardXTranslateAction();
            SubscribeKeyboardYTranslateAction();
            SubscribeKeyboardZTranslateAction();
            SubscribeToggleManipulateAction();
            SubscribeToggleKeyboardSpaceAction();
            SubscribeMouseDeltaAction();
            SubscribeMouseScrollAction();
            SubscribeXConstraintAction();
            SubscribeYConstraintAction();
            SubscribeZConstraintAction();
            SubscribeResetAction();
            SubscribeToggleCursorLockAction();
            SubscribeToggleDevicePositionTargetAction();
            SubscribeTogglePrimary2DAxisTargetAction();
            SubscribeToggleSecondary2DAxisTargetAction();
            SubscribeAxis2DAction();
            SubscribeRestingHandAxis2DAction();
            SubscribeGripAction();
            SubscribeTriggerAction();
            SubscribePrimaryButtonAction();
            SubscribeSecondaryButtonAction();
            SubscribeMenuAction();
            SubscribePrimary2DAxisClickAction();
            SubscribeSecondary2DAxisClickAction();
            SubscribePrimary2DAxisTouchAction();
            SubscribeSecondary2DAxisTouchAction();
            SubscribePrimaryTouchAction();
            SubscribeSecondaryTouchAction();
            SubscribeToggleButtonControlTargetAction();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnDisable()
        {
            UnsubscribeKeyboardXTranslateAction();
            UnsubscribeKeyboardYTranslateAction();
            UnsubscribeKeyboardZTranslateAction();
            UnsubscribeToggleKeyboardSpaceAction();
            UnsubscribeToggleManipulateAction();
            UnsubscribeMouseDeltaAction();
            UnsubscribeMouseScrollAction();
            UnsubscribeXConstraintAction();
            UnsubscribeYConstraintAction();
            UnsubscribeZConstraintAction();
            UnsubscribeResetAction();
            UnsubscribeToggleCursorLockAction();
            UnsubscribeToggleDevicePositionTargetAction();
            UnsubscribeTogglePrimary2DAxisTargetAction();
            UnsubscribeToggleSecondary2DAxisTargetAction();
            UnsubscribeAxis2DAction();
            UnsubscribeRestingHandAxis2DAction();
            UnsubscribeGripAction();
            UnsubscribeTriggerAction();
            UnsubscribePrimaryButtonAction();
            UnsubscribeSecondaryButtonAction();
            UnsubscribeMenuAction();
            UnsubscribePrimary2DAxisClickAction();
            UnsubscribeSecondary2DAxisClickAction();
            UnsubscribePrimary2DAxisTouchAction();
            UnsubscribeSecondary2DAxisTouchAction();
            UnsubscribePrimaryTouchAction();
            UnsubscribeSecondaryTouchAction();
            UnsubscribeToggleButtonControlTargetAction();
        }

        /// <summary>
        /// Process input from the user and update the state of manipulated controller device(s)
        /// related to button input controls.
        /// </summary>
        /// <param name="controllerState">The controller state that will be processed.</param>
        public virtual void ProcessButtonControlInput(ref XRSimulatedControllerState controllerState)
        {
            controllerState.grip = GripInput ? 1f : 0f;
            controllerState.WithButton(ControllerButton.GripButton, GripInput);
            controllerState.trigger = TriggerInput ? 1f : 0f;
            controllerState.WithButton(ControllerButton.TriggerButton, TriggerInput);
            controllerState.WithButton(ControllerButton.PrimaryButton, PrimaryButtonInput);
            controllerState.WithButton(ControllerButton.SecondaryButton, SecondaryButtonInput);
            controllerState.WithButton(ControllerButton.MenuButton, MenuInput);
            controllerState.WithButton(ControllerButton.Primary2DAxisClick, Primary2DAxisClickInput);
            controllerState.WithButton(ControllerButton.Secondary2DAxisClick, Secondary2DAxisClickInput);
            controllerState.WithButton(ControllerButton.Primary2DAxisTouch, Primary2DAxisTouchInput);
            controllerState.WithButton(ControllerButton.Secondary2DAxisTouch, Secondary2DAxisTouchInput);
            controllerState.WithButton(ControllerButton.PrimaryTouch, PrimaryTouchInput);
            controllerState.WithButton(ControllerButton.SecondaryTouch, SecondaryTouchInput);
        }

        /// <summary>
        /// Gets a <see cref="Vector3"/> that can be multiplied component-wise with another <see cref="Vector3"/>
        /// to reset components of the <see cref="Vector3"/>, based on axis constraint inputs.
        /// </summary>
        /// <returns></returns>
        /// <seealso cref="resetAction"/>
        /// <seealso cref="xConstraintAction"/>
        /// <seealso cref="yConstraintAction"/>
        /// <seealso cref="zConstraintAction"/>
        public Vector3 GetResetScale()
        {
            return m_XConstraintInput || m_YConstraintInput || m_ZConstraintInput
                ? new Vector3(m_XConstraintInput ? 0f : 1f, m_YConstraintInput ? 0f : 1f, m_ZConstraintInput ? 0f : 1f)
                : Vector3.zero;
        }
        

        void SubscribeKeyboardXTranslateAction() => SimulatorUtils.Subscribe(m_KeyboardXTranslateAction, OnKeyboardXTranslatePerformed, OnKeyboardXTranslateCanceled);
        void UnsubscribeKeyboardXTranslateAction() => SimulatorUtils.Unsubscribe(m_KeyboardXTranslateAction, OnKeyboardXTranslatePerformed, OnKeyboardXTranslateCanceled);

        void SubscribeKeyboardYTranslateAction() => SimulatorUtils.Subscribe(m_KeyboardYTranslateAction, OnKeyboardYTranslatePerformed, OnKeyboardYTranslateCanceled);
        void UnsubscribeKeyboardYTranslateAction() => SimulatorUtils.Unsubscribe(m_KeyboardYTranslateAction, OnKeyboardYTranslatePerformed, OnKeyboardYTranslateCanceled);

        void SubscribeKeyboardZTranslateAction() => SimulatorUtils.Subscribe(m_KeyboardZTranslateAction, OnKeyboardZTranslatePerformed, OnKeyboardZTranslateCanceled);
        void UnsubscribeKeyboardZTranslateAction() => SimulatorUtils.Unsubscribe(m_KeyboardZTranslateAction, OnKeyboardZTranslatePerformed, OnKeyboardZTranslateCanceled);

        void SubscribeToggleManipulateAction() => SimulatorUtils.Subscribe(m_ToggleManipulateAction, OnChangeManipulationTarget);
        void UnsubscribeToggleManipulateAction() => SimulatorUtils.Unsubscribe(m_ToggleManipulateAction, OnChangeManipulationTarget);

        void SubscribeToggleKeyboardSpaceAction() => SimulatorUtils.Subscribe(m_ToggleKeyboardSpaceAction, OnChangeKeyboardSpace);
        void UnsubscribeToggleKeyboardSpaceAction() => SimulatorUtils.Unsubscribe(m_ToggleKeyboardSpaceAction, OnChangeKeyboardSpace);

        void SubscribeMouseDeltaAction() => SimulatorUtils.Subscribe(m_MouseDeltaAction, OnMouseDeltaPerformed, OnMouseDeltaCanceled);
        void UnsubscribeMouseDeltaAction() => SimulatorUtils.Unsubscribe(m_MouseDeltaAction, OnMouseDeltaPerformed, OnMouseDeltaCanceled);

        void SubscribeMouseScrollAction() => SimulatorUtils.Subscribe(m_MouseScrollAction, OnMouseScrollPerformed, OnMouseScrollCanceled);
        void UnsubscribeMouseScrollAction() => SimulatorUtils.Unsubscribe(m_MouseScrollAction, OnMouseScrollPerformed, OnMouseScrollCanceled);

        void SubscribeXConstraintAction() => SimulatorUtils.Subscribe(m_XConstraintAction, OnXConstraintPerformed, OnXConstraintCanceled);
        void UnsubscribeXConstraintAction() => SimulatorUtils.Unsubscribe(m_XConstraintAction, OnXConstraintPerformed, OnXConstraintCanceled);

        void SubscribeYConstraintAction() => SimulatorUtils.Subscribe(m_YConstraintAction, OnYConstraintPerformed, OnYConstraintCanceled);
        void UnsubscribeYConstraintAction() => SimulatorUtils.Unsubscribe(m_YConstraintAction, OnYConstraintPerformed, OnYConstraintCanceled);

        void SubscribeZConstraintAction() => SimulatorUtils.Subscribe(m_ZConstraintAction, OnZConstraintPerformed, OnZConstraintCanceled);
        void UnsubscribeZConstraintAction() => SimulatorUtils.Unsubscribe(m_ZConstraintAction, OnZConstraintPerformed, OnZConstraintCanceled);

        void SubscribeResetAction() => SimulatorUtils.Subscribe(m_ResetAction, OnResetPerformed, OnResetCanceled);
        void UnsubscribeResetAction() => SimulatorUtils.Unsubscribe(m_ResetAction, OnResetPerformed, OnResetCanceled);

        void SubscribeToggleCursorLockAction() => SimulatorUtils.Subscribe(m_ToggleCursorLockAction, OnToggleCursorLockPerformed);
        void UnsubscribeToggleCursorLockAction() => SimulatorUtils.Unsubscribe(m_ToggleCursorLockAction, OnToggleCursorLockPerformed);

        void SubscribeToggleDevicePositionTargetAction() => SimulatorUtils.Subscribe(m_ToggleDevicePositionTargetAction, OnToggleDevicePositionTargetPerformed);
        void UnsubscribeToggleDevicePositionTargetAction() => SimulatorUtils.Unsubscribe(m_ToggleDevicePositionTargetAction, OnToggleDevicePositionTargetPerformed);

        void SubscribeTogglePrimary2DAxisTargetAction() => SimulatorUtils.Subscribe(m_TogglePrimary2DAxisTargetAction, OnTogglePrimary2DAxisTargetPerformed);
        void UnsubscribeTogglePrimary2DAxisTargetAction() => SimulatorUtils.Unsubscribe(m_TogglePrimary2DAxisTargetAction, OnTogglePrimary2DAxisTargetPerformed);

        void SubscribeToggleSecondary2DAxisTargetAction() => SimulatorUtils.Subscribe(m_ToggleSecondary2DAxisTargetAction, OnToggleSecondary2DAxisTargetPerformed);
        void UnsubscribeToggleSecondary2DAxisTargetAction() => SimulatorUtils.Unsubscribe(m_ToggleSecondary2DAxisTargetAction, OnToggleSecondary2DAxisTargetPerformed);

        void SubscribeAxis2DAction() => SimulatorUtils.Subscribe(m_Axis2DAction, OnAxis2DPerformed, OnAxis2DCanceled);
        void UnsubscribeAxis2DAction() => SimulatorUtils.Unsubscribe(m_Axis2DAction, OnAxis2DPerformed, OnAxis2DCanceled);

        void SubscribeRestingHandAxis2DAction() => SimulatorUtils.Subscribe(m_RestingHandAxis2DAction, OnRestingHandAxis2DPerformed, OnRestingHandAxis2DCanceled);
        void UnsubscribeRestingHandAxis2DAction() => SimulatorUtils.Unsubscribe(m_RestingHandAxis2DAction, OnRestingHandAxis2DPerformed, OnRestingHandAxis2DCanceled);

        void SubscribeGripAction() => SimulatorUtils.Subscribe(m_GripAction, OnGripPerformed, OnGripCanceled);
        void UnsubscribeGripAction() => SimulatorUtils.Unsubscribe(m_GripAction, OnGripPerformed, OnGripCanceled);

        void SubscribeTriggerAction() => SimulatorUtils.Subscribe(m_TriggerAction, OnTriggerPerformed, OnTriggerCanceled);
        void UnsubscribeTriggerAction() => SimulatorUtils.Unsubscribe(m_TriggerAction, OnTriggerPerformed, OnTriggerCanceled);

        void SubscribePrimaryButtonAction() => SimulatorUtils.Subscribe(m_PrimaryButtonAction, OnPrimaryButtonPerformed, OnPrimaryButtonCanceled);
        void UnsubscribePrimaryButtonAction() => SimulatorUtils.Unsubscribe(m_PrimaryButtonAction, OnPrimaryButtonPerformed, OnPrimaryButtonCanceled);

        void SubscribeSecondaryButtonAction() => SimulatorUtils.Subscribe(m_SecondaryButtonAction, OnSecondaryButtonPerformed, OnSecondaryButtonCanceled);
        void UnsubscribeSecondaryButtonAction() => SimulatorUtils.Unsubscribe(m_SecondaryButtonAction, OnSecondaryButtonPerformed, OnSecondaryButtonCanceled);

        void SubscribeMenuAction() => SimulatorUtils.Subscribe(m_MenuAction, OnMenuPerformed, OnMenuCanceled);
        void UnsubscribeMenuAction() => SimulatorUtils.Unsubscribe(m_MenuAction, OnMenuPerformed, OnMenuCanceled);

        void SubscribePrimary2DAxisClickAction() => SimulatorUtils.Subscribe(m_Primary2DAxisClickAction, OnPrimary2DAxisClickPerformed, OnPrimary2DAxisClickCanceled);
        void UnsubscribePrimary2DAxisClickAction() => SimulatorUtils.Unsubscribe(m_Primary2DAxisClickAction, OnPrimary2DAxisClickPerformed, OnPrimary2DAxisClickCanceled);

        void SubscribeSecondary2DAxisClickAction() => SimulatorUtils.Subscribe(m_Secondary2DAxisClickAction, OnSecondary2DAxisClickPerformed, OnSecondary2DAxisClickCanceled);
        void UnsubscribeSecondary2DAxisClickAction() => SimulatorUtils.Unsubscribe(m_Secondary2DAxisClickAction, OnSecondary2DAxisClickPerformed, OnSecondary2DAxisClickCanceled);

        void SubscribePrimary2DAxisTouchAction() => SimulatorUtils.Subscribe(m_Primary2DAxisTouchAction, OnPrimary2DAxisTouchPerformed, OnPrimary2DAxisTouchCanceled);
        void UnsubscribePrimary2DAxisTouchAction() => SimulatorUtils.Unsubscribe(m_Primary2DAxisTouchAction, OnPrimary2DAxisTouchPerformed, OnPrimary2DAxisTouchCanceled);

        void SubscribeSecondary2DAxisTouchAction() => SimulatorUtils.Subscribe(m_Secondary2DAxisTouchAction, OnSecondary2DAxisTouchPerformed, OnSecondary2DAxisTouchCanceled);
        void UnsubscribeSecondary2DAxisTouchAction() => SimulatorUtils.Unsubscribe(m_Secondary2DAxisTouchAction, OnSecondary2DAxisTouchPerformed, OnSecondary2DAxisTouchCanceled);

        void SubscribePrimaryTouchAction() => SimulatorUtils.Subscribe(m_PrimaryTouchAction, OnPrimaryTouchPerformed, OnPrimaryTouchCanceled);
        void UnsubscribePrimaryTouchAction() => SimulatorUtils.Unsubscribe(m_PrimaryTouchAction, OnPrimaryTouchPerformed, OnPrimaryTouchCanceled);

        void SubscribeSecondaryTouchAction() => SimulatorUtils.Subscribe(m_SecondaryTouchAction, OnSecondaryTouchPerformed, OnSecondaryTouchCanceled);
        void UnsubscribeSecondaryTouchAction() => SimulatorUtils.Unsubscribe(m_SecondaryTouchAction, OnSecondaryTouchPerformed, OnSecondaryTouchCanceled);

        void SubscribeToggleButtonControlTargetAction() => SimulatorUtils.Subscribe(m_ToggleButtonControlTargetAction, OnToggleButtonControlTarget);
        void UnsubscribeToggleButtonControlTargetAction() => SimulatorUtils.Unsubscribe(m_ToggleButtonControlTargetAction, OnToggleButtonControlTarget);

        void OnKeyboardXTranslatePerformed(InputAction.CallbackContext context) => m_KeyboardXTranslateInput = context.ReadValue<float>();
        void OnKeyboardXTranslateCanceled(InputAction.CallbackContext context) => m_KeyboardXTranslateInput = 0f;

        void OnKeyboardYTranslatePerformed(InputAction.CallbackContext context) => m_KeyboardYTranslateInput = context.ReadValue<float>();
        void OnKeyboardYTranslateCanceled(InputAction.CallbackContext context) => m_KeyboardYTranslateInput = 0f;

        void OnKeyboardZTranslatePerformed(InputAction.CallbackContext context) => m_KeyboardZTranslateInput = context.ReadValue<float>();
        void OnKeyboardZTranslateCanceled(InputAction.CallbackContext context) => m_KeyboardZTranslateInput = 0f;

        void OnChangeManipulationTarget(InputAction.CallbackContext context)
        {
            switch (ManipulationTarget)
            {
                case ManipulationTarget.All:
                    ManipulationTarget = ManipulationTarget.RightHand;
                    break;
                case ManipulationTarget.RightHand:
                    ManipulationTarget = ManipulationTarget.LeftHand;
                    break;
                case ManipulationTarget.LeftHand:
                    ManipulationTarget = ManipulationTarget.Head;
                    break;
                case ManipulationTarget.Head:
                    ManipulationTarget = ManipulationTarget.All;
                    break;
            }
        }

        void OnChangeKeyboardSpace(InputAction.CallbackContext context)
        {
            switch (KeyboardTranslateSpace)
            {
                case Space.Local:
                    KeyboardTranslateSpace = Space.Parent;
                    break;
                case Space.Parent:
                    KeyboardTranslateSpace = Space.Screen;
                    break;
                case Space.Screen:
                    KeyboardTranslateSpace = Space.Local;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        public Vector3 GetScaledMouseDeltaInput()
        {
            var scaledMouseDeltaInput =
                new Vector3(m_MouseDeltaInput.x * MouseXTranslateSensitivity,
                    m_MouseDeltaInput.y * MouseYTranslateSensitivity,
                    m_MouseScrollInput.y * MouseScrollTranslateSensitivity);
            return scaledMouseDeltaInput;
        }

        public Vector3 GetScaledMouseRotateInput()
        {
            // Mouse rotation
            var scaledMouseDeltaInput =
                new Vector3(m_MouseDeltaInput.x * MouseXRotateSensitivity,
                    m_MouseDeltaInput.y * MouseYRotateSensitivity * (MouseYRotateInvert ? 1f : -1f),
                    m_MouseScrollInput.y * MouseScrollRotateSensitivity);
            return scaledMouseDeltaInput;
        }

        public float ScaledKeyboardTranslateX => m_KeyboardXTranslateInput * KeyboardXTranslateSpeed;
        public float ScaledKeyboardTranslateY => m_KeyboardYTranslateInput * KeyboardYTranslateSpeed;
        public float ScaledKeyboardTranslateZ => m_KeyboardZTranslateInput * KeyboardZTranslateSpeed;

        //==============================================================================================================

        void OnMouseDeltaPerformed(InputAction.CallbackContext context) => m_MouseDeltaInput = context.ReadValue<Vector2>();
        void OnMouseDeltaCanceled(InputAction.CallbackContext context) => m_MouseDeltaInput = Vector2.zero;

        void OnMouseScrollPerformed(InputAction.CallbackContext context) => m_MouseScrollInput = context.ReadValue<Vector2>();
        void OnMouseScrollCanceled(InputAction.CallbackContext context) => m_MouseScrollInput = Vector2.zero;

        void OnXConstraintPerformed(InputAction.CallbackContext context) => m_XConstraintInput = true;
        void OnXConstraintCanceled(InputAction.CallbackContext context) => m_XConstraintInput = false;

        void OnYConstraintPerformed(InputAction.CallbackContext context) => m_YConstraintInput = true;
        void OnYConstraintCanceled(InputAction.CallbackContext context) => m_YConstraintInput = false;

        void OnZConstraintPerformed(InputAction.CallbackContext context) => m_ZConstraintInput = true;
        void OnZConstraintCanceled(InputAction.CallbackContext context) => m_ZConstraintInput = false;

        void OnResetPerformed(InputAction.CallbackContext context) => m_ResetInput = true;
        void OnResetCanceled(InputAction.CallbackContext context) => m_ResetInput = false;

        void OnToggleCursorLockPerformed(InputAction.CallbackContext context) => Cursor.lockState = EnumHelper.Negate(Cursor.lockState, DesiredCursorLockMode);

        void OnToggleDevicePositionTargetPerformed(InputAction.CallbackContext context) => axis2DTargets ^= Axis2DTargets.Position;

        void OnTogglePrimary2DAxisTargetPerformed(InputAction.CallbackContext context) => axis2DTargets ^= Axis2DTargets.Primary2DAxis;

        void OnToggleSecondary2DAxisTargetPerformed(InputAction.CallbackContext context) => axis2DTargets ^= Axis2DTargets.Secondary2DAxis;

        void OnAxis2DPerformed(InputAction.CallbackContext context) => Axis2DInput = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1f);
        void OnAxis2DCanceled(InputAction.CallbackContext context) => Axis2DInput = Vector2.zero;

        void OnRestingHandAxis2DPerformed(InputAction.CallbackContext context) => RestingHandAxis2DInput = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1f);
        void OnRestingHandAxis2DCanceled(InputAction.CallbackContext context) => RestingHandAxis2DInput = Vector2.zero;

        void OnGripPerformed(InputAction.CallbackContext context) => GripInput = true;
        void OnGripCanceled(InputAction.CallbackContext context) => GripInput = false;

        void OnTriggerPerformed(InputAction.CallbackContext context) => TriggerInput = true;
        void OnTriggerCanceled(InputAction.CallbackContext context) => TriggerInput = false;

        void OnPrimaryButtonPerformed(InputAction.CallbackContext context) => PrimaryButtonInput = true;
        void OnPrimaryButtonCanceled(InputAction.CallbackContext context) => PrimaryButtonInput = false;

        void OnSecondaryButtonPerformed(InputAction.CallbackContext context) => SecondaryButtonInput = true;
        void OnSecondaryButtonCanceled(InputAction.CallbackContext context) => SecondaryButtonInput = false;

        void OnMenuPerformed(InputAction.CallbackContext context) => MenuInput = true;
        void OnMenuCanceled(InputAction.CallbackContext context) => MenuInput = false;

        void OnPrimary2DAxisClickPerformed(InputAction.CallbackContext context) => Primary2DAxisClickInput = true;
        void OnPrimary2DAxisClickCanceled(InputAction.CallbackContext context) => Primary2DAxisClickInput = false;

        void OnSecondary2DAxisClickPerformed(InputAction.CallbackContext context) => Secondary2DAxisClickInput = true;
        void OnSecondary2DAxisClickCanceled(InputAction.CallbackContext context) => Secondary2DAxisClickInput = false;

        void OnPrimary2DAxisTouchPerformed(InputAction.CallbackContext context) => Primary2DAxisTouchInput = true;
        void OnPrimary2DAxisTouchCanceled(InputAction.CallbackContext context) => Primary2DAxisTouchInput = false;

        void OnSecondary2DAxisTouchPerformed(InputAction.CallbackContext context) => Secondary2DAxisTouchInput = true;
        void OnSecondary2DAxisTouchCanceled(InputAction.CallbackContext context) => Secondary2DAxisTouchInput = false;

        void OnPrimaryTouchPerformed(InputAction.CallbackContext context) => PrimaryTouchInput = true;
        void OnPrimaryTouchCanceled(InputAction.CallbackContext context) => PrimaryTouchInput = false;

        void OnSecondaryTouchPerformed(InputAction.CallbackContext context) => SecondaryTouchInput = true;
        void OnSecondaryTouchCanceled(InputAction.CallbackContext context) => SecondaryTouchInput = false;

        void OnToggleButtonControlTarget(InputAction.CallbackContext context) => ManipulateRightControllerButtons = !ManipulateRightControllerButtons;

        public bool ResetInputTriggered()
        {
            return m_ResetInput;
        }

        public virtual XRSimulatedControllerState ProcessAxis2DControlInput(XRSimulatedControllerState controllerState)
        {
            if (ManipulationTarget == ManipulationTarget.Head || ManipulationTarget == ManipulationTarget.All)
                return controllerState;

            if ((axis2DTargets & Axis2DTargets.Primary2DAxis) != 0)
            {
                controllerState.primary2DAxis = Axis2DInput;

                if (RestingHandAxis2DInput != Vector2.zero || m_ManipulatedRestingHandAxis2D)
                {
                    controllerState.primary2DAxis = RestingHandAxis2DInput;
                    m_ManipulatedRestingHandAxis2D = RestingHandAxis2DInput != Vector2.zero;
                }
                else
                {
                    m_ManipulatedRestingHandAxis2D = false;
                }
            }

            if ((axis2DTargets & Axis2DTargets.Secondary2DAxis) != 0)
            {
                controllerState.secondary2DAxis = Axis2DInput;

                if (RestingHandAxis2DInput != Vector2.zero || m_ManipulatedRestingHandAxis2D)
                {
                    controllerState.secondary2DAxis = RestingHandAxis2DInput;
                    m_ManipulatedRestingHandAxis2D = RestingHandAxis2DInput != Vector2.zero;
                }
                else
                {
                    m_ManipulatedRestingHandAxis2D = false;
                }
            }

            return controllerState;
        }

        public Vector3 GetConstrainedDelta(Vector3 right, Vector3 up, Vector3 forward, Vector3 deltaInput)
        {
            Vector3 deltaPosition;
            if (m_XConstraintInput && !m_YConstraintInput && m_ZConstraintInput) // XZ
            {
                deltaPosition =
                    right * deltaInput.x +
                    forward * deltaInput.y;
            }
            else if (!m_XConstraintInput && m_YConstraintInput && m_ZConstraintInput) // YZ
            {
                deltaPosition =
                    up * deltaInput.y +
                    forward * deltaInput.x;
            }
            else if (m_XConstraintInput && !m_YConstraintInput && !m_ZConstraintInput) // X
            {
                deltaPosition =
                    right * (deltaInput.x + deltaInput.y);
            }
            else if (!m_XConstraintInput && m_YConstraintInput && !m_ZConstraintInput) // Y
            {
                deltaPosition =
                    up * (deltaInput.x + deltaInput.y);
            }
            else if (!m_XConstraintInput && !m_YConstraintInput && m_ZConstraintInput) // Z
            {
                deltaPosition =
                    forward * (deltaInput.x + deltaInput.y);
            }
            else
            {
                deltaPosition =
                    right * deltaInput.x +
                    up * deltaInput.y;
            }

            // Scroll contribution
            deltaPosition +=
                forward * deltaInput.z;
            return deltaPosition;
        }

        public Vector3 GetScaledRotationDelta(Vector3 deltaInput)
        {
            Vector3 anglesDelta;
            if (m_XConstraintInput && !m_YConstraintInput && m_ZConstraintInput) // XZ
            {
                anglesDelta = new Vector3(deltaInput.y, 0f, -deltaInput.x);
            }
            else if (!m_XConstraintInput && m_YConstraintInput && m_ZConstraintInput) // YZ
            {
                anglesDelta = new Vector3(0f, deltaInput.x, -deltaInput.y);
            }
            else if (m_XConstraintInput && !m_YConstraintInput && !m_ZConstraintInput) // X
            {
                anglesDelta = new Vector3(-deltaInput.x + deltaInput.y, 0f, 0f);
            }
            else if (!m_XConstraintInput && m_YConstraintInput && !m_ZConstraintInput) // Y
            {
                anglesDelta = new Vector3(0f, deltaInput.x + -deltaInput.y, 0f);
            }
            else if (!m_XConstraintInput && !m_YConstraintInput && m_ZConstraintInput) // Z
            {
                anglesDelta = new Vector3(0f, 0f, -deltaInput.x + -deltaInput.y);
            }
            else
            {
                anglesDelta = new Vector3(deltaInput.y, deltaInput.x, 0f);
            }

            // Scroll contribution
            anglesDelta += new Vector3(0f, 0f, deltaInput.z);

            return anglesDelta;
        }

#if UNITY_EDITOR
        [ContextMenu("Import InputActionAsset")]
        private void ImportActionAsset()
        {
            EditorInputDialog.Create("Import InputActionAsset", "Reference File")
                .GenericUnityObjectField<InputActionAsset>("InputActionAsset:", out var actionAsset)
                .BooleanField("Overwrite:", out var overwriteVal)
                .OnAccept(() =>
                {
                    if (actionAsset == null || actionAsset.Value == null)
                        return;

                    var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    fields = fields.Where(x => x.GetReturnType() == typeof(InputActionReference)).ToArray();

                    foreach (var field in fields)
                    {
                        if (!overwriteVal.Value && field.GetValue(this) != null)
                            continue;
                        foreach (var map in actionAsset.Value.actionMaps)
                        {
                            foreach (var action in map.actions)
                            {
                                var name = action.name.Split('/').LastOrDefault();
                                if (name == null)
                                    continue;
                                var parts = name.SplitCamelCase(" ").Split(' ');
                                bool containsAll = true;
                                foreach (var part in parts)
                                {
                                    if (string.IsNullOrWhiteSpace(part))
                                        continue;


                                    if (!field.Name.Contains(part, StringComparison.InvariantCultureIgnoreCase))
                                        containsAll = false;
                                }

                                if (containsAll)
                                {
                                    var actionReference = GetReference(action);
                                    field.SetValue(this, actionReference);
                                    break;
                                }

                            }
                        }
                    }
                })
                .Show();
        }

        private static InputActionReference GetReference(InputAction action)
        {
            var assets = AssetDatabase.FindAssets($"t:{nameof(InputActionReference)}");
            foreach (var asset in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(asset);
                var refAssets = AssetDatabase.LoadAllAssetsAtPath(path);

                foreach (var refAsset in refAssets.OfType<InputActionReference>())
                    if (refAsset.action.id == action.id)
                        return refAsset;
            }

            return InputActionReference.Create(action);
        }
#endif
    }
}