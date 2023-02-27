using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Rhinox.Lightspeed;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;
using Debug = UnityEngine.Debug;

namespace Rhinox.XR.UnityXR.Simulator
{
    public class SimulationPlayback : MonoBehaviour
    {
        [Header("Input parameters")] 
        [SerializeField] private BetterXRDeviceSimulator _simulator;
        [SerializeField] private Transform _headTransform;
        [SerializeField] private Transform _leftHandTransform;
        [SerializeField] private Transform _rightHandTransform;
        
        [HideInInspector][SerializeField] public string Path;
        [HideInInspector][SerializeField] public string RecordingName = "MyRecording";
        

        [Header("Playback Controls")] 
        public InputActionReference StartPlaybackActionReference;
        public InputActionReference ReimportRecordingActionReference;
        public InputActionReference AbortPlaybackActionReference;

        [HideInInspector] public bool IsPlaying;

        private SimulationRecording _currentRecording;
        private Stopwatch _playbackStopwatch;
        private float _frameInterval = float.MaxValue;

        private PlaybackDeviceState _playbackDeviceState;
        private PlaybackInputDevice _playbackInputDevice;

        private TrackedPoseDriver _headTracker;
        private TrackedPoseDriver _leftHandTracker;
        private TrackedPoseDriver _rightHandTracker;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>
        /// </summary>
        private void Awake()
        {
            _playbackStopwatch = new Stopwatch();
            
            //-----------------------------
            // Set up fake input device
            //-----------------------------
            InputSystem.FlushDisconnectedDevices();

            var prevFake = InputSystem.GetDevice("PlaybackInputDevice");
            if (prevFake != null)
                _playbackInputDevice = prevFake as PlaybackInputDevice;

            _playbackInputDevice ??= InputSystem.AddDevice<PlaybackInputDevice>("PlaybackInputDevice");

            //-----------------------------
            // Get pose trackers
            //-----------------------------
            _headTracker = _headTransform.GetComponent<TrackedPoseDriver>();
            _leftHandTracker = _leftHandTransform.GetComponent<TrackedPoseDriver>();
            _rightHandTracker = _rightHandTransform.GetComponent<TrackedPoseDriver>();
        }

        private void OnEnable()
        {
            if (_simulator == null)
            {
                Debug.Log("_simulator has not been set,  disabling this SimulationPlayback.");
                this.gameObject.SetActive(false);
                return;
            }
            SimulatorUtils.Subscribe(StartPlaybackActionReference, StartPlayback);
            SimulatorUtils.Subscribe(ReimportRecordingActionReference, ImportRecording);
            SimulatorUtils.Subscribe(AbortPlaybackActionReference, AbortPlayback);
        }
        private void OnDisable()
        {
            SimulatorUtils.Unsubscribe(StartPlaybackActionReference, StartPlayback);
            SimulatorUtils.Subscribe(ReimportRecordingActionReference, ImportRecording);
            SimulatorUtils.Subscribe(AbortPlaybackActionReference, AbortPlayback);
        }
        
