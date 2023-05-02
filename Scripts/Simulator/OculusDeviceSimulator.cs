using System;
using System.Reflection;

using UnityEngine;
using UnityEngine.Assertions;
using static OVRPlugin;
using System.Collections;
using static OVRInput;

namespace Rhinox.XR.UnityXR.Simulator
{
    public class OculusDeviceSimulator : MonoBehaviour
    {
        private XRDeviceSimulatorControls _controls;
        object _controller;
        FieldInfo _controllerFieldInfo;
        MethodInfo _updateMethod;

        private OVRCameraRig _rig;
        public OVRCameraRig RIG => _rig;

        public Axis2DTargets Axis2DTargets { get; set; } = Axis2DTargets.Position;

        private Vector3 _leftControllerEuler;
        private Vector3 _rightControllerEuler;
        private Vector3 _centerEyeEuler;

        private bool _isOculusConnected;
        public bool IsOculusConnected => _isOculusConnected;
        public bool IsRightTargeted => _controls == null || _controls.ManipulateRightControllerButtons;

        [HideInInspector]
        public bool IsInputEnabled
        {
            get;
            set;
        } = true;

        [Tooltip("The Transform that contains the Camera. This is usually the \"Head\" of XR Origins. Automatically set to the first enabled camera tagged MainCamera if unset.")]
        public Transform CameraTransform;
        [SerializeField]
        public float HalfShoulderWidth = 0.18f;

        #region Setup
        protected virtual void Awake()
        {
            _controls = GetComponent<XRDeviceSimulatorControls>();
            if (_controls == null)
                Debug.LogError($"Failed to get {nameof(_controls)}.");

            _rig = FindObjectOfType<OVRCameraRig>();
            if (_rig == null)
                Debug.LogError($"Failed to find {nameof(_controls)}.");
        }

        protected virtual void OnEnable()
        {
            TrySetCamera();
        }

