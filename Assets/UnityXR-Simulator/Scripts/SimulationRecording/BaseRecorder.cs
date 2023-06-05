using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityXR_Simulator.Scripts.Attributes;
using Debug = UnityEngine.Debug;

namespace Rhinox.XR.UnityXR.Simulator
{
    /// <summary>
    /// Base class for recording simulation data.
    /// </summary>
    public abstract class BaseRecorder : MonoBehaviour
    {
        [Header("Device Transforms")]
        [SerializeField]
        protected BaseSimulator Simulator;

        [SerializeField] protected Transform HeadTransform;
        [SerializeField] protected Transform LeftHandTransform;
        [SerializeField] protected Transform RightHandTransform;

        [Header("Recording parameters")]
        [Tooltip("Starts recording on awake.")]
        [SerializeField]
        protected bool StartOnAwake;

        [Tooltip("End any running recording on destroy.")]
        [SerializeField]
        protected bool EndOnDestroy;

        /// <summary>
        /// Path to save the recording.
        /// </summary>
        [FolderBrowser]
        public string Path;

        /// <summary>
        /// Name of the recording file.
        /// </summary>
        public string RecordingName = "NewRecording";

        /// <summary>
        /// Input action reference for beginning the recording.
        /// </summary>
        [Header("Input actions")]
        public InputActionReference BeginRecordingActionReference;

        /// <summary>
        /// Input action reference for ending the recording.
        /// </summary>
        public InputActionReference EndRecordingActionReference;

        /// <summary>
        /// Gets a value indicating whether the recorder is currently recording.
        /// </summary>
        public bool IsRecording { get; private set; }

        private Stopwatch _recordingStopwatch = new Stopwatch();
        protected SimulationRecording CurrentRecording;

        /// <summary>
        /// Initializes the recorder.
        /// </summary>
        protected virtual void Awake()
        {
            Initialize();

            ValidatePath();

            // Start the recording now, if desired
            if (StartOnAwake)
                StartRecording(new InputAction.CallbackContext());
        }

        private void ValidatePath()
        {
            if (Directory.Exists(Path))
                return;

            if (string.IsNullOrEmpty(Path))
                Path = Application.dataPath;

            if (!Directory.Exists(Path))
                Directory.CreateDirectory(Path);
        }

        protected virtual void Initialize()
        {
        }

        protected virtual void OnDestroy()
        {
            if (EndOnDestroy)
                EndRecording(new InputAction.CallbackContext());

            CleanUp();
        }

        protected virtual void CleanUp()
        {
        }

        protected virtual void OnEnable()
        {
            if (Simulator == null)
            {
                Debug.Log("_simulator has not been set, disabling this SimulationRecorder.");
                this.gameObject.SetActive(false);
                return;
            }

            SubscribeRecorderActions();
            PerformOnOnEnable();
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
        /// <param name="ctx">The input action callback context.</param>
        private void StartRecording(InputAction.CallbackContext ctx)
        {
            CurrentRecording = new SimulationRecording
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
        /// <param name="ctx">The input action callback context.</param>
        private void EndRecording(InputAction.CallbackContext ctx)
        {
            //----------------------------
            // CALCULATE LENGTH
            //----------------------------
            RecordingTime temp;
            _recordingStopwatch.Stop();
            temp.Milliseconds = _recordingStopwatch.Elapsed.Milliseconds;
            temp.Seconds = _recordingStopwatch.Elapsed.Seconds;
            temp.Minutes = _recordingStopwatch.Elapsed.Minutes;
            CurrentRecording.RecordingLength = temp;
            Debug.Log($"Ended recording of {CurrentRecording.RecordingLength}");

            //----------------------------
            //Write to XML
            //----------------------------
            var serializer = new XmlSerializer(typeof(SimulationRecording));
            var stream = new FileStream(System.IO.Path.Combine(Path, $"{RecordingName}.xml"), FileMode.Create);
            serializer.Serialize(stream, CurrentRecording);
            stream.Close();
            Debug.Log($"Wrote recording to: {Path}");
            Simulator.InputEnabled = true;
            IsRecording = false;
        }

        protected virtual void FixedUpdate()
        {
            if (!IsRecording)
                return;

            var frameInputData = GetFrameInputs();
            var newFrame = new FrameData
            {
                HeadPosition = HeadTransform.position,
                HeadRotation = HeadTransform.rotation,
                LeftHandPosition = LeftHandTransform.position,
                LeftHandRotation = LeftHandTransform.rotation,
                RightHandPosition = RightHandTransform.position,
                RightHandRotation = RightHandTransform.rotation,
                FrameInputs = frameInputData
            };

            CurrentRecording.AddFrame(newFrame);
        }

        /// <summary>
        /// Returns a list of FrameInput objects representing the recorded input data for the current frame.
        /// </summary>
        /// <param name="clearInputsAfterwards">Whether to clear the recorded input data after returning it.</param>
        /// <returns>A list of FrameInput objects representing the recorded input data for the current frame.</returns>
        protected abstract List<FrameInput> GetFrameInputs(bool clearInputsAfterwards = true);
    }
}
