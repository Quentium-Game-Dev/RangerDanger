using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class ChunkGenerator : MonoBehaviour
{
    public int seed;
    // Width and height of the texture in pixels.
    public int pixWidth;
    public int pixHeight;

    // The origin of the sampled area in the plane.
    public float xOrigin;
    public float yOrigin;
    public int xMaxOrigin = 1023;
    public int yMaxOrigin = 1023;
    [Range(0, 10)]
    public int density;

    // The number of cycles of the basic noise pattern that are repeated
    // over the width and height of the texture.
    public float scale = 1.0F;

    public bool randomiseOrigin;
    public bool randomiseSeed;

    public enum Type
    {
        Perlin,
        Blue
    }
    public Type noiseType;

    public Vector3 displacement = new Vector3(-10, -10);

    // Renderer to visualise the noise
    public Renderer rend;

    public GameObject treePrefab;
    public Transform treeContainer;
    public float treeFloat;

    private Texture2D noiseTex;
    private Color[] pix;
    private Random rand;

    void Start()
    {
        if (randomiseSeed)
        {
            rand = new Random((int)System.DateTime.UtcNow.ToBinary());
        }
        else
        {
            rand = new Random(seed);
        }

        if (randomiseOrigin)
        {
            xOrigin = rand.Next(xMaxOrigin);
            yOrigin = rand.Next(yMaxOrigin);
        }
        // Set up the texture and a Color array to hold pixels during processing.
        noiseTex = new Texture2D(pixWidth, pixHeight);
        pix = new Color[noiseTex.width * noiseTex.height];
        rend.material.mainTexture = noiseTex;

        if (noiseType == Type.Perlin) GeneratePerlinNoise();
        else if (noiseType == Type.Blue) GenerateBlueNoise();
    }

    public void GeneratePerlinNoise()
    {
        // For each pixel in the texture...
        float y = 0.0F;

        while (y < noiseTex.height)
        {
            float x = 0.0F;
            while (x < noiseTex.width)
            {
                float xCoord = xOrigin + x / noiseTex.width * scale;
                float yCoord = yOrigin + y / noiseTex.height * scale;
                float sample = Mathf.PerlinNoise(xCoord, yCoord);

                var divs = 1;// rand.Next(3, 10);
                if (y % divs == 0 && x % divs == 0)
                {
                    if (sample > treeFloat) Instantiate(treePrefab, new Vector3(x / 10, y / 10), transform.rotation, transform);
                }
                pix[(int)y * noiseTex.width + (int)x] = new Color(sample, sample, sample);
                x++;
            }
            y++;
        }

        // Copy the pixel data to the texture and load it into the GPU.
        noiseTex.SetPixels(pix);
        noiseTex.Apply();
    }

    void GenerateBlueNoise()
    {
        if (treeContainer.childCount > 0)
        {
            foreach (Transform child in treeContainer)
            {
                Destroy(child.gameObject);
            }
        }

        float[,] bluenoise = new float[pixWidth, pixHeight];
        // For each pixel in the texture...
        float y = 0.0F;


        while (y < noiseTex.height)
        {
            float x = 0.0F;
            while (x < noiseTex.width)
            {
                float nx = x / noiseTex.width - 0.5f;
                float ny = y / noiseTex.height - 0.5f;
                var sample = bluenoise[(int)x,(int)y] = (float)rand.NextDouble();

                /*
                var divs = 1;// rand.Next(3, 10);
                if (y % divs == 0 && x % divs == 0)
                {
                    if (sample > treeFloat) Instantiate(treePrefab, new Vector3(x / 10, y / 10), transform.rotation, transform);
                }
                */
                pix[(int)y * noiseTex.width + (int)x] = new Color(sample, sample, sample);
                x++;
            }
            y++;
        }

        for (int yc = 0; yc < pixHeight; yc++)
        {
            for (int xc = 0; xc < pixWidth; xc++)
            {
                double max = 0;
                // there are more efficient algorithms than this
                for (int yn = yc - density; yn <= yc + density; yn++)
                {
                    for (int xn = xc - density; xn <= xc + density; xn++)
                    {
                        if (0 <= yn && yn < pixHeight && 0 <= xn && xn < pixWidth)
                        {
                            double e = bluenoise[yn,xn];
                            if (e > max) { max = e; }
                        }
                    }
                }
                if (bluenoise[yc,xc] == max)
                {
                    // place tree at xc,yc
                    Instantiate(treePrefab, new Vector3(xc, yc) + displacement, transform.rotation, treeContainer);
                }
            }
        }

        // Copy the pixel data to the texture and load it into the GPU.
        noiseTex.SetPixels(pix);
        noiseTex.Apply();
    }

    //void Update()
    //{
    //    CalcNoise();
    //}
}
