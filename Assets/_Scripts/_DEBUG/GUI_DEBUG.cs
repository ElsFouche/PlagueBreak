using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

[CustomEditor(typeof(EnemyHandler))]
public class GUI_DEBUG : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Test Damage"))
        {
            var myScript = target as EnemyHandler;
            myScript.DealDamage(1.0f);
        }
    }
}
