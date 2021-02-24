using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkGenerator : MonoBehaviour
{
    public int seed;
    // Used for custom noise function
    [Range(8, 1024)]
    public int resolution;
    public float frequency = 1f;

    public bool randomiseSeed;
    public bool randomiseHash;

    public NoiseMethodType type;
    public bool useFractal;
    [Range(1, 8)]
    public int octaves;
    [Range(1f, 4f)]
    public float lacunarity = 2f;
    [Range(0f, 1f)]
    public float persistence = 0.5f;

    // Dimensions for noise
    [Range(1, 3)]
    public int dimensions = 3;

    // Renderer to visualise the noise
    public Transform noiseRenderer;

    public Vector3 displacement = new Vector3(20, 20);

    public bool generateTrees;
    [Range(0f, 1f)]
    public float treeFloat;
    public GameObject[] treePrefabs;
    public Transform treeContainer;

    public bool generateRocks;
    [Range(0f, 1f)]
    public float rockFloat;
    public GameObject rockPrefab;
    public Transform rockContainer;

    private Texture2D noiseTex;

    void Start()
    {
        if (randomiseHash)
            Noise.RandomiseHash();
        if (randomiseSeed)
        {
            Random.InitState((int)System.DateTime.UtcNow.ToBinary());
        }
        else
        {
            Random.InitState(seed);
        }

        Generate();
    }

    void Update()
    {
        if (noiseRenderer.hasChanged)
        {
            noiseRenderer.hasChanged = false;
            Generate();
        }
    }

    public void Generate()
    {
        if (noiseTex == null)
        {
            noiseTex = new Texture2D(resolution, resolution, TextureFormat.RGB24, true);
            noiseTex.name = "Procedural Texture";
            noiseTex.wrapMode = TextureWrapMode.Clamp;
            noiseTex.filterMode = FilterMode.Trilinear;
            noiseTex.anisoLevel = 9;
            noiseRenderer.GetComponent<MeshRenderer>().material.mainTexture = noiseTex;
        }
        GenerateNoise();
    }

    public void GenerateNoise()
    {
        if (treeContainer.childCount > 0)
        {
            foreach (Transform child in treeContainer)
            {
                Destroy(child.gameObject);
            }
        }
        if (rockContainer.childCount > 0)
        {
            foreach (Transform child in rockContainer)
            {
                Destroy(child.gameObject);
            }
        }

        if (noiseTex.width != resolution || noiseTex.height != resolution)
        {
            noiseTex.Resize(resolution, resolution);
        }

        NoiseMethod method = Noise.noiseMethods[(int)type][dimensions - 1];
        var stepSize = 1f / resolution;

        // Corners of the texture
        Vector3 point00 = noiseRenderer.TransformPoint(new Vector3(-0.5f, -0.5f));
        Vector3 point10 = noiseRenderer.TransformPoint(new Vector3(0.5f, -0.5f));
        Vector3 point01 = noiseRenderer.TransformPoint(new Vector3(-0.5f, 0.5f));
        Vector3 point11 = noiseRenderer.TransformPoint(new Vector3(0.5f, 0.5f));

        for (int y = 0; y < resolution; y++)
        {
            Vector3 point0 = Vector3.Lerp(point00, point01, (y + 0.5f) * stepSize);
            Vector3 point1 = Vector3.Lerp(point10, point11, (y + 0.5f) * stepSize);
            for (int x = 0; x < resolution; x++)
            {
                Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
                float sample = 
                    useFractal ? 
                        Noise.Sum(method, point, frequency, octaves, lacunarity, persistence) : 
                        method(point, frequency);

                if (type != NoiseMethodType.Value)
                    sample = sample * 0.5f + 0.5f;

                noiseTex.SetPixel(x, y, Color.white * sample);
                // place tree at point if greater than tree float
                if (generateTrees && sample < treeFloat)    
                    Instantiate(treePrefabs[Random.Range(0, treePrefabs.Length)], new Vector3(point.x * displacement.x, point.y * displacement.y, 0) + new Vector3(Random.value, Random.value) * 0.5f - point11, transform.rotation, treeContainer);
                if (generateRocks && sample > (1 - rockFloat))
                    Instantiate(rockPrefab, new Vector3(point.x * displacement.x, point.y * displacement.y, 0) + new Vector3(Random.value, Random.value) * 0.5f - point11, transform.rotation, rockContainer);
            }
        }

        // Copy the pixel data to the texture and load it into the GPU.
        noiseTex.Apply();
    }

    //void Update()
    //{
    //    CalcNoise();
    //}
}
