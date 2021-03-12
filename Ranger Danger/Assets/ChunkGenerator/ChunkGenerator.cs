using System.Collections.Generic;
using UnityEngine;
using Serializable = System.SerializableAttribute;

public class ChunkGenerator : MonoBehaviour
{
    public int seed;
    // Used for custom noise function
    [Range(8, 4096)]
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

    [Serializable]
    public struct GeneratedObject
    {
        public bool generate;
        public Transform container;
        public GameObject[] prefabs;
        [Range(0f, 1f)]
        public float height;
        [Range(0, 16)]
        public int density;

        [HideInInspector]
        public bool canGenerate;
        [HideInInspector]
        public int quadrant;
        [HideInInspector]
        public int position;
    }
    public GeneratedObject[] generatedObjects;

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

    public int textureResolution;
    [Serializable]
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

    public int tilingFactor;
    public bool tiles;
    // must be divisible by the resolution of the noise, and the resolution of the texture

    private Texture2D noiseTex;

    void Awake()
    {
        for (int i = 0; i < generatedObjects.Length; i++)
        {
            var genObject = generatedObjects[i];
            genObject.canGenerate = genObject.generate && genObject.density != 0;
        }
        trees = generateTrees && treeDensity != 0;
        rocks = generateRocks && rockDensity != 0;

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
            if (tiles)
            {
                noiseTex = new Texture2D(textureResolution, textureResolution, TextureFormat.RGB24, true);
                noiseTex.name = "Procedural Texture";
                noiseTex.wrapMode = TextureWrapMode.Clamp;
                noiseTex.filterMode = FilterMode.Trilinear;
                noiseTex.anisoLevel = 9;
                noiseRenderer.GetComponent<MeshRenderer>().material.mainTexture = noiseTex;
            }
            else
            {
                //if (textureType == TextureType.Coloured) noiseTex = new Texture2D(resolution, resolution, TextureFormat.RGB24, true);
                //else 
                noiseTex = new Texture2D(resolution * tilingFactor, resolution * tilingFactor, TextureFormat.RGB24, true);
                noiseTex.name = "Procedural Texture";
                noiseTex.wrapMode = TextureWrapMode.Clamp;
                noiseTex.filterMode = FilterMode.Trilinear;
                noiseTex.anisoLevel = 9;
                noiseRenderer.GetComponent<MeshRenderer>().material.mainTexture = noiseTex;
            }
        }

        if (tiles) GenerateNoise();
        else GenerateNoiseBigLoop();
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

        for (int i = 0; i < generatedObjects.Length; i++)
        {
            var genObject = generatedObjects[i];
            var quadrant = resolution / genObject.density;
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

        for (int yn = 0; yn < resolution; yn++)
        {
            Vector3 point0 = Vector3.Lerp(point00, point01, (yn + 0.5f) * stepSize);
            Vector3 point1 = Vector3.Lerp(point10, point11, (yn + 0.5f) * stepSize);

            for (int xn = 0; xn < resolution; xn++)
            {
                Vector3 point = Vector3.Lerp(point0, point1, (xn + 0.5f) * stepSize);

                var sample =
                    useFractal && type != NoiseMethodType.None ?
                        Noise.Sum(method, point, frequency, octaves, lacunarity, persistence).value :
                        method(point, frequency).value;
                // setting sample in 0 to 1, instead of -1 to 1
                sample = sample * 0.5f + 0.5f;

                switch (textureType)
                {
                    case TextureType.Coloured:
                        noiseTex.SetPixel(xn, yn, colouring.Evaluate(sample));
                        break;
                    case TextureType.Blended:
                        var textures = GetTextures(sample);
                        if (textures.Length == 2)
                            noiseTex.SetPixel(xn, yn, BlendTexture(sample, xn, yn, textures[0], textures[1]));
                        else
                        {
                            noiseTex.SetPixel(xn, yn, textures[0].tex.GetPixel(xn, yn));
                        }
                        break;
                }

                // Generating objects
                if (trees && xn % treePos == 0 && yn % treePos == 0)
                {
                    // place tree at point if greater than tree float
                    if (sample > treeHeight)
                    {
                        var treePoint = point0 * scale;
                        var prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                        Instantiate(prefab, treePoint + (1f / (4f * treeDensity)) * new Vector3(1, 1) * transform.localScale.x + (new Vector3(Random.value, Random.value) - new Vector3(1f, 1f)) * (8f / treeDensity), transform.rotation, treeContainer);
                    }
                }
                if (rocks && xn % rockPos == 0 && yn % rockPos == 0)
                {
                    if (sample > rockHeight)
                    {
                        var rockPoint = point0 * scale;
                        var prefab = rockPrefabs[Random.Range(0, rockPrefabs.Length)];
                        Instantiate(prefab, rockPoint + (1f / (4f * rockDensity)) * new Vector3(1, 1) * transform.localScale.x + (new Vector3(Random.value, Random.value) - new Vector3(1f, 1f)) * (8f / rockDensity), transform.rotation, rockContainer);
                    }
                }
            }
        }

        // Copy the pixel data to the texture and load it into the GPU.
        noiseTex.Apply();
    }

    public void GenerateNoiseBigLoop()
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

