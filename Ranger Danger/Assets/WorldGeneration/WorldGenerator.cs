using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class WorldGenerator : MonoBehaviour
{
    public List<WorldChunk> allWorldChunks;
    public Queue<WorldChunk> worldChunks;
    public Stack<WorldChunk> previousWorldChunks;

    public WorldChunk currentChunk;
    public WorldChunk nextChunk;
    public EventChunk eventChunk;

    public double probOfPrevChunk;
    public double probOfRepeatChunk;

    public float eventChunkSlope = 0.75f;
    public float eventChunkMidPoint = 10; 

    public int seed;

    // Start is called before the first frame update
    void Start()
    {
        Random rand = new Random(seed);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void GenerateQueue(Random rand)
    {
        worldChunks = new Queue<WorldChunk>();

        var queueLength = 20;
        for (int i = 0; i < queueLength; i++)
        {
            worldChunks.Enqueue(allWorldChunks[rand.Next(allWorldChunks.Count)]);
        }
    }

    // method to run whenever the player steps on a different chunk
    void UpdateWorldChunk()
    {
        currentChunk = nextChunk;
    }

    // method to run whenever the previous block has left the camera
    void UpdateNextWorldChunk()
    {
        Random rand = new Random(seed);
        
        if (rand.NextDouble() < ProbabilityOfEventChunk())
        {
            nextChunk = eventChunk;
            return;
        }

        if (rand.NextDouble() < probOfRepeatChunk)
        {
            nextChunk = currentChunk;
            return;
        }

        if (rand.NextDouble() < probOfPrevChunk)
        {
            nextChunk = previousWorldChunks.Pop();
            return;
        }

        previousWorldChunks.Push(currentChunk);
        nextChunk = worldChunks.Dequeue();
    }

    double ProbabilityOfEventChunk()
    {
        return 1 / (1 + Mathf.Exp(-eventChunkSlope * (previousWorldChunks.Count - eventChunkMidPoint)));
    }
}
