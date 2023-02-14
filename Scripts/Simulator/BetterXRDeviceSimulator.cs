using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Inputs;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using InputDevice = UnityEngine.InputSystem.InputDevice;
using CommonUsages = UnityEngine.InputSystem.CommonUsages;
using XRController = UnityEngine.InputSystem.XR.XRController;

namespace Rhinox.VOLT.XR.UnityXR.Simulator
{
    /// <summary>
    /// A component which handles mouse and keyboard input from the user and uses it to
    /// drive simulated XR controllers and an XR head mounted display (HMD).
    /// </summary>
    /// <remarks>
    /// This class does not directly manipulate the camera or controllers which are part of
    /// the XR Origin, but rather drives them indirectly through simulated input devices.
    /// <br /><br />
    /// Use the Package Manager window to install the <i>XR Device Simulator</i> sample into
    /// your project to get sample mouse and keyboard bindings for Input System actions that
    /// this component expects. The sample also includes a prefab of a <see cref="GameObject"/>
    /// with this component attached that has references to those sample actions already set.
    /// To make use of this simulator, add the prefab to your scene (the prefab makes use
    /// of <see cref="InputActionManager"/> to ensure the Input System actions are enabled).
    /// <br /><br />
    /// Note that the XR Origin must read the position and rotation of the HMD and controllers
    /// by using Input System actions (such as by using <see cref="UnityEngine.XR.Interaction.Toolkit.ActionBasedController"/>
    /// and <see cref="TrackedPoseDriver"/>) for this simulator to work as expected.
    /// Attempting to use XR input subsystem device methods (such as by using <see cref="UnityEngine.InputSystem.XR.XRController"/>
    /// and <see cref="TrackedPoseDriver"/>) will not work as expected
    /// since this simulator depends on the Input System to drive the simulated devices.
    /// </remarks>
    [DefaultExecutionOrder(XRInteractionUpdateOrder.k_DeviceSimulator)]
    public class BetterXRDeviceSimulator : MonoBehaviour
    {
        [Tooltip("The Transform that contains the Camera. This is usually the \"Head\" of XR Origins. Automatically set to the first enabled camera tagged MainCamera if unset.")]
        public Transform CameraTransform;

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
        public Axis2DTargets axis2DTargets { get; set; } = Axis2DTargets.Position;

        Vector3 m_LeftControllerEuler;
        Vector3 m_RightControllerEuler;
        Vector3 m_CenterEyeEuler;

        XRSimulatedHMDState m_HMDState;
        XRSimulatedControllerState m_LeftControllerState;
        XRSimulatedControllerState m_RightControllerState;

        XRSimulatedHMD m_HMDDevice;
        public XRSimulatedHMD HMDDevice => m_HMDDevice;

        XRSimulatedController m_LeftControllerDevice;
        public XRSimulatedController LeftControllerDevice => m_LeftControllerDevice;

        XRSimulatedController m_RightControllerDevice;
        public XRSimulatedController RightControllerDevice => m_RightControllerDevice;



        private XRDeviceSimulatorControls _controls;
        private bool _simulatorLoaded;
        public bool IsLoaded => _simulatorLoaded;

        public bool IsRightTargeted => _controls == null || _controls.ManipulateRightControllerButtons;

