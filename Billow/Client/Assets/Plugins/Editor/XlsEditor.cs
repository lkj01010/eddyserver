using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using UnityEngine;
using UnityEditor;
using SmartXLS;

public class XlsEditor : EditorWindow
{
    private WorkBook book;
    private string path;
    private Vector2 scrollPos;

    public static void Init(string path)
    {
        var window = GetWindow<XlsEditor>();
        window.book = new WorkBook();
        window.book.read(path);
        window.path = path;
    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
        GUILayoutOption[] buttonLayout = { GUILayout.MaxWidth(50) };

        EditorGUILayout.BeginVertical();


        GUILayoutOption[] fieldLayout = { GUILayout.MinWidth(80), GUILayout.MaxWidth(200), GUILayout.ExpandWidth(true) };

        EditorGUILayout.BeginHorizontal();
        DrawColumnNames(buttonLayout, fieldLayout);
        EditorGUILayout.EndHorizontal();

        DrawRows(buttonLayout, fieldLayout);

        EditorGUILayout.BeginHorizontal();
        DrawAddAndSaveButtons(buttonLayout, fieldLayout);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }

    private void DrawRows(GUILayoutOption[] buttonLayout, GUILayoutOption[] fieldLayout)
    {
        for (int i = 1; i <= book.LastRow; ++i)
        {
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("-", buttonLayout))
            {
                book.deleteRange(i, 0, i, book.LastCol, WorkBook.ShiftRows);
                if (i == book.LastRow + 1)
                    break;
            }
            for (int j = 0; j <= book.LastCol; ++j)
            {
                var text = GUILayout.TextField(book.getText(i, j), fieldLayout);
                book.setText(i, j, text);
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private void DrawColumnNames(GUILayoutOption[] buttonLayout, GUILayoutOption[] fieldLayout)
    {
        GUILayout.Label("", buttonLayout);
        for (int i = 0; i <= book.LastCol; ++i)
        {
            GUILayout.Button(book.getText(0, i), fieldLayout);
        }
    }

    private void DrawAddAndSaveButtons(GUILayoutOption[] buttonLayout, GUILayoutOption[] fieldLayout)
    {
        if (GUILayout.Button("+", buttonLayout))
        {
            int row = book.LastRow + 1;
            for (int i = 0; i <= book.LastCol; ++i)
            {
                var text = GUILayout.TextField("", fieldLayout);
                book.setText(row, i, text);
            }
        }
        GUILayout.Label("", new[] { GUILayout.ExpandWidth(true) });
        if (GUILayout.Button("Save", buttonLayout))
        {
            book.write(path);
            CustomPostprocessors.OnReimported(path);
        }
    }
}
