using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace Rhinox.XR.UnityXR.Simulator
{
    /// <summary>
    /// This class records user input events and the transformation of the HMD and controllers.<br />It writes this recording to an xml file when the recording ends.
    /// </summary>
    /// <remarks>
    /// Control what gets captured by changing the booleans.
    /// </remarks>
    public class SimulationRecorder : MonoBehaviour
    {
        [Header("Device Transforms")]
        [SerializeField] private Transform _hmdTransform;
        [SerializeField] private Transform _leftHandTransform;
        [SerializeField] private Transform _rightHandTransform;

        [Header("Recording parameters")]
        [SerializeField] private int _desiredFPS = 30;

        [Header("Output parameters")]
        [SerializeField] private string FilePath = "/SimulationRecordings/";
        [SerializeField] private string RecordingName = "NewRecording";
        
        private float _frameInterval;
        private float _intervalTimer;
        
        public bool RecordHMDTransform = false;
        public bool RecordLeftHandTransform = false;
        public bool RecordRightHandTransform = false;

        private bool _isRecording = false;

        private SimulationRecording _currentRecording = null;
        
        
        /// <summary>
        /// See <see cref="MonoBehaviour"/>
        /// </summary>
        private void Awake()
        {
            _frameInterval = 1 / (float)_desiredFPS;
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>
        /// </summary>
        private void Update()
        {
            if(!_isRecording)
                return;
            
            _intervalTimer += Time.deltaTime;
            if (_intervalTimer >= _frameInterval)
            {
                _intervalTimer = 0;
                var newFrame = new FrameData();

                if (RecordHMDTransform)
                {
                    newFrame.HeadPosition = _hmdTransform.position;
                    newFrame.HeadRotation = _hmdTransform.rotation;
                }
                if (RecordLeftHandTransform)
                {
                    newFrame.LeftHandPosition = _leftHandTransform.position;
                    newFrame.LeftHandRotation = _leftHandTransform.rotation;
                }
                if (RecordRightHandTransform)
                {
                    newFrame.RightHandPosition = _rightHandTransform.position;
                    newFrame.RightHandRotation = _rightHandTransform.rotation;
                }
                
                _currentRecording.AddFrame(newFrame);
            }
        }

        /// <summary>
        /// Creates the recording and starts frame capturing.
        /// </summary>
        [ContextMenu("Start Recording")]
        private void StartRecording()
        {
            _isRecording = true;
            _intervalTimer = 0;
            _currentRecording = new SimulationRecording
            {
                FrameRate = _desiredFPS
            };
        }

        /// <summary>
        /// End frame capturing and writes the recording to an XML file.
        /// </summary>
        [ContextMenu("End Recording")]
        private void EndRecording()
        {
            _isRecording = false;
            
            //Write to XML
            var serializer = new XmlSerializer(typeof(SimulationRecording));
            var stream = new FileStream(Path.Combine(Application.dataPath + FilePath, $"{RecordingName}.xml"),
                FileMode.Create);
            serializer.Serialize(stream, _currentRecording);
            stream.Close();
        }
        
    }
}
