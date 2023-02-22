# unityxr-simulator

This package contains a simulator for UnityXR which uses mouse and keyboard to simulate VR input (movement and interactions). There is also the possibility to record and replay this input though the simulator recorder and playback.

## Setting up a scene
To set up the scene for simulation, just drag the "Simulator_XRRig" into the scene. The "XR Device Simulator Controls" inputactions are used as mouse and keyboard input, so look at this asset to see what keyboard and mouse actions match which VR actions. 
Also add a gameobject that holds both an XR Origin and XR interaction manager.

![XR_Rig](https://user-images.githubusercontent.com/55093987/220591047-f48debff-2f8e-4bf3-a0a1-93c6df6c8d9b.png)

If these are all added, it should now be possible to simulate vr input in this scene.
REMARK:
It is possible to change mouse sensitivity and movement parameters on the "XR Device SImulator Controls" script on the "Better XRSimulator" object.



##
