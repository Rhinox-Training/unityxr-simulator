using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

namespace Rhinox.XR.UnityXR.Simulator
{
    public class SimulationPlayback : MonoBehaviour
    {
        [Header("Input parameters")] 
        [SerializeField] private BetterXRDeviceSimulator _simulator;

        [Header("Output parameters")]
        [SerializeField] private string _filePath = "/SimulationRecordings/";
        [SerializeField] private string _recordingName = "NewRecording";

        [Header("Playback Controls")] 
        public InputActionReference StartPlaybackActionReference;
        public InputActionReference ReimportRecordingActionReference;

        private SimulationRecording _currentRecording;

        [HideInInspector] public bool IsPlaying;

        private Stopwatch _playbackStopwatch = new Stopwatch();
        private int _currentFrame;
        private float _frameInterval = float.MaxValue;

        private PlaybackDeviceState _playbackDeviceState;
        private PlaybackInputDevice _playbackInputDevice;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>
        /// </summary>
        private void Awake()
        {
            
            //-----------------------------
            //Set up fake input device
            //-----------------------------
            InputSystem.FlushDisconnectedDevices();

            var prevFake = InputSystem.GetDevice("PlaybackInputDevice");
            if (prevFake != null)
                _playbackInputDevice = prevFake as PlaybackInputDevice;

            _playbackInputDevice ??= InputSystem.AddDevice<PlaybackInputDevice>("PlaybackInputDevice");

        }


        private void OnEnable()
        {
            SimulatorUtils.Subscribe(StartPlaybackActionReference, StartPlayback);
            SimulatorUtils.Subscribe(ReimportRecordingActionReference, ImportRecording);
        }
        private void OnDisable()
        {
            SimulatorUtils.Unsubscribe(StartPlaybackActionReference, StartPlayback);
        }

        /// <summary>
        /// Imports a recording.
        /// </summary>
        private void ImportRecording(InputAction.CallbackContext ctx)
        {
            //Read XML
            var serializer = new XmlSerializer(typeof(SimulationRecording));
            var stream = new FileStream(Path.Combine(Application.dataPath + _filePath, $"{_recordingName}.xml"),
                FileMode.Open);
            _currentRecording = (SimulationRecording)serializer.Deserialize(stream);
            stream.Close();

            if (_currentRecording == null)
            {
                Debug.Log($"{nameof(SimulationPlayback)}, could not loud recording from XML file");
                return;
            }
            Debug.Log($"Imported recording of {_currentRecording.RecordingLength}");

            _frameInterval = 1.0f / _currentRecording.FrameRate;
        }
        private void ImportRecording()
        {
            //Read XML
            var serializer = new XmlSerializer(typeof(SimulationRecording));
            var stream = new FileStream(Path.Combine(Application.dataPath + _filePath, $"{_recordingName}.xml"),
                FileMode.Open);
            _currentRecording = (SimulationRecording)serializer.Deserialize(stream);
            stream.Close();

            if (_currentRecording == null)
            {
                Debug.Log($"{nameof(SimulationPlayback)}, could not loud recording from XML file");
                return;
            }

            Debug.Log($"Imported recording of {_currentRecording.RecordingLength}");

            _frameInterval = 1.0f / _currentRecording.FrameRate;
        }

        
        /// <summary>
        /// Disables input and the tracked pose drivers and starts the playback of the current recording.
        /// <br />
        /// If there is no current recording, the function returns early.
        /// </summary>
        [ContextMenu("Start Playback")]
        private void StartPlayback(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed)
                return;
            
            if (_currentRecording == null)
            {
                ImportRecording();
                if (_currentRecording == null)
                    return;
            }

            IsPlaying = true;
            _simulator.InputEnabled = false;

            _currentFrame = 0;
            Debug.Log("Started playback.");

            _playbackStopwatch.Restart();

            StartCoroutine(PlaybackRoutine());
        }

        private IEnumerator PlaybackRoutine()
        {
            //Set first frame
            _simulator.SetDeviceTransforms(_currentRecording.Frames.First().HeadPosition,
                _currentRecording.Frames.First().HeadRotation,
                _currentRecording.Frames.First().LeftHandPosition,
                _currentRecording.Frames.First().LeftHandRotation,
                _currentRecording.Frames.First().RightHandPosition,
                _currentRecording.Frames.First().RightHandRotation);
            foreach (var input in _currentRecording.Frames.First().FrameInputs)
                ProcessFrameInput(input);

            yield return new WaitForSecondsRealtime(_frameInterval);
            
            int loopFrame = 0;
            while (loopFrame + 1 < _currentRecording.AmountOfFrames)
            {
                var currentFrame = _currentRecording.Frames[_currentFrame];
                FrameData nextFrame;
                if (_currentFrame + 1 != _currentRecording.Frames.Count)
                    nextFrame = _currentRecording.Frames[_currentFrame + 1];
                else
                {
                    //This is used when the last remaining frames are all empty
                    yield return new WaitForSecondsRealtime(_frameInterval);
                    loopFrame++;
                    continue;
                }

                foreach (var input in currentFrame.FrameInputs)
                    ProcessFrameInput(input);
                
                InputSystem.QueueStateEvent(_playbackInputDevice, _playbackDeviceState);

                if (loopFrame == nextFrame.FrameNumber - 1)
                {
                    _currentFrame++;
                    yield return StartCoroutine(TransformLerpCoroutine(currentFrame, nextFrame));
                }
                else
                    yield return new WaitForSecondsRealtime(_frameInterval);

                
                loopFrame++;
            }

            //Set final frame
            _simulator.SetDeviceTransforms(_currentRecording.Frames.Last().HeadPosition,
                _currentRecording.Frames.Last().HeadRotation,
                _currentRecording.Frames.Last().LeftHandPosition,
                _currentRecording.Frames.Last().LeftHandRotation,
                _currentRecording.Frames.Last().RightHandPosition,
                _currentRecording.Frames.Last().RightHandRotation);

            EndPlayBack();

        }

