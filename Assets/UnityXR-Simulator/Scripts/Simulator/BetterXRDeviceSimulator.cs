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

namespace Rhinox.XR.UnityXR.Simulator
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
        /// Used to control a combination of the position (<see cref="Simulator.Axis2DTargets.Position"/>),
        /// primary 2D axis (<see cref="Simulator.Axis2DTargets.Primary2DAxis"/>), or
        /// secondary 2D axis (<see cref="Simulator.Axis2DTargets.Secondary2DAxis"/>) of manipulated device(s).
        /// </remarks>
        /// <seealso cref="keyboardXTranslateAction"/>
        /// <seealso cref="keyboardYTranslateAction"/>
        /// <seealso cref="keyboardZTranslateAction"/>
        /// <seealso cref="axis2DAction"/>
        /// <seealso cref="restingHandAxis2DAction"/>
        public Axis2DTargets Axis2DTargets { get; set; } = Axis2DTargets.Position;

        private Vector3 _leftControllerEuler;
        private Vector3 _rightControllerEuler;
        private Vector3 _centerEyeEuler;

        public XRSimulatedHMDState HMDState;
        public XRSimulatedControllerState LeftControllerState;
        public XRSimulatedControllerState RightControllerState;

        private XRSimulatedHMD _hmdDevice;
        private XRSimulatedController _leftControllerDevice;
        private XRSimulatedController _rightControllerDevice;



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

            HMDState.Reset();
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

        /// <summary>
        /// Tries to set the Camera transform to the main camera.
        /// </summary>
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

            if (_hmdDevice != null && _hmdDevice.added)
                InputState.Change(_hmdDevice, HMDState);

            if (_leftControllerDevice != null && _leftControllerDevice.added)
                InputState.Change(_leftControllerDevice, LeftControllerState);

            if (_rightControllerDevice != null && _rightControllerDevice.added)
                InputState.Change(_rightControllerDevice, RightControllerState);
        }

        /// <summary>
        /// Resets the state and transform of both controllers. Both controllers are positioned relatively to the head/HMD.
        /// </summary>
        private void ResetControllers()
        {
            LeftControllerState.Reset();
            RightControllerState.Reset();

            const float HALF_SHOULDER_WIDTH = 0.18f;
            Vector3 baseHeadOffset = Vector3.forward * 0.25f + Vector3.down * 0.15f;
            Vector3 leftOffset = Vector3.left * HALF_SHOULDER_WIDTH + baseHeadOffset;
            Vector3 rightOffset = Vector3.right * HALF_SHOULDER_WIDTH + baseHeadOffset;

            var resetScale = _controls.GetResetScale();
            _rightControllerEuler = Vector3.Scale(_rightControllerEuler, resetScale);
            _leftControllerEuler = Vector3.Scale(_leftControllerEuler, resetScale);

            PositionRelativeToHead(ref RightControllerState, rightOffset, Quaternion.Euler(_rightControllerEuler));
            PositionRelativeToHead(ref LeftControllerState, leftOffset, Quaternion.Euler(_leftControllerEuler));
        }

        /// <summary>
        /// Positions the given simulated controller state relatively to the head.
        /// </summary>
        /// <param name="state">Desired controller state to position.</param>
        /// <param name="position">The relative position.</param>
        /// <param name="rotation">An optional rotation to add.</param>
        private void PositionRelativeToHead(ref XRSimulatedControllerState state, Vector3 position, Quaternion? rotation = null)
        {
            Vector3 headPos = HMDState.centerEyePosition;
            Quaternion rot = HMDState.centerEyeRotation;
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

            if (Axis2DTargets.HasFlag(Axis2DTargets.Position))
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
                    _rightControllerEuler += anglesDelta;
                    RightControllerState.deviceRotation = Quaternion.Euler(_rightControllerEuler);
                    break;
                case ManipulationTarget.LeftHand:
                    _leftControllerEuler += anglesDelta;
                    LeftControllerState.deviceRotation = Quaternion.Euler(_leftControllerEuler);
                    break;
                case ManipulationTarget.Head:
                    _centerEyeEuler += anglesDelta;
                    HMDState.centerEyeRotation = Quaternion.Euler(_centerEyeEuler);
                    HMDState.deviceRotation = HMDState.centerEyeRotation;
                    break;
                case ManipulationTarget.All:
                    var matrixL = GetRelativeMatrixFromHead(ref LeftControllerState);
                    var matrixR = GetRelativeMatrixFromHead(ref RightControllerState);
                    _centerEyeEuler += anglesDelta;
                    HMDState.centerEyeRotation = Quaternion.Euler(_centerEyeEuler);
                    HMDState.deviceRotation = HMDState.centerEyeRotation;
                    PositionRelativeToHead(ref LeftControllerState, matrixL.GetColumn(3), matrixL.rotation);
                    PositionRelativeToHead(ref RightControllerState, matrixR.GetColumn(3), matrixR.rotation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_controls.ResetInputTriggered())
                ResetControllers();
        }
        
        /// <summary>
        /// Calculates the relative matrix from the head matrix and the given controller states matrix.
        /// </summary>
        /// <param name="state">Controller state of which the relative matrix is desired.</param>
        /// <returns>The calculated relative matrix.</returns>
        private Matrix4x4 GetRelativeMatrixFromHead(ref XRSimulatedControllerState state)
        {
            var controllerTrans = Matrix4x4.TRS(state.devicePosition,
                state.deviceRotation, Vector3.one);

            var headTrans = Matrix4x4.TRS(HMDState.devicePosition,
                HMDState.deviceRotation, Vector3.one);
            var matrix = headTrans.inverse * controllerTrans;
            return matrix;
        }

        /// <summary>
        /// Set the tracking for the devices to true and sets their tracking states to position and rotation.
        /// </summary>
        private void SetTrackingStates()
        {
            LeftControllerState.isTracked = true;
            RightControllerState.isTracked = true;
            HMDState.isTracked = true;
            LeftControllerState.trackingState = (int)(InputTrackingState.Position | InputTrackingState.Rotation);
            RightControllerState.trackingState = (int)(InputTrackingState.Position | InputTrackingState.Rotation);
            HMDState.trackingState = (int)(InputTrackingState.Position | InputTrackingState.Rotation);
        }

        /// <summary>
        /// Translates and rotates the current targets.
        /// </summary>
        /// <param name="manipulationSpace">The desired space for translation.</param>
        /// <param name="inverseCameraParentRotation">The inverse of the camera parent rotation.</param>
        /// <param name="deltaPosition">The translation needed.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void ProcessDevicePositionForTarget(Space manipulationSpace, Quaternion inverseCameraParentRotation, Vector3 deltaPosition)
        {
            Quaternion deltaRotation = Quaternion.identity;
            switch (_controls.ManipulationTarget)
            {
                case ManipulationTarget.RightHand:
                    deltaRotation = GetDeltaRotation(manipulationSpace, RightControllerState, inverseCameraParentRotation);
                    RightControllerState.devicePosition += deltaRotation * deltaPosition;
                    break;
                case ManipulationTarget.LeftHand:
                    deltaRotation = GetDeltaRotation(manipulationSpace, LeftControllerState, inverseCameraParentRotation);
                    LeftControllerState.devicePosition += deltaRotation * deltaPosition;
                    break;
                case ManipulationTarget.Head:
                    deltaRotation = GetDeltaRotation(manipulationSpace, HMDState, inverseCameraParentRotation);
                    HMDState.centerEyePosition += deltaRotation * deltaPosition;
                    HMDState.devicePosition = HMDState.centerEyePosition;
                    break;
                case ManipulationTarget.All:

                    Vector3 relativeRightPosition = RightControllerState.devicePosition - HMDState.devicePosition;
                    Vector3 relativeLeftPosition = LeftControllerState.devicePosition - HMDState.devicePosition;

                    deltaRotation = GetDeltaRotation(manipulationSpace, HMDState, inverseCameraParentRotation);
                    HMDState.centerEyePosition += deltaRotation * deltaPosition;
                    Vector3 newDevicePosition = HMDState.centerEyePosition;
                    HMDState.devicePosition = newDevicePosition;

                    RightControllerState.devicePosition = newDevicePosition + relativeRightPosition;
                    LeftControllerState.devicePosition = newDevicePosition + relativeLeftPosition;
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
                LeftControllerState = _controls.ProcessAxis2DControlInput(LeftControllerState);
                _controls.ProcessButtonControlInput(ref LeftControllerState);
            }
            else
            {
                RightControllerState = _controls.ProcessAxis2DControlInput(RightControllerState);
                _controls.ProcessButtonControlInput(ref RightControllerState);
            }
        }

        /// <summary>
        /// If the simulator has not yet been loaded, creates the simulated controllers and head mounted display.
        /// </summary>
        protected virtual void AddDevices()
        {
            if (_simulatorLoaded)
                return;

            _hmdDevice = InputSystem.AddDevice<XRSimulatedHMD>();
            if (_hmdDevice == null)
            {
                Debug.LogError($"Failed to create {nameof(XRSimulatedHMD)}.");
            }

            _leftControllerDevice = InputSystem.AddDevice<XRSimulatedController>($"{nameof(XRSimulatedController)} - {CommonUsages.LeftHand}");
            if (_leftControllerDevice != null)
            {
                InputSystem.SetDeviceUsage(_leftControllerDevice, CommonUsages.LeftHand);
            }
            else
            {
                Debug.LogError($"Failed to create {nameof(XRSimulatedController)} for {CommonUsages.LeftHand}.", this);
            }

            _rightControllerDevice = InputSystem.AddDevice<XRSimulatedController>($"{nameof(XRSimulatedController)} - {CommonUsages.RightHand}");
            if (_rightControllerDevice != null)
            {
                InputSystem.SetDeviceUsage(_rightControllerDevice, CommonUsages.RightHand);
            }
            else
            {
                Debug.LogError($"Failed to create {nameof(XRSimulatedController)} for {CommonUsages.RightHand}.", this);
            }

            _simulatorLoaded = true;
            SimulatorLoaded?.Invoke();
        }

        /// <summary>
        /// If the simulator is loaded, removes the current simulated controllers and head mounted device. Unloads the simulator.
        /// </summary>
        protected virtual void TryRemoveDevices()
        {
            if (!_simulatorLoaded)
                return;

            if (_hmdDevice != null && _hmdDevice.added)
                InputSystem.RemoveDevice(_hmdDevice);
            _hmdDevice = null;

            if (_leftControllerDevice != null && _leftControllerDevice.added)
                InputSystem.RemoveDevice(_leftControllerDevice);
            _leftControllerDevice = null;

            if (_rightControllerDevice != null && _rightControllerDevice.added)
                InputSystem.RemoveDevice(_rightControllerDevice);
            _rightControllerDevice = null;

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
    
        /// <summary>
        /// Check if the given device is not simulated and thus a real connected device.
        /// </summary>
        /// <param name="device">The device that should get checked.</param>
        /// <returns></returns>
        private bool IsRealXRDevice(InputDevice device)
        {
            if (device is XRHMD && !(device is XRSimulatedHMD))
                return true;
            if (device is XRController && !(device is XRSimulatedController))
                return true;
            return false;
        }

        /// <summary>
        /// Check if the given device is simulated and thus not a real connected device.
        /// </summary>
        /// <param name="device">The device that should get checked.</param>
        /// <returns></returns>
        private bool IsSimulatedDevice(InputDevice device)
        {
            return (_hmdDevice == device || _leftControllerDevice == device || _rightControllerDevice == device);
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
            if (_hmdDevice == null)
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
