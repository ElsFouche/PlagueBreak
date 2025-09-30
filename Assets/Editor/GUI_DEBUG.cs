#if (UNITY_EDITOR)
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemyHandler))]
public class GUI_DEBUG : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        if (GUILayout.Button("Test Wave"))
        {
            var myScript = target as EnemyHandler;
            myScript.UpdateWaveCount("Test");
        }
/*
        if (GUILayout.Button("Test Damage"))
        {
            var myScript = target as EnemyHandler;
            myScript.DealDamage(1.0f);
        }
*/
    }
}
#endif