        private IEnumerator TransformLerpCoroutine(FrameData currentFrame, FrameData nextFrame)
        {
            // (nextFrame.FrameNumber - currentFrame.FrameNumber) *
            var lerpTime = _frameInterval;
            var timer = 0f;
            while (timer< lerpTime)
            {
                _simulator.SetDeviceTransforms(
                    Vector3.Lerp(currentFrame.HeadPosition, nextFrame.HeadPosition, timer / lerpTime),
                    Quaternion.Lerp(currentFrame.HeadRotation, nextFrame.HeadRotation, timer / lerpTime),
                    Vector3.Lerp(currentFrame.LeftHandPosition, nextFrame.LeftHandPosition, timer / lerpTime),
                    Quaternion.Lerp(currentFrame.LeftHandRotation, nextFrame.LeftHandRotation, timer / lerpTime),
                    Vector3.Lerp(currentFrame.RightHandPosition, nextFrame.RightHandPosition, timer / lerpTime),
                    Quaternion.Lerp(currentFrame.RightHandRotation, nextFrame.RightHandRotation, timer / lerpTime));
                timer += Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// Stops the current playback and re-enables input.
        /// </summary>
        private void EndPlayBack()
        {
            //----------------------------
            // CALCULATE LENGTH
            //----------------------------
            _playbackStopwatch.Stop();
            RecordingTime temp;
            temp.Milliseconds = _playbackStopwatch.Elapsed.Milliseconds;
            temp.Seconds = _playbackStopwatch.Elapsed.Seconds;
            temp.Minutes = _playbackStopwatch.Elapsed.Minutes;
            Debug.Log($"Ended playback of {temp}");
            
            IsPlaying = false;
            _simulator.InputEnabled = true;
        }

        // private void Update()
        // {
        //     
        //     if (!IsPlaying)
        //         return;
        //
        //     _timer += Time.unscaledDeltaTime;
        //     if(_timer>= _frameInterval)
        //     {
        //         _timer = 0;
        //         
        //         var frame = _currentRecording.Frames[_currentFrame];
        //
        //         _hmdTransform.position = frame.HeadPosition;
        //         _hmdTransform.rotation = frame.HeadRotation;
        //
        //         _leftHandTransform.position = frame.LeftHandPosition;
        //         _leftHandTransform.rotation = frame.LeftHandRotation;
        //
        //         _rightHandTransform.position = frame.RightHandPosition;
        //         _rightHandTransform.rotation = frame.RightHandRotation;
        //
        //         foreach (var input in frame.FrameInputs)
        //             ProcessFrameInput(input);
        //         _currentFrame++;
        //         
        //         InputSystem.QueueStateEvent(_playbackInputDevice, _playbackDeviceState);
        //
        //         if (_currentFrame >= _currentRecording.AmountOfFrames)
        //             EndPlayBack();
        //
        //
        //     }
        // }

        private void ProcessFrameInput(FrameInput input)
        {
            var inputStartFloat = input.IsInputStart ? 1f : 0f;
            switch (input.InputActionName)
            {
                case "grip":
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightGrip = inputStartFloat;
                    else
                        _playbackDeviceState.LeftGrip = inputStartFloat;
                    break;
                case "primary axis 2D click":
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightPrimaryAxis2DClick = inputStartFloat;
                    else
                        _playbackDeviceState.LeftPrimaryAxis2DClick = inputStartFloat;
                    break;
                case "primary axis 2D touch":
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightPrimaryAxis2DTouch = inputStartFloat;
                    else
                        _playbackDeviceState.LeftPrimaryAxis2DTouch = inputStartFloat;
                    break;
                case "primary button":
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightPrimaryButton = inputStartFloat;
                    else
                        _playbackDeviceState.LeftPrimaryButton = inputStartFloat;
                    break;
                case "primary touch":
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightPrimaryTouch = inputStartFloat;
                    else
                        _playbackDeviceState.LeftPrimaryTouch = inputStartFloat;
                    break;
                case "secondary axis 2D click":
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightSecondaryAxis2DClick = inputStartFloat;
                    else
                        _playbackDeviceState.LeftSecondaryAxis2DClick = inputStartFloat;
                    break;
                case "secondary axis 2D touch":
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightSecondaryAxis2DTouch = inputStartFloat;
                    else
                        _playbackDeviceState.LeftSecondaryAxis2DTouch = inputStartFloat;
                    break;
                case "secondary touch":
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightSecondaryTouch = inputStartFloat;
                    else
                        _playbackDeviceState.LeftSecondaryTouch = inputStartFloat;
                    break;
                case "menu button":
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightMenuButton = inputStartFloat;
                    else
                        _playbackDeviceState.LeftMenuButton = inputStartFloat;
                    break;
                case "trigger":
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightTrigger = inputStartFloat;
                    else
                        _playbackDeviceState.LeftTrigger = inputStartFloat;
                    break;
                case "secondary button":
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.LeftSecondaryButton = inputStartFloat;
                    else
                        _playbackDeviceState.LeftPrimaryButton = inputStartFloat;
                    break;
                default:
                    Debug.Log($"{nameof(SimulationPlayback)} - ProcessFrameInput, input action *{input.InputActionName}* not found.");                    
                    return;
            }
        }
        
    }
}