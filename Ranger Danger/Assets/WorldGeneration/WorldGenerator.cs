using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Random = UnityEngine.Random;

public class WorldGenerator : MonoBehaviour
{
    public int seed;
    public bool randomiseSeed;

    public Transform player;
    public Transform cameraTransform;

    public List<WorldChunk> worldChunks;

    public WorldChunk firstChunk;
    // may potentially become a list
    public EventChunk eventChunk;

    public WorldChunk currentChunk;
    public List<WorldChunk> previousChunks;

    public int previousChunkId;
    public Stack<int> chunksExplored;

    public double probOfPrevChunk;
    public double probOfRepeatChunk;

    public float eventChunkSlope = 0.75f;
    public float eventChunkMidPoint = 10; 

    public int stackSize;

    public Vector3 cameraMargin = new Vector3(0.5f, 0.5f);

    public Vector2 playerLocation;
    public Vector2 previousPlayerLocation;
    public Vector2 cameraLocation;
    public Vector2 previousCameraLocation;
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
    public GenerationState state;

    // Start is called before the first frame update
    void Start()
    {
        Initialise();
    }

    void Initialise()
    {
        if (randomiseSeed)
            seed = (int)DateTime.UtcNow.ToBinary();

        Random.InitState(seed);

        currentChunk = InstantiateChunk(firstChunk, new Vector3(0, 0, 1));
        currentChunk.seed = seed;
        currentChunk.IsCurrent = true;
        currentChunk.IsFirst = true;

        chunksExplored = new Stack<int>();
        previousChunks = new List<WorldChunk>();
    }

    // Update is called once per frame
    void Update()
    {
        // checking if currentChunk is null, and if so, setting it as the one the player is on
        // to stop errors later on
        if (currentChunk == null) currentChunk = GetChunkAtPosition(player.position);

        CalculatePlayerLocation();
        CalculateCameraLocation();
        var cameraChangedLocation = cameraLocation != previousCameraLocation;

        var playerChangedLocation = playerLocation != previousPlayerLocation;
        var changedChunk = 
            playerChangedLocation &&
            ((playerLocation.x == -previousPlayerLocation.x && playerLocation.x != 0 && previousPlayerLocation.x != 0) ||
            (playerLocation.y == -previousPlayerLocation.y && playerLocation.y != 0 && previousPlayerLocation.y != 0));
        var movedToCentre =
            playerChangedLocation && playerLocation == Vector2.zero;

        if (cameraChangedLocation && !changedChunk && cameraLocation != Vector2.zero) //&& State == GenerationState.Clean)
        {
            CreateNextWorldChunks();
            state = GenerationState.HasGenerated;
        }
        if (movedToCentre && currentChunk != firstChunk)
        {
            currentChunk.IsExplored = true;
            foreach (var chunk in previousChunks)
            {
                chunk.ChunkVisibility = WorldChunk.Visibility.NotVisible;
            }
            previousChunks = new List<WorldChunk>();
        }
        if (changedChunk)
        {
            previousChunks.Add(currentChunk);
            currentChunk = GetChunkAtPosition(player.position);
        }
    }

    // method to run whenever the player enters a 'zone'
    void CreateNextWorldChunks()
    {
        // if sum of mod(x) and mod(y) is 2, its a corner
        var isCorner = Mathf.Abs(cameraLocation.x) + Mathf.Abs(cameraLocation.y) == 2;

        var position = currentChunk.transform.position + new Vector3(currentChunk.Width * cameraLocation.x, currentChunk.Height * cameraLocation.y);
        // Instantiating chunk adjacent to location, if one isn't already there
        var nextChunkId = GetNextChunkId();
        if (nextChunkId == -1) CreateChunk(position, nextChunkId, eventChunk);
        else if (CreateChunk(position, nextChunkId) && isCorner) nextChunkId = GetNextChunkId();

        // Instantiating edge chunks, if location is in corner
        if (isCorner)
        {
            position = currentChunk.transform.position + new Vector3(currentChunk.Width * cameraLocation.x, 0);
            if (nextChunkId == -1) CreateChunk(position, nextChunkId, eventChunk);
            else if (CreateChunk(position, nextChunkId)) nextChunkId = GetNextChunkId();

            position = currentChunk.transform.position + new Vector3(0, currentChunk.Height * cameraLocation.y);
            if (nextChunkId == -1) CreateChunk(position, nextChunkId, eventChunk);
            else CreateChunk(position, nextChunkId);
        }
    }

    void CalculatePlayerLocation()
    {
        previousPlayerLocation = playerLocation;
        var playerRelativePos = GetPlayerRelativePosition(new Vector3(10, 10), new Vector3(-10, -10));

        playerLocation = GetLocation(playerRelativePos, Vector3.zero);
    }

