using System;
using System.Diagnostics;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

namespace Rhinox.XR.UnityXR.Simulator
{
    public abstract class BasePlayback : MonoBehaviour
    {
        [Header("Input parameters")] [SerializeField]
        protected BaseSimulator Simulator;

       [FileBrowser("xml")] public string FileDirectory;

        [Header("Playback Controls")] public InputActionReference StartPlaybackActionReference;
        public InputActionReference ReimportRecordingActionReference;
        public InputActionReference AbortPlaybackActionReference;

        [HideInInspector] public bool IsPlaying;
        private SimulationRecording _currentRecording;
        private Stopwatch _playbackStopwatch;
        private int _currentFrameNumber;

        public event Action PlaybackStarted;
        public event Action PlaybackStopped;

        private void Awake()
        {
            _playbackStopwatch = new Stopwatch();
            Initialize();
        }

        protected virtual void Initialize()
        {
        }

        private void OnEnable()
        {
            if (Simulator == null)
            {
                Debug.Log("_simulator has not been set,  disabling this SimulationPlayback.");
                this.gameObject.SetActive(false);
                return;
            }

            SimulatorUtils.Subscribe(StartPlaybackActionReference, BeginPlayback);
            SimulatorUtils.Subscribe(ReimportRecordingActionReference, ImportRecording);
            SimulatorUtils.Subscribe(AbortPlaybackActionReference, AbortPlayback);
        }

        private void OnDisable()
        {
            SimulatorUtils.Unsubscribe(StartPlaybackActionReference, BeginPlayback);
            SimulatorUtils.Subscribe(ReimportRecordingActionReference, ImportRecording);
            SimulatorUtils.Subscribe(AbortPlaybackActionReference, AbortPlayback);
        }


        private void FixedUpdate()
        {
            if (!IsPlaying)
                return;

            var currentFrame = _currentRecording.Frames[_currentFrameNumber];

            ProcessFrame(currentFrame);

            _currentFrameNumber++;
            if (_currentFrameNumber >= _currentRecording.AmountOfFrames)
            {
                _playbackStopwatch.Stop();
                EndPlayBack();
            }
        }

        protected abstract void ProcessFrame(FrameData frameData);

        private void ImportRecording(InputAction.CallbackContext ctx)
        {
            if(!ctx.performed)
                return;
            ImportRecording();
        }

        private void ImportRecording()
        {
            //Read XML
            var serializer = new XmlSerializer(typeof(SimulationRecording));
            var stream = new FileStream(FileDirectory, FileMode.Open);
            _currentRecording = (SimulationRecording)serializer.Deserialize(stream);
            stream.Close();

            if (_currentRecording == null)
            {
                Debug.Log($"{nameof(SimulationPlayback)}, could not loud recording from XML file");
                return;
            }

            Debug.Log($"Imported recording of {_currentRecording.RecordingLength}");
        }

        private void AbortPlayback(InputAction.CallbackContext ctx)
        {
            EndPlayBack();
        }

        /// <summary>
        /// Disables input in the simulator and starts the playback of the current recording.
        /// </summary>
        /// <remarks>
        /// If there is no current recording or the current recordings frames are empty, the function returns early.
        /// </remarks>
        private void BeginPlayback(InputAction.CallbackContext ctx)
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

            Debug.Log("Started playback.");
            Simulator.IsInputEnabled = false;

            IsPlaying = true;
            _currentFrameNumber = 0;
            Simulator.IsInputEnabled = false;

            PlaybackStarted?.Invoke();

            _playbackStopwatch.Restart();
        }

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
            Simulator.IsInputEnabled = true;
            IsPlaying = false;

            PlaybackStopped?.Invoke();
        }
    }
}