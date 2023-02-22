# unityxr-simulator

This package contains a simulator for UnityXR which uses mouse and keyboard to simulate VR input (movement and interactions). There is also the possibility to record and replay this input though the simulator recorder and playback.

## Setting up a scene
To set up the scene for simulation, just drag the "Simulator_XRRig" into the scene. The "XR Device Simulator Controls" inputactions are used as mouse and keyboard input, so look at this asset to see what keyboard and mouse actions match which VR actions. 
Also add a gameobject that holds both an XR Origin and XR interaction manager.

![XR_Rig](https://user-images.githubusercontent.com/55093987/220591047-f48debff-2f8e-4bf3-a0a1-93c6df6c8d9b.png)

If these are all added, it should now be possible to simulate vr input in this scene. 

REMARK:
It is possible to change mouse sensitivity and movement parameters on the "XR Device SImulator Controls" script on the "Better XRSimulator" object.

## Setting up Record/Playback
To set this up, drag the "Record/Playback" prefab in the scene. By default the VR input that is simulated by the simulator are used by the recorder. These can be found in the included "XRI Default Input Actions" inputactions. After having added this object, link the simulator present in the scene on both the "Simulation Recorder" and "Simulation Playback" script present on the "Record/Playback"'s children.

![Simulation_Recorder](https://user-images.githubusercontent.com/55093987/220593049-80a64c54-a349-4e4f-acd0-d3918eb6e1e2.png)
![Simulation_Playback](https://user-images.githubusercontent.com/55093987/220593058-e2902df1-592c-4a5e-80fd-8173fa8648e7.png)

REMARKS:
- The recorder will write the ended recording to the Application.dataPath + FilePath, where FilePath is a SerializeField string field.
- The recorder does not record frames in which nothing usefull has happened (no input or transform change).
- The playback takes these missing frames into account.
- KNOWN BUG: the playback time can be either 1 second faster or slower. Especially over longer recording, this does not interfere with the input actions.
