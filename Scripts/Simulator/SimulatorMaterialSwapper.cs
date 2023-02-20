// using System;
// using System.Linq;
// using UnityEngine;
//
// namespace Rhinox.XR.UnityXR.Simulator
// {
//     [DisallowMultipleComponent]
//     [RequireComponent(typeof(BetterXRDeviceSimulator))]
//     public class SimulatorMaterialSwapper : MonoBehaviour
//     {
//         private BetterXRDeviceSimulator _simulator;
//         private bool _wasRightSelected;
//         private bool _simulationControlActive;
//         private ControllerInfo _leftControllerInfo;
//         private ControllerInfo _rightControllerInfo;
//         private MeshRenderer[] _leftHandRenderers;
//         private MeshRenderer[] _rightHandRenderers;
//         private Material[][] _originalLeftMaterials;
//         private Material[][] _originalRightMaterials;
//
//         private void OnEnable()
//         {
//             _simulator = GetComponent<BetterXRDeviceSimulator>();
//             _simulator.SimulatorLoaded += OnSimulatorLoaded;
//             _simulator.SimulatorUnloaded += OnSimulatorUnloaded;
//
//             if (_simulator.IsLoaded)
//                 OnSimulatorLoaded();
//         }
//
//         private void OnDisable()
//         {
//             _simulator.SimulatorLoaded -= OnSimulatorLoaded;
//             _simulator.SimulatorUnloaded -= OnSimulatorUnloaded;
//         }
//
//         private void OnSimulatorLoaded()
//         {
//             _simulationControlActive = true;
//             InitializeControllerRendererCache();
//         }
//
//         private void OnSimulatorUnloaded()
//         {
//             if (_leftHandRenderers != null)
//             {
//                 for (var j = 0; j < _leftHandRenderers.Length; j++)
//                 {
//                     var r = _leftHandRenderers[j];
//                     for (int i = 0; i < r.sharedMaterials.Length; ++i)
//                     {
//                         r.sharedMaterials[i] = _originalLeftMaterials[j][i];
//                     }
//                 }
//             }
//
//
//             if (_rightHandRenderers != null) 
//             {
//                 for (var j = 0; j < _rightHandRenderers.Length; j++)
//                 {
//                     var r = _rightHandRenderers[j];
//                     for (int i = 0; i < r.sharedMaterials.Length; ++i)
//                     {
//                         r.sharedMaterials[i] = _originalRightMaterials[j][i];
//                     }
//                 }
//             }
//
//             _simulationControlActive = false;
//         }
//
//         private void Update()
//         {
//             if (!_simulationControlActive)
//                 return;
//
//             if (_leftControllerInfo == null || _rightControllerInfo == null)
//                 InitializeControllerRendererCache();
//
//             if (_wasRightSelected != _simulator.IsRightTargeted)
//             {
//                 UpdateControllerMaterials(_simulator.IsRightTargeted);
//                 _wasRightSelected = _simulator.IsRightTargeted;
//             }
//         }
//
//         private void UpdateControllerMaterials(bool simulatorIsRightTargeted)
//         {
//             if (_leftHandRenderers != null)
//             {
//                 for (var j = 0; j < _leftHandRenderers.Length; j++)
//                 {
//                     var r = _leftHandRenderers[j];
//                     for (int i = 0; i < r.sharedMaterials.Length; ++i)
//                     {
//                         r.sharedMaterials[i].color = simulatorIsRightTargeted ? Color.gray : Color.white;
//                     }
//
//                     _leftHandRenderers[j] = r;
//                 }
//             }
//
//
//             if (_rightHandRenderers != null) 
//             {
//                 for (var j = 0; j < _rightHandRenderers.Length; j++)
//                 {
//                     var r = _rightHandRenderers[j];
//                     for (int i = 0; i < r.sharedMaterials.Length; ++i)
//                     {
//                         r.sharedMaterials[i].color = simulatorIsRightTargeted ? Color.white : Color.gray;
//                     }
//                     _rightHandRenderers[j] = r;
//                 }
//             }
//         }
//
//
//         private void InitializeControllerRendererCache()
//         {
//             var player = PlayerManager.Instance.ActivePlayer;
//             if (player == null)
//                 return;
//
//             player.TryGetComponentOfHand(Hand.Left, out _leftControllerInfo, true);
//             player.TryGetComponentOfHand(Hand.Right, out _rightControllerInfo, true);
//
//             if (_leftControllerInfo != null)
//             {
//                 _leftHandRenderers = _leftControllerInfo.MeshRoot.GetComponentsInChildren<MeshRenderer>();
//                 _originalLeftMaterials = _leftHandRenderers.Select(x => x.sharedMaterials).ToArray();
//                 foreach (var r in _leftHandRenderers)
//                 {
//                     for (int i = 0; i < r.sharedMaterials.Length; ++i)
//                     {
//                         r.sharedMaterials[i] = new Material(r.materials[i]);
//                     }
//                 }
//             }
//
//             if (_rightControllerInfo != null)
//             {
//                 _rightHandRenderers = _rightControllerInfo.MeshRoot.GetComponentsInChildren<MeshRenderer>();
//                 _originalRightMaterials = _rightHandRenderers.Select(x => x.sharedMaterials).ToArray();
//                 foreach (var r in _rightHandRenderers)
//                 {
//                     for (int i = 0; i < r.sharedMaterials.Length; ++i)
//                     {
//                         r.sharedMaterials[i] = new Material(r.materials[i]);
//                     }
//                 }
//             }
//
//             UpdateControllerMaterials(_simulator.IsRightTargeted);
//         }
//
//     }
// }