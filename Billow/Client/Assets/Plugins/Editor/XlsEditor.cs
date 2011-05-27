using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;
using org.in2bits.MyXls;

class XlsEditor : EditorWindow
{
    private XlsDocument document;

    public static void Init(string path)
    {
        var window = GetWindow<XlsEditor>();
        window.document = new XlsDocument(path);
    }

    void OnGUI()
    {
        var sheet = document.Workbook.Worksheets[0];
        var rows = sheet.Rows;
        EditorGUILayout.BeginVertical();
        GUILayoutOption[] buttonLayout = { GUILayout.MaxWidth(10), GUILayout.ExpandWidth(true) };

        var firstRow = rows[(ushort)rows.MinRow];
        {
            EditorGUILayout.BeginHorizontal();
            for (uint i = firstRow.MinCellCol; i <= firstRow.MaxCellCol; ++i)
            {
                GUILayout.Button(firstRow.CellAtCol((ushort)i).Value.ToString(), buttonLayout);
            }
            EditorGUILayout.EndHorizontal();
        }

        for (uint i = rows.MinRow + 1; i <= rows.MaxRow; ++i)
        {
            var row = rows[(ushort)i];
            EditorGUILayout.BeginHorizontal();
            for (uint j = firstRow.MinCellCol; j <= firstRow.MaxCellCol; ++j)
            {
                if (row.CellExists((ushort)j))
                {
                    GUILayout.TextField(row.CellAtCol((ushort)j).Value.ToString(), buttonLayout);
                }
                else
                {
                    GUILayout.TextField("", buttonLayout);
                }
            }
            EditorGUILayout.EndHorizontal();
        }
        EditorGUILayout.EndVertical();
    }
}
