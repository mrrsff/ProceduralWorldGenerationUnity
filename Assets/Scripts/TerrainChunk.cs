using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Profiling;

namespace Axegen
{
    public class TerrainChunk
    {
        public Vector2Int coord;
        public TerrainChunkObject chunkObject;
        public MeshFilter meshFilter;
        public MeshRenderer meshRenderer;
        public MeshCollider meshCollider;
        public Mesh[] lodMeshes;
        public int currentLOD;
        private bool colliderCreated = false;
        private bool initialized = false;
        private bool visible = false;

        public static MeshColliderCookingOptions cookingOptions = MeshColliderCookingOptions.WeldColocatedVertices & MeshColliderCookingOptions.UseFastMidphase & MeshColliderCookingOptions.CookForFasterSimulation;

        public TerrainChunk(Vector2Int coord, Vector3 position, Transform parent, Material material, int lod)
        {
            Profiler.BeginSample("TerrainChunk Constructor");

            this.coord = coord;
            chunkObject = PoolManager.Instance.GetChunkObject();
            chunkObject.name = "Terrain Chunk";
            chunkObject.transform.position = position;
            chunkObject.transform.parent = parent;

            currentLOD = lod;
            visible = true;

            meshFilter = chunkObject.meshFilter;
            meshRenderer = chunkObject.meshRenderer;
            meshCollider = chunkObject.meshCollider;
            meshRenderer.material = material;
            Profiler.EndSample();

            MapGenerator.instance.RequestMapData(coord, OnMapDataReceived);
        }
        void OnMapDataReceived(MapData mapData)
        {
            MapGenerator.instance.RequestMeshData(mapData, OnMeshDataReceived);
        }
        void OnMeshDataReceived(MeshData[] meshData)
        {
            lodMeshes = new Mesh[meshData.Length];
            for (int i = 0; i < meshData.Length; i++)
            {
                lodMeshes[i] = meshData[i].CreateMesh();
            }
            initialized = true;
            SetVisible(visible, currentLOD);
        }
        void CreateCollider()
        {
            if (!colliderCreated)
            {
                meshCollider.sharedMesh = lodMeshes[0];
                colliderCreated = true;
            }
        }
        public void SetVisible(bool visible, int lod = 0)
        {
            this.visible = visible;
            currentLOD = lod;

            if (!initialized) return;

            meshRenderer.enabled = visible;
            if (visible)
            {
                UpdateLOD(lod);
            }
        }

        public void UpdateLOD(int lod)
        {
            if (lodMeshes == null) return;
            currentLOD = Mathf.Clamp(lod, 0, lodMeshes.Length - 1);
            if (lodMeshes[currentLOD] != null)
            {
                meshFilter.mesh = lodMeshes[currentLOD];

                if (lod == 0) // If the chunk is close enough to the viewer
                {
                    CreateCollider();
                }
            }
        }
    }
}
