using System.IO;
using Rhinox.XR.UnityXR.Simulator;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SimulationPlayback))]
public class PlaybackEditor : Editor
{
    #region SerializedProperties
    private SerializedProperty _filePathTargetFolder;
    private SerializedProperty _recordingName;
    #endregion
    private void OnEnable()
    {
        _filePathTargetFolder = serializedObject.FindProperty("FilePathTargetFolder");
        _recordingName = serializedObject.FindProperty("RecordingName");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        var script = (SimulationPlayback)target;
        EditorGUILayout.LabelField("Output file settings",EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Output directory", EditorStyles.largeLabel);
        _filePathTargetFolder.stringValue = EditorGUILayout.TextField("Target folder name", _filePathTargetFolder.stringValue);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.LabelField("Current output directory: " + script.Path);
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space(15);
        
        EditorGUILayout.LabelField("Target file name", EditorStyles.largeLabel);
        _recordingName.stringValue = EditorGUILayout.TextField("Custom name", _recordingName.stringValue);
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.PrefixLabel("Name from file in folder:");
        if (EditorGUILayout.DropdownButton(new GUIContent(script.RecordingName),FocusType.Passive))
        {
            ShowSelector(script);
        }
        EditorGUILayout.EndHorizontal();
        serializedObject.ApplyModifiedProperties();

        serializedObject.DrawInspectorExcept("m_Script");
    }

    private void ShowSelector(SimulationPlayback recordingLoader)
    {
        if (string.IsNullOrWhiteSpace(recordingLoader.Path))
            return;
        var absPath = recordingLoader.Path;
        var dirInfo = new DirectoryInfo(absPath);
        if (!dirInfo.Exists)
            return;

        var sequenceMenu = new GenericMenu();
        var files = dirInfo.GetFiles();
        foreach (var file in files)
        {
            sequenceMenu.AddItem(new GUIContent(file.Name), false, OnSequenceSelected, file.Name);
        }
        sequenceMenu.ShowAsContext();
    }

    private void OnSequenceSelected(object userData)
    {
        if(!(userData is string dir))
            return;
        var result = dir.Split('.');
        _recordingName.stringValue = result[0];
        serializedObject.ApplyModifiedProperties();

    }
}
