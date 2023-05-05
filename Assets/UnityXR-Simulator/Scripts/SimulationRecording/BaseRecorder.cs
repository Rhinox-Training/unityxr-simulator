using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Serialization;
using Rhinox.Perceptor;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

namespace Rhinox.XR.UnityXR.Simulator
{
    public abstract class BaseRecorder : MonoBehaviour
    {
        [Header("Device Transforms")] [SerializeField]
        protected OculusDeviceSimulator _simulator;

        [SerializeField] protected Transform _headTransform;
        [SerializeField] protected Transform _leftHandTransform;
        [SerializeField] protected Transform _rightHandTransform;

        [Header("Recording parameters")] [Tooltip("Starts recording on awake.")] [SerializeField]
        protected bool _startOnAwake;

        [Tooltip("End any running recording on destroy.")] [SerializeField]
        protected bool _endOnDestroy;

        [Tooltip("Note: dead zone value should be very small for high frame rates!")] [SerializeField]
        protected float _positionDeadZone = 0.005f;

        [Tooltip("Note: dead zone value should be very small for high frame rates!")] [SerializeField]
        protected float _rotationDeadZone = 0.005f;

        [HideInInspector] public string Path;
        [HideInInspector] public string RecordingName = "NewRecording";

        [Header("Input actions")] public InputActionReference BeginRecordingActionReference;
        public InputActionReference EndRecordingActionReference;

        public bool IsRecording { get; private set; }

        private Stopwatch _recordingStopwatch = new Stopwatch();
        private SimulationRecording _currentRecording;

        /// <summary>
        /// See <see cref="MonoBehaviour"/>
        /// </summary>
        protected virtual void Awake()
        {
            Initialize();
            
            //Start the recording now, if desired
            if (_startOnAwake)
                StartRecording(new InputAction.CallbackContext());
        }

        protected virtual void Initialize()
        {
            
        }

        protected virtual void OnDestroy()
        {
            if (_endOnDestroy)
                EndRecording(new InputAction.CallbackContext());
            
            CleanUp();
        }
        
        protected virtual void CleanUp()
        {}

        protected virtual void OnEnable()
        {
            if (_simulator == null)
            {
                Debug.Log("_simulator has not been set,  disabling this SimulationRecorder.");
                this.gameObject.SetActive(false);
                return;
            }
            SubscribeRecorderActions();
            PerformOnOnDisable();
        }

        protected virtual void PerformOnOnEnable()
        {
            
        }

        protected virtual void OnDisable()
        {
            UnsubscribeRecorderActions();
            PerformOnOnDisable();
        }

        protected virtual void PerformOnOnDisable()
        {

        }
        private void SubscribeRecorderActions()
        {
            SimulatorUtils.Subscribe(BeginRecordingActionReference, StartRecording);
            SimulatorUtils.Subscribe(EndRecordingActionReference, EndRecording);
        }

        private void UnsubscribeRecorderActions()
        {
            SimulatorUtils.Unsubscribe(BeginRecordingActionReference, StartRecording);
            SimulatorUtils.Unsubscribe(EndRecordingActionReference, EndRecording);
        }

        /// <summary>
        /// Creates the recording and starts frame capturing.
        /// </summary>
        private void StartRecording(InputAction.CallbackContext ctx)
        {
            if (!ctx.performed)
                return;

            _currentRecording = new SimulationRecording
            {
                FrameRate = (int)(1f / Time.fixedDeltaTime)
            };
            _recordingStopwatch.Restart();
            Debug.Log("Started recording.");
            IsRecording = true;
        }

        /// <summary>
        /// End frame capturing and writes the recording to an XML file.
        /// </summary>
        private void EndRecording(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            //----------------------------
            // CALCULATE LENGTH
            //----------------------------
            RecordingTime temp;
            _recordingStopwatch.Stop();
            temp.Milliseconds = _recordingStopwatch.Elapsed.Milliseconds;
            temp.Seconds = _recordingStopwatch.Elapsed.Seconds;
            temp.Minutes = _recordingStopwatch.Elapsed.Minutes;
            _currentRecording.RecordingLength = temp;
            Debug.Log($"Ended recording of {_currentRecording.RecordingLength}");


            //----------------------------
            //Write to XML
            //----------------------------
            var serializer = new XmlSerializer(typeof(SimulationRecording));
            var stream = new FileStream(System.IO.Path.Combine(Path, $"{RecordingName}.xml"),
                FileMode.Create);
            serializer.Serialize(stream, _currentRecording);
            stream.Close();
            Debug.Log($"Wrote recording to: {Path}");
            _simulator.InputEnabled = true;
            IsRecording = false;
        }

        protected virtual void FixedUpdate()
        {
            if(!IsRecording)
                return;

            List<FrameInput> frameInputData = GetFrameInputs();
            var newFrame = new FrameData
            {
                HeadPosition = _headTransform.position,
                HeadRotation = _headTransform.rotation,
                LeftHandPosition = _leftHandTransform.position,
                LeftHandRotation = _leftHandTransform.rotation,
                RightHandPosition = _rightHandTransform.position,
                RightHandRotation = _rightHandTransform.rotation,
                FrameInputs = frameInputData
            };
            var previousRecordedFrame = _currentRecording.Frames.LastOrDefault();
            //Temporarily set the frame number of the current frame to the previous recorded frame
            //Otherwise they will never be equal
            newFrame.FrameNumber = previousRecordedFrame.FrameNumber;

            if (newFrame.ApproximatelyEqual(previousRecordedFrame, _positionDeadZone, _rotationDeadZone))
            {
                _currentRecording.AddEmptyFrame();
            }
            else
            {
                _currentRecording.AddFrame(newFrame);
            }
        }
        
        protected abstract List<FrameInput> GetFrameInputs(bool clearInputsAfterwards = true);
    }
}