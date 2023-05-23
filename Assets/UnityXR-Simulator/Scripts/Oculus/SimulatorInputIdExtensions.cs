using System;

namespace Rhinox.XR.UnityXR.Simulator.Oculus
{
    public static class SimulatorInputIdExtensions
    {
        public static bool TryToOVRButton(this SimulatorInputID inputID, bool isRightHand, out OVRInput.Button button)
        {
            switch (inputID)
            {
                case SimulatorInputID.Grip:
                    button = (isRightHand)
                        ? OVRInput.Button.SecondaryHandTrigger
                        : OVRInput.Button.PrimaryHandTrigger;
                    break;
                case SimulatorInputID.Trigger:
                    button = (isRightHand)
                        ? OVRInput.Button.SecondaryIndexTrigger
                        : OVRInput.Button.PrimaryIndexTrigger;
                    break;
                case SimulatorInputID.PrimaryAxisClick:
                    button = (isRightHand)
                        ? OVRInput.Button.SecondaryThumbstick
                        : OVRInput.Button.PrimaryThumbstick;
                    break;
                case SimulatorInputID.PrimaryButton:
                    button = (isRightHand)
                        ? OVRInput.Button.One
                        : OVRInput.Button.Three;
                    break;
                case SimulatorInputID.SecondaryAxisClick:
                    button = (isRightHand)
                        ? OVRInput.Button.SecondaryTouchpad
                        : OVRInput.Button.PrimaryTouchpad;
                    break;
                case SimulatorInputID.SecondaryButton:
                    button = (isRightHand)
                        ? OVRInput.Button.Two
                        : OVRInput.Button.Four;
                    break;
                case SimulatorInputID.Menu:
                    button = (isRightHand)
                        ? OVRInput.Button.Back
                        : OVRInput.Button.Start;
                    break;
                default:
                    button = OVRInput.Button.None;
                    return false;
            }

            return true;
        }

        public static bool TryToOVRAxis1D(this SimulatorInputID inputID, bool isRightHand, out OVRInput.Axis1D axis)
        {
            switch (inputID)
            {
                case SimulatorInputID.Grip:
                    axis = (isRightHand) ? OVRInput.Axis1D.SecondaryHandTrigger : OVRInput.Axis1D.PrimaryHandTrigger;
                    break;
                case SimulatorInputID.Trigger:
                    axis = (isRightHand) ? OVRInput.Axis1D.SecondaryIndexTrigger : OVRInput.Axis1D.PrimaryIndexTrigger;
                    break;
                default:
                    axis = OVRInput.Axis1D.None;
                    return false;
            }

            return true;
        }

        public static bool TryToOVRAxis2D(this SimulatorInputID inputID, bool isRightHand, out OVRInput.Axis2D axis)
        {
            switch (inputID)
            {
                case SimulatorInputID.PrimaryAxis:
                    axis = (isRightHand) ? OVRInput.Axis2D.SecondaryThumbstick : OVRInput.Axis2D.PrimaryThumbstick;
                    break;
                case SimulatorInputID.SecondaryAxis:
                    axis = (isRightHand) ? OVRInput.Axis2D.SecondaryTouchpad : OVRInput.Axis2D.PrimaryTouchpad;
                    break;
                default:
                    axis = OVRInput.Axis2D.None;
                    return false;
            }

            return true;
        }
    }
}