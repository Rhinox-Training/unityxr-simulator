using System;
using Rhinox.XR.UnityXR.Simulator;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Simulation;

/// <summary>
/// This component visualizes pressed input.A GUI is used to represent all keys.
/// </summary>
public class InputVisualization : MonoBehaviour
{
    [Header("Window Parameters")]
    [Tooltip("The desired width and height of the window")]
    [SerializeField] private Vector2Int _windowDimensions = new Vector2Int(150, 180);

    [Header("Input parameters")]
    [SerializeField] private XRDeviceSimulatorControls _deviceSimulatorControls = null;
    [SerializeField] private BetterXRDeviceSimulator _deviceSimulator = null;

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

        var windowRect = new Rect(Screen.width - _windowDimensions.x, 0, _windowDimensions.x,
            _windowDimensions.y);

        GUI.Box(windowRect, "");
        GUILayout.BeginArea(windowRect);
        
        
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
        GUILayout.Label("Used input",titleStyle);

        if(_deviceSimulatorControls.Axis2DInput.x != 0 || _deviceSimulatorControls.Axis2DInput.y != 0)
            GUILayout.Label($"2D axis input: {_deviceSimulatorControls.Axis2DInput.ToString()}");
        
        if (_deviceSimulatorControls.RestingHandAxis2DInput.x != 0 || _deviceSimulatorControls.RestingHandAxis2DInput.y != 0)
            GUILayout.Label($"Resting hand 2D axis input: {_deviceSimulatorControls.RestingHandAxis2DInput.ToString()}");

        if (_deviceSimulatorControls.GripInput)
            GUILayout.Label($"Grip pressed.");
        
        if(_deviceSimulatorControls.TriggerInput)
            GUILayout.Label($"Trigger pressed.");

        if (_deviceSimulatorControls.PrimaryButtonInput)
            GUILayout.Label($"Primary button pressed.");

        if (_deviceSimulatorControls.SecondaryButtonInput)
            GUILayout.Label($"Secondary button pressed.");

        if (_deviceSimulatorControls.MenuInput)
            GUILayout.Label($"Menu button pressed.");

        if (_deviceSimulatorControls.Primary2DAxisClickInput)
            GUILayout.Label($"Primary 2D axis clicked.");
        if (_deviceSimulatorControls.Secondary2DAxisClickInput)
            GUILayout.Label($"Secondary 2D axis clicked.");
        
        if (_deviceSimulatorControls.Primary2DAxisTouchInput)
            GUILayout.Label($"Primary 2D axis touched.");
        if (_deviceSimulatorControls.Secondary2DAxisTouchInput)
            GUILayout.Label($"Secondary 2D axis touched.");

        if (_deviceSimulatorControls.PrimaryTouchInput)
            GUILayout.Label($"Primary touch pressed.");
        if (_deviceSimulatorControls.SecondaryTouchInput)
            GUILayout.Label($"Secondary touch pressed.");
        
        GUILayout.EndArea();
    }
}
