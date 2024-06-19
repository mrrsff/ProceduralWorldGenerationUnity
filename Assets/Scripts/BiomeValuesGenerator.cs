using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Axegen
{
    public class BiomeValuesGenerator : MonoBehaviour
    {
        public static BiomeValuesGenerator instance;

        public Vector2 humidityOffset;
        public float humidityScale;

        public Vector2 temperatureOffset;
        public float temperatureScale;

        public int seed;
        private void Start()
        {
            instance = this;
            seed = MapGenerator.instance.seed;
            System.Random prng = new System.Random(seed+971);

            humidityOffset = new Vector2(prng.Next(10, 1000), prng.Next(10, 1000));
            temperatureOffset = new Vector2(prng.Next(10, 1000), prng.Next(10, 1000));
        }
        public float GetHumidity(Vector2 coord) => GetHumidity(coord.x, coord.y);
        public float GetHumidity(float x, float y)
        {
            float xCoord = x / humidityScale + humidityOffset.x;
            float zCoord = y / humidityScale + humidityOffset.y;

            return Mathf.PerlinNoise(xCoord, zCoord);
        }
        public float GetTemperature(Vector2 coord) => GetTemperature(coord.x, coord.y);
        public float GetTemperature(float x, float y)
        {
            float xCoord = x / temperatureScale + temperatureOffset.x;
            float zCoord = y / temperatureScale + temperatureOffset.y;

            return Mathf.PerlinNoise(xCoord, zCoord);
        }
    }
}
