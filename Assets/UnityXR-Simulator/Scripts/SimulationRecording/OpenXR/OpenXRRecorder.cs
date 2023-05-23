using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Rhinox.Lightspeed;
using Rhinox.Lightspeed.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

#if UNITY_EDITOR
using UnityEditor;
using Rhinox.GUIUtils.Editor;
#endif

namespace Rhinox.XR.UnityXR.Simulator
{
    public class OpenXRRecorder : BaseRecorder
    {
        [Space(15)] [Header("Open XR Input actions")] [SerializeField]
        private InputActionReference _leftGripInputActionReference;

        [SerializeField] private InputActionReference _leftGripValueInputActionReference;
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
        [SerializeField] private InputActionReference _leftTriggerValueInputActionReference;
        [SerializeField] private InputActionReference _leftSecondaryButtonActionReference;

        [Space(10)] [SerializeField] private InputActionReference _rightGripInputActionReference;
        [SerializeField] private InputActionReference _rightGripValueInputActionReference;
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
        [SerializeField] private InputActionReference _rightTriggerValueInputActionReference;
        [SerializeField] private InputActionReference _rightSecondaryButtonActionReference;

        private List<FrameInput> _currentFrameInput = new List<FrameInput>();

        protected override void PerformOnOnEnable()
        {
            SubscribeControllerActions();
        }

        protected override void PerformOnOnDisable()
        {
            UnSubscribeControllerActions();
        }

        protected override List<FrameInput> GetFrameInputs(bool clearInputsAfterwards = true)
        {
            if (clearInputsAfterwards)
            {
                var returnValue = new List<FrameInput>(_currentFrameInput);
                _currentFrameInput.Clear();
                return returnValue;
            }

            return _currentFrameInput;
        }

