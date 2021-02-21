using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NextWorldChunks
{
    public int EventChunkIdx;
    public List<WorldChunk> Chunks;

    public NextWorldChunks()
    {
        Chunks = new List<WorldChunk>();
    }

    public void Add(WorldChunk chunk)
    {
        if (chunk is EventChunk)
            EventChunkIdx = Chunks.Count;

        Chunks.Add(chunk);
    }
}
