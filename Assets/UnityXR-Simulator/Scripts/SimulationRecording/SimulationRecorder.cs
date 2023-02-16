using System.Collections.Generic;
using System.IO;
using System.Xml.Serialization;
using UnityEngine;
using UnityEngine.InputSystem;

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
        [SerializeField] private List<InputActionReference> _inputToRecord = new List<InputActionReference>();
        
        [Header("Output parameters")]
        [SerializeField] private string FilePath = "/SimulationRecordings/";
        [SerializeField] private string RecordingName = "NewRecording";

        private float _frameInterval;
        private float _intervalTimer;

        private bool _isRecording;

        private SimulationRecording _currentRecording ;
        private List<FrameInput> _currentFrameInput = new List<FrameInput>();
        
        /// <summary>
        /// See <see cref="MonoBehaviour"/>
        /// </summary>
        private void Awake()
        {
            _frameInterval = 1 / (float)_desiredFPS;
        }

        private void OnEnable()
        {
            SubscribeControllerActions();
        }

        private void OnDisable()
        {
            UnSubscribeControllerActions();
        }

        private void SubscribeControllerActions()
        {
            foreach (var actionReference in _inputToRecord)
            {
                SimulatorUtils.Subscribe(actionReference,RecordInputAction);
            }
        }
        private void UnSubscribeControllerActions()
        {
            foreach (var actionReference in _inputToRecord)
            {
                SimulatorUtils.Unsubscribe(actionReference, RecordInputAction);
            }
        }
        
        private void RecordInputAction(InputAction.CallbackContext ctx)
        {
            if (!_isRecording)
                return;
            
            if (ctx.performed)
            {   
                Debug.Log($"Performed: {ctx.action.name} with {ctx.action.actionMap.name}, asset = {ctx.action.actionMap.asset}");
                var inputFrame = new FrameInput()
                {
                    InputActionName = ctx.action.name,
                    InputAssetName = ctx.action.actionMap.name,
                    InputMapName = ctx.action.actionMap.asset.name
                };
                _currentFrameInput.Add(inputFrame);
            }
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
                var newFrame = new FrameData
                {
                    HeadPosition = _hmdTransform.position,
                    HeadRotation = _hmdTransform.rotation,
                    LeftHandPosition = _leftHandTransform.position,
                    LeftHandRotation = _leftHandTransform.rotation,
                    RightHandPosition = _rightHandTransform.position,
                    RightHandRotation = _rightHandTransform.rotation,
                    FrameInputs = new List<FrameInput>(_currentFrameInput)
                };
                Debug.Log("Frame Added");
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
