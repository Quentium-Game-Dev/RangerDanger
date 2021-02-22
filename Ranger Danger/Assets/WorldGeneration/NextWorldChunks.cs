using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NextWorldChunks
{
    public int EventChunkIdx;
    public List<WorldChunk> Chunks;

    public NextWorldChunks()
    {
        Chunks = new List<WorldChunk>(3);
    }

    public int Count()
    {
        return Chunks.Count;
    }

    public void Add(WorldChunk chunk)
    {
        if (chunk is EventChunk)
            EventChunkIdx = 0;

        Chunks.Add(chunk);
    }
    public void Remove(int i)
    {
        Chunks.RemoveAt(i);
    }
    public void Replace(int i, WorldChunk chunk)
    {
        Chunks[i] = chunk;
    }
}