        private void ImportRecording(InputAction.CallbackContext ctx)
        {
            var serializer = new XmlSerializer(typeof(SimulationRecording));
            var stream = new FileStream(System.IO.Path.Combine(Path, $"{RecordingName}.xml"),
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
            var stream = new FileStream(System.IO.Path.Combine(Path, $"{RecordingName}.xml"),
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

        private void AbortPlayback(InputAction.CallbackContext ctx)
        {
            StopAllCoroutines();
            EndPlayBack();
        }
        
        /// <summary>
        /// Disables input in the simulator and starts the playback of the current recording.
        /// </summary>
        /// <remarks>
        /// If there is no current recording or the current recordings frames are empty, the function returns early.
        /// </remarks>
        [ContextMenu("Start Playback")]
        private void StartPlayback(InputAction.CallbackContext ctx)
        {
            if (IsPlaying)
            {
                Debug.Log("Is currently playing, please wait until playback ends or stop the current playback");
                return;
            }
            // Import a recording if none is present.
            // If no recording could be imported, abort.
            if (_currentRecording == null)
            {
                ImportRecording();
                if (_currentRecording == null)
                    return;
            }
            if (_currentRecording.AmountOfFrames == 0 || _currentRecording.Frames.Count == 0)
            {
                _currentRecording = null;
                Debug.Log("Current recording is empty, abandoning playback. Please import a new recording.");
                return;
            }

            IsPlaying = true;
            SetInput(false);
            
            Debug.Log("Started playback.");

            _playbackStopwatch.Restart();

            StartCoroutine(PlaybackRoutine());
        }

        private IEnumerator PlaybackRoutine()
        {
            //Set first frame state
            {
                var firstFrame = _currentRecording.Frames.First();
                foreach (var input in firstFrame.FrameInputs)
                    ProcessFrameInput(input);
                _headTransform.position = firstFrame.HeadPosition;
                _headTransform.rotation = firstFrame.HeadRotation;
                _leftHandTransform.position = firstFrame.LeftHandPosition;
                _leftHandTransform.rotation = firstFrame.LeftHandRotation;
                _rightHandTransform.position = firstFrame.RightHandPosition;
                _rightHandTransform.rotation = firstFrame.RightHandRotation;
                yield return new WaitForSecondsRealtime(_frameInterval);
            }

            int loopFrame = 0;
            int currentRecordedFrame = 0;
            while (loopFrame + 1 < _currentRecording.AmountOfFrames)
            {
                var currentFrame = _currentRecording.Frames[currentRecordedFrame];
                FrameData nextFrame;
                if (currentRecordedFrame + 1 != _currentRecording.Frames.Count)
                    nextFrame = _currentRecording.Frames[currentRecordedFrame + 1];
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
                    currentRecordedFrame++;
                    yield return StartCoroutine(TransformLerpCoroutine(currentFrame, nextFrame));
                }
                else
                    yield return new WaitForSecondsRealtime(_frameInterval);

                loopFrame++;
            }

            //Set final frame state
            {
                var final = _currentRecording.Frames.Last();
                foreach (var input in final.FrameInputs)
                    ProcessFrameInput(input);
                _headTransform.position = final.HeadPosition;
                _headTransform.rotation = final.HeadRotation;
                _leftHandTransform.position = final.LeftHandPosition;
                _leftHandTransform.rotation = final.LeftHandRotation;
                _rightHandTransform.position = final.RightHandPosition;
                _rightHandTransform.rotation = final.RightHandRotation;
                yield return new WaitForSecondsRealtime(_frameInterval);
            }
            
            EndPlayBack();

        }

        private IEnumerator TransformLerpCoroutine(FrameData currentFrame, FrameData nextFrame)
        {
            var timer = 0f;
            while (timer< _frameInterval)
            {
                _headTransform.position = Vector3.Lerp(currentFrame.HeadPosition, nextFrame.HeadPosition,
                    timer / _frameInterval);
                _headTransform.rotation = Quaternion.Lerp(currentFrame.HeadRotation, nextFrame.HeadRotation,
                    timer / _frameInterval);
                _leftHandTransform.position = Vector3.Lerp(currentFrame.LeftHandPosition, nextFrame.LeftHandPosition,
                    timer / _frameInterval);
                _leftHandTransform.rotation = Quaternion.Lerp(currentFrame.LeftHandRotation, nextFrame.LeftHandRotation,
                    timer / _frameInterval);
                _rightHandTransform.position = Vector3.Lerp(currentFrame.RightHandPosition, nextFrame.RightHandPosition,
                    timer / _frameInterval);
                _rightHandTransform.rotation = Quaternion.Lerp(currentFrame.RightHandRotation, nextFrame.RightHandRotation,
                    timer / _frameInterval);
                
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
            SetInput(true);
        }

        private void SetInput(bool state)
        {
            _headTracker.enabled = state;
            _leftHandTracker.enabled = state;
            _rightHandTracker.enabled = state;
            _simulator.InputEnabled = state;
        }
        
        private void ProcessFrameInput(FrameInput input)
        {
            var inputStartFloat = input.IsInputStart ? 1f : 0f;
            var vector2Value = Vector2.zero;
            
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
                        _playbackDeviceState.RightSecondaryButton = inputStartFloat;
                    else
                        _playbackDeviceState.LeftSecondaryButton = inputStartFloat;
                    break;
                case "Primary Axis 2D":
                    if (!SimulatorUtils.TryParseVector2(input.Value, out vector2Value))
                        break;
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightPrimaryAxis = vector2Value;
                    else
                        _playbackDeviceState.LeftPrimaryAxis = vector2Value;
                    break;
                case "Secondary Axis 2D":
                    if (!SimulatorUtils.TryParseVector2(input.Value, out vector2Value))
                        break;
                    if (input.IsRightControllerInput)
                        _playbackDeviceState.RightPrimaryAxis = vector2Value;
                    else
                        _playbackDeviceState.LeftPrimaryAxis = vector2Value;
                    break;
                default:
                    Debug.Log($"{nameof(SimulationPlayback)} - ProcessFrameInput, input action *{input.InputActionName}* not found.");                    
                    return;
            }
        }
        
    }
}