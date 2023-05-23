using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Utilities;

[StructLayout(LayoutKind.Auto)]
public struct PlaybackDeviceState: IInputStateTypeInfo
{
    public FourCC format => new FourCC('P', 'B', 'D', 'C');

    [InputControl(name = "LeftGrip", layout = "Axis")]
    public float LeftGrip;

    [InputControl(name = "LeftPrimaryAxis", layout = "Vector2")]
    public Vector2 LeftPrimaryAxis;
    
    [InputControl(name = "LeftPrimaryAxis2DClick", layout = "Button")]
    public float LeftPrimaryAxis2DClick;
    
    [InputControl(name = "LeftPrimaryAxis2DTouch", layout = "Button")]
    public float LeftPrimaryAxis2DTouch;
    
    [InputControl(name = "LeftPrimaryButton", layout = "Button")]
    public float LeftPrimaryButton;
    
    [InputControl(name = "LeftPrimaryTouch", layout = "Button")]
    public float LeftPrimaryTouch;

    [InputControl(name = "LeftSecondaryAxis", layout = "Vector2")]
    public Vector2 LeftSecondaryAxis;
    
    [InputControl(name = "LeftSecondaryAxis2DClick", layout = "Button")]
    public float LeftSecondaryAxis2DClick;
    
    [InputControl(name = "LeftSecondaryAxis2DTouch", layout = "Button")]
    public float LeftSecondaryAxis2DTouch;
    
    [InputControl(name = "LeftSecondaryButton", layout = "Button")]
    public float LeftSecondaryButton;
    
    [InputControl(name = "LeftSecondaryTouch", layout = "Button")]
    public float LeftSecondaryTouch;

    [InputControl(name = "LeftMenuButton", layout = "Button")]
    public float LeftMenuButton;
    
    [InputControl(name = "LeftTrigger", layout = "Axis")]
    public float LeftTrigger;    

    

    [InputControl(name = "RightGrip", layout = "Axis")]
    public float RightGrip;

    [InputControl(name = "RightPrimaryAxis", layout = "Vector2")]
    public Vector2 RightPrimaryAxis;
    
    [InputControl(name = "RightPrimaryAxis2DClick", layout = "Button")]
    public float RightPrimaryAxis2DClick;
    
    [InputControl(name = "RightPrimaryAxis2DTouch", layout = "Button")]
    public float RightPrimaryAxis2DTouch;
    
    [InputControl(name = "RightPrimaryButton", layout = "Button")]
    public float RightPrimaryButton;
    
    [InputControl(name = "RightPrimaryTouch", layout = "Button")]
    public float RightPrimaryTouch;

    [InputControl(name = "RightSecondaryAxis", layout = "Vector2")]
    public Vector2 RightSecondaryAxis;
    
    [InputControl(name = "RightSecondaryAxis2DClick", layout = "Button")]
    public float RightSecondaryAxis2DClick;
    
    [InputControl(name = "RightSecondaryAxis2DTouch", layout = "Button")]
    public float RightSecondaryAxis2DTouch;
    
    [InputControl(name = "RightSecondaryButton", layout = "Button")]
    public float RightSecondaryButton;
    
    [InputControl(name = "RightSecondaryTouch", layout = "Button")]
    public float RightSecondaryTouch;

    [InputControl(name = "RightMenuButton", layout = "Button")]
    public float RightMenuButton;
    
    [InputControl(name = "RightTrigger", layout = "Axis")]
    public float RightTrigger;    
    
}
