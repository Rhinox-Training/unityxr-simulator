using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.XR;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;
using CommonUsages = UnityEngine.InputSystem.CommonUsages;
using InputDevice = UnityEngine.InputSystem.InputDevice;

namespace Rhinox.XR.UnityXR.Simulator
{
    public class XRDeviceSimulator : BaseSimulator
    {
        public XRSimulatedHMDState HMDState;
        public XRSimulatedControllerState LeftControllerState;
        public XRSimulatedControllerState RightControllerState;

        private XRSimulatedHMD _hmdDevice;
        private XRSimulatedController _leftControllerDevice;
        private XRSimulatedController _rightControllerDevice;
        private bool _simulatorLoaded;

        public event Action SimulatorLoaded;
        public event Action SimulatorUnloaded;
        public bool UsesRealVR { get; private set; }

        protected override void Initialize()
        {
            base.Initialize();

            HMDState.Reset();
            ResetControllers();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            InputSystem.onDeviceChange += OnInputDeviceChange;
            if (InputSystem.devices.Count == 0 || InputSystem.devices.OfType<XRHMD>().All(x => !(x is XRSimulatedHMD)))
                AddDevices();
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>.
        /// </summary>
        protected void OnDisable()
        {
            InputSystem.onDeviceChange -= OnInputDeviceChange;

            TryRemoveDevices();
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
                    if (InputSystem.devices.Where(x => x.enabled && x.added).Any(IsRealXRDevice))
                    {
                        // NOP
                        UsesRealVR = true;
                    }
                    else
                    {
                        UsesRealVR = false;
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
            switch (device)
            {
                case XRHMD _ when !(device is XRSimulatedHMD):
                case XRController _ when !(device is XRSimulatedController):
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// If the simulator is loaded, removes the current simulated controllers and head mounted device. Unloads the simulator.
        /// </summary>
        private void TryRemoveDevices()
        {
            if (!_simulatorLoaded)
                return;

            if (_hmdDevice is { added: true })
                InputSystem.RemoveDevice(_hmdDevice);
            _hmdDevice = null;

            if (_leftControllerDevice is { added: true })
                InputSystem.RemoveDevice(_leftControllerDevice);
            _leftControllerDevice = null;

            if (_rightControllerDevice is { added: true })
                InputSystem.RemoveDevice(_rightControllerDevice);
            _rightControllerDevice = null;

            _simulatorLoaded = false;
            SimulatorUnloaded?.Invoke();
        }

        protected override void SetTrackingStates()
        {
            LeftControllerState.isTracked = true;
            RightControllerState.isTracked = true;
            HMDState.isTracked = true;
            LeftControllerState.trackingState = (int)(InputTrackingState.Position | InputTrackingState.Rotation);
            RightControllerState.trackingState = (int)(InputTrackingState.Position | InputTrackingState.Rotation);
            HMDState.trackingState = (int)(InputTrackingState.Position | InputTrackingState.Rotation);
        }

        /// <summary>
        /// If the simulator has not yet been loaded, creates the simulated controllers and head mounted display.
        /// </summary>
        private void AddDevices()
        {
            if (_simulatorLoaded)
                return;

            _hmdDevice = InputSystem.AddDevice<XRSimulatedHMD>();
            if (_hmdDevice == null)
            {
                Debug.LogError($"Failed to create {nameof(XRSimulatedHMD)}.");
            }

            _leftControllerDevice =
                InputSystem.AddDevice<XRSimulatedController>(
                    $"{nameof(XRSimulatedController)} - {CommonUsages.LeftHand}");
            if (_leftControllerDevice != null)
            {
                InputSystem.SetDeviceUsage(_leftControllerDevice, CommonUsages.LeftHand);
            }
            else
            {
                Debug.LogError($"Failed to create {nameof(XRSimulatedController)} for {CommonUsages.LeftHand}.", this);
            }

            _rightControllerDevice =
                InputSystem.AddDevice<XRSimulatedController>(
                    $"{nameof(XRSimulatedController)} - {CommonUsages.RightHand}");
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

        protected override void OnUpdateEnd()
        {
            if (_hmdDevice is { added: true })
                InputState.Change(_hmdDevice, HMDState);

            if (_leftControllerDevice is { added: true })
                InputState.Change(_leftControllerDevice, LeftControllerState);

            if (_rightControllerDevice is { added: true })
                InputState.Change(_rightControllerDevice, RightControllerState);
        }

        protected override void ProcessControlInput()
        {
            if (!_controls.ManipulateRightControllerButtons)
            {
                LeftControllerState = _controls.ProcessAxis2DControlInput(LeftControllerState);
                ProcessButtonControlInput(ref LeftControllerState);
            }
            else
            {
                RightControllerState = _controls.ProcessAxis2DControlInput(RightControllerState);
                ProcessButtonControlInput(ref RightControllerState);
            }
        }

        /// <summary>
        /// Process input from the user and update the state of manipulated controller device(s)
        /// related to button input controls.
        /// </summary>
        /// <param name="controllerState">The controller state that will be processed.</param>
        private void ProcessButtonControlInput(ref XRSimulatedControllerState controllerState)
        {
            controllerState.grip = _controls.GripInput ? 1f : 0f;
            controllerState.WithButton(ControllerButton.GripButton, _controls.GripInput);
            controllerState.trigger = _controls.TriggerInput ? 1f : 0f;
            controllerState.WithButton(ControllerButton.TriggerButton, _controls.TriggerInput);
            controllerState.WithButton(ControllerButton.PrimaryButton, _controls.PrimaryButtonInput);
            controllerState.WithButton(ControllerButton.SecondaryButton, _controls.SecondaryButtonInput);
            controllerState.WithButton(ControllerButton.MenuButton, _controls.MenuInput);
            controllerState.WithButton(ControllerButton.Primary2DAxisClick, _controls.Primary2DAxisClickInput);
            controllerState.WithButton(ControllerButton.Secondary2DAxisClick, _controls.Secondary2DAxisClickInput);
            controllerState.WithButton(ControllerButton.Primary2DAxisTouch, _controls.Primary2DAxisTouchInput);
            controllerState.WithButton(ControllerButton.Secondary2DAxisTouch, _controls.Secondary2DAxisTouchInput);
            controllerState.WithButton(ControllerButton.PrimaryTouch, _controls.PrimaryTouchInput);
            controllerState.WithButton(ControllerButton.SecondaryTouch, _controls.SecondaryTouchInput);
        }

        protected override void UsePoseInput(Vector3 anglesDelta)
        {
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
                    _centerEyeEuler += anglesDelta;
                    HMDState.centerEyeRotation = Quaternion.Euler(_centerEyeEuler);
                    HMDState.deviceRotation = HMDState.centerEyeRotation;
                    var matrixL = GetRelativeMatrixFromHead(ref LeftControllerState);
                    var matrixR = GetRelativeMatrixFromHead(ref RightControllerState);
                    PositionRelativeToHead(ref LeftControllerState, matrixL.GetColumn(3), matrixL.rotation);
                    PositionRelativeToHead(ref RightControllerState, matrixR.GetColumn(3), matrixR.rotation);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
        /// Positions the given simulated controller state relatively to the head.
        /// </summary>
        /// <param name="state">Desired controller state to position.</param>
        /// <param name="position">The relative position.</param>
        /// <param name="rotation">An optional rotation to add.</param>
        private void PositionRelativeToHead(ref XRSimulatedControllerState state, Vector3 position,
            Quaternion? rotation = null)
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

        protected override void ProcessDevicePositionForTarget(Space manipulationSpace,
            Quaternion inverseCameraParentRotation,
            Vector3 deltaPosition)
        {
            Quaternion deltaRotation;
            switch (_controls.ManipulationTarget)
            {
                case ManipulationTarget.RightHand:
                    deltaRotation = GetDeltaRotation(manipulationSpace, RightControllerState,
                        inverseCameraParentRotation);
                    RightControllerState.devicePosition += deltaRotation * deltaPosition;
                    break;
                case ManipulationTarget.LeftHand:
                    deltaRotation = GetDeltaRotation(manipulationSpace, LeftControllerState,
                        inverseCameraParentRotation);
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

        static Quaternion GetDeltaRotation(Space translateSpace, in XRSimulatedControllerState state,
            in Quaternion inverseCameraParentRotation)
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

        static Quaternion GetDeltaRotation(Space translateSpace, in XRSimulatedHMDState state,
            in Quaternion inverseCameraParentRotation)
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

        protected override void ResetControllers()
        {
            LeftControllerState.Reset();
            RightControllerState.Reset();

            Vector3 baseHeadOffset = Vector3.forward * 0.25f + Vector3.down * 0.15f;
            Vector3 leftOffset = Vector3.left * HALF_SHOULDER_WIDTH + baseHeadOffset;
            Vector3 rightOffset = Vector3.right * HALF_SHOULDER_WIDTH + baseHeadOffset;

            var resetScale = _controls.GetResetScale();
            _rightControllerEuler = Vector3.Scale(_rightControllerEuler, resetScale);
            _leftControllerEuler = Vector3.Scale(_leftControllerEuler, resetScale);

            PositionRelativeToHead(ref RightControllerState, rightOffset, Quaternion.Euler(_rightControllerEuler));
            PositionRelativeToHead(ref LeftControllerState, leftOffset, Quaternion.Euler(_leftControllerEuler));
        }
    }
}