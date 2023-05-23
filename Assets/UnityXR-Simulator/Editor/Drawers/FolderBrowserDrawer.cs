using UnityEditor;
using UnityEngine;
using UnityXR_Simulator.Scripts.Attributes;

namespace UnityXR_Simulator.Editor.Drawers
{
	[CustomPropertyDrawer(typeof(FolderBrowserAttribute))]
    public class FolderBrowserDrawer :PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            if (property.propertyType == SerializedPropertyType.String)
            {
                // Draw label
                position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

                // Calculate rects
                Rect textFieldRect = new Rect(position.x, position.y, position.width - 60f, position.height);
                Rect buttonRect = new Rect(position.x + position.width - 60f, position.y, 60f, position.height);

                // Draw text field
                string newValue = EditorGUI.TextField(textFieldRect, property.stringValue);

                // Draw button
                if (GUI.Button(buttonRect, "Browse"))
                {
                    string path = EditorUtility.OpenFolderPanel("Select File", Application.dataPath,"");
                    if (!string.IsNullOrEmpty(path))
                    {
                        newValue = path;
                    }
                }

                // Apply changes
                if (newValue != property.stringValue)
                {
                    property.stringValue = newValue;
                    property.serializedObject.ApplyModifiedProperties();
                }
                
            }
            else
            {
                EditorGUI.PropertyField(position, property, label);
            }
        }
    }
}