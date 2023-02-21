using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.XR;

namespace Rhinox.XR.UnityXR.Simulator
{
    public class SimulationPlayback : MonoBehaviour
    {
        [Header("Device Transforms")] 
        [SerializeField] private Transform _hmdTransform;
        [SerializeField] private Transform _leftHandTransform;
        [SerializeField] private Transform _rightHandTransform;

        [Header("Input parameters")] [SerializeField]
        private string FilePath = "/SimulationRecordings/";

        [SerializeField] private string _recordingName = "NewRecording";

        [Header("Playback Controls")] 
        public InputActionReference StartPlaybackActionReference;
        private SimulationRecording _currentRecording;

        [HideInInspector] public bool IsPlaying;

        private int _currentFrame;
        
        private float _frameInterval = float.MaxValue;
        private float _timer = 0;

        private TrackedPoseDriver _headTrackedPoseDriver;
        private TrackedPoseDriver _leftHandTrackedPoseDriver;
        private TrackedPoseDriver _rightHandTrackedPoseDriver;

        private PlaybackDeviceState _playbackDeviceState;
        private PlaybackInputDevice _playbackInputDevice;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>
        /// </summary>
        private void Awake()
        {
            //Get the pose drivers 
            _headTrackedPoseDriver = _hmdTransform.GetComponent<TrackedPoseDriver>();
            _leftHandTrackedPoseDriver = _leftHandTransform.GetComponent<TrackedPoseDriver>();
            _rightHandTrackedPoseDriver = _rightHandTransform.GetComponent<TrackedPoseDriver>();
            
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
            SimulatorUtils.Subscribe(StartPlaybackActionReference,StartPlayback);
        }

        private void OnDisable()
        {
            SimulatorUtils.Unsubscribe(StartPlaybackActionReference, StartPlayback);
        }

        /// <summary>
        /// Imports a recording.
        /// </summary>
        private void ImportRecording()
        {
            //Read XML
            var serializer = new XmlSerializer(typeof(SimulationRecording));
            var stream = new FileStream(Path.Combine(Application.dataPath + FilePath, $"{_recordingName}.xml"),
                FileMode.Open);
            _currentRecording = (SimulationRecording)serializer.Deserialize(stream);
            stream.Close();

            if (_currentRecording == null)
            {
                Debug.Log($"{nameof(SimulationPlayback)}, could not loud recording from XML file");
                return;
            }

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
            }

            SetInputActive(false);
            IsPlaying = true;

            _timer = 0;
            _currentFrame = 0;
            // StartCoroutine(PlaybackCoroutine());
        }

        /// <summary>
        /// Enables input and the tracked pose driver of the head.
        /// </summary>
        /// <param name="state"></param>
        private void SetInputActive(bool state)
        {
            // _deviceSimulator.enabled = state;
            // _deviceSimulatorControls.enabled = state;
            _headTrackedPoseDriver.enabled = state;
            _leftHandTrackedPoseDriver.enabled = state;
            _rightHandTrackedPoseDriver.enabled = state;
        }

        /// <summary>
        /// Stops the current playback and re-enables input.
        /// </summary>
        private void EndPlayBack()
        {
            IsPlaying = false;
            SetInputActive(true);
        }

        private void Update()
        {
            
            if (!IsPlaying)
                return;

            _timer += Time.unscaledDeltaTime;
            if(_timer>= _frameInterval)
            {
                _timer = 0;
                
                var frame = _currentRecording.Frames[_currentFrame];

                _hmdTransform.position = frame.HeadPosition;
                _hmdTransform.rotation = frame.HeadRotation;

                _leftHandTransform.position = frame.LeftHandPosition;
                _leftHandTransform.rotation = frame.LeftHandRotation;

                _rightHandTransform.position = frame.RightHandPosition;
                _rightHandTransform.rotation = frame.RightHandRotation;

                foreach (var input in frame.FrameInputs)
                    ProcessFrameInput(input);
                _currentFrame++;
                
                InputSystem.QueueStateEvent(_playbackInputDevice, _playbackDeviceState);

                if (_currentFrame >= _currentRecording.RecordingLength)
                    EndPlayBack();


            }
        }

        // private IEnumerator PlaybackCoroutine()
        // {
        //
        // }
        //

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