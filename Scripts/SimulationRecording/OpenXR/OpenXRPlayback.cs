using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace Rhinox.XR.UnityXR.Simulator
{
    public class OpenXRPlayback : BasePlayback
    {
        [Header("Transforms")] [SerializeField]
        private Transform _headTransform;

        [SerializeField] private Transform _leftHandTransform;
        [SerializeField] private Transform _rightHandTransform;

        private PlaybackDeviceState _playbackDeviceState;
        private PlaybackInputDevice _playbackInputDevice;

        private TrackedPoseDriver _headTracker;
        private TrackedPoseDriver _leftHandTracker;
        private TrackedPoseDriver _rightHandTracker;

        protected override void Initialize()
        {
            SetupFakeInputDevice();
            GetPoseTrackers();
            PlaybackStarted += OnPlaybackStart;
            PlaybackStopped += OnPlaybackEnd;
        }

        private void SetupFakeInputDevice()
        {
            InputSystem.FlushDisconnectedDevices();
            _playbackInputDevice = InputSystem.GetDevice("PlaybackInputDevice") as PlaybackInputDevice;
            _playbackInputDevice ??= InputSystem.AddDevice<PlaybackInputDevice>("PlaybackInputDevice");
        }

        private void GetPoseTrackers()
        {
            _headTracker = _headTransform.GetComponent<TrackedPoseDriver>();
            _leftHandTracker = _leftHandTransform.GetComponent<TrackedPoseDriver>();
            _rightHandTracker = _rightHandTransform.GetComponent<TrackedPoseDriver>();
        }

        private void OnPlaybackStart()
        {
            SetPoseTrackersState(false);
        }

        private void OnPlaybackEnd()
        {
            SetPoseTrackersState(true);
        }

        private void SetPoseTrackersState(bool state)
        {
            _headTracker.enabled = state;
            _leftHandTracker.enabled = state;
            _rightHandTracker.enabled = state;
        }

        protected override void ProcessFrame(FrameData frameData)
        {
            UpdateTransforms(frameData);
            ProcessFrameInputs(frameData.FrameInputs);
            InputSystem.QueueStateEvent(_playbackInputDevice, _playbackDeviceState);
        }

        private void UpdateTransforms(FrameData frameData)
        {
            _headTransform.position = frameData.HeadPosition;
            _headTransform.rotation = frameData.HeadRotation;
            _leftHandTransform.position = frameData.LeftHandPosition;
            _leftHandTransform.rotation = frameData.LeftHandRotation;
            _rightHandTransform.position = frameData.RightHandPosition;
            _rightHandTransform.rotation = frameData.RightHandRotation;
        }

        private void ProcessFrameInputs(List<FrameInput> frameInputs)
        {
            foreach (var input in frameInputs)
                ProcessFrameInput(input);
        }

        private void ProcessFrameInput(FrameInput input)
        {
            var inputStartFloat = input.IsInputStart ? 1f : 0f;
            Vector2 vector2Value;
            float.TryParse(input.Value, out float val);

            switch (input.InputActionID)
            {
                case SimulatorInputID.Grip:
                    if (input.InputType == SimulatorInputType.Button && input.IsInputStart == false)
                    {
                        if (input.IsRightControllerInput)
                            _playbackDeviceState.RightGrip = 0;
                        else
                            _playbackDeviceState.LeftGrip = 0;
                        
                    }
                    else
                    {
                        if (input.IsRightControllerInput)
                            _playbackDeviceState.RightGrip = val;
                        else
                            _playbackDeviceState.LeftGrip = val;
                    }
                    break;
                case SimulatorInputID.PrimaryAxisClick:
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightPrimaryAxis2DClick = inputStartFloat;
                    else
                        _playbackDeviceState.LeftPrimaryAxis2DClick = inputStartFloat;
                    break;
                case SimulatorInputID.PrimaryAxisTouch:
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightPrimaryAxis2DTouch = inputStartFloat;
                    else
                        _playbackDeviceState.LeftPrimaryAxis2DTouch = inputStartFloat;
                    break;
                case SimulatorInputID.PrimaryButton:
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightPrimaryButton = inputStartFloat;
                    else
                        _playbackDeviceState.LeftPrimaryButton = inputStartFloat;
                    break;
                case SimulatorInputID.PrimaryTouch:
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightPrimaryTouch = inputStartFloat;
                    else
                        _playbackDeviceState.LeftPrimaryTouch = inputStartFloat;
                    break;
                case SimulatorInputID.SecondaryAxisClick:
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightSecondaryAxis2DClick = inputStartFloat;
                    else
                        _playbackDeviceState.LeftSecondaryAxis2DClick = inputStartFloat;
                    break;
                case SimulatorInputID.SecondaryAxisTouch:
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightSecondaryAxis2DTouch = inputStartFloat;
                    else
                        _playbackDeviceState.LeftSecondaryAxis2DTouch = inputStartFloat;
                    break;
                case SimulatorInputID.SecondaryTouch:
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightSecondaryTouch = inputStartFloat;
                    else
                        _playbackDeviceState.LeftSecondaryTouch = inputStartFloat;
                    break;
                case SimulatorInputID.Menu:
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightMenuButton = inputStartFloat;
                    else
                        _playbackDeviceState.LeftMenuButton = inputStartFloat;
                    break;
                case SimulatorInputID.Trigger:
                    if (input.InputType == SimulatorInputType.Button && input.IsInputStart == false)
                    {
                        if (input.IsRightControllerInput)
                            _playbackDeviceState.RightTrigger = 0;
                        else
                            _playbackDeviceState.LeftTrigger = 0;

                    }
                    else
                    {
                        if (input.IsRightControllerInput)
                            _playbackDeviceState.RightTrigger = val;
                        else
                            _playbackDeviceState.LeftTrigger = val;
                    }
                    break;
                case SimulatorInputID.SecondaryButton:
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightSecondaryButton = inputStartFloat;
                    else
                        _playbackDeviceState.LeftSecondaryButton = inputStartFloat;
                    break;
                case SimulatorInputID.PrimaryAxis:
                    if (!SimulatorUtils.TryParseVector2(input.Value, out vector2Value))
                        break;
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightPrimaryAxis = vector2Value;
                    else
                        _playbackDeviceState.LeftPrimaryAxis = vector2Value;
                    break;
                case SimulatorInputID.SecondaryAxis:
                    if (!SimulatorUtils.TryParseVector2(input.Value, out vector2Value))
                        break;
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightPrimaryAxis = vector2Value;
                    else
                        _playbackDeviceState.LeftPrimaryAxis = vector2Value;
                    break;
                default:
                    Debug.Log(
                        $"{nameof(OpenXRPlayback)} - ProcessFrameInput, input action *{input.InputActionID}* not found.");
                    return;
            }
        }
    }
}