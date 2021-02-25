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

    public float scale = 1f;

    // Renderer to visualise the noise
    public Transform noiseRenderer;

    public Vector3 displacement = new Vector3(20, 20);

    public bool generateTrees;
    [Range(0f, 1f)]
    public float treeFloat;
    // trees per quadrant row/column
    // more than 16 is just nuts
    [Range(0, 16)]
    public int treeDensity;
    public GameObject[] treePrefabs;
    public Transform treeContainer;
    private bool trees;

    public bool generateRocks;
    [Range(0f, 1f)]
    public float rockFloat;
    [Range(0, 16)]
    public int rockDensity;
    public GameObject[] rockPrefabs;
    public Transform rockContainer;
    private bool rocks;

    public Gradient colouring;

    private Texture2D noiseTex;
    private Sprite sprite;

    void Start()
    {
        trees = generateTrees && treeDensity != 0;
        rocks = generateRocks && rockDensity != 0;

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

        int treePos = 0;
        int rockPos = 0;
        if (trees)
        {
            var treeQuadrant = resolution / treeDensity;
            treePos = (int)(0.5f * treeQuadrant);
        }
        if (rocks)
        {
            var rockQuadrant = resolution / treeDensity;
            rockPos = (int)(0.5f * rockQuadrant);
        }

        NoiseMethod method = Noise.noiseMethods[(int)type][dimensions - 1];
        var stepSize = 1f / resolution;

        // Corners of the texture
        Vector3 point00 = noiseRenderer.TransformPoint(new Vector3(-0.5f, -0.5f)) / scale;
        Vector3 point10 = noiseRenderer.TransformPoint(new Vector3(0.5f, -0.5f)) / scale;
        Vector3 point01 = noiseRenderer.TransformPoint(new Vector3(-0.5f, 0.5f)) / scale;
        Vector3 point11 = noiseRenderer.TransformPoint(new Vector3(0.5f, 0.5f)) / scale;

        for (int y = 0; y < resolution; y++)
        {
            Vector3 point0 = Vector3.Lerp(point00, point01, (y + 0.5f) * stepSize);
            Vector3 point1 = Vector3.Lerp(point10, point11, (y + 0.5f) * stepSize);
            for (int x = 0; x < resolution; x++)
            {
                Vector3 point = Vector3.Lerp(point0, point1, (x + 0.5f) * stepSize);
                float sample = 
                    useFractal ? 
                        Noise.Sum(method, point, frequency, octaves, lacunarity, persistence).value : 
                        method(point, frequency).value;

                sample = sample * 0.5f + 0.5f;

                noiseTex.SetPixel(x, y, colouring.Evaluate(sample));
                if (trees && x % treePos == 0 && y % treePos == 0)
                {
                    // place tree at point if greater than tree float
                    if (sample < treeFloat)
                    {
                        var treePoint = point * scale;
                        var prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                        Instantiate(prefab, treePoint + (1f / (4f * treeDensity)) * new Vector3(1, 1) * transform.localScale.x + (new Vector3(Random.value, Random.value) - new Vector3(1f, 1f)) * (8f / treeDensity), transform.rotation, treeContainer);
                    }
                }
                if (rocks && x % rockPos == 0 && y % rockPos == 0)
                {
                    if (sample > (1 - rockFloat))
                    {
                        var rockPoint = point * scale;
                        var prefab = rockPrefabs[Random.Range(0, rockPrefabs.Length)];
                        Instantiate(prefab, rockPoint + (1f / (4f * rockDensity)) * new Vector3(1, 1) * transform.localScale.x + (new Vector3(Random.value, Random.value) - new Vector3(1f, 1f)) * (8f / rockDensity), transform.rotation, rockContainer);
                    }
                }
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
