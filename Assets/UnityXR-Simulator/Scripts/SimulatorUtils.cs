using System;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine.InputSystem;

namespace Rhinox.XR.UnityXR.Simulator
{
    internal static class SimulatorUtils
    {
        public static void Subscribe(InputActionReference reference,
            Action<InputAction.CallbackContext> performed = null,
            Action<InputAction.CallbackContext> canceled = null)
        {
            var action = GetInputAction(reference);
            if (action != null)
            {
                if (performed != null)
                    action.performed += performed;
                if (canceled != null)
                    action.canceled += canceled;
            }
        }

        public static void Unsubscribe(InputActionReference reference,
            Action<InputAction.CallbackContext> performed = null,
            Action<InputAction.CallbackContext> canceled = null)
        {
            var action = GetInputAction(reference);
            if (action != null)
            {
                if (performed != null)
                    action.performed -= performed;
                if (canceled != null)
                    action.canceled -= canceled;
            }
        }

        private static InputAction GetInputAction(InputActionReference actionReference)
        {
#pragma warning disable IDE0031 // Use null propagation -- Do not use for UnityEngine.Object types
            return actionReference != null ? actionReference.action : null;
#pragma warning restore IDE0031
        }
        public static string GetCurrentBindingPrefix(InputActionReference actionRef)
        {
            if (actionRef == null || actionRef.action == null)
                return string.Empty;

            var firstBinding = actionRef.action.bindings.FirstOrDefault();
            if (firstBinding == default)
                return string.Empty;

            string answer = firstBinding.effectivePath.Split('/').LastOrDefault();
            if (answer != null)
                return "[" + answer.ToUpperInvariant() + "]";
            return "";
        }
        
        
    }
}