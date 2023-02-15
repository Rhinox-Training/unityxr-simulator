using System.Collections.Generic;
using System.Xml.Serialization;

namespace Rhinox.XR.UnityXR.Simulator
{
    [XmlRoot(ElementName = "SimulationRecording")]
    public class SimulationRecording
    {
        [XmlElement(ElementName = "RecordingLength")]
        public int RecordingLength { get; set; }

        [XmlArray(ElementName = "Frames")] public List<FrameData> Frames = new List<FrameData>();

        /// <summary>
        /// Adds a new frame to the recording. 
        /// </summary>
        /// <param name="newFrame">The frame that gets appended to the recording.</param>
        public void AddFrame(FrameData newFrame)
        {
            newFrame.FrameNumber = Frames.Count;

            //Add the frame.
            Frames.Add(newFrame);
            RecordingLength++;
        }
    }
}