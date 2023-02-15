using Rhinox.VOLT.XR.UnityXR.Simulator;
using UnityEngine;
using UnityEngine.Assertions;

/// <summary>
/// This component visualizes pressed input.A GUI is used to represent all keys.
/// </summary>
public class InputVisualization : MonoBehaviour
{
    [Header("Window Parameters")]
    [Tooltip("The desired position of the bottomLeft vertex of the window.")]
    [SerializeField] private Vector2Int _windowPos = new Vector2Int(0, 0);

    [Tooltip("The desired width and height of the window")]
    [SerializeField] private Vector2Int _windowDimensions = new Vector2Int(150, 180);

    [Header("Input parameters")]
    [SerializeField] private XRDeviceSimulatorControls _deviceSimulatorControls = null;

    /// <summary>
    /// See <see cref="MonoBehaviour"/>
    /// </summary>
    private void OnValidate()
    {
        Assert.AreNotEqual(_deviceSimulatorControls, null, $"{nameof(InputVisualization)}, device simulator not set!");
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

        //Left controller window
        var windowRect = new Rect(_windowPos.x, _windowPos.y, _windowDimensions.x,
            _windowDimensions.y);

        GUI.Box(windowRect, "");
        GUILayout.BeginArea(windowRect);

        GUILayout.Label("Used input",titleStyle);
        GUILayout.Label(
            _deviceSimulatorControls.ManipulateRightControllerButtons
                ? $"Current manipulated controller: right"
                : $"Current manipulated controller: left", subTitleStyle);

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
