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
    private static CoreDriver instance;
    public static CoreDriver Instance { get { return instance; } }

    private void Awake()
    {
        if (Instance != null)
            throw new InvalidOperationException("CoreFramework实例已存在");
        Init();
    }

    private void Init()
    {
        if (instance == null)
        {
            instance = this;
            UnityEngine.Object.DontDestroyOnLoad(this.gameObject);
#if UNITY_EDITOR
            EditorApplication.update -= this.Update;
            EditorApplication.update += this.Update;
#endif
        }
    }

    private void Update()
    {
        Eddy.SimpleDispatcher.CurrentDispatcher.Update();
    }
}
