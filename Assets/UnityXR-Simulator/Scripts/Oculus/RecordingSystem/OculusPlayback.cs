using System;
using UnityEngine;

namespace Rhinox.XR.UnityXR.Simulator.Oculus
{
    /// <summary>
    /// Handles playback of Oculus device inputs for simulation.
    /// </summary>
    public class OculusPlayback : BasePlayback
    {
        private OculusDeviceSimulator _oculusDeviceSimulator;
        private OVRPlugin.ControllerState5 _playbackState = new OVRPlugin.ControllerState5();

        /// <summary>
        /// Initializes the OculusPlayback.
        /// </summary>
        protected override void Initialize()
        {
            _oculusDeviceSimulator = Simulator as OculusDeviceSimulator;

            if (_oculusDeviceSimulator == null)
            {
                Debug.LogWarning("_oculusDeviceSimulator was not found, disabling this object");
                gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Processes a frame of recorded data.
        /// </summary>
        /// <param name="frameData">The frame data to process.</param>
        protected override void ProcessFrame(FrameData frameData)
        {
            // Set the rig transforms
            _oculusDeviceSimulator.SetRigTransforms(frameData.HeadPosition, frameData.HeadRotation,
                frameData.LeftHandPosition, frameData.LeftHandRotation,
                frameData.RightHandPosition, frameData.RightHandRotation);

            _oculusDeviceSimulator.PushControllerState(_playbackState);

            // Process input
            foreach (FrameInput input in frameData.FrameInputs)
            {
                ProcessFrameInput(input);
            }
        }

        private void ProcessFrameInput(FrameInput input)
        {
            foreach (SimulatorInputID inputID in Enum.GetValues(typeof(SimulatorInputID)))
            {
                if (input.InputActionID == inputID)
                {
                    switch (input.InputType)
                    {
                        case SimulatorInputType.Button:
                            ProcessButton(input);
                            break;
                        case SimulatorInputType.Axis2D:
                            ProcessAxis2D(input);
                            break;
                        case SimulatorInputType.Axis1D:
                            ProcessAxis1D(input);
                            break;
                        default:
                            return;
                    }
                }
            }
        }

        private void ProcessButton(FrameInput input)
        {
            // Get the OVR Button
            if (!input.InputActionID.TryToOVRButton(input.IsRightControllerInput, out var button))
                return;

            // Put it in the playback state
            if (input.IsInputStart)
                _playbackState.Buttons |= (uint)button;
            else
                _playbackState.Buttons &= ~(uint)button;
        }

        private void ProcessAxis1D(FrameInput input)
        {
            // Get the OVR Axis1D
            if (!input.InputActionID.TryToOVRAxis1D(input.IsRightControllerInput, out var axis))
                return;

            if (!float.TryParse(input.Value, out var result))
                return;

            switch (axis)
            {
                case OVRInput.Axis1D.PrimaryIndexTrigger:
                    _playbackState.LIndexTrigger = result;
                    break;
                case OVRInput.Axis1D.PrimaryHandTrigger:
                    _playbackState.LHandTrigger = result;
                    break;
                case OVRInput.Axis1D.SecondaryIndexTrigger:
                    _playbackState.RIndexTrigger = result;
                    break;
                case OVRInput.Axis1D.SecondaryHandTrigger:
                    _playbackState.RHandTrigger = result;
                    break;
                default:
                    Debug.Log($"Axis1D {axis} not implemented");
                    return;
            }
        }

        private void ProcessAxis2D(FrameInput input)
        {
            // Get the OVR Axis2D
            if (!input.InputActionID.TryToOVRAxis2D(input.IsRightControllerInput, out var axis))
                return;

            if (!SimulatorUtils.TryParseVector2(input.Value, out var resultUnityVector2))
                return;

            OVRPlugin.Vector2f resultOvrVector2 = new OVRPlugin.Vector2f
            {
                x = resultUnityVector2.x,
                y = resultUnityVector2.y
            };

            switch (axis)
            {
                case OVRInput.Axis2D.PrimaryThumbstick:
                    _playbackState.LThumbstick = resultOvrVector2;
                    break;
                case OVRInput.Axis2D.PrimaryTouchpad:
                    _playbackState.LTouchpad = resultOvrVector2;
                    break;
                case OVRInput.Axis2D.SecondaryThumbstick:
                    _playbackState.RThumbstick = resultOvrVector2;
                    break;
                case OVRInput.Axis2D.SecondaryTouchpad:
                    _playbackState.RTouchpad = resultOvrVector2;
                    break;
            }
        }
    }
}
