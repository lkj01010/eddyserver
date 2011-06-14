using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Eddy;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class CoreDriver : MonoBehaviour
{
    public event Action OnUpdate;

    private void Awake()
    {
        Init();
    }

    private void Init()
    {
#if UNITY_EDITOR
            EditorApplication.update -= this.Update;
            EditorApplication.update += this.Update;
#endif
    }

    private void Update()
    {
        Init();

        if (OnUpdate != null)
            OnUpdate();

        Eddy.SimpleDispatcher.CurrentDispatcher.Update();
    }
}
