using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

public static class FindSharedClassContent
{
    public static void LogSharedContent(Type type1, Type type2)
    {
        var commonMethods = GetCommonMethods(type1, type2);
        var commonFields = GetCommonFields(type1, type2);

        Debug.Log("Common Methods:");
        Debug.Log(string.Join(", ", commonMethods));

        Debug.Log("\nCommon Fields:");
        Debug.Log(string.Join(", ", commonFields));
    }

    private static List<string> GetCommonMethods(Type typeA, Type typeB)
    {
        var methodsA = typeA.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                        BindingFlags.DeclaredOnly);
        var methodsB = typeB.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                        BindingFlags.DeclaredOnly);

        var commonMethods = new List<string>();

        foreach (var methodA in methodsA)
        {
            foreach (var methodB in methodsB)
            {
                if (methodA.Name == methodB.Name)
                {
                    commonMethods.Add(methodA.Name);
                }
            }
        }

        return commonMethods;
    }

    private static List<string> GetCommonFields(Type typeA, Type typeB)
    {
        var fieldsA = typeA.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                      BindingFlags.DeclaredOnly);
        var fieldsB = typeB.GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance |
                                      BindingFlags.DeclaredOnly);

        var commonFields = new List<string>();

        foreach (var fieldA in fieldsA)
        {
            foreach (var fieldB in fieldsB)
            {
                if (fieldA.Name == fieldB.Name)
                {
                    commonFields.Add(fieldA.Name);
                }
            }
        }

        return commonFields;
    }
}