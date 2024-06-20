using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Axegen
{
    public class EndlessTerrain : MonoBehaviour
    {
        public static EndlessTerrain instance;
        private void Awake()
        {
            instance = this;
        } 

        [Header("Terrain Settings")]
        [Range(1, 50)]
        public int renderDistance;  // The number of chunks to be visible in the view distance
        public int lodStep = 3;
        public Transform viewer;
        public int chunkSize;
        public int LODCount => renderDistance / lodStep;
        
        public Material terrainMaterial;
        
        public Vector2 viewerPosition;
        public Vector2 viewerPositionOld;
        
        public static MapGenerator mapGenerator;

        public Dictionary<Vector2Int, TerrainChunk> terrainChunks = new Dictionary<Vector2Int, TerrainChunk>();
        public List<TerrainChunk> terrainChunksVisibleLastUpdate = new List<TerrainChunk>();
        public List<TerrainChunk> terrainChunksVisible = new List<TerrainChunk>();

        bool updateChunks = true;

        public void UpdateRenderDistance(int value)
        {
            renderDistance = value;
            updateChunks = true;
        }
        private void Start()
        {
            viewer = Camera.main.transform;
            mapGenerator = FindObjectOfType<MapGenerator>();

            StartCoroutine(UpdateVisibleChunksRoutine());
        }

        private void Update()
        {
            viewerPosition = new Vector2(viewer.position.x, viewer.position.z);
            if ((viewerPositionOld - viewerPosition).sqrMagnitude > chunkSize * chunkSize) // If the viewer has moved more than a chunk
            {
                viewerPositionOld = viewerPosition;
                updateChunks = true;
            }
        }
        IEnumerator UpdateVisibleChunksRoutine()
        {
            WaitUntil waitUpdateChunks = new WaitUntil(() => updateChunks);
            while (true)
            {
                yield return waitUpdateChunks;
                UpdateVisibleChunks();
                updateChunks = false;
            }
        }
        private void UpdateVisibleChunks()
        {
            for (int i = 0; i < terrainChunksVisibleLastUpdate.Count; i++)
            {
                terrainChunksVisibleLastUpdate[i].SetVisible(false);
            }
            terrainChunksVisibleLastUpdate.Clear(); 
            int currentChunkCoordX = Mathf.RoundToInt(viewerPosition.x / chunkSize);
            int currentChunkCoordY = Mathf.RoundToInt(viewerPosition.y / chunkSize);

            for (int yOffset = 0; yOffset <= renderDistance; yOffset++)
            {
                for (int xOffset = 0; xOffset <= renderDistance; xOffset++)
                {
                    // Manhattan distance
                    if (xOffset + yOffset > renderDistance)
                    {
                        continue;
                    }
                    int xPositive = currentChunkCoordX + xOffset;
                    int xNegative = currentChunkCoordX - xOffset;
                    int yPositive = currentChunkCoordY + yOffset;
                    int yNegative = currentChunkCoordY - yOffset;

                    Vector2Int[] vector2Ints = new Vector2Int[]
                    {
                        new Vector2Int(xPositive, yPositive), // Top Right
                        new Vector2Int(xPositive, yNegative), // Bottom Right
                        new Vector2Int(xNegative, yNegative), // Bottom Left
                        new Vector2Int(xNegative, yPositive) // Top Left                        
                    };

                    int distance = Mathf.Max(xOffset, yOffset);
                    int lod = distance / lodStep;

                    for (int j = 0; j < vector2Ints.Length; j++)
                    {
                        Vector2Int viewedChunkCoord = vector2Ints[j];
                        if (terrainChunks.ContainsKey(viewedChunkCoord))
                        {
                            terrainChunks[viewedChunkCoord].SetVisible(true, lod);
                        }
                        else
                        {
                            Vector3 position = new Vector3(viewedChunkCoord.x * (chunkSize), 0, viewedChunkCoord.y * (chunkSize));
                            terrainChunks.Add(viewedChunkCoord, new TerrainChunk(viewedChunkCoord, position, transform, terrainMaterial, lod));
                        }
                        terrainChunksVisibleLastUpdate.Add(terrainChunks[viewedChunkCoord]);
                    }
                }
            }
        }
    }
}
