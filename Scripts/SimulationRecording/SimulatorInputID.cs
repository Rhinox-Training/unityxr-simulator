namespace Rhinox.XR.UnityXR.Simulator
{
    public enum SimulatorInputID
    {
        Grip = 0,
        Trigger = 1,
        PrimaryAxis = 2,
        PrimaryAxisClick = 3,
        PrimaryAxisTouch = 4,
        PrimaryButton = 5,
        PrimaryTouch = 6,
        SecondaryAxis = 7,
        SecondaryAxisClick = 8,
        SecondaryAxisTouch = 9,
        SecondaryButton = 10,
        SecondaryTouch = 11,
        Menu = 12,
        
        
        Invalid = 13
    }

    public enum SimulatorInputType
    {
        Button = 0x1,
        Axis2D = 0x2,
        Axis1D = 0x4
    }
}