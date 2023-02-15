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
    }
}