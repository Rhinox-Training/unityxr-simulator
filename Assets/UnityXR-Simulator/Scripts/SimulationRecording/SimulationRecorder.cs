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
        
        [Header("Output parameters")]
        [SerializeField] private string FilePath = "/SimulationRecordings/";
        [SerializeField] private string RecordingName = "NewRecording";

        [Header("Input actions")] 
        [SerializeField] private InputActionReference _leftGripInputActionReference;
        [SerializeField] private InputActionReference _leftTriggerInputActionReference;
        [SerializeField] private InputActionReference _leftPrimaryButtonInputActionReference;
        [SerializeField] private InputActionReference _leftSecondaryButtonActionReference;
        [SerializeField] private InputActionReference _leftMenuButtonInputActionReference;
        [Space(10)]
        [SerializeField] private InputActionReference _rightGripInputActionReference;
        [SerializeField] private InputActionReference _rightTriggerInputActionReference;
        [SerializeField] private InputActionReference _rightPrimaryButtonInputActionReference;
        [SerializeField] private InputActionReference _rightSecondaryButtonActionReference;
        [SerializeField] private InputActionReference _rightMenuButtonInputActionReference;
        
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
            SimulatorUtils.Subscribe(_leftGripInputActionReference, OnGripPressed);
            SimulatorUtils.Subscribe(_rightGripInputActionReference, OnGripPressed);
            
            SimulatorUtils.Subscribe(_leftTriggerInputActionReference, OnTriggerPressed);
            SimulatorUtils.Subscribe(_rightTriggerInputActionReference, OnTriggerPressed);
            
            SimulatorUtils.Subscribe(_leftPrimaryButtonInputActionReference, OnPrimaryButtonPressed);
            SimulatorUtils.Subscribe(_rightPrimaryButtonInputActionReference, OnPrimaryButtonPressed);
            
            SimulatorUtils.Subscribe(_leftSecondaryButtonActionReference, OnSecondaryButtonPressed);
            SimulatorUtils.Subscribe(_rightSecondaryButtonActionReference, OnSecondaryButtonPressed);

            SimulatorUtils.Subscribe(_leftMenuButtonInputActionReference, OnMenuButtonPressed);
            SimulatorUtils.Subscribe(_rightMenuButtonInputActionReference, OnMenuButtonPressed);
        }
        private void UnSubscribeControllerActions()
        {
            SimulatorUtils.Unsubscribe(_leftGripInputActionReference, OnGripPressed);
            SimulatorUtils.Unsubscribe(_rightGripInputActionReference, OnGripPressed);
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
                _currentFrameInput.Clear();
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
        
        
        
        
        
        //-----------------------
        // INPUT EVENTS
        //-----------------------
        private void OnGripPressed(InputAction.CallbackContext ctx)
        {
            if(!_isRecording)
                return;

            if (!ctx.performed)
                return;

            var frameInput = new FrameInput();
            frameInput.InputActionName = "grip";

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftGripInputActionReference.action)
            {
                //LEFT
                Debug.Log("Left grip used");
                frameInput.IsRightControllerInput = false;

            }
            else if(ctx.action == _rightGripInputActionReference.action)
            {
                //RIGHT
                Debug.Log("Right grip used");
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }
            _currentFrameInput.Add(frameInput);
        }

        private void OnTriggerPressed(InputAction.CallbackContext ctx)
        {
            if (!_isRecording)
                return;

            if (!ctx.performed)
                return;

            var frameInput = new FrameInput();
            frameInput.InputActionName = "trigger";

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftTriggerInputActionReference.action)
            {
                //LEFT
                Debug.Log("Left trigger used");
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightTriggerInputActionReference.action)
            {
                //RIGHT
                Debug.Log("Right trigger used");
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }

        private void OnPrimaryButtonPressed(InputAction.CallbackContext ctx)
        {
            if (!_isRecording)
                return;

            if (!ctx.performed)
                return;

            var frameInput = new FrameInput();
            frameInput.InputActionName = "primary button";

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftPrimaryButtonInputActionReference.action)
            {
                //LEFT
                Debug.Log("Left primary button used");
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightPrimaryButtonInputActionReference.action)
            {
                //RIGHT
                Debug.Log("Right primary button used");
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }

        private void OnSecondaryButtonPressed(InputAction.CallbackContext ctx)
        {
            if (!_isRecording)
                return;

            if (!ctx.performed)
                return;

            var frameInput = new FrameInput();
            frameInput.InputActionName = "secondary button";

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftSecondaryButtonActionReference.action)
            {
                //LEFT
                Debug.Log("Left secondary button used");
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightSecondaryButtonActionReference.action)
            {
                //RIGHT
                Debug.Log("Right secondary button used");
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }

        private void OnMenuButtonPressed(InputAction.CallbackContext ctx)
        {
            if (!_isRecording)
                return;

            if (!ctx.performed)
                return;

            var frameInput = new FrameInput();
            frameInput.InputActionName = "menu button";

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftMenuButtonInputActionReference.action)
            {
                //LEFT
                Debug.Log("Left primary menu used");
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightMenuButtonInputActionReference.action)
            {
                //RIGHT
                Debug.Log("Right menu button used");
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }
    }
}
