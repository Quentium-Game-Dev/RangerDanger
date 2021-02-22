using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class WorldGenerator : MonoBehaviour
{
    public Transform Player;
    public Transform CameraTransform;

    public List<WorldChunk> AllWorldChunks;
    public Stack<WorldChunk> WorldChunks;
    public Stack<WorldChunk> PreviousWorldChunks;

    public WorldChunk FirstChunk;
    public WorldChunk CurrentChunk;
    public NextWorldChunks NextWorldChunks;
    public EventChunk EventChunk;

    public double ProbOfPrevChunk;
    public double ProbOfRepeatChunk;

    public float EventChunkSlope = 0.75f;
    public float EventChunkMidPoint = 10; 

    public int Seed;
    public int StackSize;

    public Vector3 CameraMargin = new Vector3(0.5f, 0.5f);

    public Vector2 PreviousLocation;
    public Vector2 Location;
        // -1, 1  NorthEast
        //  0, 1  North
        //  1, 1  NorthWest
        // -1, 0  East
        //  0, 0  Centre
        //  1, 0  West
        // -1,-1  SouthEast
        //  0,-1  South
        //  1,-1  SouthWest
    
    public enum GenerationState
    {
        Clean,
        HasGenerated
    }
    public GenerationState State;

    // Start is called before the first frame update
    void Start()
    {
        Initialise();
    }

    void Initialise()
    {
        NextWorldChunks = new NextWorldChunks();
        PreviousWorldChunks = new Stack<WorldChunk>();

        CurrentChunk = InstantiateChunk(FirstChunk, new Vector3(0,0,1));
        CurrentChunk.IsCurrent = true;
        GenerateQueue();
    }

    // Update is called once per frame
    void Update()
    {
        CalculatePlayerLocation();
        if (Location != PreviousLocation && Location != Vector2.zero) //&& State == GenerationState.Clean)
        {
            CreateNextWorldChunks();
            State = GenerationState.HasGenerated;
        }
    }

    void GenerateQueue()
    {
        var rand = new Random(Seed);
        WorldChunks = new Stack<WorldChunk>();

        for (int i = 0; i < StackSize; i++)
        {
            WorldChunks.Push(AllWorldChunks[rand.Next(AllWorldChunks.Count)]);
        }
    }

    // method to run whenever the player enters a 'zone'
    void CreateNextWorldChunks()
    {
        // if sum of mod(x) and mod(y) is 2, its a corner
        var isCorner = Mathf.Abs(Location.x) + Mathf.Abs(Location.y) == 2;
        GenerateNextWorldChunks(isCorner);

        var position = CurrentChunk.transform.position + new Vector3(CurrentChunk.Width * Location.x, CurrentChunk.Height * Location.y);
        // Instantiating chunk adjacent to location, if one isn't already there
        CreateChunk(position, isCorner);

        // Instantiating edge chunks, if location is in corner
        if (isCorner)
        {
            position = CurrentChunk.transform.position + new Vector3(CurrentChunk.Width * Location.x, 0);
            CreateChunk(position, isCorner);

            position = CurrentChunk.transform.position + new Vector3(0, CurrentChunk.Height * Location.y);
            CreateChunk(position, isCorner);
        }
    }

    void CalculatePlayerLocation()
    {
        PreviousLocation = Location;

        var cameraExtent = GetCameraExtent();
        var chunkBoundsMax = GetChunkBoundsMax(cameraExtent);
        var chunkBoundsMin = GetChunkBoundsMin(cameraExtent);

        var cameraRelativePos = CameraTransform.InverseTransformPoint(CurrentChunk.transform.position);

        int x;
        int y;

        if (cameraRelativePos.x < chunkBoundsMin.x) x = 1;
        else if (cameraRelativePos.x > chunkBoundsMax.x) x = -1;
        else x = 0;

        if (cameraRelativePos.y > chunkBoundsMax.y) y = -1;
        else if (cameraRelativePos.y < chunkBoundsMin.y) y = 1;
        else y = 0;

        Location = new Vector2(x, y);
    }

    #region private helper methods
    // Instantiates a chunk and returns the created one
    private void CreateChunk(Vector3 position, bool isCorner)
    {
        var chunkIndex = 0;
        var max = isCorner ? 3 : 1;
        WorldChunk prefab = null;

        while (chunkIndex < max)
        {
            prefab = NextWorldChunks.Chunks[chunkIndex];
            // If its a prefab, use it
            if (prefab.gameObject != null && !prefab.gameObject.activeInHierarchy) break;

            chunkIndex++;
            if (chunkIndex == max) return;
        }
        prefab.ChunkIndex = chunkIndex;

        var chunkAtPosition = ChunkAtPosition(position);
        if (chunkAtPosition == null)
        {
            var chunk = InstantiateChunk(prefab, position);
            NextWorldChunks.Replace(chunkIndex, chunk);
        }
        else
        {
            //NextWorldChunks.Replace(chunkIndex, chunkAtPosition);
        }
    }
    private WorldChunk InstantiateChunk(WorldChunk prefab, Vector3 position)
    {
        var chunk = Instantiate(
                prefab,
                position,
                Quaternion.identity);
        chunk.ChunkDestroyEvent += OnChunkDestroy;
        chunk.ChunkIndex = prefab.ChunkIndex;

        return chunk;
    }

    private void OnChunkDestroy(WorldChunk chunk)
    {
        // If the current chunk is destroyed, pick the one the player is standing on to be the next chunk
        if (chunk.IsCurrent)
        {
            foreach (var c in NextWorldChunks.Chunks)
            {
                if (c != null && c.IsPositionInChunk(Player.position))
                {
                    CurrentChunk = c;
                    c.IsCurrent = true;
                    NextWorldChunks.Remove(c.ChunkIndex);
                    State = GenerationState.Clean;
                    break;
                }
            }
            ResetChunkIndex();
        }
        // Else remove it from the next chunks, add it back to the stack
        // as this would only happen if the player didn't actually explore the chunk,
        // just skirted the edge
        // TODO: this adds a null thing as its been destroyed, would be nice to eventually
        // recycle unused world chunks
        else
        {
            NextWorldChunks.Remove(chunk.ChunkIndex);
            //WorldChunks.Push(chunk);

            // Resetting chunk index
            ResetChunkIndex();
            State = GenerationState.Clean;
        }
    }

    private void ResetChunkIndex()
    {
        for (var i = 0; i < NextWorldChunks.Count(); i++)
        {
            NextWorldChunks.Chunks[i].ChunkIndex = i;
        }
    }

    private Vector3 GetCameraExtent()
    {
        var camHeight = Camera.main.orthographicSize * 2f;
        var camWidth = camHeight * Camera.main.aspect;
        return new Vector3(camWidth / 2, camHeight / 2);
    }
    private Vector3 GetChunkBoundsMax(Vector3 cameraExtent)
    {
        var spriteMax = CurrentChunk.Sprite.bounds.max;
        return spriteMax - cameraExtent - CameraMargin;
    }
    private Vector3 GetChunkBoundsMin(Vector3 cameraExtent)
    {
        var spriteMin = CurrentChunk.Sprite.bounds.min;
        return spriteMin + cameraExtent + CameraMargin;
    }

    private void GenerateNextWorldChunks(bool isCorner)
    {
        var rand = new Random(Seed);

        WorldChunk specificChunk = null;
        if (rand.NextDouble() < ProbabilityOfEventChunk())
        {
            specificChunk = EventChunk;
        }

        if (rand.NextDouble() < ProbOfRepeatChunk)
        {
            specificChunk = CurrentChunk;
        }

        if (rand.NextDouble() < ProbOfPrevChunk)
        {
            specificChunk = PreviousWorldChunks.Pop();
        }

        PopulateNextWorldChunks(specificChunk, isCorner);
    }
    private void PopulateNextWorldChunks(WorldChunk chunk, bool isCorner)
    {
        var max = isCorner ? 3 : 1;
        var i = NextWorldChunks.Count();
        while (NextWorldChunks.Count() < max)
        {
            NextWorldChunks.Add(chunk ?? WorldChunks.Pop());
            i++;
        }
    }

    private WorldChunk ChunkAtPosition(Vector3 pos)
    {
        foreach (var chunk in NextWorldChunks.Chunks)
        {
            if (chunk != null && chunk.gameObject != null && chunk.gameObject.activeInHierarchy && chunk.transform.position.Equals(pos))
            {
                return chunk;
            }
        }
        return null;
    }

    private double ProbabilityOfEventChunk()
    {
        return 1 / (1 + Mathf.Exp(-EventChunkSlope * (PreviousWorldChunks.Count - EventChunkMidPoint)));
    }

    #endregion
}
