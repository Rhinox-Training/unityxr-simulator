using System;
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
    [SerializeField] private InputActionReference _leftGripInputActionReference;
    [SerializeField] private InputActionReference _leftTriggerInputActionReference;
    [SerializeField] private InputActionReference _leftPrimaryButtonInputActionReference;
    [SerializeField] private InputActionReference _leftSecondaryButtonActionReference;
    [Space(10)] 
    [SerializeField] private InputActionReference _rightGripInputActionReference;
    [SerializeField] private InputActionReference _rightTriggerInputActionReference;
    [SerializeField] private InputActionReference _rightPrimaryButtonInputActionReference;
    [SerializeField] private InputActionReference _rightSecondaryButtonActionReference;

    private bool _leftGripPressed = false;
    private bool _rightGripPressed = false;
    private bool _leftTriggerPressed = false;
    private bool _rightTriggerPressed = false;
    private bool _leftPrimaryButtonPressed = false;
    private bool _rightPrimaryButtonPressed = false;
    private bool _leftSecondaryButtonPressed = false;
    private bool _rightSecondaryButtonPressed = false;
    
    /// <summary>
    /// See <see cref="MonoBehaviour"/>
    /// </summary>
    private void OnValidate()
    {
        Assert.AreNotEqual(_deviceSimulatorControls, null,
            $"{nameof(InputVisualization)}, device simulator controls not set!");
        Assert.AreNotEqual(_deviceSimulator, null, $"{nameof(InputVisualization)}, device simulator not set!");
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

        var subTitleStyle = new GUIStyle()
        {
            fontStyle = FontStyle.Italic,
            fontSize = 12,
            alignment = TextAnchor.MiddleCenter,

            normal =
            {
                textColor = Color.white
            }
        };

        var windowRect = new Rect(Screen.width - 300, 0, 300, Screen.height);

        GUI.Box(windowRect, "");
        GUILayout.BeginArea(windowRect);

        //--------------------------
        // Simulator Controls
        //--------------------------
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

        //--------------------------
        // Simulator Info
        //--------------------------
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
        
        if(_leftTriggerPressed)
            GUILayout.Label("Left TRIGGER pressed.");
        if (_rightTriggerPressed)
            GUILayout.Label("Right TRIGGER pressed.");
        
        if(_leftPrimaryButtonPressed)
            GUILayout.Label("Left PRIMARY BUTTON pressed.");
        if (_rightPrimaryButtonPressed)
            GUILayout.Label("Right PRIMARY BUTTON pressed.");
        
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
        
        SimulatorUtils.Subscribe(_leftTriggerInputActionReference, OnTriggerPressed, OnTriggerCancelled);
        SimulatorUtils.Subscribe(_rightTriggerInputActionReference, OnTriggerPressed, OnTriggerCancelled);

        SimulatorUtils.Subscribe(_leftPrimaryButtonInputActionReference, OnPrimaryButtonPressed, OnPrimaryButtonCancelled);
        SimulatorUtils.Subscribe(_rightPrimaryButtonInputActionReference, OnPrimaryButtonPressed, OnPrimaryButtonCancelled);


        SimulatorUtils.Subscribe(_leftSecondaryButtonActionReference, OnSecondaryButtonPressed, OnSecondaryButtonCancelled);
        SimulatorUtils.Subscribe(_rightSecondaryButtonActionReference, OnSecondaryButtonPressed, OnSecondaryButtonCancelled);
    }

    private void OnDisable()
    {
        SimulatorUtils.Unsubscribe(_leftGripInputActionReference, OnGripPressed, OnGripCancelled);
        SimulatorUtils.Unsubscribe(_rightGripInputActionReference, OnGripPressed, OnGripCancelled);

        SimulatorUtils.Unsubscribe(_leftTriggerInputActionReference, OnTriggerPressed, OnTriggerCancelled);
        SimulatorUtils.Unsubscribe(_rightTriggerInputActionReference, OnTriggerPressed, OnTriggerCancelled);

        SimulatorUtils.Unsubscribe(_leftPrimaryButtonInputActionReference, OnPrimaryButtonPressed,
            OnPrimaryButtonCancelled);
        SimulatorUtils.Unsubscribe(_rightPrimaryButtonInputActionReference, OnPrimaryButtonPressed,
            OnPrimaryButtonCancelled);

        SimulatorUtils.Unsubscribe(_leftSecondaryButtonActionReference, OnSecondaryButtonPressed,
            OnSecondaryButtonCancelled);
        SimulatorUtils.Unsubscribe(_rightSecondaryButtonActionReference, OnSecondaryButtonPressed,
            OnSecondaryButtonCancelled);
    }

    private void OnGripPressed(InputAction.CallbackContext ctx)
    {
        if (!ctx.performed)
            return;

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
        if (!ctx.performed)
            return;

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
        if (!ctx.performed)
            return;

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
        if (!ctx.performed)
            return;

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

}
