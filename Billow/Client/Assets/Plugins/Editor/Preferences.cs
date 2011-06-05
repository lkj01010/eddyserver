using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Eddy.Extensions;

public class Preferences : EditorWindow
{
    private static Dictionary<string, string> defaultPrefs = new Dictionary<string, string>()
    {
        {"服务器数据导出路径", Application.dataPath + "/../../ServerData" }
    };

    [MenuItem("Eddy/Preferences")]
    public static void Init()
    {
        GetWindow<Preferences>();
    }

    private void OnGUI()
    {
        EditorGUILayout.BeginVertical();
        foreach (var pair in defaultPrefs)
        {
            EditorGUILayout.BeginHorizontal();
            DrawItem(pair);
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }

    private static void DrawItem(KeyValuePair<string, string> pair)
    {
        EditorGUILayout.PrefixLabel(pair.Key);
        var value = EditorGUILayout.TextField(GetString(pair.Key));
        if (value != pair.Value)
            SetString(pair.Key, value);
    }

    public static string GetString(string key)
    {
        if (EditorPrefs.HasKey(key))
            return EditorPrefs.GetString(key);
        return defaultPrefs.GetValueOrDefault(key);
    }

    public static void SetString(string key, string value)
    {
        EditorPrefs.SetString(key, value);
    }
}
