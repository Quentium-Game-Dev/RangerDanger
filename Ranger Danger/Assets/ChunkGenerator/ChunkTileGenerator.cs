using UnityEngine;
using Serializable = System.SerializableAttribute;

public class ChunkTileGenerator : MonoBehaviour
{
    public int seed;
    // Used for custom noise function
    [Range(2, 512)]
    public int resolution;
    public float frequency = 1f;

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

    public Vector3 chunkSize;

    public bool generateObjects;
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
        public int position;
    }
    public GeneratedObject[] generatedObjects;

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

    public Transform tileContainer;
    public GameObject tilePrefab;
    public Shader textureBlend;

    public bool logging;

    private Texture2D noiseTex;

    void Awake()
    {
        for (int i = 0; i < generatedObjects.Length; i++)
        {
            var genObject = generatedObjects[i];
            genObject.canGenerate = genObject.generate && genObject.density != 0;
        }

        Random.InitState(seed);
        chunkSize = new Vector3(resolution, resolution);

        if (textureType == TextureType.Coloured) tileContainer = null;
        else
        {
            tileContainer.localScale = new Vector3(1f / (resolution + 2), 1f / (resolution + 2), 1f / (resolution + 2));
        }

        //Generate();
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

        CreateTiledChunk();
    }

    public void CreateTiledChunk()
    {
        foreach (GeneratedObject genObject in generatedObjects)
        {
            if (genObject.container.childCount > 0)
            {
                foreach (Transform child in genObject.container)
                {
                    Destroy(child.gameObject);
                }
            }
        }
        if (textureType == TextureType.Blended)
        {
            if (tileContainer != null && tileContainer.childCount > 0)
            {
                foreach (Transform child in tileContainer)
                {
                    Destroy(child.gameObject);
                }
            }
            //tileContainer.localScale = new Vector3(1f / resolution, 1f / resolution, 1f / resolution);
        }

        for (int i = 0; i < generatedObjects.Length; i++)
        {
            var genObject = generatedObjects[i];
            var quadrant = resolution / genObject.density;
            genObject.position = (int)(0.5f * quadrant);
        }

        NoiseSample[,] noise = GenerateNoise();

        for (int yn = 1; yn < resolution + 1; yn++)
        {
            for (int xn = 1; xn < resolution + 1; xn++)
            {
                var sample = noise[xn, yn];

                switch (textureType)
                {
                    case TextureType.Coloured:
                        noiseTex.SetPixel(xn, yn, colouring.Evaluate(sample.value));
                        break;
                    case TextureType.Blended:
                        noiseTex.SetPixel(xn, yn, colouring.Evaluate(sample.value));
                        CreateTile(sample, noise, xn, yn);
                        break;
                }

                // Generating objects
                if (generateObjects)
                {
                    foreach (GeneratedObject genObject in generatedObjects)
                    {
                        if (genObject.canGenerate && xn % genObject.position == 0 && yn % genObject.position == 0 && sample.value > genObject.height)
                        {
                            var genPoint = (sample.point0 * scale) + (1f / (4f * genObject.density)) * new Vector3(1, 1) * transform.localScale.x + (new Vector3(Random.value, Random.value) - new Vector3(1f, 1f)) * (8f / genObject.density);
                            var prefab = genObject.prefabs[Random.Range(0, genObject.prefabs.Length)];
                            Instantiate(prefab, genPoint, transform.rotation, genObject.container);
                        }
                    }
                }
            }
        }

        // Copy the pixel data to the texture and load it into the GPU.
        noiseTex.Apply();
    }

    private NoiseSample[,] GenerateNoise()
    {
        NoiseSample[,] noise = new NoiseSample[resolution + 2, resolution + 2];

        NoiseMethod method = Noise.noiseMethods[(int)type][dimensions - 1];
        var stepSize = 1f / (resolution + 2);

        // Corners of the texture
        Vector3 point00 = noiseRenderer.TransformPoint(new Vector3(-0.5f, -0.5f)) / scale;
        Vector3 point10 = noiseRenderer.TransformPoint(new Vector3(0.5f, -0.5f)) / scale;
        Vector3 point01 = noiseRenderer.TransformPoint(new Vector3(-0.5f, 0.5f)) / scale;
        Vector3 point11 = noiseRenderer.TransformPoint(new Vector3(0.5f, 0.5f)) / scale;

        for (int yn = 0; yn < resolution + 2; yn++)
        {
            Vector3 point0 = Vector3.Lerp(point00, point01, (yn + 0.5f) * stepSize);
            Vector3 point1 = Vector3.Lerp(point10, point11, (yn + 0.5f) * stepSize);

            for (int xn = 0; xn < resolution + 2; xn++)
            {
                Vector3 point = Vector3.Lerp(point0, point1, (xn + 0.5f) * stepSize);

                var sample =
                    useFractal && type != NoiseMethodType.None ?
                        Noise.Sum(method, point, frequency, octaves, lacunarity, persistence) :
                        method(point, frequency);
                // setting sample in 0 to 1, instead of -1 to 1
                sample = sample * 0.5f + 0.5f;
                sample.point = point;
                sample.point0 = point0;

                noise[xn, yn] = sample;
            }
        }
        return noise;
    }

    private void CreateTile(NoiseSample sample, NoiseSample[,] noise, int x, int y)
    {
        var noiseX0 = noise[x - 1, y].value;
        var noiseY0 = noise[x, y - 1].value;
        var noiseXY00 = noise[x - 1, y - 1].value;
        var noiseXY10 = noise[x + 1, y - 1].value;
        var noiseX1 = noise[x + 1, y].value;
        var noiseY1 = noise[x, y + 1].value;
        var noiseXY01 = noise[x - 1, y + 1].value;
        var noiseXY11 = noise[x + 1, y + 1].value;
        if (logging)
        {
            print(string.Format("({0}, {1})", x, y));
            print("sample: " + sample.value);
            print("");
            print("x0: " + noiseX0);
            print("x1: " + noiseX1);
            print("y0: " + noiseY0);
            print("y1: " + noiseY1);
            print("xy00: " + noiseXY00);
            print("xy11: " + noiseXY11);
            print("xy01: " + noiseXY01);
            print("xy10: " + noiseXY10);
            print("");
        }

        var textures = GetTextures(sample.value, noiseX0, noiseX1, noiseY0, noiseY1);

        var tile = Instantiate(tilePrefab, sample.point * scale - new Vector3(0,0,0.1f), transform.rotation, tileContainer);
        tile.name = string.Format("Tile ({0}, {1})", x, y);
        var tileRenderer = tile.GetComponent<MeshRenderer>();
        if (textures.Length == 1)
        {
            tileRenderer.material.mainTexture = textures[0].tex;
        }
        else
        {
            Material material = new Material(textureBlend);
            material.SetTexture("_Texture1", textures[0].tex);
            material.SetTexture("_Texture2", textures[1].tex);

            var minHeight = textures[0].heightMax;
            var maxHeight = textures[1].heightMin;

            var blend = NoiseToBlend(sample.value, minHeight, maxHeight);

            var x0 = NoiseToBlend(noiseX0, minHeight, maxHeight);
            var x1 = NoiseToBlend(noiseX1, minHeight, maxHeight);
            var y0 = NoiseToBlend(noiseY0, minHeight, maxHeight);
            var y1 = NoiseToBlend(noiseY1, minHeight, maxHeight);

            var xy00 = NoiseToBlend(noiseXY00, minHeight, maxHeight);
            var xy11 = NoiseToBlend(noiseXY11, minHeight, maxHeight);
            var xy10 = NoiseToBlend(noiseXY10, minHeight, maxHeight);
            var xy01 = NoiseToBlend(noiseXY01, minHeight, maxHeight);

            if (logging)
            {
                print("blend: " + blend);
                print("x0: " + x0);
                print("x1: " + x1);
                print("y0: " + y0);
                print("y1: " + y1);
                print("xy00: " + xy00);
                print("xy11: " + xy11);
                print("xy01: " + xy01);
                print("xy10: " + xy10);
                print("");
            }

            material.SetFloat("_Blend", blend);
            material.SetFloat("_BlendX0", x0);
            material.SetFloat("_BlendX1", x1);
            material.SetFloat("_BlendY0", y0);
            material.SetFloat("_BlendY1", y1);
            material.SetFloat("_BlendXY00", xy00);
            material.SetFloat("_BlendXY10", xy10);
            material.SetFloat("_BlendXY11", xy11);
            material.SetFloat("_BlendYX01", xy01);

            tileRenderer.material = material;
        }
    }

    private float NoiseToBlend(float noise, float minHeight, float maxHeight)
    {
        var diff = maxHeight - minHeight;
        return Mathf.Clamp((noise - minHeight) / diff, 0, 1);
    }

    private GroundTexture[] GetTextures(float sample, float x0, float x1, float y0, float y1)
    {
        GroundTexture[] textures = null;
        int idx = 0;

        for (int i = 0; i < groundTextures.Length; i++)
        {
            GroundTexture texA = groundTextures[i];
            GroundTexture texB;
            if (i < groundTextures.Length - 1) texB = groundTextures[i + 1];
            // If we reach this point, its only the last texture
            else {
                textures = new GroundTexture[] { texA };
                idx = i;
                break;
            }

            // in this case, we need to blend the textures
            if (sample > texA.heightMax && sample < texB.heightMin)
            {
                return new GroundTexture[] { texA, texB };
            }
            // in this case, we only need that texture
            if (sample >= texA.heightMin && sample < texA.heightMax)
            {
                textures = new GroundTexture[] { texA };
                idx = i;
                break;
            }
        }

        if (textures.Length == 1)
        {
            GroundTexture previousTex = new GroundTexture { name = null };
            GroundTexture nextTex = new GroundTexture { name = null };
            GroundTexture currentTex = textures[0];

            if (idx != 0)
                previousTex = groundTextures[idx - 1];
            if (idx != groundTextures.Length - 1)
                nextTex = groundTextures[idx + 1];

            if (previousTex.name != null &&
                    currentTex.heightMin > x0 || currentTex.heightMin > x1 ||
                    currentTex.heightMin > y0 || currentTex.heightMin > y1)
            {
                return new GroundTexture[] { previousTex, currentTex };
            }
            if (nextTex.name != null &&
                    currentTex.heightMax < x0 || currentTex.heightMax < x1 ||
                    currentTex.heightMax < y0 || currentTex.heightMax < y1)
            {
                return new GroundTexture[] { currentTex, nextTex };
            }
        }

        return textures;
    }

    // Manual blending of textures, very inefficient
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

