using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunk : MonoBehaviour
{
    public delegate void ChunkDestoryDelegate(WorldChunk chunk);
    public event ChunkDestoryDelegate ChunkDestroyEvent;

    // Various example properties for now
    public WorldChunk Prefab;
    public int Width;
    public int Height;
    public Sprite Sprite;

    public bool IsVisible = true;
    private float TimePass = 0.1f;

    public List<Vector2> EnemySpawnPoints;
    public List<GameObject> EnemyPrefabs;

    public bool IsCurrent = false;
    public bool IsExplored = false;
    public bool IsFirst = false;
    public int ChunkId;

    public enum Visibility
    {
        Fresh,
        Visible,
        NotVisible
    }
    public Visibility ChunkVisibility;

    private void Start()
    {
        ChunkVisibility = Visibility.Fresh;
        Sprite = GetComponent<SpriteRenderer>().sprite;
        var size = Sprite.bounds.max - Sprite.bounds.min;
        Width = (int)size.x;
        Height = (int)size.y;
    }

    // Update is called once per frame
    void Update()
    {
        if (ChunkVisibility != Visibility.Fresh)
        {
            TimePass += Time.deltaTime;

            if (TimePass > 0.5f)
            {
                ChunkVisibility = Visibility.NotVisible;
                ChunkDestroyEvent(this);
                Destroy(gameObject);
            }
        }
    }

    // method to check if the world chunk is in view
    private void OnWillRenderObject()
    {
        if (TimePass >= 0.1f)
        {
            if (Camera.current.name != "SceneCamera")
            {
                //print(gameObject.name + " is being rendered by " + Camera.current.name + " at " + Time.time);
                TimePass = 0.0f;
                ChunkVisibility = Visibility.Visible;
            }
        }
    }

    public bool IsPositionInChunk(Vector3 pos)
    {
        var globalPosition = transform.TransformPoint(Vector3.zero);
        return
            pos.x > globalPosition.x - Width/2 && pos.x < globalPosition.x + Width/2 &&
            pos.y > globalPosition.y - Height/2 && pos.y < globalPosition.y + Height/2;
    }

    // method to spawn in enemies
    public void SpawnEnemy()
    {

    }
}
