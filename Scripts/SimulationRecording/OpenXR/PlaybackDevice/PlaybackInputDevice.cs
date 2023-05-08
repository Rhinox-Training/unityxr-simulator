using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.HID;
using UnityEngine.InputSystem.Layouts;
using UnityEngine.InputSystem.LowLevel;

#if UNITY_EDITOR
[InitializeOnLoad]
#endif
[InputControlLayout(displayName = "PlaybackInputDevice", stateType = typeof(PlaybackDeviceState))]
public class PlaybackInputDevice : InputDevice
{
    public AxisControl LeftGrip { get; private set; }
    public Vector2Control LeftPrimaryAxis { get; private set; }
    public ButtonControl LeftPrimaryAxis2DClick { get; private set; }
    public ButtonControl LeftPrimaryAxis2DTouch { get; private set; }
    public ButtonControl LeftPrimaryButton { get; private set; }
    public ButtonControl LeftPrimaryTouch { get; private set; }
    public Vector2Control LeftSecondaryAxis { get; private set; }
    public ButtonControl LeftSecondaryAxis2DClick { get; private set; }
    public ButtonControl LeftSecondaryAxis2DTouch { get; private set; }
    public ButtonControl LeftSecondaryButton { get; private set; }
    public ButtonControl LeftSecondaryTouch { get; private set; }
    public ButtonControl LeftMenuButton { get; private set; }
    public AxisControl LeftTrigger { get; private set; }

    public AxisControl RightGrip { get; private set; }
    public Vector2Control RightPrimaryAxis { get; private set; }
    public ButtonControl RightPrimaryAxis2DClick { get; private set; }
    public ButtonControl RightPrimaryAxis2DTouch { get; private set; }
    public ButtonControl RightPrimaryButton { get; private set; }
    public ButtonControl RightPrimaryTouch { get; private set; }
    public Vector2Control RightSecondaryAxis { get; private set; }
    public ButtonControl RightSecondaryAxis2DClick { get; private set; }
    public ButtonControl RightSecondaryAxis2DTouch { get; private set; }
    public ButtonControl RightSecondaryButton { get; private set; }
    public ButtonControl RightSecondaryTouch { get; private set; }
    public ButtonControl RightMenuButton { get; private set; }
    public AxisControl RightTrigger { get; private set; }
    
    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void InitializeInPlayer()
    {
    }
    static PlaybackInputDevice()
    {
        InputSystem.RegisterLayout<PlaybackInputDevice>();
    }
    
    
    protected override void FinishSetup()
    {
        base.FinishSetup();

        LeftGrip = GetChildControl<AxisControl>("LeftGrip");
        LeftPrimaryAxis = GetChildControl<Vector2Control>("LeftPrimaryAxis");
        LeftPrimaryAxis2DClick = GetChildControl<ButtonControl>("LeftPrimaryAxis2DClick");
        LeftPrimaryAxis2DTouch = GetChildControl<ButtonControl>("LeftPrimaryAxis2DTouch");
        LeftPrimaryButton = GetChildControl<ButtonControl>("LeftPrimaryButton");
        LeftPrimaryTouch = GetChildControl<ButtonControl>("LeftPrimaryTouch");
        LeftSecondaryAxis = GetChildControl<Vector2Control>("LeftSecondaryAxis");
        LeftSecondaryAxis2DClick = GetChildControl<ButtonControl>("LeftSecondaryAxis2DClick");
        LeftSecondaryAxis2DTouch = GetChildControl<ButtonControl>("LeftSecondaryAxis2DTouch");
        LeftSecondaryButton = GetChildControl<ButtonControl>("LeftSecondaryButton");
        LeftSecondaryTouch = GetChildControl<ButtonControl>("LeftSecondaryTouch");
        LeftMenuButton = GetChildControl<ButtonControl>("LeftMenuButton");
        LeftTrigger = GetChildControl<AxisControl>("LeftTrigger");

        RightGrip = GetChildControl<AxisControl>("RightGrip");
        RightPrimaryAxis = GetChildControl<Vector2Control>("RightPrimaryAxis");
        RightPrimaryAxis2DClick = GetChildControl<ButtonControl>("RightPrimaryAxis2DClick");
        RightPrimaryAxis2DTouch = GetChildControl<ButtonControl>("RightPrimaryAxis2DTouch");
        RightPrimaryButton = GetChildControl<ButtonControl>("RightPrimaryButton");
        RightPrimaryTouch = GetChildControl<ButtonControl>("RightPrimaryTouch");
        RightSecondaryAxis = GetChildControl<Vector2Control>("RightSecondaryAxis");
        RightSecondaryAxis2DClick = GetChildControl<ButtonControl>("RightSecondaryAxis2DClick");
        RightSecondaryAxis2DTouch = GetChildControl<ButtonControl>("RightSecondaryAxis2DTouch");
        RightSecondaryButton = GetChildControl<ButtonControl>("RightSecondaryButton");
        RightSecondaryTouch = GetChildControl<ButtonControl>("RightSecondaryTouch");
        RightMenuButton = GetChildControl<ButtonControl>("RightMenuButton");
        RightTrigger = GetChildControl<AxisControl>("RightTrigger");
    }
}
