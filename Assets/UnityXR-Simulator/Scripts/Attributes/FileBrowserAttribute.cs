using System;
using UnityEngine;

[AttributeUsage(AttributeTargets.Field)]
public class FileBrowserAttribute : PropertyAttribute
{
    public string Extension { get; private set; }

    public FileBrowserAttribute(string extension = "*")
    {
        Extension = extension;
    }
}