        for (int i = 0; i < generatedObjects.Length; i++)
        {
            var genObject = generatedObjects[i];
            var quadrant = resolution / genObject.density;
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

        int yn = 0;
        Vector3 point0 = Vector3.Lerp(point00, point01, (yn + 0.5f) * stepSize);
        Vector3 point1 = Vector3.Lerp(point10, point11, (yn + 0.5f) * stepSize);

        int xn = 0;
        // lerp point between point0 and point1
        Vector3 p0 = Vector3.Lerp(point0, point1, (xn + 0.5f) * stepSize);
        // lerp point between point0 and point1 for next coord
        Vector3 p1 = Vector3.Lerp(point0, point1, (xn + 1f + 0.5f) * stepSize);

        float sample0 =
            useFractal ?
                Noise.Sum(method, p0, frequency, octaves, lacunarity, persistence).value :
                method(p0, frequency).value;
        // setting sample in 0 to 1, instead of -1 to 1
        sample0 = sample0 * 0.5f + 0.5f;

        float sample1 =
            useFractal ?
                Noise.Sum(method, p1, frequency, octaves, lacunarity, persistence).value :
                method(p1, frequency).value;
        // setting sample in 0 to 1, instead of -1 to 1
        sample1 = sample1 * 0.5f + 0.5f;
        for (int yt = 0; yt < resolution * tilingFactor; yt++)
        {
            // yt = 0 ... 7 -> yn = 0, yt = 8 ... 15 -> yn = 1
            var ynNew = Mathf.FloorToInt(yt / tilingFactor);
            if (yn != ynNew)
            {
                yn = ynNew;
                point0 = Vector3.Lerp(point00, point01, (yn + 0.5f) * stepSize);
                point1 = Vector3.Lerp(point10, point11, (yn + 0.5f) * stepSize);
            }

            for (int xt = 0; xt < resolution * tilingFactor; xt++)
            {
                var xnNew = Mathf.FloorToInt((xt + 1) * tilingFactor);
                if (xn != xnNew)
                {
                    xn = xnNew;
                    // lerp point between point0 and point1
                    p0 = Vector3.Lerp(point0, point1, (xn + 0.5f) * stepSize);
                    // lerp point between point0 and point1 for next coord
                    p1 = Vector3.Lerp(point0, point1, (xn + 1f + 0.5f) * stepSize);
                    sample0 =
                        useFractal ?
                            Noise.Sum(method, p0, frequency, octaves, lacunarity, persistence).value :
                            method(p0, frequency).value;
                    // setting sample in 0 to 1, instead of -1 to 1
                    sample0 = sample0 * 0.5f + 0.5f;

                    sample1 =
                        useFractal ?
                            Noise.Sum(method, p1, frequency, octaves, lacunarity, persistence).value :
                            method(p1, frequency).value;
                    // setting sample in 0 to 1, instead of -1 to 1
                    sample1 = sample1 * 0.5f + 0.5f;

                    // Generating objects
                    if (trees && xn % treePos == 0 && yn % treePos == 0)
                    {
                        // place tree at point if greater than tree float
                        if (sample0 > treeHeight)
                        {
                            var treePoint = point0 * scale;
                            var prefab = treePrefabs[Random.Range(0, treePrefabs.Length)];
                            Instantiate(prefab, treePoint + (1f / (4f * treeDensity)) * new Vector3(1, 1) * transform.localScale.x + (new Vector3(Random.value, Random.value) - new Vector3(1f, 1f)) * (8f / treeDensity), transform.rotation, treeContainer);
                        }
                    }
                    if (rocks && xn % rockPos == 0 && yn % rockPos == 0)
                    {
                        if (sample0 > rockHeight)
                        {
                            var rockPoint = point0 * scale;
                            var prefab = rockPrefabs[Random.Range(0, rockPrefabs.Length)];
                            Instantiate(prefab, rockPoint + (1f / (4f * rockDensity)) * new Vector3(1, 1) * transform.localScale.x + (new Vector3(Random.value, Random.value) - new Vector3(1f, 1f)) * (8f / rockDensity), transform.rotation, rockContainer);
                        }
                    }
                }

                var sample = Mathf.Lerp(sample0, sample1, xt / (xn * tilingFactor));

                switch (textureType)
                {
                    case TextureType.Coloured:
                        noiseTex.SetPixel(xt, yt, colouring.Evaluate(sample));
                        break;
                    case TextureType.Blended:
                        var textures = GetTextures(sample);
                        if (textures.Length == 2)
                            noiseTex.SetPixel(xt, yt, BlendTexture(sample, xt % textureResolution, yt % textureResolution, textures[0], textures[1]));
                        else
                        {
                            noiseTex.SetPixel(xt, yt, textures[0].tex.GetPixel(xt % textureResolution, yt % textureResolution));
                        }
                        break;
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

    private Color ColourInterpolation(Color colour0, Color colour1, int x0, int y0, int n)
    {
        float t = n / tilingFactor;
        Color colour = new Color(Mathf.Lerp(colour0.r, colour1.r, t), Mathf.Lerp(colour0.g, colour1.g, t),
            Mathf.Lerp(colour0.b, colour1.b, t), Mathf.Lerp(colour0.a, colour1.a, t));
        return colour;
    }

    //void Update()
    //{
    //    CalcNoise();
    //}
}