        public event Action SimulatorLoaded;
        public event Action SimulatorUnloaded;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void Awake()
        {
            _controls = GetComponent<XRDeviceSimulatorControls>();

            m_HMDState.Reset();
            ResetControllers();
            XRSimulatedHMD temp;

        }


        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnEnable()
        {
            InputSystem.onDeviceChange += OnInputDeviceChange;
            // Has VR Active
            // Find the Camera if necessary
            TrySetCamera();

            if (InputSystem.devices.Count == 0 || InputSystem.devices.OfType<XRHMD>().All(x => !(x is XRSimulatedHMD)))
                AddDevices();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void OnDisable()
        {
            InputSystem.onDeviceChange -= OnInputDeviceChange;

            TryRemoveDevices();
        }

        private void TrySetCamera()
        {
            if (CameraTransform == null)
            {
                var mainCamera = Camera.main;
                if (mainCamera != null)
                    CameraTransform = mainCamera.transform;
            }
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected virtual void Update()
        {
            if (_controls.DesiredCursorLockMode != Cursor.lockState)
                Cursor.lockState = _controls.DesiredCursorLockMode;

            ProcessPoseInput();
            ProcessControlInput();

            if (m_HMDDevice != null && m_HMDDevice.added)
                InputState.Change(m_HMDDevice, m_HMDState);

            if (m_LeftControllerDevice != null && m_LeftControllerDevice.added)
                InputState.Change(m_LeftControllerDevice, m_LeftControllerState);

            if (m_RightControllerDevice != null && m_RightControllerDevice.added)
                InputState.Change(m_RightControllerDevice, m_RightControllerState);
        }

        private void ResetControllers()
        {
            m_LeftControllerState.Reset();
            m_RightControllerState.Reset();

            const float HALF_SHOULDER_WIDTH = 0.18f;
            Vector3 baseHeadOffset = Vector3.forward * 0.25f + Vector3.down * 0.15f;
            Vector3 leftOffset = Vector3.left * HALF_SHOULDER_WIDTH + baseHeadOffset;
            Vector3 rightOffset = Vector3.right * HALF_SHOULDER_WIDTH + baseHeadOffset;

            var resetScale = _controls.GetResetScale();
            m_RightControllerEuler = Vector3.Scale(m_RightControllerEuler, resetScale);
            m_LeftControllerEuler = Vector3.Scale(m_LeftControllerEuler, resetScale);

            PositionRelativeToHead(ref m_RightControllerState, rightOffset, Quaternion.Euler(m_RightControllerEuler));
            PositionRelativeToHead(ref m_LeftControllerState, leftOffset, Quaternion.Euler(m_LeftControllerEuler));
        }


        private void PositionRelativeToHead(ref XRSimulatedControllerState state, Vector3 position, Quaternion? rotation = null)
        {
            Vector3 headPos = m_HMDState.centerEyePosition;
            Quaternion rot = m_HMDState.centerEyeRotation;
            var headMatrix = Matrix4x4.TRS(headPos, rot, Vector3.one);

            Vector3 realPos = headMatrix * new Vector4(position.x, position.y, position.z, 1);
            state.devicePosition = realPos;
            if (rotation.HasValue)
            {
                Quaternion realRot = rot * rotation.Value;
                state.deviceRotation = realRot;
            }
        }

        /// <summary>
        /// Process input from the user and update the state of manipulated device(s)
        /// related to position and rotation.
        /// </summary>
        protected virtual void ProcessPoseInput()
        {
            SetTrackingStates();

            if (CameraTransform == null)
                return;

            var cameraParent = CameraTransform.parent;
            var cameraParentRotation = cameraParent != null ? cameraParent.rotation : Quaternion.identity;
            var inverseCameraParentRotation = Quaternion.Inverse(cameraParentRotation);

            if (axis2DTargets.HasFlag(Axis2DTargets.Position))
            {
                // Determine frame of reference
                EnumHelper.GetAxes(_controls.KeyboardTranslateSpace, CameraTransform, out var right, out var up, out var forward);

                // Keyboard translation
                var deltaPosition =
                    right * (_controls.ScaledKeyboardTranslateX * Time.deltaTime) +
                    up * (_controls.ScaledKeyboardTranslateY * Time.deltaTime) +
                    forward * (_controls.ScaledKeyboardTranslateZ * Time.deltaTime);

                ProcessDevicePositionForTarget(_controls.KeyboardTranslateSpace, inverseCameraParentRotation, deltaPosition);
            }


            // Mouse rotation
            var scaledMouseDeltaInput = _controls.GetScaledMouseRotateInput();

            Vector3 anglesDelta = _controls.GetScaledRotationDelta(scaledMouseDeltaInput);

            switch (_controls.ManipulationTarget)
            {
                case ManipulationTarget.RightHand:
                    m_RightControllerEuler += anglesDelta;
                    m_RightControllerState.deviceRotation = Quaternion.Euler(m_RightControllerEuler);
                    break;
                case ManipulationTarget.LeftHand:
                    m_LeftControllerEuler += anglesDelta;
                    m_LeftControllerState.deviceRotation = Quaternion.Euler(m_LeftControllerEuler);
                    break;
                case ManipulationTarget.Head:
                    m_CenterEyeEuler += anglesDelta;
                    m_HMDState.centerEyeRotation = Quaternion.Euler(m_CenterEyeEuler);
                    m_HMDState.deviceRotation = m_HMDState.centerEyeRotation;
                    break;
                case ManipulationTarget.All:
                    var matrixL = GetRelativeMatrixFromHead(ref m_LeftControllerState);
                    var matrixR = GetRelativeMatrixFromHead(ref m_RightControllerState);
                    m_CenterEyeEuler += anglesDelta;
                    m_HMDState.centerEyeRotation = Quaternion.Euler(m_CenterEyeEuler);
                    m_HMDState.deviceRotation = m_HMDState.centerEyeRotation;
                    PositionRelativeToHead(ref m_LeftControllerState, matrixL.GetColumn(3), matrixL.rotation);
                    PositionRelativeToHead(ref m_RightControllerState, matrixR.GetColumn(3), matrixR.rotation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_controls.ResetInputTriggered())
                ResetControllers();
        }

        private Matrix4x4 GetRelativeMatrixFromHead(ref XRSimulatedControllerState state)
        {
            var controllerTrans = Matrix4x4.TRS(state.devicePosition,
                state.deviceRotation, Vector3.one);

            var headTrans = Matrix4x4.TRS(m_HMDState.devicePosition,
                m_HMDState.deviceRotation, Vector3.one);
            var matrix = headTrans.inverse * controllerTrans;
            return matrix;
        }

        private void SetTrackingStates()
        {
            m_LeftControllerState.isTracked = true;
            m_RightControllerState.isTracked = true;
            m_HMDState.isTracked = true;
            m_LeftControllerState.trackingState = (int)(InputTrackingState.Position | InputTrackingState.Rotation);
            m_RightControllerState.trackingState = (int)(InputTrackingState.Position | InputTrackingState.Rotation);
            m_HMDState.trackingState = (int)(InputTrackingState.Position | InputTrackingState.Rotation);
        }

        private void ProcessDevicePositionForTarget(Space manipulationSpace, Quaternion inverseCameraParentRotation, Vector3 deltaPosition)
        {
            Quaternion deltaRotation = Quaternion.identity;
            switch (_controls.ManipulationTarget)
            {
                case ManipulationTarget.RightHand:
                    deltaRotation = GetDeltaRotation(manipulationSpace, m_RightControllerState, inverseCameraParentRotation);
                    m_RightControllerState.devicePosition += deltaRotation * deltaPosition;
                    break;
                case ManipulationTarget.LeftHand:
                    deltaRotation = GetDeltaRotation(manipulationSpace, m_LeftControllerState, inverseCameraParentRotation);
                    m_LeftControllerState.devicePosition += deltaRotation * deltaPosition;
                    break;
                case ManipulationTarget.Head:
                    deltaRotation = GetDeltaRotation(manipulationSpace, m_HMDState, inverseCameraParentRotation);
                    m_HMDState.centerEyePosition += deltaRotation * deltaPosition;
                    m_HMDState.devicePosition = m_HMDState.centerEyePosition;
                    break;
                case ManipulationTarget.All:

                    Vector3 relativeRightPosition = m_RightControllerState.devicePosition - m_HMDState.devicePosition;
                    Vector3 relativeLeftPosition = m_LeftControllerState.devicePosition - m_HMDState.devicePosition;

                    deltaRotation = GetDeltaRotation(manipulationSpace, m_HMDState, inverseCameraParentRotation);
                    m_HMDState.centerEyePosition += deltaRotation * deltaPosition;
                    Vector3 newDevicePosition = m_HMDState.centerEyePosition;
                    m_HMDState.devicePosition = newDevicePosition;

                    m_RightControllerState.devicePosition = newDevicePosition + relativeRightPosition;
                    m_LeftControllerState.devicePosition = newDevicePosition + relativeLeftPosition;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Process input from the user and update the state of manipulated controller device(s)
        /// </summary>
        protected virtual void ProcessControlInput()
        {
            if (!_controls.ManipulateRightControllerButtons)
            {
                m_LeftControllerState = _controls.ProcessAxis2DControlInput(m_LeftControllerState);
                _controls.ProcessButtonControlInput(ref m_LeftControllerState);
            }
            else
            {
                m_RightControllerState = _controls.ProcessAxis2DControlInput(m_RightControllerState);
                _controls.ProcessButtonControlInput(ref m_RightControllerState);
            }
        }

        protected virtual void AddDevices()
        {
            if (_simulatorLoaded)
                return;

            m_HMDDevice = InputSystem.AddDevice<XRSimulatedHMD>();
            if (m_HMDDevice == null)
            {
                Debug.LogError($"Failed to create {nameof(XRSimulatedHMD)}.");
            }

            m_LeftControllerDevice = InputSystem.AddDevice<XRSimulatedController>($"{nameof(XRSimulatedController)} - {CommonUsages.LeftHand}");
            if (m_LeftControllerDevice != null)
            {
                InputSystem.SetDeviceUsage(m_LeftControllerDevice, CommonUsages.LeftHand);
            }
            else
            {
                Debug.LogError($"Failed to create {nameof(XRSimulatedController)} for {CommonUsages.LeftHand}.", this);
            }

            m_RightControllerDevice = InputSystem.AddDevice<XRSimulatedController>($"{nameof(XRSimulatedController)} - {CommonUsages.RightHand}");
            if (m_RightControllerDevice != null)
            {
                InputSystem.SetDeviceUsage(m_RightControllerDevice, CommonUsages.RightHand);
            }
            else
            {
                Debug.LogError($"Failed to create {nameof(XRSimulatedController)} for {CommonUsages.RightHand}.", this);
            }

            _simulatorLoaded = true;
            SimulatorLoaded?.Invoke();
        }

        protected virtual void TryRemoveDevices()
        {
            if (!_simulatorLoaded)
                return;

            if (m_HMDDevice != null && m_HMDDevice.added)
                InputSystem.RemoveDevice(m_HMDDevice);
            m_HMDDevice = null;

            if (m_LeftControllerDevice != null && m_LeftControllerDevice.added)
                InputSystem.RemoveDevice(m_LeftControllerDevice);
            m_LeftControllerDevice = null;

            if (m_RightControllerDevice != null && m_RightControllerDevice.added)
                InputSystem.RemoveDevice(m_RightControllerDevice);
            m_RightControllerDevice = null;

            _simulatorLoaded = false;
            SimulatorUnloaded?.Invoke();
        }

        void OnInputDeviceChange(InputDevice device, InputDeviceChange change)
        {
            switch (change)
            {
                case InputDeviceChange.Added:
                case InputDeviceChange.Reconnected:
                    if (IsRealXRDevice(device))
                        TryRemoveDevices();
                    break;
                case InputDeviceChange.Removed:
                case InputDeviceChange.Disconnected:
                case InputDeviceChange.Disabled:
                    if (InputSystem.devices.Where(x => x.enabled && x.added).Any(x => IsRealXRDevice(x)))
                    {
                        // NOP
                    }
                    else
                    {
                        AddDevices();
                    }
                    break;
            }
        }

        private bool IsRealXRDevice(InputDevice device)
        {
            if (device is XRHMD && !(device is XRSimulatedHMD))
                return true;
            if (device is XRController && !(device is XRSimulatedController))
                return true;
            return false;
        }

        private bool IsSimulatedDevice(InputDevice device)
        {
            return (m_HMDDevice == device || m_LeftControllerDevice == device || m_RightControllerDevice == device);
        }



        static Quaternion GetDeltaRotation(Space translateSpace, in XRSimulatedControllerState state, in Quaternion inverseCameraParentRotation)
        {
            switch (translateSpace)
            {
                case Space.Local:
                    return state.deviceRotation * inverseCameraParentRotation;
                case Space.Parent:
                    return Quaternion.identity;
                case Space.Screen:
                    return inverseCameraParentRotation;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(translateSpace)}={translateSpace}.");
                    return Quaternion.identity;
            }
        }

        static Quaternion GetDeltaRotation(Space translateSpace, in XRSimulatedHMDState state, in Quaternion inverseCameraParentRotation)
        {
            switch (translateSpace)
            {
                case Space.Local:
                    return state.centerEyeRotation * inverseCameraParentRotation;
                case Space.Parent:
                    return Quaternion.identity;
                case Space.Screen:
                    return inverseCameraParentRotation;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(translateSpace)}={translateSpace}.");
                    return Quaternion.identity;
            }
        }

#if UNITY_EDITOR
        private void OnGUI()
        {
            if (m_HMDDevice == null)
                return;

            GUILayout.Label($"{GetCurrentBindingPrefix(_controls.ToggleManipulateAction)} Mode: {_controls.ManipulationTarget}");
            GUILayout.Label($"{GetCurrentBindingPrefix(_controls.ToggleKeyboardSpaceAction)} Keyboard Space: {_controls.KeyboardTranslateSpace}");
            GUILayout.Label($"{GetCurrentBindingPrefix(_controls.ToggleButtonControlTargetAction)} Controller Buttons: {(_controls.ManipulateRightControllerButtons ? "Right" : "Left")}");
        }

        private static string GetCurrentBindingPrefix(InputActionReference actionRef)
        {
            if (actionRef == null || actionRef.action == null)
                return string.Empty;

            var firstBinding = actionRef.action.bindings.FirstOrDefault();
            if (firstBinding == default)
                return string.Empty;

            string answer = firstBinding.effectivePath.Split('/').LastOrDefault();
            if (answer != null)
                return "[" + answer.ToUpperInvariant() + "]";
            return "";
        }
#endif
    }
}
