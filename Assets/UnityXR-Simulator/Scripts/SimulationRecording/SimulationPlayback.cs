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

        [SerializeField] private string RecordingName = "NewRecording";
        [SerializeField] private BetterXRDeviceSimulator _deviceSimulator;
        [SerializeField] private XRDeviceSimulatorControls _deviceSimulatorControls;

        private SimulationRecording _currentRecording;
        private bool _isPlaying = false;
        private int _currentFrame = 0;

        private float _frameInterval = float.MaxValue;
        private float _intervalTimer = 0;

        private TrackedPoseDriver _headTrackedPoseDriver;
        private TrackedPoseDriver _leftHandTrackedPoseDriver;
        private TrackedPoseDriver _rightHandTrackedPoseDriver;

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
            _leftHandTrackedPoseDriver = _leftHandTransform.GetComponent<TrackedPoseDriver>();
            _rightHandTrackedPoseDriver = _rightHandTransform.GetComponent<TrackedPoseDriver>();
            
        }

        /// <summary>
        /// Imports a recording.
        /// </summary>
        [ContextMenu("Import recording")]
        private void ImportRecording()
        {
            //Write to XML
            var serializer = new XmlSerializer(typeof(SimulationRecording));
            var stream = new FileStream(Path.Combine(Application.dataPath + FilePath, $"{RecordingName}.xml"),
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
        /// If there is no current recording, the function returns ealy.
        /// </summary>
        [ContextMenu("Start Playback")]
        private void StartPlayback()
        {
            if (_currentRecording == null)
            {
                Debug.Log("Import a recording, before playing it.");
                return;
            }

            //Disable the device simulator and simulator controls, so no input is processed while the simulation plays.
            SetInputActive(false);
            _isPlaying = true;
            _currentFrame = 0;
        }

        /// <summary>
        /// Enables input and the tracked pose drivers.
        /// </summary>
        /// <param name="state"></param>
        private void SetInputActive(bool state)
        {
            _deviceSimulator.enabled = state;
            _deviceSimulatorControls.enabled = state;
            _headTrackedPoseDriver.enabled = state;
            _leftHandTrackedPoseDriver.enabled = state;
            _rightHandTrackedPoseDriver.enabled = state;
        }

        /// <summary>
        /// Stops the current playback and re-enables input.
        /// </summary>
        private void EndPlayBack()
        {
            _isPlaying = false;
            SetInputActive(true);
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>
        /// </summary>
        private void Update()
        {
            if (!_isPlaying)
                return;

            _intervalTimer += Time.deltaTime;
            if (_intervalTimer >= _frameInterval)
            {
                _intervalTimer = 0;

                var frame = _currentRecording.Frames[_currentFrame];

                _hmdTransform.position = frame.HeadPosition;
                _hmdTransform.rotation = frame.HeadRotation;

                _leftHandTransform.position = frame.LeftHandPosition;
                _leftHandTransform.rotation = frame.LeftHandRotation;

                _rightHandTransform.position = frame.RightHandPosition;
                _rightHandTransform.rotation = frame.RightHandRotation;
                
                _currentFrame++;
                if (_currentFrame >= _currentRecording.RecordingLength)
                    EndPlayBack();
            }
        }
    }
}