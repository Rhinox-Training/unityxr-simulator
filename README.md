# unityxr-simulator

This package contains a simulator for UnityXR which uses mouse and keyboard to simulate VR input (movement and interactions). There is also the possibility to record and replay this input though the simulator recorder and playback.
There are two simulators, one for Unity OpenXR and one for Oculus XR.

## Setting up a scene
To set up the scene for simulation, just drag the "OpenXR_SimulatorRig" or "OVR_SimulatorRig" into the scene. The "XR Device Simulator Controls" inputactions are used as mouse and keyboard input, so look at this asset to see what keyboard and mouse actions match which VR actions. 
Also add a gameobject that holds both an XR Origin and XR interaction manager.

![SimulatorRigPrefabs](https://github.com/Rhinox-Training/unityxr-simulator/assets/55093987/9ec1b234-2b3c-48d9-a5af-9476c345b6d8)

If these are all added, it should now be possible to simulate vr input in this scene. 

REMARK:
It is possible to change mouse sensitivity and movement parameters on the "XR Device SImulator Controls" script on the "Better XRSimulator" object.

## Setting up Record/Playback
To set this up, drag the "(Oculus/OpenXR)Recorder" and "(Oculus/OpenXR)Playback" prefabs in the scene. By default the VR input that is simulated by the simulator are used by the recorder. These can be found in the included "XRI Default Input Actions" inputactions. After having added this object, link the simulator present in the scene on both the "Simulation Recorder" and "Simulation Playback" scripts present on the prefabs. If this reference is not made correctly, the recorder or playback will disable itself.

![SharedRecorderSettings](https://github.com/Rhinox-Training/unityxr-simulator/assets/55093987/cef66827-e635-4241-911f-7201ade9e775)

The recorder and playback use the FixedUpdate for their functionality to assure a constant framerate.

![SharedPlaybackSettings](https://github.com/Rhinox-Training/unityxr-simulator/assets/55093987/bafbc0c8-fcaa-4b89-9b0b-b4b31063aa15)

# License

Apache-2.0 © Rhinox NV
