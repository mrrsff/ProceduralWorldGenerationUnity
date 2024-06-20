using System;
using System.Collections.Generic;
using UnityEngine;

namespace Axegen
{
    [System.Serializable]
    public class TerrainType
    {
        public string name;
        public float altitude;
        [Tooltip("Does this terrain type vary in height?")]
        public bool varyHeight = false;
        [Tooltip("Variance in height for this terrain type")]
        public float variance = 1;
        public float heightMultiplier;
        public Color color;
    }

    [CreateAssetMenu(fileName = "Biome Data", menuName = "Axegen/BiomeData")]
    public class BiomeData : ScriptableObject
    {
        [Header("Biome Settings")]
        public string biomeName;
        public float humidity;
        public float temperature;

        [Header("Terrain Settings")]
        public List<TerrainType> terrainTypes = new();
        public float colorBlendFactor = 0.1f;
        public float heightBlendFactor = 0.1f;

        private List<TerrainType> sortedTerrainTypes;

        bool isSorted = false;

        private System.Random rng = new System.Random();
        private float Random(float min, float max) => (float)rng.NextDouble() * (max - min) + min;

        private float GetBlendFactor(TerrainType closestTerrain, TerrainType blendTerrain, float height)
        {
            return Mathf.InverseLerp(closestTerrain.altitude, blendTerrain.altitude, height);
        }

        public void SortTerrains()
        {
            sortedTerrainTypes = new List<TerrainType>(terrainTypes);
            sortedTerrainTypes.Sort((a, b) => a.altitude.CompareTo(b.altitude));
        }
        public Color GetColor(float height)
        {
            int closestTerrainIndex = GetClosestTerrainIndex(height);
            TerrainType closestTerrain = sortedTerrainTypes[closestTerrainIndex];

            TerrainType blendTerrain = GetBlendTerrain(closestTerrainIndex);

            if (blendTerrain == closestTerrain)
            {
                return closestTerrain.color;
            }

            float factor = GetBlendFactor(closestTerrain, blendTerrain, height) * colorBlendFactor;

            return Color.Lerp(closestTerrain.color, blendTerrain.color, factor);
        }
        public float GetHeightMultiplier(int x, int y, ref float[,] data)
        {
            float height = data[x, y];
            int closestTerrainIndex = GetClosestTerrainIndex(height);
            TerrainType closestTerrain = sortedTerrainTypes[closestTerrainIndex]; 

            TerrainType blendTerrain = GetBlendTerrain(closestTerrainIndex);

            float heightMultiplier = closestTerrain.heightMultiplier * (closestTerrain.varyHeight ? Random(1, closestTerrain.variance) : 1);

            if (blendTerrain != closestTerrain)
            {
                float factor = GetBlendFactor(closestTerrain, blendTerrain, height) * heightBlendFactor;

                float blendHeight = blendTerrain.heightMultiplier * (blendTerrain.varyHeight ? Random(1, blendTerrain.variance) : 1);

                heightMultiplier = Mathf.Lerp(heightMultiplier, blendHeight, factor);
            }

            return heightMultiplier;
        }

        private void OnValidate()
        {
            isSorted = false;
        }

        public int GetClosestTerrainIndex(float height)
        {
            if (!isSorted)
            {
                SortTerrains();
                isSorted = true;
            }
            // Find closest terrain type
            int index = 0;
            bool found = false;
            // Check if height between min and max
            if (height <= sortedTerrainTypes[0].altitude)
            {
                return 0;
            }
            else if (height >= sortedTerrainTypes[sortedTerrainTypes.Count - 1].altitude)
            {
                return sortedTerrainTypes.Count - 1;
            }

            for (int i = 1; i < sortedTerrainTypes.Count; i++)
            {
                if (height <= sortedTerrainTypes[i].altitude)
                {
                    index = i - 1;
                    found = true;
                    break;
                }
            }
            if (!found)
            {
                index = sortedTerrainTypes.Count - 1;
            }

            return index;
        }

        public TerrainType GetBlendTerrain(int index)
        {
            return index == sortedTerrainTypes.Count - 1 ? sortedTerrainTypes[index] : sortedTerrainTypes[index + 1];
        }
    }
}
