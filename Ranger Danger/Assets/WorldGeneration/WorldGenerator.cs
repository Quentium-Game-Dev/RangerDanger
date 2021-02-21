using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class WorldGenerator : MonoBehaviour
{
    public Transform Player;

    public List<WorldChunk> AllWorldChunks;
    public Stack<WorldChunk> WorldChunks;
    public Stack<WorldChunk> PreviousWorldChunks;

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
        HasGenerated,
        Dirty
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
        GenerateQueue();
    }

    // Update is called once per frame
    void Update()
    {
        // Setting current chunk if it got destroyed
        if (CurrentChunk == null)
        {
            foreach (var chunk in NextWorldChunks.Chunks)
            {
                if (chunk.IsPositionInChunk(Player.position))
                {
                    CurrentChunk = chunk;
                    NextWorldChunks.Chunks.Remove(chunk);
                    State = GenerationState.Clean;
                    break;
                }
            }
        }

        if (State != GenerationState.Clean && NextWorldChunks != null)
        {
            foreach (var chunk in NextWorldChunks.Chunks)
            {
                if (chunk == null) NextWorldChunks.Chunks.Remove(chunk);
            }
            State = GenerationState.Clean;
        }

        // Destroying the current (previous) chunk, when its no longer visible
        // TODO: also need to remove from memory
        /*
        if (CurrentChunk != null && !CurrentChunk.IsVisible && State != GenerationState.Clean)
        {
            PreviousWorldChunks.Push(CurrentChunk);
            Destroy(CurrentChunk);
            CurrentChunk = null;
        }

        // destroying any of the next chunks if they become no longer visible
        // putting them back into the worldchunks stack, as they won't have been properly explored yet
        if (NextWorldChunks != null && NextWorldChunks.Chunks.Count > 0 && State != GenerationState.Clean)
        {
            foreach (var chunk in NextWorldChunks.Chunks)
            {
                if (!chunk.IsVisible)
                {
                    NextWorldChunks.Chunks.Remove(chunk);
                    WorldChunks.Push(chunk);
                    Destroy(chunk);
                    State = GenerationState.Clean;
                }
            }
        }
        if (NextWorldChunks != null && NextWorldChunks.Chunks.Count == 0 && State != GenerationState.Clean)
            State = GenerationState.Dirty;
        */

        CalculatePlayerLocation();
        if (Location != PreviousLocation && Location != Vector2.zero && State == GenerationState.Clean)
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
        NextWorldChunks = GetNextWorldChunks(isCorner);

        var position = CurrentChunk.transform.position + new Vector3(CurrentChunk.Width * Location.x, CurrentChunk.Height * Location.y);
        // Instantiating chunk adjacent to location, if one isn't already there
        var chunk = ChunkAtPosition(position);
        if (chunk == null)
            NextWorldChunks.Chunks[0] = Instantiate(
                NextWorldChunks.Chunks[0],
                position,
                Quaternion.identity);
        else
            NextWorldChunks.Chunks[0] = chunk;

        // Instantiating edge chunks, if location is in corner
        if (isCorner)
        {
            position = CurrentChunk.transform.position + new Vector3(CurrentChunk.Width * Location.x, 0);
            chunk = ChunkAtPosition(position);
            if (chunk == null)
                NextWorldChunks.Chunks[1] = Instantiate(
                    NextWorldChunks.Chunks[1],
                    position,
                    Quaternion.identity);
            else
                NextWorldChunks.Chunks[1] = chunk;

            position = CurrentChunk.transform.position + new Vector3(0, CurrentChunk.Height * Location.y);
            chunk = ChunkAtPosition(position);
            if (chunk == null)
                NextWorldChunks.Chunks[2] = Instantiate(
                    NextWorldChunks.Chunks[2],
                    position,
                    Quaternion.identity);
            else
                NextWorldChunks.Chunks[2] = chunk;
        }
    }

    void CalculatePlayerLocation()
    {
        PreviousLocation = Location;

        var cameraExtent = GetCameraExtent();
        var chunkBoundsMax = GetChunkBoundsMax(cameraExtent);
        var chunkBoundsMin = GetChunkBoundsMin(cameraExtent);

        var cameraRelativePos = Camera.main.transform.InverseTransformPoint(CurrentChunk.transform.position);

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

    private NextWorldChunks GetNextWorldChunks(bool isCorner)
    {
        var rand = new Random(Seed);
        var count = NextWorldChunks.Chunks.Count;
        
        if (rand.NextDouble() < ProbabilityOfEventChunk())
        {
            if (count == 0)
                NextWorldChunks.Add(EventChunk);

            while (isCorner && count < 3)
                NextWorldChunks.Add(WorldChunks.Pop());

            return NextWorldChunks;
        }

        if (rand.NextDouble() < ProbOfRepeatChunk)
        {
            if (count == 0)
                NextWorldChunks.Add(CurrentChunk);

            while (isCorner && count < 3)
                NextWorldChunks.Add(CurrentChunk);
            
            return NextWorldChunks;
        }

        if (rand.NextDouble() < ProbOfPrevChunk)
        {
            if (count == 0)
                NextWorldChunks.Add(PreviousWorldChunks.Pop());

            while (isCorner && count < 3)
                NextWorldChunks.Add(PreviousWorldChunks.Pop());

            return NextWorldChunks;
        }

        if (count == 0)
            NextWorldChunks.Add(WorldChunks.Pop());
        while (isCorner && count < 3)
            NextWorldChunks.Add(WorldChunks.Pop());
        
        return NextWorldChunks;
    }

    private WorldChunk ChunkAtPosition(Vector3 pos)
    {
        foreach (var chunk in NextWorldChunks.Chunks)
        {
            if (chunk != null && chunk.transform.position.Equals(pos))
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
