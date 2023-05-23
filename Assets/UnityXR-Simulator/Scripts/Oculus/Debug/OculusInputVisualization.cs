//using Rhinox.XR.Oculus.Simulator;
using UnityEngine;
//using Rhinox.XR;
using System;

/// <summary>
/// This component visualizes pressed input.A GUI is used to represent all keys.
/// </summary>
/// 
namespace Rhinox.XR.UnityXR.Simulator
{
    public class OculusInputVisualization : MonoBehaviour
    {
        [Header("Input parameters")]
        [SerializeField] private XRDeviceSimulatorControls _deviceSimulatorControls;
        [SerializeField] private OculusDeviceSimulator _deviceSimulator;
        // [SerializeField] private SimulationRecorder _recorder;
        // [SerializeField] private SimulationPlayback _playback;

        private void Start()
        {
            if (_deviceSimulatorControls == null)
                Debug.LogError($"{nameof(OculusInputVisualization)}, device simulator controls not set!");
            if (_deviceSimulator == null)
                Debug.LogError($"{nameof(OculusInputVisualization)}, device simulator not set!");
            // if (_recorder == null)
            //     Debug.LogError($"{nameof(OculusInputVisualization)}, recorder not set!");
            // if (_playback == null)
            //     Debug.LogError($"{nameof(OculusInputVisualization)}, playback not set!");
        }

        /// <summary>
        /// See <see cref="MonoBehaviour"/>
        /// </summary>
        private void OnGUI()
        {
            if (_deviceSimulatorControls == null || _deviceSimulator == null /*|| _recorder == null || _playback == null*/)
                return;

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
            // GUILayout.Label(
            //     $"{SimulatorUtils.GetCurrentBindingPrefix(_recorder.BeginRecordingActionReference)} to start recording");
            // GUILayout.Label(
            //     $"{SimulatorUtils.GetCurrentBindingPrefix(_recorder.EndRecordingActionReference)} to end recording");
            // GUILayout.Label(
            //     $"{SimulatorUtils.GetCurrentBindingPrefix(_playback.StartPlaybackActionReference)} to start playback");

            //--------------------------
            // Simulator Info
            //--------------------------
            // if (_recorder.IsRecording)
            //     GUILayout.Label("Currently recording.");
            // if (_playback.IsPlaying)
            //     GUILayout.Label("Currently playing back.");
            //--------------------------
            // DEVICE POSITIONS
            //--------------------------
            GUILayout.Label("Device transforms", titleStyle);

            GUILayout.Label($"HMD position: {_deviceSimulator.RIG.centerEyeAnchor.localPosition}");
            GUILayout.Label($"HMD rotation: {_deviceSimulator.RIG.centerEyeAnchor.localEulerAngles}");

            //var rightHand = OVRManager.GetOpenVRControllerOffset(UnityEngine.XR.XRNode.RightHand);
            GUILayout.Label($"Right controller position: {_deviceSimulator.RIG.rightHandAnchor.localPosition}");
            GUILayout.Label($"Right controller rotation: {_deviceSimulator.RIG.rightHandAnchor.localEulerAngles}");

            //var leftHand = OVRManager.GetOpenVRControllerOffset(UnityEngine.XR.XRNode.LeftHand);
            GUILayout.Label($"Left controller position: {_deviceSimulator.RIG.leftHandAnchor.localPosition}");
            GUILayout.Label($"Left controller rotation: {_deviceSimulator.RIG.leftHandAnchor.localEulerAngles}");

            GUILayout.Space(10);
            GUILayout.Label(
                _deviceSimulatorControls.ManipulateRightControllerButtons
                    ? $"Current manipulated controller: right"
                    : $"Current manipulated controller: left");

            //--------------------------
            // INPUT 
            //--------------------------
            GUILayout.Space(10);
            GUILayout.Label("Used input", titleStyle);

            foreach (OVRInput.Button btn in Enum.GetValues(typeof(OVRInput.Button)))
            {
                if (btn == OVRInput.Button.Any || btn == OVRInput.Button.None)
                    continue;

                if (OVRInput.Get(btn))
                    GUILayout.Label($"{btn} pressed");
            }

            GUILayout.EndArea();
        }
    }
}