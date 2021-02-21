using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldChunk : MonoBehaviour
{
    // Various example properties for now
    public int Width;
    public int Height;
    public Sprite Sprite;

    public bool IsVisible;
    private float TimePass;

    public List<Vector2> EnemySpawnPoints;
    public List<GameObject> EnemyPrefabs;

    // Update is called once per frame
    void Update()
    {
        TimePass += Time.deltaTime;

        //if (IsVisible) print("Is visible");
        //else print("Is not visible");
        //print("Time pass: " + TimePass);

        if (TimePass > 0.11f)
        {
            IsVisible = false;
        }
    }

    // method to check if the world chunk is in view
    private void OnWillRenderObject()
    {
        if (TimePass > 0.1f)
        {
            //print(gameObject.name + " is being rendered by " + Camera.current.name + " at " + Time.time);
            if (Camera.current.name != "SceneCamera")
            {
                TimePass = 0.0f;
                IsVisible = true;
            }
        }
    }

    public bool IsPositionInChunk(Vector3 pos)
    {
        return
            pos.x > transform.position.x - Width && pos.x < transform.position.x + Width &&
            pos.y > transform.position.y - Height && pos.y < transform.position.y + Height;
    }

    // method to spawn in enemies
    public void SpawnEnemy()
    {

    }
}
