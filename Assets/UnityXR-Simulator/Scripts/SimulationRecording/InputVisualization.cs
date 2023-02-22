using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.InputSystem;
using Rhinox.XR.UnityXR.Simulator;


/// <summary>
/// This component visualizes pressed input.A GUI is used to represent all keys.
/// </summary>
public class InputVisualization : MonoBehaviour
{
    [Header("Input parameters")]
    [SerializeField] private XRDeviceSimulatorControls _deviceSimulatorControls;
    [SerializeField] private BetterXRDeviceSimulator _deviceSimulator;
    [SerializeField] private SimulationRecorder _recorder;
    [SerializeField] private SimulationPlayback _playback;
    [Space(10)]
    [Header("Input actions")]
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
    
    public Rect ControlsWindowRect;
    public Rect InputWindowRect;

    private bool _leftGripPressed;
    private bool _rightGripPressed;
    
    private bool _leftPrimaryAxis2DClick;
    private bool _rightPrimaryAxis2DClick;
    
    private bool _leftPrimaryAxis2DTouch;
    private bool _rightPrimaryAxis2DTouch;
    
    private bool _leftPrimaryButtonPressed;
    private bool _rightPrimaryButtonPressed;
    
    private bool _leftPrimaryTouch;
    private bool _rightPrimaryTouch;
    
    private bool _leftSecondaryAxis2DClick;
    private bool _rightSecondaryAxis2DClick;
    
    private bool _leftSecondaryAxis2DTouch;
    private bool _rightSecondaryAxis2DTouch;
    
    private bool _leftSecondaryTouch;
    private bool _rightSecondaryTouch;
    
    private bool _leftMenuButtonPressed;
    private bool _rightMenuButtonPressed;
    
    private bool _leftTriggerPressed;
    private bool _rightTriggerPressed;
    
    private bool _leftSecondaryButtonPressed;
    private bool _rightSecondaryButtonPressed;
    
    /// <summary>
    /// See <see cref="MonoBehaviour"/>
    /// </summary>
    private void OnValidate()
    {
        Assert.AreNotEqual(_deviceSimulatorControls, null,
            $"{nameof(InputVisualization)}, device simulator controls not set!");
        Assert.AreNotEqual(_deviceSimulator, null, $"{nameof(InputVisualization)}, device simulator not set!");
    }

    private void Awake()
    {
        InputWindowRect.x = Screen.width - 300;
        InputWindowRect.y = 0;
        InputWindowRect.width = 300;
        InputWindowRect.height = 2f * Screen.height / 3;

        ControlsWindowRect.x = 0;
        ControlsWindowRect.y = 0;
        ControlsWindowRect.width = 200;
        ControlsWindowRect.height = Screen.height / 2f;
    }

