using System;
using UnityEngine;

namespace Rhinox.XR.UnityXR.Simulator.Oculus
{
    public static class OVRInputExtensions
    {
        /// <summary>
        /// Returns true if the button is from the right hand through the out isFromRightHand parameters.
        /// </summary>
        /// <param name="button"></param>
        /// <param name="isFromRightHand"></param>
        /// <returns>Whether this button is supported.</returns>
        public static bool IsFromRightHand(this OVRInput.Button button, out bool isFromRightHand)
        {
            switch (button)
            {
                case OVRInput.Button.One:
                case OVRInput.Button.Two:
                case OVRInput.Button.SecondaryShoulder:
                case OVRInput.Button.SecondaryIndexTrigger:
                case OVRInput.Button.SecondaryHandTrigger:
                case OVRInput.Button.SecondaryThumbstick:
                case OVRInput.Button.SecondaryThumbstickUp:
                case OVRInput.Button.SecondaryThumbstickDown:
                case OVRInput.Button.SecondaryThumbstickLeft:
                case OVRInput.Button.SecondaryThumbstickRight:
                case OVRInput.Button.SecondaryTouchpad:
                {
                    isFromRightHand = true;
                    break;
                }
                case OVRInput.Button.Three:
                case OVRInput.Button.Four:
                case OVRInput.Button.Start:
                case OVRInput.Button.Back:
                case OVRInput.Button.PrimaryShoulder:
                case OVRInput.Button.PrimaryIndexTrigger:
                case OVRInput.Button.PrimaryHandTrigger:
                case OVRInput.Button.PrimaryThumbstick:
                case OVRInput.Button.PrimaryThumbstickUp:
                case OVRInput.Button.PrimaryThumbstickDown:
                case OVRInput.Button.PrimaryThumbstickLeft:
                case OVRInput.Button.PrimaryThumbstickRight:
                case OVRInput.Button.PrimaryTouchpad:
                {
                    isFromRightHand = false;
                    break;
                }
                default:
                {
                    isFromRightHand = false;
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// Returns true if the axis is from the right hand through the out isFromRightHand parameters.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="isFromRightHand"></param>
        /// <returns>Whether this axis is supported.</returns>
        public static bool IsFromRightHand(this OVRInput.Axis1D axis, out bool isFromRightHand)
        {
            switch (axis)
            {
                case OVRInput.Axis1D.PrimaryIndexTrigger:
                case OVRInput.Axis1D.PrimaryHandTrigger:
                case OVRInput.Axis1D.PrimaryIndexTriggerCurl:
                case OVRInput.Axis1D.PrimaryIndexTriggerSlide:
                case OVRInput.Axis1D.PrimaryThumbRestForce:
                case OVRInput.Axis1D.PrimaryStylusForce:
                {
                    isFromRightHand = false;
                    return true;
                }
                case OVRInput.Axis1D.SecondaryIndexTrigger:
                case OVRInput.Axis1D.SecondaryHandTrigger:
                case OVRInput.Axis1D.SecondaryIndexTriggerCurl:
                case OVRInput.Axis1D.SecondaryIndexTriggerSlide:
                case OVRInput.Axis1D.SecondaryThumbRestForce:
                case OVRInput.Axis1D.SecondaryStylusForce:
                {
                    isFromRightHand = true;
                    return true;
                }
                default:
                {
                    isFromRightHand = false;
                    return false;
                }
            }
        }

        /// <summary>
        /// Returns true if the axis is from the right hand through the out isFromRightHand parameters.
        /// </summary>
        /// <param name="axis"></param>
        /// <param name="isFromRightHand"></param>
        /// <returns>Whether this axis is supported.</returns>
        public static bool IsFromRightHand(this OVRInput.Axis2D axis, out bool isFromRightHand)
        {
            switch (axis)
            {
                case OVRInput.Axis2D.PrimaryThumbstick:
                case OVRInput.Axis2D.PrimaryTouchpad:
                {
                    isFromRightHand = false;
                    return true;
                }
                case OVRInput.Axis2D.SecondaryThumbstick:
                case OVRInput.Axis2D.SecondaryTouchpad:
                {
                    isFromRightHand = true;
                    return true;
                }
                default:
                {
                    isFromRightHand = false;
                    return false;
                }
            }
        }

        public static SimulatorInputID ToSimulatorInputID(this OVRInput.Button button)
        {
            switch (button)
            {
                case OVRInput.Button.One:
                case OVRInput.Button.Three:
                {
                    return SimulatorInputID.PrimaryButton;
                }
                case OVRInput.Button.Two:
                case OVRInput.Button.Four:
                {
                    return SimulatorInputID.SecondaryButton;
                }

                case OVRInput.Button.Start:
                case OVRInput.Button.Back:
                {
                    return SimulatorInputID.Menu;
                }

                case OVRInput.Button.PrimaryIndexTrigger:
                case OVRInput.Button.SecondaryIndexTrigger:
                {
                    return SimulatorInputID.Trigger;
                }

                case OVRInput.Button.PrimaryHandTrigger:
                case OVRInput.Button.SecondaryHandTrigger:
                {
                    return SimulatorInputID.Grip;
                }

                case OVRInput.Button.PrimaryThumbstick:
                case OVRInput.Button.SecondaryThumbstick:
                {
                    return SimulatorInputID.PrimaryAxisClick;
                }

                default:
                    return SimulatorInputID.Invalid;
            }
        }

        public static SimulatorInputID ToSimulatorInputID(this OVRInput.Axis1D axis)
        {
            switch (axis)
            {
                case OVRInput.Axis1D.PrimaryIndexTrigger:
                case OVRInput.Axis1D.SecondaryIndexTrigger:
                    return SimulatorInputID.Trigger;
                case OVRInput.Axis1D.PrimaryHandTrigger:
                case OVRInput.Axis1D.SecondaryHandTrigger:
                    return SimulatorInputID.Grip;
                default:
                    return SimulatorInputID.Invalid;
            }
        }

        public static SimulatorInputID ToSimulatorInputID(this OVRInput.Axis2D axis)
        {
            switch (axis)
            {
                case OVRInput.Axis2D.PrimaryThumbstick:
                case OVRInput.Axis2D.SecondaryThumbstick:
                    return SimulatorInputID.PrimaryAxis;
                
                case OVRInput.Axis2D.PrimaryTouchpad:
                case OVRInput.Axis2D.SecondaryTouchpad:
                    return SimulatorInputID.SecondaryAxis;
                
                default:
                    return SimulatorInputID.Invalid;
            }
        }


    }
}