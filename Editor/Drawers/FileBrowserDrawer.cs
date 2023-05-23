using UnityEditor;
using UnityEngine;

[CustomPropertyDrawer(typeof(FileBrowserAttribute))]
public class FileBrowserDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        FileBrowserAttribute browserAttribute = (FileBrowserAttribute)attribute;
        if (property.propertyType == SerializedPropertyType.String)
        {
            Rect buttonRect = new Rect(position.x + position.width - 75f, position.y, 75f, position.height);
            position.width -= 80f;

            EditorGUI.PropertyField(position, property, label);

            EditorGUI.BeginChangeCheck();
            if (GUI.Button(buttonRect, "Browse"))
            {
                string path = EditorUtility.OpenFilePanel("Select File", Application.dataPath, browserAttribute.Extension);
                if (!string.IsNullOrEmpty(path))
                {
                    property.stringValue = path;
                }
            }

            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
            }
        }
        else
        {
            EditorGUI.PropertyField(position, property, label);
        }
    }
}