    /// <summary>
    /// See <see cref="MonoBehaviour"/>
    /// </summary>
    private void OnGUI()
    {
        var titleStyle = new GUIStyle()
        {
            fontStyle = FontStyle.Bold,
            fontSize = 16,
            alignment = TextAnchor.MiddleCenter,

            normal =
            {
                textColor = Color.white
            }
        };

        GUI.Box(ControlsWindowRect,"");
        GUILayout.BeginArea(ControlsWindowRect);
        //--------------------------
        // Simulator Controls
        //--------------------------
        GUILayout.Space(10);
        GUILayout.Label("Controls",titleStyle);
        GUILayout.Label(
            $"{SimulatorUtils.GetCurrentBindingPrefix(_deviceSimulatorControls.ToggleManipulateAction)} Mode: {_deviceSimulatorControls.ManipulationTarget}");
        GUILayout.Label(
            $"{SimulatorUtils.GetCurrentBindingPrefix(_deviceSimulatorControls.ToggleKeyboardSpaceAction)} Keyboard Space: {_deviceSimulatorControls.KeyboardTranslateSpace}");
        GUILayout.Label(
            $"{SimulatorUtils.GetCurrentBindingPrefix(_deviceSimulatorControls.ToggleButtonControlTargetAction)} Controller Buttons: {(_deviceSimulatorControls.ManipulateRightControllerButtons ? "Right" : "Left")}");
        GUILayout.Label(
            $"{SimulatorUtils.GetCurrentBindingPrefix(_recorder.BeginRecordingActionReference)} to start recording");
        GUILayout.Label(
            $"{SimulatorUtils.GetCurrentBindingPrefix(_recorder.EndRecordingActionReference)} to end recording");
        GUILayout.Label(
            $"{SimulatorUtils.GetCurrentBindingPrefix(_playback.StartPlaybackActionReference)} to start playback");
        GUILayout.Label(
            $"{SimulatorUtils.GetCurrentBindingPrefix(_playback.ReimportRecordingActionReference)} to (re)import recording");
        GUILayout.Label(
            $"{SimulatorUtils.GetCurrentBindingPrefix(_playback.AbortPlaybackActionReference)} to abort playback");
        GUILayout.EndArea();
        
        
        
        //--------------------------
        // Simulator INPUT WINDOW
        //--------------------------

        GUI.Box(InputWindowRect, "");
        GUILayout.BeginArea(InputWindowRect);
        //--------------------------
        // Simulator Info
        //--------------------------
        GUILayout.Space(10);
        if(_recorder.IsRecording)
            GUILayout.Label("Currently recording.");
        if (_playback.IsPlaying)
            GUILayout.Label("Currently playing back.");
        //--------------------------
        // DEVICE POSITIONS
        //--------------------------
        GUILayout.Label("Device transforms",titleStyle);

        GUILayout.Label($"HMD position: {_deviceSimulator.HMDState.devicePosition}");
        GUILayout.Label($"HMD rotation: {_deviceSimulator.HMDState.deviceRotation}");

        GUILayout.Label($"Right controller position: {_deviceSimulator.RightControllerState.devicePosition}");
        GUILayout.Label($"Right controller rotation: {_deviceSimulator.RightControllerState.deviceRotation}");
        
        GUILayout.Label($"Left controller position: {_deviceSimulator.LeftControllerState.devicePosition}");
        GUILayout.Label($"Left controller rotation: {_deviceSimulator.LeftControllerState.deviceRotation}");

        GUILayout.Space(10);
        GUILayout.Label(
            _deviceSimulatorControls.ManipulateRightControllerButtons
                ? $"Current manipulated controller: right"
                : $"Current manipulated controller: left");
        
        //--------------------------
        // INPUT 
        //--------------------------
        GUILayout.Label("Used input", titleStyle);
        
        if(_leftGripPressed)
            GUILayout.Label("Left GRIP pressed.");
        if (_rightGripPressed)
            GUILayout.Label("Right GRIP pressed.");

        if (_leftPrimaryAxis2DClick)
            GUILayout.Label("Left PRIMARY AXIS 2D Click pressed.");
        if (_rightPrimaryAxis2DClick)
            GUILayout.Label("Right PRIMARY AXIS 2D Click pressed.");

        if (_leftPrimaryAxis2DTouch)
            GUILayout.Label("Left PRIMARY AXIS 2D TOUCH pressed.");
        if (_rightPrimaryAxis2DTouch)
            GUILayout.Label("Right PRIMARY AXIS 2D TOUCH pressed.");
        
        if(_leftTriggerPressed)
            GUILayout.Label("Left TRIGGER pressed.");
        if (_rightTriggerPressed)
            GUILayout.Label("Right TRIGGER pressed.");
        
        if(_leftPrimaryButtonPressed)
            GUILayout.Label("Left PRIMARY BUTTON pressed.");
        if (_rightPrimaryButtonPressed)
            GUILayout.Label("Right PRIMARY BUTTON pressed.");

        if (_leftPrimaryTouch)
            GUILayout.Label("Left PRIMARY TOUCH pressed.");
        if (_rightPrimaryTouch)
            GUILayout.Label("Right PRIMARY TOUCH pressed.");

        if (_leftSecondaryAxis2DClick)
            GUILayout.Label("Left SECONDARY AXIS 2D Click pressed.");
        if (_rightSecondaryAxis2DClick)
            GUILayout.Label("Right SECONDARY AXIS 2D Click pressed.");

        if (_leftSecondaryAxis2DTouch)
            GUILayout.Label("Left SECONDARY AXIS 2D TOUCH pressed.");
        if (_rightSecondaryAxis2DTouch)
            GUILayout.Label("Right SECONDARY AXIS 2D TOUCH pressed.");

        if (_leftSecondaryTouch)
            GUILayout.Label("Left SECONDARY TOUCH pressed.");
        if (_rightSecondaryTouch)
            GUILayout.Label("Right SECONDARY TOUCH pressed.");

        if (_leftMenuButtonPressed)
            GUILayout.Label("Left MENU BUTTON pressed.");
        if (_rightMenuButtonPressed)
            GUILayout.Label("Right MENU BUTTON pressed.");
        
        if(_leftSecondaryButtonPressed)
            GUILayout.Label("Left SECONDARY BUTTON pressed.");
        if(_rightSecondaryButtonPressed)
            GUILayout.Label("Right SECONDARY BUTTON pressed.");
        
        GUILayout.EndArea();
    }

