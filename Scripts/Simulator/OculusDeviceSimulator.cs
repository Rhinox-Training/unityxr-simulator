using System;
using System.Collections;
using System.Reflection;
using UnityEngine;
using UnityEngine.Assertions;

namespace Rhinox.XR.UnityXR.Simulator
{
    public class OculusDeviceSimulator : BaseSimulator
    {
        private object _controller;
        private FieldInfo _controllerFieldInfo;
        private MethodInfo _updateMethod;

        private OVRCameraRig _rig;
        public OVRCameraRig RIG => _rig;

        public bool IsOculusConnected { get; }

        protected override void Initialize()
        {
            _rig = FindObjectOfType<OVRCameraRig>();
            if (_rig == null)
                Debug.LogError($"Failed to find {nameof(_controls)}.");
        }

        private void Start()
        {
            TrySetActiveControllerType();
        }

        private void TrySetActiveControllerType()
        {
            // Retrieve the activeControllerType field and set its value to OVRInput.Controller.Touch
            var activeControllerType = typeof(OVRInput).GetField("activeControllerType",
                BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
            if (activeControllerType != null)
            {
                //counts for both LTouch and RTouch
                activeControllerType.SetValue(null, OVRInput.Controller.Touch);
            }

            // Retrieve the controllers field and get the second controller in the list
            var controllerList = typeof(OVRInput).GetField("controllers",
                BindingFlags.Static | BindingFlags.Instance | BindingFlags.NonPublic);
            if (controllerList != null)
            {
                var list = (IList)controllerList.GetValue(null);
                _controller = list[1];
            }

            // Get the type of the _controller object
            var controllerType = _controller.GetType();

            // Retrieve the currentState field and Update method from the controller's type
            _controllerFieldInfo = controllerType.GetField("currentState", BindingFlags.Instance | BindingFlags.Public);
            _updateMethod = controllerType.GetMethod("Update", BindingFlags.Instance | BindingFlags.Public);
        }

        protected override void Update()
        {
            if (!OVRPlugin.initialized)
                base.Update();
        }


        protected override void ProcessControlInput()
        {
            OVRPlugin.ControllerState5 state = new OVRPlugin.ControllerState5();

            //controls uses the same inputActionManager as UnityXR, but the naming of OVR is different
            //below webpage gives a nice table of translating the UnityXR to Oculus input Controls
            //https://docs.unity3d.com/Manual/xr_input.html
            if (_controls.ManipulateRightControllerButtons)
            {
                #region Buttons

                if (_controls.GripInput)
                    state.Buttons |= (uint)OVRInput.RawButton.RHandTrigger;
                if (_controls.TriggerInput)
                    state.Buttons |= (uint)OVRInput.RawButton.RIndexTrigger;
                if (_controls.PrimaryButtonInput)
                    state.Buttons |= (uint)OVRInput.RawButton.B;
                if (_controls.SecondaryButtonInput)
                    state.Buttons |= (uint)OVRInput.RawButton.A;
                if (_controls.Primary2DAxisClickInput)
                    state.Buttons |= (uint)OVRInput.RawButton.RThumbstick;
                if (_controls.Primary2DAxisTouchInput)
                    state.Buttons |= (uint)OVRInput.RawTouch.RThumbstick;
                if (_controls.PrimaryTouchInput)
                    state.Touches |= (uint)OVRInput.RawTouch.B;
                if (_controls.SecondaryTouchInput)
                    state.Touches |= (uint)OVRInput.RawTouch.A;

                #endregion
            }
            else
            {
                #region Buttons

                if (_controls.GripInput)
                    state.Buttons |= (uint)OVRInput.RawButton.LHandTrigger;
                if (_controls.TriggerInput)
                    state.Buttons |= (uint)OVRInput.RawButton.LIndexTrigger;
                if (_controls.PrimaryButtonInput)
                    state.Buttons |= (uint)OVRInput.RawButton.Y;
                if (_controls.SecondaryButtonInput)
                    state.Buttons |= (uint)OVRInput.RawButton.X;
                if (_controls.MenuInput)
                    state.Buttons |= (uint)OVRInput.RawButton.Start;
                if (_controls.Primary2DAxisClickInput)
                    state.Buttons |= (uint)OVRInput.RawButton.LThumbstick;
                if (_controls.Primary2DAxisTouchInput)
                    state.Buttons |= (uint)OVRInput.RawTouch.LThumbstick;
                if (_controls.PrimaryTouchInput)
                    state.Touches |= (uint)OVRInput.RawTouch.Y;
                if (_controls.SecondaryTouchInput)
                    state.Touches |= (uint)OVRInput.RawTouch.X;

                #endregion
            }

            state = _controls.ProcessAxis2DControlInput(state);

            _updateMethod.Invoke(_controller, null);
            _controllerFieldInfo.SetValue(_controller, state);
        }

        protected override void UsePoseInput(Vector3 anglesDelta)
        {
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

                    PositionRelativeToHead(out Vector3 controllerPos, out Quaternion controllerRot,
                        matrixL.GetColumn(3),
                        matrixL.rotation);
                    _rig.leftHandAnchor.localPosition = controllerPos;
                    _rig.leftHandAnchor.localRotation = controllerRot;
                    PositionRelativeToHead(out controllerPos, out controllerRot, matrixR.GetColumn(3),
                        matrixR.rotation);
                    _rig.rightHandAnchor.localPosition = controllerPos;
                    _rig.rightHandAnchor.localRotation = controllerRot;

                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        private Matrix4x4 GetRelativeMatrixFromHead(in Transform controller)
        {
            var controllerTrans = Matrix4x4.TRS(controller.localPosition, controller.localRotation, Vector3.one);
            var headTrans = Matrix4x4.TRS(_rig.centerEyeAnchor.localPosition, _rig.centerEyeAnchor.localRotation,
                Vector3.one);
            var matrix = headTrans.inverse * controllerTrans;
            return matrix;
        }

        private void PositionRelativeToHead(out Vector3 controllerPos, out Quaternion controllerRot, Vector3 position,
            Quaternion? rotation = null)
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

        protected override void ProcessDevicePositionForTarget(Space manipulationSpace,
            Quaternion inverseCameraParentRotation,
            Vector3 deltaPosition)
        {
            Quaternion deltaRotation;
            switch (_controls.ManipulationTarget)
            {
                case ManipulationTarget.RightHand:
                    deltaRotation = GetDeltaRotation(manipulationSpace, _rig.rightHandAnchor.localRotation,
                        inverseCameraParentRotation);
                    _rig.rightHandAnchor.localPosition += deltaRotation * deltaPosition;
                    break;

                case ManipulationTarget.LeftHand:
                    deltaRotation = GetDeltaRotation(manipulationSpace, _rig.leftHandAnchor.localRotation,
                        inverseCameraParentRotation);
                    _rig.leftHandAnchor.localPosition += deltaRotation * deltaPosition;
                    break;

                case ManipulationTarget.Head:
                    deltaRotation = GetDeltaRotation(manipulationSpace, _rig, inverseCameraParentRotation);
                    _rig.centerEyeAnchor.localPosition += deltaRotation * deltaPosition;
                    break;

                case ManipulationTarget.All:
                    var localPosition = _rig.centerEyeAnchor.localPosition;
                    Vector3 relativeRightPosition =
                        _rig.rightHandAnchor.localPosition - localPosition;
                    Vector3 relativeLeftPosition =
                        _rig.leftHandAnchor.localPosition - localPosition;

                    deltaRotation = GetDeltaRotation(manipulationSpace, _rig, inverseCameraParentRotation);
                    localPosition += deltaRotation * deltaPosition;
                    _rig.centerEyeAnchor.localPosition = localPosition;

                    _rig.rightHandAnchor.localPosition = localPosition + relativeRightPosition;
                    _rig.leftHandAnchor.localPosition = localPosition + relativeLeftPosition;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        static Quaternion GetDeltaRotation(Space translateSpace, in Quaternion controllerOrientation,
            in Quaternion inverseCameraParentRotation)
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

        static Quaternion GetDeltaRotation(Space translateSpace, in OVRCameraRig cameraRig,
            in Quaternion inverseCameraParentRotation)
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

        protected override void ResetControllers()
        {
            Vector3 baseHeadOffset = Vector3.forward * 0.25f + Vector3.down * 0.15f;
            Vector3 leftOffset = Vector3.left * HALF_SHOULDER_WIDTH + baseHeadOffset;
            Vector3 rightOffset = Vector3.right * HALF_SHOULDER_WIDTH + baseHeadOffset;

            var resetScale = _controls.GetResetScale();
            _rightControllerEuler = Vector3.Scale(_rightControllerEuler, resetScale);
            _leftControllerEuler = Vector3.Scale(_leftControllerEuler, resetScale);

            PositionRelativeToHead(out Vector3 controllerPos, out Quaternion controllerRot, leftOffset,
                Quaternion.Euler(_leftControllerEuler));
            _rig.leftHandAnchor.localPosition = controllerPos;
            _rig.leftHandAnchor.localRotation = controllerRot;
            PositionRelativeToHead(out controllerPos, out controllerRot, rightOffset,
                Quaternion.Euler(_rightControllerEuler));
            _rig.rightHandAnchor.localPosition = controllerPos;
            _rig.rightHandAnchor.localRotation = controllerRot;
        }
    }
}