using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

class SVNCommentWindow : EditorWindow
{
    private string comment = "Commit by unity";

    public event Action<string> Confirm;

    public static SVNCommentWindow Init()
    {
        var window = GetWindow<SVNCommentWindow>();
        window.Show();
        window.title = "提交";
        return window;
    }

    public void OnGUI()
    {
        comment = EditorGUI.TextArea(new Rect(10, 25, position.width - 20, 100),
                    comment);

        if (GUI.Button(new Rect(40, 130, 50, 30), "确定"))
        {
            if (Confirm != null)
            {
                Confirm(comment);
                Confirm = null;
            }
            Close();
        }
        else if (GUI.Button(new Rect(position.width - 90, 130, 50, 30), "取消"))
        {
            Close();
        }
    }
}
