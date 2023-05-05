using Rhinox.XR.UnityXR.Simulator;
using UnityEngine;
using UnityEngine.InputSystem;

public class RealVRMovement : MonoBehaviour
{
    [SerializeField] private XRDeviceSimulatorControls _controls;
    [SerializeField] private OpenXRDeviceSimulator _simulator;
    [SerializeField] private InputActionReference _leftHandMoveActionReference;
    [SerializeField] private InputActionReference _rightHandMoveActionReference;
    private float _moveX;
    private float _moveY;
    private float _moveZ;

    private void Awake()
    {
        _controls = GetComponentInChildren<XRDeviceSimulatorControls>();
    }

    private void OnEnable()
    {
        SimulatorUtils.Subscribe(_leftHandMoveActionReference,OnLeftHandMovePerformed,OnLeftHandMoveCancelled);
        SimulatorUtils.Subscribe(_rightHandMoveActionReference,OnRightHandMovePerformed,OnRightHandMoveCancelled);
    }

    private void OnDisable()
    {
        SimulatorUtils.Unsubscribe(_leftHandMoveActionReference, OnLeftHandMovePerformed, OnLeftHandMoveCancelled);
        SimulatorUtils.Unsubscribe(_rightHandMoveActionReference, OnRightHandMovePerformed, OnRightHandMoveCancelled);
    }

    private void OnLeftHandMovePerformed(InputAction.CallbackContext ctx)
    {
        var movement = ctx.ReadValue<Vector2>();
        _moveX = movement.x;
        _moveZ = movement.y;
    }

    private void OnLeftHandMoveCancelled(InputAction.CallbackContext ctx)
    {
        _moveX = 0;
        _moveZ = 0;
    }
    private void OnRightHandMovePerformed(InputAction.CallbackContext ctx)
    {
        var movement = ctx.ReadValue<Vector2>();
        _moveY = movement.y;
    }

    private void OnRightHandMoveCancelled(InputAction.CallbackContext ctx)
    {
        _moveY = 0;
    }
    
    private void Update()
    {
        if (!_simulator.UsesRealVR)
            return;

        var transform1 = transform;
        var newPos = transform1.position;
        newPos += transform1.forward * (_moveZ * (_controls.KeyboardZTranslateSpeed * Time.deltaTime));
        newPos += transform1.right * (_moveX * (_controls.KeyboardXTranslateSpeed * Time.deltaTime));
        newPos += transform1.up * (_moveY * (_controls.KeyboardYTranslateSpeed * Time.deltaTime));
        transform1.position = newPos;
    }
}
