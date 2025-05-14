#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using System;
using System.Collections.Generic;

public class ExecutionOrderDebugger : EditorWindow
{
    [MenuItem("Tools/Debug/Print Script Execution Orders")]
    public static void PrintExecutionOrders()
    {
        // Find all MonoBehaviour script assets
        MonoScript[] scripts = Resources.FindObjectsOfTypeAll<MonoScript>();

        List<(string name, int order)> executionOrders = new();

        foreach (var script in scripts)
        {
            if (script == null) continue;

            Type scriptClass = script.GetClass();
            if (scriptClass == null || !typeof(MonoBehaviour).IsAssignableFrom(scriptClass))
                continue;

            int order = MonoImporter.GetExecutionOrder(script);
            executionOrders.Add((scriptClass.FullName, order));
        }

        // Sort by execution order
        executionOrders.Sort((a, b) => a.order.CompareTo(b.order));

        Debug.Log("<b>=== Script Execution Orders ===</b>");
        foreach (var entry in executionOrders)
        {
            Debug.Log($"<b>{entry.name}</b> : <color=cyan>{entry.order}</color>");
        }
    }
}
#endif
