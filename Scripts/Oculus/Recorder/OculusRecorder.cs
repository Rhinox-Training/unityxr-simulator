using System.Collections.Generic;

namespace Rhinox.XR.UnityXR.Simulator.Oculus
{
    public class OculusRecorder : BaseRecorder
    {
        private List<FrameInput> _currentFrameInput = new List<FrameInput>();

        protected override List<FrameInput> GetFrameInputs(bool clearInputsAfterwards = true)
        {
            throw new System.NotImplementedException();
        }

        private void Update()
        {
            if(!IsRecording)
                return;
            
            
        }
    }
    
}