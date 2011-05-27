using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;
using SmartXLS;

class XlsEditor : EditorWindow
{
    private WorkBook book;
    private string path;

    public static void Init(string path)
    {
        var window = GetWindow<XlsEditor>();
        window.book = new WorkBook();
        window.book.read(path);
        window.path = path;
    }

    void OnGUI()
    {
        EditorGUILayout.BeginVertical();

        if (GUILayout.Button("Save", new[] { GUILayout.MaxWidth(50) }))
        {
            book.write(path);
        }

        GUILayoutOption[] buttonLayout = { GUILayout.MinWidth(40), GUILayout.MaxWidth(100), GUILayout.ExpandWidth(true) };

        {
            EditorGUILayout.BeginHorizontal();
            for (int i = 0; i <= book.LastCol; ++i)
            {
                GUILayout.Button(book.getText(0, i), buttonLayout);
            }
            EditorGUILayout.EndHorizontal();
        }

        for (int i = 1; i <= book.LastRow; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j <= book.LastCol; ++j)
            {
                GUILayout.TextField(book.getText(i, j), buttonLayout);
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }
}
