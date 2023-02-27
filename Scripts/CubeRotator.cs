using UnityEngine;
using UnityEngine.InputSystem;

namespace Rhinox.XR.UnityXR.Simulator
{
    public class CubeRotator : MonoBehaviour
    {
        [SerializeField] private float _rotationSpeed = 2f;
        [SerializeField] private InputActionReference _joystickInputActionReference;
        private Vector2 _currentInputValue;

        private void Update()
        {
            var currentRotation = transform.eulerAngles;
            currentRotation.y += _rotationSpeed * _currentInputValue.x * Time.deltaTime;
            currentRotation.x += _rotationSpeed * _currentInputValue.y * Time.deltaTime;
            transform.eulerAngles = currentRotation;
        }


        private void OnEnable()
        {
            SimulatorUtils.Subscribe( _joystickInputActionReference,OnJoystickPerformed,OnJoystickCancelled);
        }

        private void OnDisable()
        {
            SimulatorUtils.Unsubscribe(_joystickInputActionReference, OnJoystickPerformed, OnJoystickCancelled);
        }

        private void OnJoystickPerformed(InputAction.CallbackContext ctx)
        {
            _currentInputValue = ctx.ReadValue<Vector2>();
        }

        private void OnJoystickCancelled(InputAction.CallbackContext ctx)
        {
            _currentInputValue = Vector2.zero;
        }
        
        
    }
}

