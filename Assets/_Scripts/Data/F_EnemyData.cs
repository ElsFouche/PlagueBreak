using System;
using System.Runtime.Serialization;
using UnityEditor.ShaderGraph.Drawing;
using UnityEngine;

[Serializable]
public struct F_EnemyData
{
    public E_EnemyTypes.EnemyType enemyType;
    [Range(1.0f, 25.0f)]
    public float baseHealth;
    [Range(1.0f, 25.0f)]
    public float baseDamage;
    [Range(1, 10)]
    public int partOfWave;
}