using EasyButtons;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System;
using UnityEngine.Profiling;
using System.Threading.Tasks;
using static UnityEngine.Mesh;

namespace Axegen
{
    public class MapGenerator : MonoBehaviour
    {
        public static MapGenerator instance;

        #region Settings
        public Material terrainMaterial;
        public BiomeCollection biomeCollection;


        [Header("Noise Settings")]
        public Vector2 offset;
        [Range(20, 500)]
        public float noiseScale;
        [Range(1, 10)]
        public int octaves;
        [Range(0, 1)]
        public float persistance;
        [Range(1, 10)]
        public float lacunarity;
        public int seed;
        #endregion

        private int chunkSize;
        private int lodCount;
        public Vector2Int GetCoord(Vector3 position) => new Vector2Int(Mathf.FloorToInt(position.x / chunkSize), Mathf.FloorToInt(position.z / chunkSize));

        public float[,] GetHeightMap(Vector2Int coord) => Noise.GenerateNoiseMap(chunkSize+1, chunkSize+1, seed, noiseScale, octaves, persistance, lacunarity, new Vector2(coord.x * chunkSize, coord.y * chunkSize) + offset, Noise.NormalizeMode.Global);

        Queue<ThreadInfo<MapData>> mapDataThreadInfoQueue = new Queue<ThreadInfo<MapData>>();
        public Queue<ThreadInfo<MeshData[]>> meshDataThreadInfoQueue = new Queue<ThreadInfo<MeshData[]>>();

        private void Awake()
        {
            instance = this;
            System.Random prng = new System.Random(seed);
            offset = new Vector2(prng.Next(100000, 10000000), prng.Next(100000, 10000000));
            lodCount = EndlessTerrain.instance.LODCount;
            chunkSize = EndlessTerrain.instance.chunkSize;

            biomeCollection.chunkSize = chunkSize;
        }

        private void Start()
        {
            StartCoroutine(MapDataRoutine());
            StartCoroutine(MeshDataRoutine());
        }
        #region MapData
        public void RequestMapData(Vector2Int coord, Action<MapData> callback)
        {
            Task.Run(() => MapDataThread(coord, callback));
        }
        private void MapDataThread(Vector2Int coord, Action<MapData> callback)
        {
            try
            {
                MapData mapData = TerrainGenerator.CreateMapData(GetHeightMap(coord), coord);
                lock (mapDataThreadInfoQueue)
                {
                    mapDataThreadInfoQueue.Enqueue(new ThreadInfo<MapData>(callback, mapData));
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error in MapDataThread: " + e);
            }
        }
        IEnumerator MapDataRoutine()
        {
            while (true)
            {
                if (mapDataThreadInfoQueue.Count > 0)
                {
                    ThreadInfo<MapData> threadInfo = mapDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
                yield return null;
            }
        }
        #endregion

        #region MeshData
        public void RequestMeshData(MapData mapData, Action<MeshData[]> callback)
        {
            Task.Run(() => MeshDataThread(mapData, callback));
        }
        private void MeshDataThread(MapData mapData, Action<MeshData[]> callback)
        {
            try
            {
                MeshData[] meshData = TerrainGenerator.CreateMeshDataWithLOD(mapData, biomeCollection, lodCount);
                lock (meshDataThreadInfoQueue)
                {
                    meshDataThreadInfoQueue.Enqueue(new ThreadInfo<MeshData[]>(callback, meshData));
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Error in MeshDataThread: " + e);
            }
        }
        IEnumerator MeshDataRoutine()
        {
            while (true)
            {
                if (meshDataThreadInfoQueue.Count > 0)
                {
                    ThreadInfo<MeshData[]> threadInfo = meshDataThreadInfoQueue.Dequeue();
                    threadInfo.callback(threadInfo.parameter);
                }
                yield return null;
            }
        }
        #endregion
    }
    public struct ThreadInfo<T>
    {
        public readonly Action<T> callback;
        public readonly T parameter;

        public ThreadInfo(Action<T> callback, T parameter)
        {
            this.callback = callback;
            this.parameter = parameter;
        }
    }
    public struct MeshData
    {
        public Vector3[] vertices;
        public int[] triangles;
        public Vector2[] uvs;
        public Color[] colors;
        public int lod;

        public MeshData(Vector3[] vertices, int[] triangles, Vector2[] uvs, Color[] colors, int lod)
        {
            this.vertices = vertices;
            this.triangles = triangles;
            this.uvs = uvs;
            this.colors = colors;
            this.lod = lod;
        }
        public Mesh CreateMesh()
        {
            Profiler.BeginSample("CreateMesh");
            Mesh mesh = new Mesh
            {
                vertices = vertices,
                triangles = triangles,
                uv = uvs,
                colors = colors
            };
            mesh.RecalculateNormals();
            //mesh.RecalculateBounds();
            //mesh.RecalculateTangents();

            Profiler.EndSample();

            return mesh;
        }
    }
    public struct MapData
    {
        public float[,] heightMap;
        public Vector2Int chunkCoords;

        public MapData(float[,] heightMap, Vector2Int chunkCoords)
        {
            this.heightMap = heightMap;
            this.chunkCoords = chunkCoords;
        }
    }
}
