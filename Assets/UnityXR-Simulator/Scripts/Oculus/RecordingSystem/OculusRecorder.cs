using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Rhinox.XR.UnityXR.Simulator.Oculus
{
    public class OculusRecorder : BaseRecorder
    {
        private List<FrameInput> _currentFrameInput = new List<FrameInput>();
        private List<FrameInput> _previousFrameInput = new List<FrameInput>();

        protected override List<FrameInput> GetFrameInputs(bool clearInputsAfterwards = true)
        {
            _previousFrameInput = _currentFrameInput;
            
            if (clearInputsAfterwards)
            {
                var returnValue = new List<FrameInput>(_currentFrameInput);
                _currentFrameInput.Clear();
                return returnValue;
            }

            return _currentFrameInput;
        }

        /// <summary>
        /// Called every frame to capture Oculus input data and register it as a FrameInput object
        /// </summary>
        private void Update()
        {
            if(!IsRecording)
                return;

            // Capture input from this frame

            // Iterate through all OVRInput.Button values
            foreach (OVRInput.Button inputButton in Enum.GetValues(typeof(OVRInput.Button)))
            {
                // Ignore 'Any' and 'None' buttons
                if (inputButton == OVRInput.Button.Any || inputButton == OVRInput.Button.None)
                    continue;
                if (OVRInput.GetDown(inputButton))
                    RegisterFrameInput(inputButton, true);
                else if (OVRInput.GetUp(inputButton))
                    RegisterFrameInput(inputButton, false);
            }

            // Iterate through all OVRInput.Axis1D values
            foreach (OVRInput.Axis1D axis in Enum.GetValues(typeof(OVRInput.Axis1D)))
            {
                // Ignore 'Any' and 'None' axes
                if (axis == OVRInput.Axis1D.Any || axis == OVRInput.Axis1D.None)
                    continue;

                // Get the current state of the axis
                float state = OVRInput.Get(axis);

                // Register the input if it is non-zero
                if (!Mathf.Approximately(state, 0))
                {
                    RegisterFrameInput(axis, state.ToString(CultureInfo.InvariantCulture));
                    continue;
                }

                // Check if the axis is from the right hand controller
                if (!axis.IsFromRightHand(out bool isFromRightHand))
                    continue;

                // Get the simulator input ID for the axis
                var axisID = axis.ToSimulatorInputID();

                // Ignore invalid input IDs
                if (axisID == SimulatorInputID.Invalid)
                    continue;

                // Find the index of the previous frame input with matching action ID, input type, and controller
                int previousFrameIdx = _previousFrameInput.FindIndex(x =>
                    x.InputActionID == axisID &&
                    x.InputType == SimulatorInputType.Axis1D &&
                    x.IsRightControllerInput == isFromRightHand);

                // Ignore input if it was not found in the previous frame
                if (previousFrameIdx == -1)
                    continue;

                // Parse the previous frame input value as a float
                float prevFrameAxisValue  = float.Parse(_previousFrameInput[previousFrameIdx].Value);

                // Register the input if the previous frame input value is non-zero
                if (!Mathf.Approximately(prevFrameAxisValue , 0))
                    RegisterFrameInput(axis, state.ToString());
            }


            // Iterate through all OVRInput.Axis2D values
            foreach (OVRInput.Axis2D axis in Enum.GetValues(typeof(OVRInput.Axis2D)))
            {
                if (axis == OVRInput.Axis2D.Any ||axis == OVRInput.Axis2D.None)
                    continue;

                var axisValue = OVRInput.Get(axis);
                if (!Mathf.Approximately(axisValue.x, 0) || !Mathf.Approximately(axisValue.y, 0))
                {
                    RegisterFrameInput(axis, axisValue.ToString());
                    continue;
                }

                // Check if the axis is from the right hand controller
                if (!axis.IsFromRightHand(out bool isFromRightHand))
                    continue;

                // Get the simulator input ID for the axis
                var axisID = axis.ToSimulatorInputID();

                // Ignore invalid input IDs
                if (axisID == SimulatorInputID.Invalid)
                    continue;

                // Find the index of the previous frame input with matching action ID, input type, and controller
                int previousFrameIdx = _previousFrameInput.FindIndex(x =>
                    x.InputActionID == axisID &&
                    x.InputType == SimulatorInputType.Axis1D &&
                    x.IsRightControllerInput == isFromRightHand);

                if (previousFrameIdx == -1)
                    continue;

                //Get value and check if it's magnitude is 0
                if (!SimulatorUtils.TryParseVector2(_previousFrameInput[previousFrameIdx].Value, out var inputValue))
                    continue;

                if (!Mathf.Approximately(inputValue.x, 0) || !Mathf.Approximately(inputValue.y, 0))
                    RegisterFrameInput(axis, axisValue.ToString());
            }
        }

        /// <summary>
        /// Registers a button input as a FrameInput object
        /// </summary>
        /// <param name="button">The input button to register</param>
        /// <param name="isInputStart">Whether the input is a button press or release</param>
        private void RegisterFrameInput(OVRInput.Button button, bool isInputStart)
        {
            if(!button.IsFromRightHand(out bool isFromRightHand))
                return;
            
            var buttonID = button.ToSimulatorInputID();
            if(buttonID == SimulatorInputID.Invalid)
                return;
            
            FrameInput newInput = new FrameInput()
            {
                InputActionID = buttonID,
                InputType = SimulatorInputType.Button,
                IsRightControllerInput = isFromRightHand,
                IsInputStart = isInputStart,
            };
            
            _currentFrameInput.Add(newInput);
        }

        /// <summary>
        /// Registers a 2D axis input as a FrameInput object
        /// </summary>
        /// <param name="axis">The input axis to register</param>
        /// <param name="axisVal">The value of the input axis</param>
        private void RegisterFrameInput(OVRInput.Axis2D axis, string axisVal)
        {
            if (!axis.IsFromRightHand(out bool isFromRightHand))
                return;

            var axisID = axis.ToSimulatorInputID();
            if (axisID == SimulatorInputID.Invalid)
                return;

            FrameInput newInput = new FrameInput()
            {
                InputActionID = axisID,
                InputType = SimulatorInputType.Axis2D,
                IsRightControllerInput = isFromRightHand,
                Value = axisVal,
            };

            _currentFrameInput.Add(newInput);
        }

        /// <summary>
        /// Registers a 1D axis input as a FrameInput object
        /// </summary>
        /// <param name="axis">The input axis to register</param>
        /// <param name="axisVal">The value of the input axis</param>
        private void RegisterFrameInput(OVRInput.Axis1D axis, string axisVal)
        {
            if (!axis.IsFromRightHand(out bool isFromRightHand))
                return;

            var axisID = axis.ToSimulatorInputID();
            if (axisID == SimulatorInputID.Invalid)
                return;

            FrameInput newInput = new FrameInput()
            {
                InputActionID = axisID,
                InputType = SimulatorInputType.Axis1D,
                IsRightControllerInput = isFromRightHand,
                Value = axisVal,
            };

            _currentFrameInput.Add(newInput);
        }
        
    }
    
}