using UnityEngine;
using System.Collections.Generic;

namespace StoatVsVole
{
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class EnvironmentGroundSpawner : MonoBehaviour
    {
        [Header("General Ground Settings")]
        public Material groundMaterial;
        public int resolution = 128;
        public float heightMultiplier = 10f;
        public GlobalSettings globalSettings;

        [Header("Wall Settings")]
        [Range(0f, 1f)] public float wallRadiusFraction = 1f;

        [Header("Noise Blending Settings")]
        [Range(0f, 1f)] public float perlinWeight = 0.5f;

        [Header("Perlin Noise Settings")]
        public float perlinNoiseScale = 10f;
        public int perlinOctaves = 4;
        public float perlinLacunarity = 2f;
        public float perlinPersistence = 0.5f;

        [Header("Ridged Noise Settings")]
        public float ridgedNoiseScale = 10f;
        public int ridgedOctaves = 4;
        public float ridgedLacunarity = 2f;
        public float ridgedPersistence = 0.5f;
        public float ridgedSharpness = 2f;
        public int seed = 0;

        private Mesh mesh;
        private Vector3[] vertices;

        void Start()
        {
            GenerateGround();
        }

        public float GetWallRadius() => globalSettings.worldSize * 0.5f * wallRadiusFraction;

        public void GenerateGround()
        {
            mesh = new Mesh();
            mesh.name = "Procedural Ground";

            int vertsPerLine = resolution + 1;
            vertices = new Vector3[vertsPerLine * vertsPerLine];
            Vector2[] uv = new Vector2[vertices.Length];
            List<int> triangleList = new List<int>();

            float halfSize = globalSettings.worldSize / 2f;
            float[,] heightMap = new float[vertsPerLine, vertsPerLine];
            float minHeight = float.MaxValue;
            float maxHeight = float.MinValue;

            System.Random prng = new System.Random(seed);
            Vector2[] perlinOffsets = new Vector2[perlinOctaves];
            Vector2[] ridgedOffsets = new Vector2[ridgedOctaves];

            for (int i = 0; i < perlinOctaves; i++)
                perlinOffsets[i] = new Vector2(prng.Next(-100000, 100000), prng.Next(-100000, 100000));
            for (int i = 0; i < ridgedOctaves; i++)
                ridgedOffsets[i] = new Vector2(prng.Next(-100000, 100000), prng.Next(-100000, 100000));

            for (int z = 0; z < vertsPerLine; z++)
            {
                for (int x = 0; x < vertsPerLine; x++)
                {
                    float percentX = (float)x / resolution;
                    float percentZ = (float)z / resolution;
                    float worldX = -halfSize + percentX * globalSettings.worldSize;
                    float worldZ = -halfSize + percentZ * globalSettings.worldSize;

                    float perlin = CalculatePerlin(worldX, worldZ, perlinOffsets);
                    float ridged = CalculateRidged(worldX, worldZ, ridgedOffsets);
                    float height = Mathf.Lerp(ridged, perlin, perlinWeight);

                    heightMap[x, z] = height;
                    minHeight = Mathf.Min(minHeight, height);
                    maxHeight = Mathf.Max(maxHeight, height);
                }
            }

            for (int z = 0; z < vertsPerLine; z++)
            {
                for (int x = 0; x < vertsPerLine; x++)
                {
                    int i = x + z * vertsPerLine;
                    float percentX = (float)x / resolution;
                    float percentZ = (float)z / resolution;
                    float worldX = -halfSize + percentX * globalSettings.worldSize;
                    float worldZ = -halfSize + percentZ * globalSettings.worldSize;

                    float normalizedHeight = Mathf.InverseLerp(minHeight, maxHeight, heightMap[x, z]);
                    float height = normalizedHeight * heightMultiplier;

                    vertices[i] = new Vector3(worldX, height, worldZ);
                    uv[i] = new Vector2(percentX, percentZ);
                }
            }

            for (int z = 0; z < resolution; z++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    int a = x + z * vertsPerLine;
                    int b = x + (z + 1) * vertsPerLine;
                    int c = (x + 1) + (z + 1) * vertsPerLine;
                    int d = (x + 1) + z * vertsPerLine;

                    triangleList.Add(a); triangleList.Add(b); triangleList.Add(c);
                    triangleList.Add(a); triangleList.Add(c); triangleList.Add(d);
                }
            }

            mesh.vertices = vertices;
            mesh.triangles = triangleList.ToArray();
            mesh.uv = uv;
            mesh.RecalculateNormals();

            GetComponent<MeshFilter>().mesh = mesh;
            // GetComponent<MeshRenderer>().material = groundMaterial;
            GetComponent<MeshCollider>().sharedMesh = mesh;
        }

        public float GetHeightAtWorldPosition(Vector3 position)
        {
            Vector3 localPos = transform.InverseTransformPoint(position);
            float percentX = Mathf.Clamp01((localPos.x + globalSettings.worldSize / 2f) / globalSettings.worldSize);
            float percentZ = Mathf.Clamp01((localPos.z + globalSettings.worldSize / 2f) / globalSettings.worldSize);

            int x = Mathf.RoundToInt(percentX * resolution);
            int z = Mathf.RoundToInt(percentZ * resolution);
            int index = z * (resolution + 1) + x;

            if (vertices != null && index >= 0 && index < vertices.Length)
            {
                return transform.TransformPoint(vertices[index]).y;
            }

            return position.y;
        }

        private float CalculatePerlin(float x, float z, Vector2[] offsets)
        {
            float height = 0f, frequency = 1f, amplitude = 1f;
            for (int i = 0; i < perlinOctaves; i++)
            {
                float sx = (x / perlinNoiseScale) * frequency + offsets[i].x;
                float sz = (z / perlinNoiseScale) * frequency + offsets[i].y;
                float noise = Mathf.PerlinNoise(sx, sz) * 2f - 1f;
                height += noise * amplitude;
                amplitude *= perlinPersistence;
                frequency *= perlinLacunarity;
            }
            return height;
        }

        private float CalculateRidged(float x, float z, Vector2[] offsets)
        {
            float height = 0f, frequency = 1f, amplitude = 1f;
            for (int i = 0; i < ridgedOctaves; i++)
            {
                float sx = (x / ridgedNoiseScale) * frequency + offsets[i].x;
                float sz = (z / ridgedNoiseScale) * frequency + offsets[i].y;
                float noise = 1f - Mathf.Abs(Mathf.PerlinNoise(sx, sz) * 2f - 1f);
                height += Mathf.Pow(noise, ridgedSharpness) * amplitude;
                amplitude *= ridgedPersistence;
                frequency *= ridgedLacunarity;
            }
            return height;
        }
    }
}
