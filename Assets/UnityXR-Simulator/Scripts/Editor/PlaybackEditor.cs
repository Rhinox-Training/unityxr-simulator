using System.IO;
using Rhinox.XR.UnityXR.Simulator;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(SimulationPlayback))]
public class PlaybackEditor : Editor
{
    #region SerializedProperties
    private SerializedProperty _path;
    private SerializedProperty _recordingName;
    #endregion
    private void OnEnable()
    {
        _path = serializedObject.FindProperty("Path");
        _recordingName = serializedObject.FindProperty("RecordingName");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        var script = (SimulationPlayback)target;
        EditorGUILayout.LabelField("Input file settings",EditorStyles.boldLabel);
        if (GUILayout.Button("Import file"))
        {
            var chosenFile = EditorUtility.OpenFilePanel("Choose target folder", script.Path, "xml");
            if (chosenFile.Length != 0)
            {
                var pathIndex = chosenFile.LastIndexOf('/');
                var fileExtensionIndex = chosenFile.LastIndexOf('.');
                var newFileName = chosenFile.Substring(pathIndex + 1, fileExtensionIndex - pathIndex - 1);
                _recordingName.stringValue = newFileName;
                _path.stringValue = chosenFile.Substring(0, pathIndex);
                serializedObject.ApplyModifiedProperties();
            }
        }

        EditorGUILayout.LabelField("Input directory", EditorStyles.largeLabel);
        _path.stringValue = EditorGUILayout.TextField("Target folder name", _path.stringValue);

        EditorGUILayout.HelpBox("Current input directory: " + script.Path,MessageType.Info);
        
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
