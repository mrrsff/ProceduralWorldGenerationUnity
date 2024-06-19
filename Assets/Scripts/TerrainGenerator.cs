using UnityEngine;

namespace Axegen
{
    public static class TerrainGenerator
    {
        public static MeshData CreateMeshData(MapData mapData, BiomeCollection biomes, int lod = 0)
        {
            float[,] data = mapData.heightMap;
            int dataSize = data.GetLength(0);

            int step = 1 << lod;
            int verticesPerLine = (dataSize - 1) / step + 1;

            int sqrVertices = verticesPerLine * verticesPerLine;
            Vector3[] vertices = new Vector3[sqrVertices];
            Vector2[] uvs = new Vector2[sqrVertices];
            Color[] colors = new Color[sqrVertices];
            int[] triangles = new int[(sqrVertices - 2 * verticesPerLine + 1) * 6];

            var altitudeAndColorMap = biomes.GetAltitudeAndColorMap(mapData.chunkCoords, ref data);
            float[,] altitudeMap = altitudeAndColorMap.Item1;
            Color[,] colorMap = altitudeAndColorMap.Item2;

            for (int y = 0; y < verticesPerLine; y++)
            {
                for (int x = 0; x < verticesPerLine; x++)
                {
                    int heightIndex = y * verticesPerLine + x;
                    int xStep = x * step;
                    int yStep = y * step;
                    vertices[heightIndex] = new Vector3(xStep, altitudeMap[xStep, yStep], yStep);
                    uvs[heightIndex] = new Vector2((float)x / (verticesPerLine - 1), (float)y / (verticesPerLine - 1));
                    colors[heightIndex] = colorMap[xStep, yStep];
                }
            }

            int index = 0;
            for (int y = 0; y < verticesPerLine - 1; y++)
            {
                for (int x = 0; x < verticesPerLine - 1; x++)
                {
                    int baseIndex = y * verticesPerLine + x;
                    triangles[index] = baseIndex;
                    triangles[index + 1] = baseIndex + verticesPerLine;
                    triangles[index + 2] = baseIndex + 1;
                    triangles[index + 3] = baseIndex + 1;
                    triangles[index + 4] = baseIndex + verticesPerLine;
                    triangles[index + 5] = baseIndex + verticesPerLine + 1;
                    index += 6;
                }
            }

            return new MeshData
            {
                vertices = vertices,
                triangles = triangles,
                uvs = uvs,
                colors = colors,
                lod = lod
            };
        }
        public static MeshData[] CreateMeshDataWithLOD(MapData mapData, BiomeCollection biomes, int lodCount)
        {
            try
            {
                MeshData[] meshData = new MeshData[lodCount];
                for (int i = 0; i < lodCount; i++)
                {
                    meshData[i] = CreateMeshData(mapData, biomes, i);
                }
                return meshData;
            }
            catch(System.Exception e)
            {
                Debug.LogError("Error creating mesh data with LOD: " + e);

                return null;
            }
        }
        public static MapData CreateMapData(float[,] data, Vector2Int coords)
        {
            return new MapData
            {
                heightMap = data,
                chunkCoords = coords
            };
        }

    }
}