    void CalculateCameraLocation()
    {
        previousCameraLocation = cameraLocation;
        var cameraRelativePos = cameraTransform.InverseTransformPoint(currentChunk.transform.position);

        cameraLocation = GetLocation(cameraRelativePos, cameraMargin);
    }

    #region private helper methods

    private Vector3 GetLocation(Vector3 relativePosition, Vector3 margin)
    {
        var cameraExtent = GetCameraExtent();
        var chunkTileGenerator = currentChunk.GetComponentInChildren<ChunkTileGenerator>();
        // width should equal height
        var chunkSize = chunkTileGenerator == null ? new Vector3(currentChunk.Width, currentChunk.Height) : chunkTileGenerator.chunkSize;
        var chunkBoundsMax = GetChunkBoundsMax(chunkSize, cameraExtent, margin);
        var chunkBoundsMin = GetChunkBoundsMin(chunkSize, cameraExtent, margin);

        int x = 0;
        int y = 0;

        if (relativePosition.x < chunkBoundsMin.x) x = 1;
        if (relativePosition.x > chunkBoundsMax.x) x = -1;

        if (relativePosition.y > chunkBoundsMax.y) y = -1;
        if (relativePosition.y < chunkBoundsMin.y) y = 1;

        return new Vector2(x, y);
    }

    // Tries to Instantiate a chunk, and returns true if it was successful
    private bool CreateChunk(Vector3 position, int chunkId = -1, WorldChunk specificPrefab = null)
    {
        var chunkAtPosition = GetChunkAtPosition(position - new Vector3(0, 0, 1));
        if (chunkAtPosition != null) return false;

        WorldChunk prefab;
        if (chunkId == -1 && specificPrefab != null) prefab = specificPrefab;
        else prefab = worldChunks[chunkId];
        
        InstantiateChunk(prefab, position, chunkId);
        return true;
    }
    private WorldChunk InstantiateChunk(WorldChunk prefab, Vector3 position, int chunkId = -1)
    {
        var chunk = Instantiate(
                prefab,
                position,
                prefab.transform.rotation);
        chunk.ChunkDestroyEvent += OnChunkDestroy;
        chunk.ChunkId = chunkId;

        return chunk;
    }
    private void OnChunkDestroy(WorldChunk chunk)
    {
        if (chunk.IsExplored)
            chunksExplored.Push(chunk.ChunkId);
    }
    private Vector3 GetCameraExtent()
    {
        var camHeight = Camera.main.orthographicSize * 2f;
        var camWidth = camHeight * Camera.main.aspect;
        return new Vector3(camWidth / 2, camHeight / 2);
    }
    private Vector3 GetChunkBoundsMax(Vector3 chunkSize, Vector3 cameraExtent, Vector3 margin)
    {
        var chunkMax = chunkSize / 2f;
        return chunkMax - cameraExtent - margin;
    }
    private Vector3 GetChunkBoundsMin(Vector3 chunkSize, Vector3 cameraExtent, Vector3 margin)
    {
        var chunkMin = -chunkSize / 2f;
        return chunkMin + cameraExtent + margin;
    }

    private Vector3 GetPlayerRelativePosition(Vector3 boundsMax, Vector3 boundsMin)
    {
        var rel = cameraTransform.InverseTransformPoint(currentChunk.Texture.transform.position);
        if (rel.x > boundsMax.x) rel.x -= 20;
        if (rel.x < boundsMin.x) rel.x += 20;
        if (rel.y > boundsMax.y) rel.y -= 20;
        if (rel.y < boundsMin.y) rel.y += 20;

        return rel;
    }

    private int GetNextChunkId()
    {
        if (Random.value < ProbabilityOfEventChunk())
        {
            print("Using event chunk");
            return -1;
        }

        if (Random.value < probOfRepeatChunk && !currentChunk.IsFirst)
        {
            print("Using current chunk");
            return currentChunk.ChunkId;
        }

        if (Random.value < probOfPrevChunk && chunksExplored.Count > 0)
        {
            print("Using previous chunk");
            return chunksExplored.Pop();
        }
        
        var nextId = Random.Range(0, worldChunks.Count);
        //print(nextId);
        return nextId;
    }
    
    private WorldChunk GetChunkAtPosition(Vector3 pos)
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(pos, Vector3.forward, 1f);
        Debug.DrawRay(pos, Vector3.forward, Color.red, 100);

        if (hits.Length == 0) return null;

        var hit = hits.FirstOrDefault(h => h.collider.gameObject.layer == LayerMask.NameToLayer("Ground"));
        if (hit.collider != null)
            return hit.collider.GetComponent<WorldChunk>();

        return null;
    }

    private double ProbabilityOfEventChunk()
    {
        return 1 / (1 + Mathf.Exp(-eventChunkSlope * (chunksExplored.Count - eventChunkMidPoint)));
    }

    #endregion
}