        private void SubscribeControllerActions()
        {
            SimulatorUtils.Subscribe(_leftGripInputActionReference, OnGripPressed, OnGripCancelled);
            SimulatorUtils.Subscribe(_rightGripInputActionReference, OnGripPressed, OnGripCancelled);

            SimulatorUtils.Subscribe(_leftGripValueInputActionReference, OnGripValueTriggered, OnGripValueCancelled);
            SimulatorUtils.Subscribe(_rightGripValueInputActionReference, OnGripValueTriggered, OnGripValueCancelled);

            SimulatorUtils.Subscribe(_leftPrimaryAxisActionReference, OnPrimaryAxis2DTriggered,
                OnPrimaryAxis2DCancelled);
            SimulatorUtils.Subscribe(_rightPrimaryAxisActionReference, OnPrimaryAxis2DTriggered,
                OnPrimaryAxis2DCancelled);

            SimulatorUtils.Subscribe(_leftSecondaryAxisActionReference, OnSecondaryAxis2DTriggered,
                OnSecondaryAxis2DCancelled);
            SimulatorUtils.Subscribe(_rightSecondaryAxisActionReference, OnSecondaryAxis2DTriggered,
                OnSecondaryAxis2DCancelled);

            SimulatorUtils.Subscribe(_leftPrimaryAxis2DClickActionReference, OnPrimaryAxis2DClick,
                OnPrimaryAxis2DClickCancelled);
            SimulatorUtils.Subscribe(_rightPrimaryAxis2DClickActionReference, OnPrimaryAxis2DClick,
                OnPrimaryAxis2DClickCancelled);

            SimulatorUtils.Subscribe(_leftPrimaryAxis2DTouchActionReference, OnPrimaryAxis2DTouch,
                OnPrimaryAxis2DTouchCancelled);
            SimulatorUtils.Subscribe(_rightPrimaryAxis2DTouchActionReference, OnPrimaryAxis2DTouch,
                OnPrimaryAxis2DTouchCancelled);

            SimulatorUtils.Subscribe(_leftPrimaryButtonInputActionReference, OnPrimaryButtonPressed,
                OnPrimaryButtonCancelled);
            SimulatorUtils.Subscribe(_rightPrimaryButtonInputActionReference, OnPrimaryButtonPressed,
                OnPrimaryButtonCancelled);

            SimulatorUtils.Subscribe(_leftPrimaryTouchInputActionReference, OnPrimaryTouch, OnPrimaryTouchCancelled);
            SimulatorUtils.Subscribe(_rightPrimaryTouchInputActionReference, OnPrimaryTouch, OnPrimaryTouchCancelled);

            SimulatorUtils.Subscribe(_leftSecondaryAxis2DClickActionReference, OnSecondaryAxis2DClick,
                OnSecondaryAxis2DClickCancelled);
            SimulatorUtils.Subscribe(_rightSecondaryAxis2DClickActionReference, OnSecondaryAxis2DClick,
                OnSecondaryAxis2DClickCancelled);

            SimulatorUtils.Subscribe(_leftSecondaryAxis2DTouchActionReference, OnSecondaryAxis2DTouch,
                OnSecondaryAxis2DTouchCancelled);
            SimulatorUtils.Subscribe(_rightSecondaryAxis2DTouchActionReference, OnSecondaryAxis2DTouch,
                OnSecondaryAxis2DTouchCancelled);

            SimulatorUtils.Subscribe(_leftSecondaryTouchActionReference, OnSecondaryTouch, OnSecondaryTouchCancelled);
            SimulatorUtils.Subscribe(_rightSecondaryTouchActionReference, OnSecondaryTouch, OnSecondaryTouchCancelled);

            SimulatorUtils.Subscribe(_leftMenuButtonActionReference, OnMenuButtonPressed, OnMenuButtonCancelled);
            SimulatorUtils.Subscribe(_rightMenuButtonActionReference, OnMenuButtonPressed, OnMenuButtonCancelled);

            SimulatorUtils.Subscribe(_leftTriggerInputActionReference, OnTriggerPressed, OnTriggerCancelled);
            SimulatorUtils.Subscribe(_rightTriggerInputActionReference, OnTriggerPressed, OnTriggerCancelled);

            SimulatorUtils.Subscribe(_leftTriggerValueInputActionReference, OnTriggerValueTriggered,
                OnTriggerValueCancelled);
            SimulatorUtils.Subscribe(_rightTriggerValueInputActionReference, OnTriggerValueTriggered,
                OnTriggerValueCancelled);

            SimulatorUtils.Subscribe(_leftSecondaryButtonActionReference, OnSecondaryButtonPressed,
                OnSecondaryButtonCancelled);
            SimulatorUtils.Subscribe(_rightSecondaryButtonActionReference, OnSecondaryButtonPressed,
                OnSecondaryButtonCancelled);
        }

