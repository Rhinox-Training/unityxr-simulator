using System;
using UnityEngine;

namespace Rhinox.XR.UnityXR.Simulator
{
    public abstract class BaseSimulator : MonoBehaviour
    {
        /// <summary>
        /// The Transform that contains the Camera. This is usually the \"Head\" of XR Origins. Automatically set to the first enabled camera tagged MainCamera if unset
        /// </summary>
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

        /// <summary>
        /// Whether simulator input is currently enabled and processed.
        /// </summary>
        public bool InputEnabled { get; set; } = true;

        protected XRDeviceSimulatorControls _controls;

        public bool IsRightTargeted => _controls == null || _controls.ManipulateRightControllerButtons;

        [HideInInspector] public bool IsInputEnabled { get; set; } = true;
        
        protected Vector3 _leftControllerEuler;
        protected Vector3 _rightControllerEuler;
        protected Vector3 _centerEyeEuler;
        protected const float HALF_SHOULDER_WIDTH = 0.18f;
        
        protected virtual void Awake()
        {
            _controls = GetComponent<XRDeviceSimulatorControls>();
            Initialize();
        }

        protected virtual void Initialize()
        {

        }

        protected virtual void OnEnable()
        {
            TrySetCamera();
        }

        /// <summary>
        /// Tries to set the camera to the main camera, if no camera has been set yet.
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

        protected virtual void Update()
        {
            if (_controls.DesiredCursorLockMode != Cursor.lockState)
                Cursor.lockState = _controls.DesiredCursorLockMode;

            if (IsInputEnabled)
            {
                ProcessPoseInput();
                ProcessControlInput();
            }

            if (_controls.ResetInputTriggered())
                ResetControllers();
            
            OnUpdateEnd();
        }

        protected virtual void OnUpdateEnd()
        {
            
        }

        /// <summary>
        /// Process input from the user and update the state of manipulated controller device(s)
        /// </summary>
        protected abstract void ProcessControlInput();

        /// <summary>
        /// Process input from the user and update the state of manipulated device(s)
        /// related to position and rotation.
        /// </summary>
        protected void ProcessPoseInput()
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
                EnumHelper.GetAxes(_controls.KeyboardTranslateSpace, CameraTransform, out var right, out var up,
                    out var forward);

                // Keyboard translation
                var deltaPosition =
                    right * (_controls.ScaledKeyboardTranslateX * Time.deltaTime) +
                    up * (_controls.ScaledKeyboardTranslateY * Time.deltaTime) +
                    forward * (_controls.ScaledKeyboardTranslateZ * Time.deltaTime);
                
                ProcessDevicePositionForTarget(_controls.KeyboardTranslateSpace, inverseCameraParentRotation,
                    deltaPosition);
            }


            // Mouse rotation
            var scaledMouseDeltaInput = _controls.GetScaledMouseRotateInput();

            Vector3 anglesDelta = _controls.GetScaledRotationDelta(scaledMouseDeltaInput);

            UsePoseInput(anglesDelta);

            if (_controls.ResetInputTriggered())
                ResetControllers();
        }

        protected abstract void UsePoseInput(Vector3 anglesDelta);
        
        protected virtual void SetTrackingStates()
        {
        }

        /// <summary>
        /// Translates and rotates the current targets.
        /// </summary>
        /// <param name="manipulationSpace">The desired space for translation.</param>
        /// <param name="inverseCameraParentRotation">The inverse of the camera parent rotation.</param>
        /// <param name="deltaPosition">The translation needed.</param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        protected abstract void ProcessDevicePositionForTarget(Space manipulationSpace,
            Quaternion inverseCameraParentRotation,
            Vector3 deltaPosition);

        /// <summary>
        /// Resets the state and transform of both controllers. Both controllers are positioned relatively to the head/HMD.
        /// </summary>
        protected abstract void ResetControllers();
    }
}