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

    private Vector2 Location;
        // -1, 1  NorthEast
        //  0, 1  North
        //  1, 1  NorthWest
        // -1, 0  East
        //  0, 0  Centre
        //  1, 0  West
        // -1,-1  SouthEast
        //  0,-1  South
        //  1,-1  SouthWest

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Destroying the current (previous) chunk, when its no longer visible
        if (!CurrentChunk.IsVisible)
        {
            Destroy(CurrentChunk.gameObject);
            foreach(var chunk in NextWorldChunks.Chunks)
            {
                if (chunk.IsPositionInChunk(Player.position))
                {
                    CurrentChunk = chunk;
                    NextWorldChunks.Chunks.Remove(chunk);
                }
                break;
            }
        }

        // destroying any of the next chunks if they become no longer visible
        // putting them back into the worldchunks stack, as they won't have been properly explored yet
        if (NextWorldChunks != null && NextWorldChunks.Chunks.Count > 0)
        {
            foreach (var chunk in NextWorldChunks.Chunks)
            {
                if (!chunk.IsVisible)
                {
                    NextWorldChunks.Chunks.Remove(chunk);
                    WorldChunks.Push(chunk);
                    Destroy(chunk);
                }
            }
        }
    }

    void GenerateQueue(Random rand)
    {
        WorldChunks = new Stack<WorldChunk>();

        var queueLength = 20;
        for (int i = 0; i < queueLength; i++)
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

        // Instantiating chunk adjacent to location, if one isn't already there
        Instantiate(
            NextWorldChunks.Chunks[0],
            CurrentChunk.transform.position + new Vector3(CurrentChunk.Width * Location.x, CurrentChunk.Height * Location.y),
            Quaternion.identity);

        // Instantiating edge chunks, if location is in corner
        if (isCorner)
        {
            Instantiate(
                NextWorldChunks.Chunks[1],
                CurrentChunk.transform.position + new Vector3(CurrentChunk.Width * Location.x, 0),
                Quaternion.identity);
            Instantiate(
                NextWorldChunks.Chunks[2],
                CurrentChunk.transform.position + new Vector3(0, CurrentChunk.Height * Location.y),
                Quaternion.identity);
        }
    }

    private NextWorldChunks GetNextWorldChunks(bool isCorner)
    {
        Random rand = new Random(Seed);
        var nextChunks = new NextWorldChunks();
        
        if (rand.NextDouble() < ProbabilityOfEventChunk())
        {
            PreviousWorldChunks.Push(CurrentChunk);
            nextChunks.Add(EventChunk);
            if (isCorner)
            {
                nextChunks.Add(WorldChunks.Pop());
                nextChunks.Add(WorldChunks.Pop());
            }
            return nextChunks;
        }

        if (rand.NextDouble() < ProbOfRepeatChunk)
        {
            nextChunks.Add(CurrentChunk);
            if (isCorner)
            {
                nextChunks.Add(CurrentChunk);
                nextChunks.Add(CurrentChunk);
            }
            return nextChunks;
        }

        if (rand.NextDouble() < ProbOfPrevChunk)
        {
            nextChunks.Add(PreviousWorldChunks.Pop());
            if (isCorner)
            {
                nextChunks.Add(PreviousWorldChunks.Pop());
                nextChunks.Add(PreviousWorldChunks.Pop());
            }
            return nextChunks;
        }

        PreviousWorldChunks.Push(CurrentChunk);
        nextChunks.Add(WorldChunks.Pop());
        if (isCorner)
        {
            nextChunks.Add(WorldChunks.Pop());
            nextChunks.Add(WorldChunks.Pop());
        }
        return nextChunks;
    }

    double ProbabilityOfEventChunk()
    {
        return 1 / (1 + Mathf.Exp(-EventChunkSlope * (PreviousWorldChunks.Count - EventChunkMidPoint)));
    }
}
