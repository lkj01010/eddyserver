using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;
using Tables;
using ProtoBuf;
using UnityEngine;
using UnityEditor;

public class TblEditor : EditorWindow
{
    private object proto;
    private Vector2 scrollPos;

    public static void Init(string path)
    {
        var window = GetWindow<TblEditor>();

        window.proto = Deserialize(path);
    }

    private static object Deserialize(string path)
    {
        var assembly = Assembly.GetAssembly(typeof(TableAttribute));
        var type = assembly.GetType(Path.GetFileNameWithoutExtension(path));
        var holderType = typeof(Tables.TableHolder<>).MakeGenericType(type);
        MethodInfo method = typeof(ProtoBuf.Serializer).GetMethod("Deserialize", BindingFlags.Static | BindingFlags.Public);
        MethodInfo generic = method.MakeGenericMethod(holderType);
        var stream = new FileStream(path, FileMode.Open, FileAccess.Read);
        return generic.Invoke(null, new object[] { stream });
    }

    private void DrawColumnNames(IEnumerable<PropertyInfo> properties, GUILayoutOption[] fieldLayout)
    {
        foreach (var property in properties)
        {
            var attribute = Attribute.GetCustomAttribute(property, typeof(ProtoBuf.ProtoMemberAttribute)) as ProtoBuf.ProtoMemberAttribute;
            var name = GetFieldName(property, attribute);

            GUILayout.Button(name, fieldLayout);
        }
    }

    private IEnumerable<PropertyInfo> GetProperties()
    {
        var type = (proto.GetType().GetGenericArguments())[0];
        var properties = from property in type.GetProperties()
                         where property.IsDefined(typeof(ProtoBuf.ProtoMemberAttribute), false)
                         select property;
        return properties;
    }

    private static IEnumerable<IEnumerable<string>> GetRows<T>(TableHolder<T> holder, IEnumerable<PropertyInfo> properties)
    {
        var rows = from data in holder.Data
                     select (from property in properties
                     select property.GetValue(data, null).ToString());
        return rows;
    }

    private void DrawRows(IEnumerable<PropertyInfo> properties, GUILayoutOption[] fieldLayout)
    {
        var type = (proto.GetType().GetGenericArguments())[0];
        var methodInfo = typeof(TblEditor).GetMethod("GetRows", BindingFlags.Static | BindingFlags.NonPublic);
        var generic = methodInfo.MakeGenericMethod(type);
        var rows = (generic.Invoke(null, new object[] { proto, properties })) as IEnumerable<IEnumerable<string>>;

        foreach (var row in rows)
        {
            EditorGUILayout.BeginHorizontal();
            foreach (var value in row)
            {
                GUILayout.Label(value, fieldLayout);
            }
            EditorGUILayout.EndHorizontal();
        }
    }

    private static string GetFieldName(PropertyInfo property, ProtoBuf.ProtoMemberAttribute attribute)
    {
        var name = attribute.Name;

        if (name == null || name == "")
            name = property.Name;
        return name;
    }

    void OnGUI()
    {
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos);

        EditorGUILayout.BeginVertical();

        GUILayoutOption[] fieldLayout = { GUILayout.MinWidth(80), GUILayout.MaxWidth(200), GUILayout.ExpandWidth(true) };
        var properties = GetProperties();

        EditorGUILayout.BeginHorizontal();
        DrawColumnNames(properties, fieldLayout);
        EditorGUILayout.EndHorizontal();

        DrawRows(properties, fieldLayout);

        EditorGUILayout.EndVertical();
        EditorGUILayout.EndScrollView();
    }
}

