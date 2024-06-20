using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Axegen
{
    [CreateAssetMenu(fileName = "Biome Collection", menuName = "Axegen/Biome Collection")]
    public class BiomeCollection : ScriptableObject
    {
        public float biomeBlendThreshold = 0.2f;
        public int chunkSize;
        public BiomeData[] biomes;

        Dictionary<Vector2Int, Dictionary<BiomeData, float>[,]> chunkWeightsCache = new();

        private System.Random prng = new System.Random();

        public BiomeData ChooseBiome(int x, int z, Vector2Int chunkCoord)
        {
            float temperature = BiomeValuesGenerator.instance.GetTemperature(x + chunkCoord.x * chunkSize, z + chunkCoord.y * chunkSize);
            float humidity = BiomeValuesGenerator.instance.GetHumidity(x + chunkCoord.x * chunkSize, z + chunkCoord.y * chunkSize);

            BiomeData bestBiome = null;
            float bestMatch = float.MaxValue;

            foreach (BiomeData biome in biomes)
            {
                float match = GetBiomeMatchValue(biome, temperature, humidity);
                if (match < bestMatch)
                {
                    bestMatch = match;
                    bestBiome = biome;
                }
            }
            return bestBiome;
        }
        public Dictionary<BiomeData, float> GetBiomeWeights(int x, int z, Vector2Int chunkCoord)
        {
            Dictionary<BiomeData, float> result = new Dictionary<BiomeData, float>();
            float temperature = BiomeValuesGenerator.instance.GetTemperature(x + chunkCoord.x * chunkSize, z + chunkCoord.y * chunkSize);
            float humidity = BiomeValuesGenerator.instance.GetHumidity(x + chunkCoord.x * chunkSize, z + chunkCoord.y * chunkSize);
            foreach (BiomeData biome in biomes)
            {
                float match = GetBiomeMatchValue(biome, temperature, humidity);
                if (match > biomeBlendThreshold)
                {
                    continue;
                }
                match = 1 / (match + 0.1f);
                match = Mathf.Pow(match + 1, 10);
                result.Add(biome, match);
            }
            return result;
        }

        public Dictionary<BiomeData, float>[,] GetChunkBiomeWeights(Vector2Int chunkCoord)
        {
            if (chunkWeightsCache.TryGetValue(chunkCoord, out var cachedWeights))
            {
                return cachedWeights;
            }
            // Create all data at once.
            Dictionary<BiomeData, float>[,] result = new Dictionary<BiomeData, float>[chunkSize + 1, chunkSize + 1];
            for (int z = 0; z < chunkSize + 1; z++)
            {
                for (int x = 0; x < chunkSize + 1; x++)
                {
                    if (result[x, z] == null)
                    {
                        result[x, z] = new Dictionary<BiomeData, float>();
                    }
                    float temperature = BiomeValuesGenerator.instance.GetTemperature(x + chunkCoord.x * chunkSize, z + chunkCoord.y * chunkSize);
                    float humidity = BiomeValuesGenerator.instance.GetHumidity(x + chunkCoord.x * chunkSize, z + chunkCoord.y * chunkSize);
                    foreach (BiomeData biome in biomes)
                    {
                        float match = GetBiomeMatchValue(biome, temperature, humidity);
                        //if (match > biomeBlendThreshold)
                        //{
                        //    continue;
                        //}
                        match = 1 / (match + 0.1f);
                        match = Mathf.Pow(match + 1, 10);
                        result[x, z].Add(biome, match);
                    }
                }
            }
            if (chunkWeightsCache.Count > 256)
            {
                chunkWeightsCache.Clear();
            }
            chunkWeightsCache.Add(chunkCoord, result);
            return result;
        }
        public Tuple<float[,], Color[,]> GetAltitudeAndColorMap(Vector2Int chunkCoord, ref float[,] data)
        {
            float[,] altitudeMap = new float[chunkSize + 1, chunkSize + 1];
            Color[,] colorMap = new Color[chunkSize + 1, chunkSize + 1];
            var chunkWeights = GetChunkBiomeWeights(chunkCoord);
            //float[,] riverMap = riverSettings.GetRiverHeightMultiplierMap(chunkCoord);
            for (int z = 0; z < chunkSize + 1; z++)
            {
                for (int x = 0; x < chunkSize + 1; x++)
                {
                    try
                    {
                        var weights = chunkWeights[x, z];
                        float totalWeight = 0;
                        float totalAltitude = 0;

                        Color colorValue = Color.black;
                        foreach (var weight in weights)
                        {
                            totalWeight += weight.Value;
                            totalAltitude += weight.Key.GetHeightMultiplier(x, z, ref data) * weight.Value;
                            colorValue += weight.Key.GetColor(data[x, z]) * weight.Value;
                        }
                        altitudeMap[x, z] = totalAltitude / totalWeight; // This is the weighted average of the altitudes.
                        colorMap[x, z] = colorValue / totalWeight;

                        //// Apply river height multiplier
                        //altitudeMap[x, z] *= riverMap[x, z];
                    }
                    catch (Exception e)
                    {
                        Debug.Log("Error in GetAltitudeAndColorMap: " + e);
                    }
                }
            }

            return new Tuple<float[,], Color[,]>(altitudeMap, colorMap);
        }
        /// <summary>
        /// The lesser the better.
        /// </summary>
        /// <param name="biome"></param>
        /// <param name="temperature"></param>
        /// <param name="humidity"></param>
        /// <returns></returns>
        private float GetBiomeMatchValue(BiomeData biome, float temperature, float humidity) 
        {
            float temperatureMatch = Mathf.Abs(biome.temperature - temperature);
            float humidityMatch = Mathf.Abs(biome.humidity - humidity);

            return temperatureMatch + humidityMatch;
        }
        private void OnValidate()
        {
            foreach (BiomeData biome in biomes)
            {
                biome.SortTerrains();
            }
        }
    }
}