        private void UnSubscribeControllerActions()
        {
            SimulatorUtils.Unsubscribe(_leftGripInputActionReference, OnGripPressed, OnGripCancelled);
            SimulatorUtils.Unsubscribe(_rightGripInputActionReference, OnGripPressed, OnGripCancelled);

            SimulatorUtils.Unsubscribe(_leftPrimaryAxisActionReference, OnPrimaryAxis2DTriggered,
                OnPrimaryAxis2DCancelled);
            SimulatorUtils.Unsubscribe(_rightPrimaryAxisActionReference, OnPrimaryAxis2DTriggered,
                OnPrimaryAxis2DCancelled);

            SimulatorUtils.Unsubscribe(_leftSecondaryAxisActionReference, OnSecondaryAxis2DTriggered,
                OnSecondaryAxis2DCancelled);
            SimulatorUtils.Unsubscribe(_rightSecondaryAxisActionReference, OnSecondaryAxis2DTriggered,
                OnSecondaryAxis2DCancelled);

            SimulatorUtils.Unsubscribe(_leftPrimaryAxis2DClickActionReference, OnPrimaryAxis2DClick,
                OnPrimaryAxis2DClickCancelled);
            SimulatorUtils.Unsubscribe(_rightPrimaryAxis2DClickActionReference, OnPrimaryAxis2DClick,
                OnPrimaryAxis2DClickCancelled);

            SimulatorUtils.Unsubscribe(_leftPrimaryAxis2DTouchActionReference, OnPrimaryAxis2DTouch,
                OnPrimaryAxis2DTouchCancelled);
            SimulatorUtils.Unsubscribe(_rightPrimaryAxis2DTouchActionReference, OnPrimaryAxis2DTouch,
                OnPrimaryAxis2DTouchCancelled);

            SimulatorUtils.Unsubscribe(_leftPrimaryButtonInputActionReference, OnPrimaryButtonPressed,
                OnPrimaryButtonCancelled);
            SimulatorUtils.Unsubscribe(_rightPrimaryButtonInputActionReference, OnPrimaryButtonPressed,
                OnPrimaryButtonCancelled);

            SimulatorUtils.Unsubscribe(_leftPrimaryTouchInputActionReference, OnPrimaryTouch, OnPrimaryTouchCancelled);
            SimulatorUtils.Unsubscribe(_rightPrimaryTouchInputActionReference, OnPrimaryTouch, OnPrimaryTouchCancelled);

            SimulatorUtils.Unsubscribe(_leftSecondaryAxis2DClickActionReference, OnSecondaryAxis2DClick,
                OnSecondaryAxis2DClickCancelled);
            SimulatorUtils.Unsubscribe(_rightSecondaryAxis2DClickActionReference, OnSecondaryAxis2DClick,
                OnSecondaryAxis2DClickCancelled);

            SimulatorUtils.Unsubscribe(_leftSecondaryAxis2DTouchActionReference, OnSecondaryAxis2DTouch,
                OnSecondaryAxis2DTouchCancelled);
            SimulatorUtils.Unsubscribe(_rightSecondaryAxis2DTouchActionReference, OnSecondaryAxis2DTouch,
                OnSecondaryAxis2DTouchCancelled);

            SimulatorUtils.Unsubscribe(_leftSecondaryTouchActionReference, OnSecondaryTouch, OnSecondaryTouchCancelled);
            SimulatorUtils.Unsubscribe(_rightSecondaryTouchActionReference, OnSecondaryTouch,
                OnSecondaryTouchCancelled);

            SimulatorUtils.Unsubscribe(_leftMenuButtonActionReference, OnMenuButtonPressed, OnMenuButtonCancelled);
            SimulatorUtils.Unsubscribe(_rightMenuButtonActionReference, OnMenuButtonPressed, OnMenuButtonCancelled);

            SimulatorUtils.Unsubscribe(_leftTriggerInputActionReference, OnTriggerPressed, OnTriggerCancelled);
            SimulatorUtils.Unsubscribe(_rightTriggerInputActionReference, OnTriggerPressed, OnTriggerCancelled);

            SimulatorUtils.Unsubscribe(_leftTriggerValueInputActionReference, OnTriggerValueTriggered,
                OnTriggerValueCancelled);
            SimulatorUtils.Unsubscribe(_rightTriggerValueInputActionReference, OnTriggerValueTriggered,
                OnTriggerValueCancelled);

            SimulatorUtils.Unsubscribe(_leftSecondaryButtonActionReference, OnSecondaryButtonPressed,
                OnSecondaryButtonCancelled);
            SimulatorUtils.Unsubscribe(_rightSecondaryButtonActionReference, OnSecondaryButtonPressed,
                OnSecondaryButtonCancelled);
        }


