using System;
using UnityEngine;

[Serializable]
public class E_EnemyTypes : ScriptableObject
{
    public enum EnemyType 
    { 
        None,
        Basic,
        GlassCannon,
        Tank,
        Boss
    }
}