using System.Collections;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.Assertions;
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
        [SerializeField] private BetterXRDeviceSimulator _deviceSimulator;
        [SerializeField] private XRDeviceSimulatorControls _deviceSimulatorControls;

        private SimulationRecording _currentRecording;
        private bool _isPlaying;
        private int _currentFrame;

        private float _frameInterval = float.MaxValue;

        private TrackedPoseDriver _headTrackedPoseDriver;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>
        /// </summary>
        private void OnValidate()
        {
            Assert.AreNotEqual(_deviceSimulator,null,$"{nameof(SimulationPlayback)}, device simulator not linked!");
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>
        /// </summary>
        private void Awake()
        {
            //Get the pose drivers 
            _headTrackedPoseDriver = _hmdTransform.GetComponent<TrackedPoseDriver>();
            
        }

        /// <summary>
        /// Imports a recording.
        /// </summary>
        [ContextMenu("Import recording")]
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
        private void StartPlayback()
        {
            if (_currentRecording == null)
            {
                ImportRecording();
            }

            //Disable the device simulator and simulator controls, so no input is processed while the simulation plays.
            SetInputActive(false);
            _isPlaying = true;

            StartCoroutine(PlaybackCoroutine());
            _currentFrame = 0;
        }

        /// <summary>
        /// Enables input and the tracked pose driver of the head.
        /// </summary>
        /// <param name="state"></param>
        private void SetInputActive(bool state)
        {
            // _deviceSimulator.enabled = state;
            // _deviceSimulatorControls.enabl   ed = state;
            _headTrackedPoseDriver.enabled = state;
        }

        /// <summary>
        /// Stops the current playback and re-enables input.
        /// </summary>
        private void EndPlayBack()
        {
            _isPlaying = false;
            SetInputActive(true);
        }

        private IEnumerator PlaybackCoroutine()
        {
            while (_currentFrame < _currentRecording.RecordingLength)
            {
                var frame = _currentRecording.Frames[_currentFrame];

                _hmdTransform.position = frame.HeadPosition;
                _hmdTransform.rotation = frame.HeadRotation;

                _leftHandTransform.position = frame.LeftHandPosition;
                _leftHandTransform.rotation = frame.LeftHandRotation;

                _rightHandTransform.position = frame.RightHandPosition;
                _rightHandTransform.rotation = frame.RightHandRotation;

                // Reset();
                foreach (var input in frame.FrameInputs)
                    ProcessFrameInput(input);
                _currentFrame++;
                
                yield return new WaitForSecondsRealtime(_frameInterval);
            }

            EndPlayBack();
        }
        

        // private void Reset()
        // {
        //     _deviceSimulatorControls.GripInput = false;
        //     _deviceSimulatorControls.TriggerInput = false;
        //     _deviceSimulatorControls.PrimaryButtonInput = false;
        //     _deviceSimulatorControls.SecondaryButtonInput = false;
        // }
        

        private void ProcessFrameInput(FrameInput input)
        {
            _deviceSimulatorControls.ManipulateRightControllerButtons = input.IsRightControllerInput;
            
            switch (input.InputActionName)
            {
                case "grip":
                    Debug.Log("Grip");
                    _deviceSimulatorControls.GripInput = input.IsInputStart;
                    break;
                case "trigger":
                    Debug.Log("trigger");
                    _deviceSimulatorControls.TriggerInput = input.IsInputStart;
                    break;
                case "primary button":
                    Debug.Log("Primary Button");
                    _deviceSimulatorControls.PrimaryButtonInput = input.IsInputStart;
                    break;
                case "secondary button":
                    Debug.Log("Secondary Button");
                    _deviceSimulatorControls.SecondaryButtonInput = input.IsInputStart;
                    break;
                default:
                    Debug.Log($"{nameof(SimulationPlayback)} - ProcessFrameInput, input action not found.");                    
                    return;
            }
        }
        
    }
}