    private void OnEnable()
    {
        SimulatorUtils.Subscribe(_leftGripInputActionReference, OnGripPressed, OnGripCancelled);
        SimulatorUtils.Subscribe(_rightGripInputActionReference, OnGripPressed, OnGripCancelled);

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

    private void OnDisable()
    {
        SimulatorUtils.Unsubscribe(_leftGripInputActionReference, OnGripPressed, OnGripCancelled);
        SimulatorUtils.Unsubscribe(_rightGripInputActionReference, OnGripPressed, OnGripCancelled);

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
    
    private void OnGripPressed(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftGripInputActionReference.action)
            _leftGripPressed = true;
        else if (ctx.action == _rightGripInputActionReference.action)
            _rightGripPressed = true;
    }
    private void OnGripCancelled(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftGripInputActionReference.action)
            _leftGripPressed = false;
        else if (ctx.action == _rightGripInputActionReference.action)
            _rightGripPressed = false;
    }

    private void OnTriggerPressed(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftTriggerInputActionReference.action)
            _leftTriggerPressed = true;
        else if (ctx.action == _rightTriggerInputActionReference.action)
            _rightTriggerPressed = true;
    }
    private void OnTriggerCancelled(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftTriggerInputActionReference.action)
            _leftTriggerPressed = false;
        else if (ctx.action == _rightTriggerInputActionReference.action)
            _rightTriggerPressed = false;
    }

    private void OnPrimaryButtonPressed(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftPrimaryButtonInputActionReference.action)
            _leftPrimaryButtonPressed = true;
        else if (ctx.action == _rightPrimaryButtonInputActionReference.action)
            _rightPrimaryButtonPressed = true;
    }
    private void OnPrimaryButtonCancelled(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftPrimaryButtonInputActionReference.action)
            _leftPrimaryButtonPressed = false;
        else if (ctx.action == _rightPrimaryButtonInputActionReference.action)
            _rightPrimaryButtonPressed = false;
    }

    private void OnSecondaryButtonPressed(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftSecondaryButtonActionReference.action)
            _leftSecondaryButtonPressed = true;
        else if (ctx.action == _rightSecondaryButtonActionReference.action)
            _rightSecondaryButtonPressed = true;
    }
    private void OnSecondaryButtonCancelled(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftSecondaryButtonActionReference.action)
            _leftSecondaryButtonPressed = false;
        else if (ctx.action == _rightSecondaryButtonActionReference.action)
            _rightSecondaryButtonPressed = false;
    }

    private void OnPrimaryAxis2DClick(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftPrimaryAxis2DClickActionReference.action)
            _leftPrimaryAxis2DClick = true;
        else if (ctx.action == _rightPrimaryAxis2DClickActionReference.action)
            _rightPrimaryAxis2DClick = true;
    }
    private void OnPrimaryAxis2DClickCancelled(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftPrimaryAxis2DClickActionReference.action)
            _leftPrimaryAxis2DClick = false;
        else if (ctx.action == _rightPrimaryAxis2DClickActionReference.action)
            _rightPrimaryAxis2DClick = false;
    }

    private void OnPrimaryAxis2DTouch(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftPrimaryAxis2DTouchActionReference.action)
            _leftPrimaryAxis2DTouch = true;
        else if (ctx.action == _rightPrimaryAxis2DTouchActionReference.action)
            _rightPrimaryAxis2DTouch = true;
    }
    private void OnPrimaryAxis2DTouchCancelled(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftPrimaryAxis2DTouchActionReference.action)
            _leftPrimaryAxis2DTouch = false;
        else if (ctx.action == _rightPrimaryAxis2DTouchActionReference.action)
            _rightPrimaryAxis2DTouch = false;
    }

    private void OnPrimaryTouch(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftPrimaryTouchInputActionReference.action)
            _leftPrimaryTouch = true;
        else if (ctx.action == _rightPrimaryTouchInputActionReference.action)
            _rightPrimaryTouch = true;
    }
    private void OnPrimaryTouchCancelled(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftPrimaryTouchInputActionReference.action)
            _leftPrimaryTouch = false;
        else if (ctx.action == _rightPrimaryTouchInputActionReference.action)
            _rightPrimaryTouch = false;
    }

    private void OnSecondaryAxis2DClick(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftSecondaryAxis2DClickActionReference.action)
            _leftSecondaryAxis2DClick = true;
        else if (ctx.action == _rightSecondaryAxis2DClickActionReference.action)
            _rightSecondaryAxis2DClick = true;
    }
    private void OnSecondaryAxis2DClickCancelled(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftSecondaryAxis2DClickActionReference.action)
            _leftSecondaryAxis2DClick = false;
        else if (ctx.action == _rightSecondaryAxis2DClickActionReference.action)
            _rightSecondaryAxis2DClick = false;
    }

    private void OnSecondaryAxis2DTouch(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftSecondaryAxis2DTouchActionReference.action)
            _leftSecondaryAxis2DTouch = true;
        else if (ctx.action == _rightSecondaryAxis2DTouchActionReference.action)
            _rightSecondaryAxis2DTouch = true;
    }
    private void OnSecondaryAxis2DTouchCancelled(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftSecondaryAxis2DTouchActionReference.action)
            _leftSecondaryAxis2DTouch = false;
        else if (ctx.action == _rightSecondaryAxis2DTouchActionReference.action)
            _rightSecondaryAxis2DTouch = false;
    }

    private void OnSecondaryTouch(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftSecondaryTouchActionReference.action)
            _leftSecondaryTouch = true;
        else if (ctx.action == _rightSecondaryTouchActionReference.action)
            _rightSecondaryTouch = true;
    }
    private void OnSecondaryTouchCancelled(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftSecondaryTouchActionReference.action)
            _leftSecondaryTouch = false;
        else if (ctx.action == _rightSecondaryTouchActionReference.action)
            _rightSecondaryTouch = false;
    }

    private void OnMenuButtonPressed(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftMenuButtonActionReference.action)
            _leftMenuButtonPressed = true;
        else if (ctx.action == _rightMenuButtonActionReference.action)
            _rightMenuButtonPressed = true;
    }
    private void OnMenuButtonCancelled(InputAction.CallbackContext ctx)
    {
        //Check if the used controller was the left or right controller
        if (ctx.action == _leftMenuButtonActionReference.action)
            _leftMenuButtonPressed = false;
        else if (ctx.action == _rightMenuButtonActionReference.action)
            _rightMenuButtonPressed = false;
    }

}