        //-----------------------
        // INPUT EVENTS
        //-----------------------
        private void OnGripPressed(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionID = SimulatorInputID.Grip,
                InputType = SimulatorInputType.Button,
                IsInputStart = true
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

        private void OnGripCancelled(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionID = SimulatorInputID.Grip,
                InputType = SimulatorInputType.Button,
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

        private void OnGripValueTriggered(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionID = SimulatorInputID.Grip,
                InputType = SimulatorInputType.Axis1D,
                Value = ctx.ReadValue<float>().ToString(CultureInfo.InvariantCulture)
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftGripValueInputActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;
            }
            else if (ctx.action == _rightGripValueInputActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
                return;

            _currentFrameInput.Add(frameInput);
        }

        private void OnGripValueCancelled(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionID = SimulatorInputID.Grip,
                InputType = SimulatorInputType.Axis1D,
                Value = 0.ToString()
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftGripValueInputActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;
            }
            else if (ctx.action == _rightGripValueInputActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
                return;

            _currentFrameInput.Add(frameInput);
        }

        private void OnTriggerValueTriggered(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionID = SimulatorInputID.Trigger,
                InputType = SimulatorInputType.Axis1D,
                Value = ctx.ReadValue<float>().ToString(CultureInfo.InvariantCulture)
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftTriggerValueInputActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;
            }
            else if (ctx.action == _rightTriggerValueInputActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
                return;

            _currentFrameInput.Add(frameInput);
        }

        private void OnTriggerValueCancelled(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionID = SimulatorInputID.Trigger,
                InputType = SimulatorInputType.Axis1D,
                Value = 0.ToString()
            };

            //Check if the used controller was the left or right controller
            if (ctx.action == _leftTriggerValueInputActionReference.action)
            {
                //LEFT
                frameInput.IsRightControllerInput = false;
            }
            else if (ctx.action == _rightTriggerValueInputActionReference.action)
            {
                //RIGHT
                frameInput.IsRightControllerInput = true;
            }
            else
                return;

            _currentFrameInput.Add(frameInput);
        }

        private void OnPrimaryAxis2DClick(InputAction.CallbackContext ctx)
        {
            if (!IsRecording)
                return;

            var frameInput = new FrameInput
            {
                InputActionID = SimulatorInputID.PrimaryAxisClick,
                InputType = SimulatorInputType.Button,
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
                InputActionID = SimulatorInputID.PrimaryAxisClick,
                InputType = SimulatorInputType.Button,
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
                InputActionID = SimulatorInputID.PrimaryAxisTouch,
                InputType = SimulatorInputType.Button,
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
                InputActionID = SimulatorInputID.PrimaryAxisTouch,
                InputType = SimulatorInputType.Button,
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
                InputActionID = SimulatorInputID.PrimaryButton,
                InputType = SimulatorInputType.Button,
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
                InputActionID = SimulatorInputID.PrimaryButton,
                InputType = SimulatorInputType.Button,
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
                InputActionID = SimulatorInputID.PrimaryTouch,
                InputType = SimulatorInputType.Button,
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
                InputActionID = SimulatorInputID.PrimaryTouch,
                InputType = SimulatorInputType.Button,
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
                InputActionID = SimulatorInputID.SecondaryAxisClick,
                InputType = SimulatorInputType.Button,
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
                InputActionID = SimulatorInputID.SecondaryAxisClick,
                InputType = SimulatorInputType.Button,
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
                InputActionID = SimulatorInputID.SecondaryAxisTouch,
                InputType = SimulatorInputType.Button,
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
                InputActionID = SimulatorInputID.SecondaryAxisTouch,
                InputType = SimulatorInputType.Button,
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
                InputActionID = SimulatorInputID.SecondaryTouch,
                InputType = SimulatorInputType.Button,
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
                InputActionID = SimulatorInputID.SecondaryTouch,
                InputType = SimulatorInputType.Button,
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
                InputActionID = SimulatorInputID.SecondaryButton,
                InputType = SimulatorInputType.Button,
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
                InputActionID = SimulatorInputID.SecondaryButton,
                InputType = SimulatorInputType.Button,
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
                InputActionID = SimulatorInputID.Menu,
                InputType = SimulatorInputType.Button,
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
                InputActionID = SimulatorInputID.Menu,
                InputType = SimulatorInputType.Button,
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
                InputActionID = SimulatorInputID.Trigger,
                InputType = SimulatorInputType.Button,
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
                InputActionID = SimulatorInputID.Menu,
                InputType = SimulatorInputType.Button,
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
            if (!IsRecording)
                return;

            var value = ctx.ReadValue<Vector2>();
            Debug.Log(value);
            var frameInput = new FrameInput
            {
                InputActionID = SimulatorInputID.PrimaryAxis,
                InputType = SimulatorInputType.Axis2D,
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
                InputActionID = SimulatorInputID.PrimaryAxis,
                InputType = SimulatorInputType.Axis2D,
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
                InputActionID = SimulatorInputID.SecondaryAxis,
                InputType = SimulatorInputType.Axis2D,
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
                InputActionID = SimulatorInputID.SecondaryAxis,
                InputType = SimulatorInputType.Axis2D,
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

                    var fields = GetType()
                        .GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
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