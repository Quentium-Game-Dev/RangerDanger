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

    public float scale = 1f;

    // Renderer to visualise the noise
    public Transform noiseRenderer;

    public Vector3 displacement = new Vector3(20, 20);

    public bool generateTrees;
    [Range(0f, 1f)]
    public float treeHeight;
    // trees per quadrant row/column
    // more than 16 is just nuts
    [Range(0, 16)]
    public int treeDensity;
    public GameObject[] treePrefabs;
    public Transform treeContainer;
    private bool trees;

    public bool generateRocks;
    [Range(0f, 1f)]
    public float rockHeight;
    [Range(0, 16)]
    public int rockDensity;
    public GameObject[] rockPrefabs;
    public Transform rockContainer;
    private bool rocks;
    
    public enum TextureType
    {
        Coloured,
        Blended
    }
    public TextureType textureType;
    public Gradient colouring;
    
    [System.Serializable]
    public struct GroundTexture
    {
        public string name;
        public Texture2D tex;
        [Range(0f, 1f)]
        public float heightMin;
        [Range(0f, 1f)]
        public float heightMax;
    }
    public GroundTexture[] groundTextures;
    // Should look something like this
    //  |--------,------------,-------,------------,-------|
    //  0             blend                blend           1
    // amin     amax         bmin    bmax         cmin    cmax
    //
    // So the first texture has heightMin = 0, and last has heightMax = 1
    // and the rest have heightMax < heightMin for the next one along

    private Texture2D noiseTex;

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

                switch (textureType)
                {
                    case TextureType.Coloured:
                        noiseTex.SetPixel(x, y, colouring.Evaluate(sample));
                        break;
                    case TextureType.Blended:
                        var textures = GetTextures(sample);
                        if (textures.Length == 2)
                            noiseTex.SetPixel(x, y, BlendTexture(sample, x, y, textures[0], textures[1]));
                        else
                            noiseTex.SetPixel(x, y, textures[0].tex.GetPixel(x, y));
                        break;
                }   

                if (trees && x % treePos == 0 && y % treePos == 0)
                {
                    // place tree at point if greater than tree float
                    if (sample > treeHeight)
                    {
                        var treePoint = point * scale;
                        var prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                        Instantiate(prefab, treePoint + (1f / (4f * treeDensity)) * new Vector3(1, 1) * transform.localScale.x + (new Vector3(Random.value, Random.value) - new Vector3(1f, 1f)) * (8f / treeDensity), transform.rotation, treeContainer);
                    }
                }
                if (rocks && x % rockPos == 0 && y % rockPos == 0)
                {
                    if (sample > rockHeight)
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

    private GroundTexture[] GetTextures(float sample)
    {
        for (int i = 0; i < groundTextures.Length; i++)
        {
            GroundTexture texA = groundTextures[i];
            GroundTexture texB;
            if (i < groundTextures.Length - 1) texB = groundTextures[i + 1];
            // If we reach this point, its only the last texture
            else return new GroundTexture[] { texA };

            // in this case, we need to blend the textures
            if (sample > texA.heightMax && sample < texB.heightMin)
            {
                return new GroundTexture[] { texA, texB };
            }
            // in this case, we only need that texture
            if (sample >= texA.heightMin && sample < texA.heightMax)
            {
                return new GroundTexture[] { texA };
            }
        }

        // Should never hit this
        return null;
    }

    private Color BlendTexture(float sample, int x, int y, GroundTexture a, GroundTexture b)
    {
        var aPix = a.tex.GetPixel(x, y);
        var bPix = b.tex.GetPixel(x, y);

        var maxDiff = b.heightMin - a.heightMax;
        var currentDiff = sample - a.heightMax;
        if (currentDiff < 0)
        {
            return aPix;
        }
        else if (currentDiff > maxDiff)
        {
            return bPix;
        }
        var t = currentDiff / maxDiff;

        var colour = new Color(Mathf.Lerp(aPix.r, bPix.r, t), Mathf.Lerp(aPix.g, bPix.g, t), Mathf.Lerp(aPix.b, bPix.b, t), 1);

        return colour;
    }

    //void Update()
    //{
    //    CalcNoise();
    //}
}
