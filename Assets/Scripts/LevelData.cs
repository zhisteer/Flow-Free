using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "LevelData", menuName = "Scriptable Objects/LevelData")]
public class LevelData : ScriptableObject
{
    [Serializable]
    public struct Pair
    {
        public Vector2Int a;
        public Vector2Int b;
    }

    [Serializable]
    public class Level
    {
        public List<Pair> pairs;

        public Level()
        {
            pairs = new List<Pair>();
        }
    }

    public List<Level> levels = new();

    
}