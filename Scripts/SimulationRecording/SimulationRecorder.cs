using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Serialization;
using Rhinox.GUIUtils.Editor;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Debug = UnityEngine.Debug;

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
        [SerializeField] private BetterXRDeviceSimulator _simulator;
        [SerializeField] private Transform _headTransform;
        [SerializeField] private Transform _leftHandTransform;
        [SerializeField] private Transform _rightHandTransform;

        [Header("Recording parameters")] 
        [Tooltip("Starts recording on awake.")]
        [SerializeField] private bool _startOnAwake;
        [Tooltip("End any running recording on destroy.")]
        [SerializeField] private bool _endOnDestroy;
        [SerializeField] private int _desiredFPS = 30;
        [Tooltip("Note: dead zone value should be very small for high frame rates!")]
        [SerializeField] private float _positionDeadZone = 0.005f;

        [Tooltip("Note: dead zone value should be very small for high frame rates!")]
        [SerializeField] private float _rotationDeadZone = 0.005f;

        [HideInInspector] public string Path;
        [HideInInspector] public string RecordingName = "NewRecording";

        [Header("Input actions")] 
        public InputActionReference BeginRecordingActionReference;
        public InputActionReference EndRecordingActionReference;
        [Space(15)]
        [SerializeField] private InputActionReference _leftGripInputActionReference;
        [SerializeField] private InputActionReference _leftPrimaryAxisActionReference;
        [SerializeField] private InputActionReference _leftPrimaryAxis2DClickActionReference;
        [SerializeField] private InputActionReference _leftPrimaryAxis2DTouchActionReference;
        [SerializeField] private InputActionReference _leftPrimaryButtonInputActionReference;
        [SerializeField] private InputActionReference _leftPrimaryTouchInputActionReference;
        [SerializeField] private InputActionReference _leftSecondaryAxisActionReference;
        [SerializeField] private InputActionReference _leftSecondaryAxis2DClickActionReference;
        [SerializeField] private InputActionReference _leftSecondaryAxis2DTouchActionReference;
        [SerializeField] private InputActionReference _leftSecondaryTouchActionReference;
        [SerializeField] private InputActionReference _leftMenuButtonActionReference;
        [SerializeField] private InputActionReference _leftTriggerInputActionReference;
        [SerializeField] private InputActionReference _leftSecondaryButtonActionReference;
        
        [Space(10)] 
        [SerializeField] private InputActionReference _rightGripInputActionReference;
        [SerializeField] private InputActionReference _rightPrimaryAxisActionReference;
        [SerializeField] private InputActionReference _rightPrimaryAxis2DClickActionReference;
        [SerializeField] private InputActionReference _rightPrimaryAxis2DTouchActionReference;
        [SerializeField] private InputActionReference _rightPrimaryButtonInputActionReference;
        [SerializeField] private InputActionReference _rightPrimaryTouchInputActionReference;
        [SerializeField] private InputActionReference _rightSecondaryAxisActionReference;
        [SerializeField] private InputActionReference _rightSecondaryAxis2DClickActionReference;
        [SerializeField] private InputActionReference _rightSecondaryAxis2DTouchActionReference;
        [SerializeField] private InputActionReference _rightSecondaryTouchActionReference;
        [SerializeField] private InputActionReference _rightMenuButtonActionReference;
        [SerializeField] private InputActionReference _rightTriggerInputActionReference;
        [SerializeField] private InputActionReference _rightSecondaryButtonActionReference;
        
        private float _frameInterval;

        [HideInInspector]
        public bool IsRecording;

        private Stopwatch _recordingStopwatch = new Stopwatch();
        private SimulationRecording _currentRecording ;
        private List<FrameInput> _currentFrameInput = new List<FrameInput>();

        /// <summary>
        /// See <see cref="MonoBehaviour"/>
        /// </summary>
        private void Awake()
        {
            _frameInterval = 1 / (float)_desiredFPS;

            if (_startOnAwake)
                StartRecording(new InputAction.CallbackContext());
        }

        private void OnDestroy()
        {
            if(_endOnDestroy)
                EndRecording(new InputAction.CallbackContext());
        }

        private void OnEnable()
        {
            if (_simulator == null)
            {
                Debug.Log("_simulator has not been set,  disabling this SimulationRecorder.");
                this.gameObject.SetActive(false);
                return;
            }
            
            SubscribeControllerActions();
            SubscribeRecorderActions();
        }
        private void OnDisable()
        {
            UnSubscribeControllerActions();
            UnsubscribeRecorderActions();
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
        
        private void SubscribeControllerActions()
        {
            SimulatorUtils.Subscribe(_leftGripInputActionReference, OnGripPressed, OnGripCancelled);
            SimulatorUtils.Subscribe(_rightGripInputActionReference, OnGripPressed, OnGripCancelled);
            
            SimulatorUtils.Subscribe(_leftPrimaryAxisActionReference,OnPrimaryAxis2DTriggered,OnPrimaryAxis2DCancelled);
            SimulatorUtils.Subscribe(_rightPrimaryAxisActionReference, OnPrimaryAxis2DTriggered, OnPrimaryAxis2DCancelled);

            SimulatorUtils.Subscribe(_leftSecondaryAxisActionReference, OnSecondaryAxis2DTriggered, OnSecondaryAxis2DCancelled);
            SimulatorUtils.Subscribe(_rightSecondaryAxisActionReference, OnSecondaryAxis2DTriggered, OnSecondaryAxis2DCancelled);

            SimulatorUtils.Subscribe(_leftPrimaryAxis2DClickActionReference, OnPrimaryAxis2DClick, OnPrimaryAxis2DClickCancelled);
            SimulatorUtils.Subscribe(_rightPrimaryAxis2DClickActionReference, OnPrimaryAxis2DClick, OnPrimaryAxis2DClickCancelled);
    
            SimulatorUtils.Subscribe(_leftPrimaryAxis2DTouchActionReference, OnPrimaryAxis2DTouch, OnPrimaryAxis2DTouchCancelled);
            SimulatorUtils.Subscribe(_rightPrimaryAxis2DTouchActionReference, OnPrimaryAxis2DTouch, OnPrimaryAxis2DTouchCancelled);
            
            SimulatorUtils.Subscribe(_leftPrimaryButtonInputActionReference, OnPrimaryButtonPressed, OnPrimaryButtonCancelled);
            SimulatorUtils.Subscribe(_rightPrimaryButtonInputActionReference, OnPrimaryButtonPressed, OnPrimaryButtonCancelled);
    
            SimulatorUtils.Subscribe(_leftPrimaryTouchInputActionReference, OnPrimaryTouch, OnPrimaryTouchCancelled);
            SimulatorUtils.Subscribe(_rightPrimaryTouchInputActionReference, OnPrimaryTouch, OnPrimaryTouchCancelled);
    
            SimulatorUtils.Subscribe(_leftSecondaryAxis2DClickActionReference, OnSecondaryAxis2DClick, OnSecondaryAxis2DClickCancelled);
            SimulatorUtils.Subscribe(_rightSecondaryAxis2DClickActionReference, OnSecondaryAxis2DClick, OnSecondaryAxis2DClickCancelled);
    
            SimulatorUtils.Subscribe(_leftSecondaryAxis2DTouchActionReference, OnSecondaryAxis2DTouch, OnSecondaryAxis2DTouchCancelled);
            SimulatorUtils.Subscribe(_rightSecondaryAxis2DTouchActionReference, OnSecondaryAxis2DTouch, OnSecondaryAxis2DTouchCancelled);
    
            SimulatorUtils.Subscribe(_leftSecondaryTouchActionReference, OnSecondaryTouch, OnSecondaryTouchCancelled);
            SimulatorUtils.Subscribe(_rightSecondaryTouchActionReference, OnSecondaryTouch, OnSecondaryTouchCancelled);
    
            SimulatorUtils.Subscribe(_leftMenuButtonActionReference, OnMenuButtonPressed, OnMenuButtonCancelled);
            SimulatorUtils.Subscribe(_rightMenuButtonActionReference, OnMenuButtonPressed, OnMenuButtonCancelled);
            
            SimulatorUtils.Subscribe(_leftTriggerInputActionReference, OnTriggerPressed, OnTriggerCancelled);
            SimulatorUtils.Subscribe(_rightTriggerInputActionReference, OnTriggerPressed, OnTriggerCancelled);
    
            SimulatorUtils.Subscribe(_leftSecondaryButtonActionReference, OnSecondaryButtonPressed, OnSecondaryButtonCancelled);
            SimulatorUtils.Subscribe(_rightSecondaryButtonActionReference, OnSecondaryButtonPressed, OnSecondaryButtonCancelled);
        }
        private void UnSubscribeControllerActions()
        {
            SimulatorUtils.Unsubscribe(_leftGripInputActionReference, OnGripPressed, OnGripCancelled);
            SimulatorUtils.Unsubscribe(_rightGripInputActionReference, OnGripPressed, OnGripCancelled);

            SimulatorUtils.Unsubscribe(_leftPrimaryAxisActionReference, OnPrimaryAxis2DTriggered, OnPrimaryAxis2DCancelled);
            SimulatorUtils.Unsubscribe(_rightPrimaryAxisActionReference, OnPrimaryAxis2DTriggered, OnPrimaryAxis2DCancelled);

            SimulatorUtils.Unsubscribe(_leftSecondaryAxisActionReference, OnSecondaryAxis2DTriggered, OnSecondaryAxis2DCancelled);
            SimulatorUtils.Unsubscribe(_rightSecondaryAxisActionReference, OnSecondaryAxis2DTriggered, OnSecondaryAxis2DCancelled);
            
            SimulatorUtils.Unsubscribe(_leftPrimaryAxis2DClickActionReference, OnPrimaryAxis2DClick, OnPrimaryAxis2DClickCancelled);
            SimulatorUtils.Unsubscribe(_rightPrimaryAxis2DClickActionReference, OnPrimaryAxis2DClick, OnPrimaryAxis2DClickCancelled);
    
            SimulatorUtils.Unsubscribe(_leftPrimaryAxis2DTouchActionReference, OnPrimaryAxis2DTouch, OnPrimaryAxis2DTouchCancelled);
            SimulatorUtils.Unsubscribe(_rightPrimaryAxis2DTouchActionReference, OnPrimaryAxis2DTouch, OnPrimaryAxis2DTouchCancelled);
            
            SimulatorUtils.Unsubscribe(_leftPrimaryButtonInputActionReference, OnPrimaryButtonPressed, OnPrimaryButtonCancelled);
            SimulatorUtils.Unsubscribe(_rightPrimaryButtonInputActionReference, OnPrimaryButtonPressed, OnPrimaryButtonCancelled);
    
            SimulatorUtils.Unsubscribe(_leftPrimaryTouchInputActionReference, OnPrimaryTouch, OnPrimaryTouchCancelled);
            SimulatorUtils.Unsubscribe(_rightPrimaryTouchInputActionReference, OnPrimaryTouch, OnPrimaryTouchCancelled);
    
            SimulatorUtils.Unsubscribe(_leftSecondaryAxis2DClickActionReference, OnSecondaryAxis2DClick, OnSecondaryAxis2DClickCancelled);
            SimulatorUtils.Unsubscribe(_rightSecondaryAxis2DClickActionReference, OnSecondaryAxis2DClick, OnSecondaryAxis2DClickCancelled);
    
            SimulatorUtils.Unsubscribe(_leftSecondaryAxis2DTouchActionReference, OnSecondaryAxis2DTouch, OnSecondaryAxis2DTouchCancelled);
            SimulatorUtils.Unsubscribe(_rightSecondaryAxis2DTouchActionReference, OnSecondaryAxis2DTouch, OnSecondaryAxis2DTouchCancelled);
    
            SimulatorUtils.Unsubscribe(_leftSecondaryTouchActionReference, OnSecondaryTouch, OnSecondaryTouchCancelled);
            SimulatorUtils.Unsubscribe(_rightSecondaryTouchActionReference, OnSecondaryTouch, OnSecondaryTouchCancelled);
    
            SimulatorUtils.Unsubscribe(_leftMenuButtonActionReference, OnMenuButtonPressed, OnMenuButtonCancelled);
            SimulatorUtils.Unsubscribe(_rightMenuButtonActionReference, OnMenuButtonPressed, OnMenuButtonCancelled);
            
            SimulatorUtils.Unsubscribe(_leftTriggerInputActionReference, OnTriggerPressed, OnTriggerCancelled);
            SimulatorUtils.Unsubscribe(_rightTriggerInputActionReference, OnTriggerPressed, OnTriggerCancelled);
    
            SimulatorUtils.Unsubscribe(_leftSecondaryButtonActionReference, OnSecondaryButtonPressed, OnSecondaryButtonCancelled);
            SimulatorUtils.Unsubscribe(_rightSecondaryButtonActionReference, OnSecondaryButtonPressed, OnSecondaryButtonCancelled);
        }

        /// <summary>
        /// Creates the recording and starts frame capturing.
        /// </summary>
        [ContextMenu("Start Recording")]
        private void StartRecording(InputAction.CallbackContext ctx)
        {
            IsRecording = true;
            _currentRecording = new SimulationRecording
            {
                FrameRate = _desiredFPS
            };
            _frameInterval = 1.0f / _desiredFPS;
            _recordingStopwatch.Restart();
            Debug.Log("Started recording.");
            
            StartCoroutine(RecordingCoroutine());
        }

        private IEnumerator RecordingCoroutine()
        {
            while (IsRecording)
            {
                var newFrame = new FrameData
                            {
                                HeadPosition = _headTransform.position,
                                HeadRotation = _headTransform.rotation,
                                LeftHandPosition = _leftHandTransform.position,
                                LeftHandRotation = _leftHandTransform.rotation,
                                RightHandPosition = _rightHandTransform.position,
                                RightHandRotation = _rightHandTransform.rotation,
                                FrameInputs = new List<FrameInput>(_currentFrameInput)
                            };
                var previousRecordedFrame = _currentRecording.Frames.LastOrDefault();
                //Temporarily set the frame number of the current frame to the previous recorded frame
                //Otherwise they will never be equal
                newFrame.FrameNumber = previousRecordedFrame.FrameNumber;
                
                if (newFrame.ApproximatelyEqual(previousRecordedFrame,_positionDeadZone,_rotationDeadZone) )
                {
                    _currentRecording.AddEmptyFrame();
                }
                else
                {
                    _currentRecording.AddFrame(newFrame);
                }
                _currentFrameInput.Clear();

                yield return new WaitForSecondsRealtime(_frameInterval);
            }
            
        }

        /// <summary>
        /// End frame capturing and writes the recording to an XML file.
        /// </summary>
        [ContextMenu("End Recording")]
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
            //Create the target directory just in case

            var serializer = new XmlSerializer(typeof(SimulationRecording));
            var stream = new FileStream(System.IO.Path.Combine(Path, $"{RecordingName}.xml"),
                FileMode.Create);
            serializer.Serialize(stream, _currentRecording);
            stream.Close();
            Debug.Log($"Wrote recording to: {Path}");
            _simulator.InputEnabled = true;
            IsRecording = false;
        }
        
        
        //-----------------------
        // INPUT EVENTS
        //-----------------------
        private void OnGripPressed(InputAction.CallbackContext ctx)
        {
            if(!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "grip",
                IsInputStart = true
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftGripInputActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if(ctx.action == _rightGripInputActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }
            _currentFrameInput.Add(frameInput);
        }
        private void OnGripCancelled(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "grip",
                IsInputStart = false
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftGripInputActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightGripInputActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }

        private void OnPrimaryAxis2DClick(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "primary axis 2D click",
                IsInputStart = true
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftPrimaryAxis2DClickActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightPrimaryAxis2DClickActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }
        private void OnPrimaryAxis2DClickCancelled(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "primary axis 2D click",
                IsInputStart = false
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftPrimaryAxis2DClickActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightPrimaryAxis2DClickActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }

        private void OnPrimaryAxis2DTouch(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "primary axis 2D touch",
                IsInputStart = true
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftPrimaryAxis2DTouchActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightPrimaryAxis2DTouchActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }
        private void OnPrimaryAxis2DTouchCancelled(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "primary axis 2D touch",
                IsInputStart = false
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftPrimaryAxis2DTouchActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightPrimaryAxis2DTouchActionReference.action)
            {
                //RIGHT
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
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "primary button",
                IsInputStart = true
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftPrimaryButtonInputActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightPrimaryButtonInputActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }
        private void OnPrimaryButtonCancelled(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "primary button",
                IsInputStart = false
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftPrimaryButtonInputActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightPrimaryButtonInputActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }

        private void OnPrimaryTouch(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "primary touch",
                IsInputStart = true
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftPrimaryTouchInputActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightPrimaryTouchInputActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }
        private void OnPrimaryTouchCancelled(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "primary touch",
                IsInputStart = false
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftPrimaryTouchInputActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightPrimaryTouchInputActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }

        private void OnSecondaryAxis2DClick(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "secondary axis 2D click",
                IsInputStart = true
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftSecondaryAxis2DClickActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightSecondaryAxis2DClickActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }
        private void OnSecondaryAxis2DClickCancelled(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "secondary axis 2D click",
                IsInputStart = false
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftSecondaryAxis2DClickActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightSecondaryAxis2DClickActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }

        private void OnSecondaryAxis2DTouch(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "secondary axis 2D touch",
                IsInputStart = true
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftSecondaryAxis2DTouchActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightSecondaryAxis2DTouchActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }
        private void OnSecondaryAxis2DTouchCancelled(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "secondary axis 2D touch",
                IsInputStart = false
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftSecondaryAxis2DTouchActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightSecondaryAxis2DTouchActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }

        private void OnSecondaryTouch(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "secondary touch",
                IsInputStart = true
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftSecondaryTouchActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightSecondaryTouchActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }
        private void OnSecondaryTouchCancelled(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "secondary touch",
                IsInputStart = false
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftSecondaryTouchActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightSecondaryTouchActionReference.action)
            {
                //RIGHT
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
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "secondary button",
                IsInputStart = true
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftSecondaryButtonActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightSecondaryButtonActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }
        private void OnSecondaryButtonCancelled(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "secondary button",
                IsInputStart = false
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftSecondaryButtonActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightSecondaryButtonActionReference.action)
            {
                //RIGHT
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
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "menu button",
                IsInputStart = true
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftMenuButtonActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightMenuButtonActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }
        private void OnMenuButtonCancelled(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "menu button",
                IsInputStart = false
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftMenuButtonActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightMenuButtonActionReference.action)
            {
                //RIGHT
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
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "trigger",
                IsInputStart = true
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftTriggerInputActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightTriggerInputActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }
        private void OnTriggerCancelled(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "trigger",
                IsInputStart = false
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftTriggerInputActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightTriggerInputActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
            {
                return;
            }

            _currentFrameInput.Add(frameInput);
        }

        private void OnPrimaryAxis2DTriggered(InputAction.CallbackContext ctx)
        {
            if(!IsRecording)
                return;

            var value = ctx.ReadValue<Vector2>();
            Debug.Log(value);
            var frameInput = new FrameInput
            {
                InputActionName = "Primary Axis 2D",
                Value = value.ToString()
            };
            
            //Check if the used controller was the left or right controller
            if (ctx.action == _leftPrimaryAxisActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightPrimaryAxisActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
                return;
            
            _currentFrameInput.Add(frameInput);
        }
        private void OnPrimaryAxis2DCancelled(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "Primary Axis 2D",
                Value = Vector2.zero.ToString()
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftPrimaryAxisActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightPrimaryAxisActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
                return;

            _currentFrameInput.Add(frameInput);
        }

        private void OnSecondaryAxis2DTriggered(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var value = ctx.ReadValue<Vector2>();
            Debug.Log(value);
            var frameInput = new FrameInput
            {
                InputActionName = "Secondary Axis 2D",
                Value = value.ToString()
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftSecondaryAxisActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightSecondaryAxisActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
                return;

            _currentFrameInput.Add(frameInput);
        }

        private void OnSecondaryAxis2DCancelled(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionName = "Secondary Axis 2D",
                Value = Vector2.zero.ToString()
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftSecondaryAxisActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;

            }
            else if (ctx.action == _rightSecondaryAxisActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
                return;

            _currentFrameInput.Add(frameInput);
        }
        
         #if UNITY_EDITOR
        [ContextMenu("Import InputActionAsset")]
        private void ImportActionAsset()
        {
            EditorInputDialog.Create("Import InputActionAsset", "Reference File")
                .GenericUnityObjectField<InputActionAsset>("InputActionAsset:", out var actionAsset)
                .BooleanField("Overwrite:", out var overwriteVal)
                .OnAccept(() =>
                {
                    if (actionAsset == null || actionAsset.Value == null)
                        return;

                    var fields = GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    fields = fields.Where(x => x.GetReturnType() == typeof(InputActionReference)).ToArray();

                    foreach (var field in fields)
                    {
                        if (!overwriteVal.Value && field.GetValue(this) != null)
                            continue;
                        foreach (var map in actionAsset.Value.actionMaps)
                        {
                            foreach (var action in map.actions)
                            {
                                var name = action.name.Split('/').LastOrDefault();
                                if (name == null)
                                    continue;
                                var parts = name.SplitCamelCase(" ").Split(' ');
                                bool containsAll = true;
                                foreach (var part in parts)
                                {
                                    if (string.IsNullOrWhiteSpace(part))
                                        continue;


                                    if (!field.Name.Contains(part, StringComparison.InvariantCultureIgnoreCase))
                                        containsAll = false;
                                }

                                if (containsAll)
                                {
                                    var actionReference = GetReference(action);
                                    field.SetValue(this, actionReference);
                                    break;
                                }

                            }
                        }
                    }
                })
                .Show();
        }

        private static InputActionReference GetReference(InputAction action)
        {
            var assets = AssetDatabase.FindAssets($"t:{nameof(InputActionReference)}");
            foreach (var asset in assets)
            {
                var path = AssetDatabase.GUIDToAssetPath(asset);
                var refAssets = AssetDatabase.LoadAllAssetsAtPath(path);

                foreach (var refAsset in refAssets.OfType<InputActionReference>())
                    if (refAsset.action.id == action.id)
                        return refAsset;
            }

            return InputActionReference.Create(action);
        }
#endif
    }
    
   

}
