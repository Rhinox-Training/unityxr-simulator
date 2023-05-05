using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Rhinox.XR.UnityXR.Simulator
{
    public struct FrameData
    {
        public int FrameNumber;

        public Vector3 HeadPosition;
        public Quaternion HeadRotation;

        public Vector3 LeftHandPosition;
        public Quaternion LeftHandRotation;

        public Vector3 RightHandPosition;
        public Quaternion RightHandRotation;

        public List<FrameInput> FrameInputs;

        /// <summary>
        /// Checks if 2 frames are almost the same.
        /// </summary>
        /// <remarks>
        /// !!Does not check if the frame numbers are the same!!
        /// </remarks>
        /// <param name="other">The other frame to compare this frame to.</param>
        /// <param name="positionDeadZone">This value acts as the dead zone for each component of the position vector. </param>
        /// <param name="rotationDeadZone">This value acts as the dead zone for each component of the rotation quaternion.</param>
        /// <returns></returns>
        public bool ApproximatelyEqual(FrameData other, float positionDeadZone, float rotationDeadZone)
        {
            return AreVectorsApproxEqual(HeadPosition, other.HeadPosition, positionDeadZone) &&
                    AreQuaternionsApproxEqual(HeadRotation, other.HeadRotation, rotationDeadZone) &&
                    AreVectorsApproxEqual(LeftHandPosition, other.LeftHandPosition, positionDeadZone) &&
                    AreQuaternionsApproxEqual(LeftHandRotation, other.LeftHandRotation, rotationDeadZone) &&
                    AreVectorsApproxEqual(RightHandPosition, other.RightHandPosition, positionDeadZone) &&
                        AreQuaternionsApproxEqual(RightHandRotation, other.RightHandRotation, rotationDeadZone) &&
                    FrameInputs.All(other.FrameInputs.Contains) && FrameInputs.Count == other.FrameInputs.Count;
        }

        private bool AreVectorsApproxEqual(Vector3 v1, Vector3 v2, float deadZone)
        {
            if (v1.x < v2.x - deadZone || v1.x > v2.x + deadZone)
                return false;
            if (v1.y < v2.y - deadZone || v1.y > v2.y + deadZone)
                return false;
            if (v1.z < v2.z - deadZone || v1.z > v2.z + deadZone)
                return false;
            return true;
        }

        private bool AreQuaternionsApproxEqual(Quaternion v1, Quaternion v2, float deadZone)
        {
            if (v1.x < v2.x - deadZone || v1.x > v2.x + deadZone)
                return false;
            if (v1.y < v2.y - deadZone || v1.y > v2.y + deadZone)
                return false;
            if (v1.z < v2.z - deadZone || v1.z > v2.z + deadZone)
                return false;
            if (v1.w < v2.w - deadZone || v1.w > v2.w + deadZone)
                return false;
            return true;
        }
    }

    public struct FrameInput
    {
        public bool IsRightControllerInput;
        public string InputActionName;
        public bool IsInputStart;
        public string Value;
    }
}