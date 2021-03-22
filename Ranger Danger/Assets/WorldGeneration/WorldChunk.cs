using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunk : MonoBehaviour
{
    public delegate void ChunkDestoryDelegate(WorldChunk chunk);
    public event ChunkDestoryDelegate ChunkDestroyEvent;

    // Various example properties for now
    public WorldChunk Prefab;
    public int Width = 2000;
    public int Height = 2000;
    public Transform Texture;

    public bool IsVisible = true;
    private float TimePass = 0.1f;

    public List<Vector2> EnemySpawnPoints;
    public List<GameObject> EnemyPrefabs;

    public bool IsCurrent = false;
    public bool IsExplored = false;
    public bool IsFirst = false;
    public int ChunkId;

    public int seed;

    public enum Visibility
    {
        Fresh,
        Visible,
        NotVisible
    }
    public Visibility ChunkVisibility;

    public void Awake()
    {
        ChunkVisibility = Visibility.Fresh;
        var chunkGen = GetComponentInChildren<ChunkTileGenerator>();
        if (chunkGen != null) chunkGen.seed = seed;
    }

    // Update is called once per frame
    void Update()
    {
        if (ChunkVisibility == Visibility.NotVisible)
        {
            TimePass += Time.deltaTime;

            if (TimePass > 0.5f)
            {
                ChunkDestroyEvent(this);
                Destroy(gameObject);
            }
        }
    }

    private void OnBecameInvisible()
    {
        print("not visible");
    }

    public bool IsPositionInChunk(Vector3 pos)
    {
        var globalPosition = transform.TransformPoint(Vector3.zero);
        return
            pos.x > globalPosition.x - Width/2f && pos.x < globalPosition.x + Width/2f &&
            pos.y > globalPosition.y - Height/2f && pos.y < globalPosition.y + Height/2f;
    }

    // method to spawn in enemies
    public void SpawnEnemy()
    {

    }
}
