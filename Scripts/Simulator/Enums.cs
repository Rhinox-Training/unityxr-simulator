using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace Rhinox.VOLT.XR.UnityXR.Simulator
{
    public enum ManipulationTarget
    {
        RightHand,
        LeftHand,
        Head,
        All
    }
    
    /// <summary>
    /// The coordinate space in which to operate.
    /// </summary>
    public enum Space
    {
        /// <summary>
        /// Applies translations of a controller or HMD relative to its own coordinate space, considering its own rotations.
        /// Will translate a controller relative to itself, independent of the camera.
        /// </summary>
        Local,
 
        /// <summary>
        /// Applies translations of a controller or HMD relative to its parent. If the object does not have a parent, meaning
        /// it is a root object, the parent coordinate space is the same as the world coordinate space. This is the same
        /// as <see cref="Local"/> but without considering its own rotations.
        /// </summary>
        Parent,

        /// <summary>
        /// Applies translations of a controller or HMD relative to the screen.
        /// Will translate a controller relative to the camera, independent of the controller's orientation.
        /// </summary>
        Screen,
    }

    /// <summary>
    /// The transformation mode in which to operate.
    /// </summary>
    public enum TransformationMode
    {
        Translate,
        Rotate
    }

    /// <summary>
    /// The target device control(s) to update from input.
    /// </summary>
    /// <remarks>
    /// <see cref="FlagsAttribute"/> to support updating multiple controls from input
    /// (e.g. to drive the primary and secondary 2D axis on a controller from the same input).
    /// </remarks>
    [Flags]
    public enum Axis2DTargets
    {
        /// <summary>
        /// Do not update device state from input.
        /// </summary>
        None = 0,

        /// <summary>
        /// Update device position from input.
        /// </summary>
        Position = 1 << 0,

        /// <summary>
        /// Update the primary touchpad or joystick on a controller device from input.
        /// </summary>
        Primary2DAxis = 1 << 1,

        /// <summary>
        /// Update the secondary touchpad or joystick on a controller device from input.
        /// </summary>
        Secondary2DAxis = 1 << 2,
    }

    public static class EnumHelper
    {
        public static TransformationMode Negate(TransformationMode mode)
        {
            switch (mode)
            {
                case TransformationMode.Rotate:
                    return TransformationMode.Translate;
                case TransformationMode.Translate:
                    return TransformationMode.Rotate;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(mode)}={mode}.");
                    return TransformationMode.Rotate;
            }
        }

        public static CursorLockMode Negate(CursorLockMode mode, CursorLockMode fallback)
        {
            switch (mode)
            {
                case CursorLockMode.None:
                    return fallback;
                case CursorLockMode.Locked:
                case CursorLockMode.Confined:
                    return CursorLockMode.None;
                default:
                    Assert.IsTrue(false, $"Unhandled {nameof(mode)}={mode}.");
                    return CursorLockMode.None;
            }
        }
        
        public static void GetAxes(Space translateSpace, Transform cameraTransform, out Vector3 right, out Vector3 up, out Vector3 forward)
        {
            if (cameraTransform == null)
                throw new ArgumentNullException(nameof(cameraTransform));

            switch (translateSpace)
            {
                case Space.Local:
                    // Makes the assumption that the Camera and the Controllers are siblings
                    // (meaning they share a parent GameObject).
                    var cameraParent = cameraTransform.parent;
                    if (cameraParent != null)
                    {
                        right = cameraParent.TransformDirection(Vector3.right);
                        up = cameraParent.TransformDirection(Vector3.up);
                        forward = cameraParent.TransformDirection(Vector3.forward);
                    }
                    else
                    {
                        right = Vector3.right;
                        up = Vector3.up;
                        forward = Vector3.forward;
                    }

                    break;
                case Space.Parent:
                    right = Vector3.right;
                    up = Vector3.up;
                    forward = Vector3.forward;
                    break;
                case Space.Screen:
                    right = cameraTransform.TransformDirection(Vector3.right);
                    up = cameraTransform.TransformDirection(Vector3.up);
                    forward = cameraTransform.TransformDirection(Vector3.forward);
                    break;
                default:
                    right = Vector3.right;
                    up = Vector3.up;
                    forward = Vector3.forward;
                    Assert.IsTrue(false, $"Unhandled {nameof(translateSpace)}={translateSpace}.");
                    return;
            }
        }
    }
}