        private void Start()
        {
            TrySetActiveControllerType();
            ResetControllers();
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

        private void TrySetActiveControllerType()
        {
            var activeControllerType = typeof(OVRInput).GetField("activeControllerType", BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
            if (activeControllerType != null)
            {
                //counts for bouth LTouch and RTouch
                activeControllerType.SetValue(null, OVRInput.Controller.Touch);
            }

            var controllerList = typeof(OVRInput).GetField("controllers", BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
            if (controllerList != null)
            {
                var list = (IList)controllerList.GetValue(null);
                _controller = list[1];
            }

            var controllerType = _controller.GetType();
            _controllerFieldInfo = controllerType.GetField("currentState", BindingFlags.Instance | BindingFlags.Public);

            _updateMethod = controllerType.GetMethod("Update", BindingFlags.Instance | BindingFlags.Public);
        }
        #endregion

        protected virtual void Update()
        {
            if (OVRPlugin.initialized)
                return;

            if (_controls.DesiredCursorLockMode != Cursor.lockState)
                Cursor.lockState = _controls.DesiredCursorLockMode;

            if (IsInputEnabled)
            {
                ProcessPoseInput();
                ProcessControlInput();
            }
        }

        #region Movement
        protected virtual void ProcessPoseInput()
        {
            if (CameraTransform == null)
                return;

            var cameraParent = CameraTransform.parent;
            var cameraParentRotation = cameraParent != null ? cameraParent.rotation : Quaternion.identity;
            var inverseCameraParentRotation = Quaternion.Inverse(cameraParentRotation);

            //movement
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

            //Mouse rotation
            var scaledMouseDeltaInput = _controls.GetScaledMouseRotateInput();
            Vector3 anglesDelta = _controls.GetScaledRotationDelta(scaledMouseDeltaInput);

            switch (_controls.ManipulationTarget)
            {
                case ManipulationTarget.RightHand:
                    _rightControllerEuler += anglesDelta;
                    _rig.rightHandAnchor.localRotation = Quaternion.Euler(_rightControllerEuler);
                    break;
                case ManipulationTarget.LeftHand:
                    _leftControllerEuler += anglesDelta;
                    _rig.leftHandAnchor.localRotation = Quaternion.Euler(_leftControllerEuler);
                    break;
                case ManipulationTarget.Head:
                    _centerEyeEuler += anglesDelta;
                    _rig.centerEyeAnchor.localRotation = Quaternion.Euler(_centerEyeEuler);
                    break;
                case ManipulationTarget.All:
                    var matrixL = GetRelativeMatrixFromHead(_rig.leftHandAnchor);
                    var matrixR = GetRelativeMatrixFromHead(_rig.rightHandAnchor);
                    _centerEyeEuler += anglesDelta;
                    _rig.centerEyeAnchor.localRotation = Quaternion.Euler(_centerEyeEuler);

                    Vector3 controllerPos;
                    Quaternion controllerRot;
                    PositionRelativeToHead(out controllerPos, out controllerRot, matrixL.GetColumn(3), matrixL.rotation);
                    _rig.leftHandAnchor.localPosition = controllerPos;
                    _rig.leftHandAnchor.localRotation = controllerRot;
                    PositionRelativeToHead(out controllerPos, out controllerRot, matrixR.GetColumn(3), matrixR.rotation);
                    _rig.rightHandAnchor.localPosition = controllerPos;
                    _rig.rightHandAnchor.localRotation = controllerRot;

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (_controls.ResetInputTriggered())
                ResetControllers();

        }

        private void ProcessDevicePositionForTarget(Space manipulationSpace, Quaternion inverseCameraParentRotation, Vector3 deltaPosition)
        {
            Quaternion deltaRotation = Quaternion.identity;
            switch (_controls.ManipulationTarget)
            {
                case ManipulationTarget.RightHand:
                    deltaRotation = GetDeltaRotation(manipulationSpace, _rig.rightHandAnchor.localRotation, inverseCameraParentRotation);
                    _rig.rightHandAnchor.localPosition += deltaRotation * deltaPosition;
                    break;

                case ManipulationTarget.LeftHand:
                    deltaRotation = GetDeltaRotation(manipulationSpace, _rig.leftHandAnchor.localRotation, inverseCameraParentRotation);
                    _rig.leftHandAnchor.localPosition += deltaRotation * deltaPosition;
                    break;

                case ManipulationTarget.Head:
                    deltaRotation = GetDeltaRotation(manipulationSpace, _rig, inverseCameraParentRotation);
                    _rig.centerEyeAnchor.localPosition += deltaRotation * deltaPosition;
                    break;

                case ManipulationTarget.All:
                    Vector3 relativeRightPosition = _rig.rightHandAnchor.localPosition - _rig.centerEyeAnchor.localPosition;
                    Vector3 relativeLeftPosition = _rig.leftHandAnchor.localPosition - _rig.centerEyeAnchor.localPosition;

                    deltaRotation = GetDeltaRotation(manipulationSpace, _rig, inverseCameraParentRotation);
                    _rig.centerEyeAnchor.localPosition += deltaRotation * deltaPosition;

                    _rig.rightHandAnchor.localPosition = _rig.centerEyeAnchor.localPosition + relativeRightPosition;
                    _rig.leftHandAnchor.localPosition = _rig.centerEyeAnchor.localPosition + relativeLeftPosition;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        static Quaternion GetDeltaRotation(Space translateSpace, in Quaternion controllerOrientation, in Quaternion inverseCameraParentRotation)
        {
            switch (translateSpace)
            {
                case Space.Local:
                    return controllerOrientation * inverseCameraParentRotation;
                case Space.Parent:
                    return Quaternion.identity;
                case Space.Screen:
                    return inverseCameraParentRotation;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(translateSpace)}={translateSpace}.");
                    return Quaternion.identity;
            }
        }

        static Quaternion GetDeltaRotation(Space translateSpace, in OVRCameraRig cameraRig, in Quaternion inverseCameraParentRotation)
        {
            switch (translateSpace)
            {
                case Space.Local:
                    return cameraRig.centerEyeAnchor.localRotation * inverseCameraParentRotation;
                case Space.Parent:
                    return Quaternion.identity;
                case Space.Screen:
                    return inverseCameraParentRotation;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(translateSpace)}={translateSpace}.");
                    return Quaternion.identity;
            }
        }

        private Matrix4x4 GetRelativeMatrixFromHead(in Transform controller)
        {
            var controllerTrans = Matrix4x4.TRS(controller.localPosition, controller.localRotation, Vector3.one);
            var headTrans = Matrix4x4.TRS(_rig.centerEyeAnchor.localPosition, _rig.centerEyeAnchor.localRotation, Vector3.one);
            var matrix = headTrans.inverse * controllerTrans;
            return matrix;
        }

        private void PositionRelativeToHead(out Vector3 controllerPos, out Quaternion controllerRot, Vector3 position, Quaternion? rotation = null)
        {
            Vector3 headPos = _rig.centerEyeAnchor.localPosition;
            Quaternion rot = _rig.centerEyeAnchor.localRotation;
            var headMatrix = Matrix4x4.TRS(headPos, rot, Vector3.one);

            Vector3 realPos = headMatrix * new Vector4(position.x, position.y, position.z, 1);
            controllerPos = realPos;
            if (rotation.HasValue)
            {
                Quaternion realRot = rot * rotation.Value;
                controllerRot = realRot;
            }
            else
                controllerRot = Quaternion.identity;
        }
        #endregion

        #region Controllers
        protected virtual void ProcessControlInput()
        {
            OVRPlugin.ControllerState5 state = new ControllerState5();

            //controlls uses the sameinputActionManager as UnityXR, but the naming of OVR is different
            //below webpage gives a nice table of translating the UnityXR to Oculus input Controls
            //https://docs.unity3d.com/Manual/xr_input.html
            if (_controls.ManipulateRightControllerButtons)
            {
                #region Buttons
                if (_controls.GripInput)
                    state.Buttons |= (uint)RawButton.RHandTrigger;
                if (_controls.TriggerInput)
                    state.Buttons |= (uint)RawButton.RIndexTrigger;
                if (_controls.PrimaryButtonInput)
                    state.Buttons |= (uint)RawButton.B;
                if (_controls.SecondaryButtonInput)
                    state.Buttons |= (uint)RawButton.A;
                if (_controls.Primary2DAxisClickInput)
                    state.Buttons |= (uint)RawButton.RThumbstick;
                if (_controls.Primary2DAxisTouchInput)
                    state.Buttons |= (uint)RawTouch.RThumbstick;
                if (_controls.PrimaryTouchInput)
                    state.Touches |= (uint)RawTouch.B;
                if (_controls.SecondaryTouchInput)
                    state.Touches |= (uint)RawTouch.A;
                #endregion
            }
            else
            {
                #region Buttons
                if (_controls.GripInput)
                    state.Buttons |= (uint)RawButton.LHandTrigger;
                if (_controls.TriggerInput)
                    state.Buttons |= (uint)RawButton.LIndexTrigger;
                if (_controls.PrimaryButtonInput)
                    state.Buttons |= (uint)RawButton.Y;
                if (_controls.SecondaryButtonInput)
                    state.Buttons |= (uint)RawButton.X;
                if (_controls.MenuInput)
                    state.Buttons |= (uint)RawButton.Start;
                if (_controls.Primary2DAxisClickInput)
                    state.Buttons |= (uint)RawButton.LThumbstick;
                if (_controls.Primary2DAxisTouchInput)
                    state.Buttons |= (uint)RawTouch.LThumbstick;
                if (_controls.PrimaryTouchInput)
                    state.Touches |= (uint)RawTouch.Y;
                if (_controls.SecondaryTouchInput)
                    state.Touches |= (uint)RawTouch.X;
                #endregion
            }

            state = _controls.ProcessAxis2DControlInput(state);

            _updateMethod.Invoke(_controller, null);
            _controllerFieldInfo.SetValue(_controller, state);
        }

        private void ResetControllers()
        {
            Vector3 baseHeadOffset = Vector3.forward * 0.25f + Vector3.down * 0.15f;
            Vector3 leftOffset = Vector3.left * HalfShoulderWidth + baseHeadOffset;
            Vector3 rightOffset = Vector3.right * HalfShoulderWidth + baseHeadOffset;

            var resetScale = _controls.GetResetScale();
            _rightControllerEuler = Vector3.Scale(_rightControllerEuler, resetScale);
            _leftControllerEuler = Vector3.Scale(_leftControllerEuler, resetScale);

            Vector3 controllerPos;
            Quaternion controllerRot;
            PositionRelativeToHead(out controllerPos, out controllerRot, leftOffset, Quaternion.Euler(_leftControllerEuler));
            _rig.leftHandAnchor.localPosition = controllerPos;
            _rig.leftHandAnchor.localRotation = controllerRot;
            PositionRelativeToHead(out controllerPos, out controllerRot, rightOffset, Quaternion.Euler(_rightControllerEuler));
            _rig.rightHandAnchor.localPosition = controllerPos;
            _rig.rightHandAnchor.localRotation = controllerRot;
        }
        #endregion

        #region ExternalUseFunction

        public void SetRigTransforms(Vector3 headPos, Quaternion headRot, Vector3 leftHandPos, Quaternion leftHandRot, Vector3 rightHandPos, Quaternion rightHandRot)
        {
            _rig.centerEyeAnchor.position = headPos;
            _rig.centerEyeAnchor.rotation = headRot;
            _rig.leftHandAnchor.position = leftHandPos;
            _rig.leftHandAnchor.rotation = leftHandRot;
            _rig.rightHandAnchor.position = rightHandPos;
            _rig.rightHandAnchor.rotation = rightHandRot;
        }

        public void PushControllerState(ControllerState5 state)
        {
            _updateMethod.Invoke(_controller, null);
            _controllerFieldInfo.SetValue(_controller, state);
        }

        #endregion

    }
}