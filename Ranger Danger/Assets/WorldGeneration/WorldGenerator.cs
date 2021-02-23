using System.Linq;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class WorldGenerator : MonoBehaviour
{
    public Transform Player;
    public Transform CameraTransform;

    public List<WorldChunk> WorldChunks;

    public WorldChunk FirstChunk;
    // may potentially become a list
    public EventChunk EventChunk;

    public WorldChunk CurrentChunk;

    public int PreviousChunkId;
    public Stack<int> ChunksExplored;

    public double ProbOfPrevChunk;
    public double ProbOfRepeatChunk;

    public float EventChunkSlope = 0.75f;
    public float EventChunkMidPoint = 10; 

    public int Seed;
    public int StackSize;

    public Vector3 CameraMargin = new Vector3(0.5f, 0.5f);

    public Vector2 PlayerLocation;
    public Vector2 PreviousPlayerLocation;
    public Vector2 CameraLocation;
    public Vector2 PreviousCameraLocation;
    // -1, 1  NorthEast
    //  0, 1  North
    //  1, 1  NorthWest
    // -1, 0  East
    //  0, 0  Centre
    //  1, 0  West
    // -1,-1  SouthEast
    //  0,-1  South
    //  1,-1  SouthWest

    private Random Rand;
    
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
        Rand = new Random(Seed);
        CurrentChunk = InstantiateChunk(FirstChunk, new Vector3(0, 0, 1));
        CurrentChunk.IsCurrent = true;
        CurrentChunk.IsFirst = true;

        ChunksExplored = new Stack<int>();
    }

    // Update is called once per frame
    void Update()
    {
        // checking if CurrentChunk is null, and if so, setting it as the one the player is on
        // to stop errors later on
        if (CurrentChunk == null) CurrentChunk = GetChunkAtPosition(Player.position);

        CalculatePlayerLocation();
        CalculateCameraLocation();
        var cameraChangedLocation = CameraLocation != PreviousCameraLocation;

        var playerChangedLocation = PlayerLocation != PreviousPlayerLocation;
        var changedChunk = 
            playerChangedLocation &&
            ((PlayerLocation.x == -PreviousPlayerLocation.x && PlayerLocation.x != 0 && PreviousPlayerLocation.x != 0) ||
            (PlayerLocation.y == -PreviousPlayerLocation.y && PlayerLocation.y != 0 && PreviousPlayerLocation.y != 0));
        var movedToCentre =
            playerChangedLocation && PlayerLocation == Vector2.zero;

        if (cameraChangedLocation && !changedChunk && CameraLocation != Vector2.zero) //&& State == GenerationState.Clean)
        {
            CreateNextWorldChunks();
            State = GenerationState.HasGenerated;
        }
        if (movedToCentre && CurrentChunk != FirstChunk)
        {
            CurrentChunk.IsExplored = true;
        }
        if (changedChunk)
        {
            CurrentChunk = GetChunkAtPosition(Player.position);
        }
    }

    // method to run whenever the player enters a 'zone'
    void CreateNextWorldChunks()
    {
        // if sum of mod(x) and mod(y) is 2, its a corner
        var isCorner = Mathf.Abs(CameraLocation.x) + Mathf.Abs(CameraLocation.y) == 2;

        var position = CurrentChunk.transform.position + new Vector3(CurrentChunk.Width * CameraLocation.x, CurrentChunk.Height * CameraLocation.y);
        // Instantiating chunk adjacent to location, if one isn't already there
        var nextChunkId = GetNextChunkId();
        if (nextChunkId == -1) CreateChunk(position, nextChunkId, EventChunk);
        else if (CreateChunk(position, nextChunkId) && isCorner) nextChunkId = GetNextChunkId();

        // Instantiating edge chunks, if location is in corner
        if (isCorner)
        {
            position = CurrentChunk.transform.position + new Vector3(CurrentChunk.Width * CameraLocation.x, 0);
            if (nextChunkId == -1) CreateChunk(position, nextChunkId, EventChunk);
            else if (CreateChunk(position, nextChunkId)) nextChunkId = GetNextChunkId();

            position = CurrentChunk.transform.position + new Vector3(0, CurrentChunk.Height * CameraLocation.y);
            if (nextChunkId == -1) CreateChunk(position, nextChunkId, EventChunk);
            else CreateChunk(position, nextChunkId);
        }
    }

    void CalculatePlayerLocation()
    {
        PreviousPlayerLocation = PlayerLocation;
        var playerRelativePos = GetPlayerRelativePosition(new Vector3(10, 10), new Vector3(-10, -10));

        PlayerLocation = GetLocation(playerRelativePos, Vector3.zero);
    }

    void CalculateCameraLocation()
    {
        PreviousCameraLocation = CameraLocation;
        var cameraRelativePos = CameraTransform.InverseTransformPoint(CurrentChunk.transform.position);

        CameraLocation = GetLocation(cameraRelativePos, CameraMargin);
    }

    #region private helper methods

    private Vector3 GetLocation(Vector3 relativePosition, Vector3 margin)
    {
        var cameraExtent = GetCameraExtent();
        var chunkBoundsMax = GetChunkBoundsMax(cameraExtent, margin);
        var chunkBoundsMin = GetChunkBoundsMin(cameraExtent, margin);

        int x;
        int y;

        if (relativePosition.x < chunkBoundsMin.x) x = 1;
        else if (relativePosition.x > chunkBoundsMax.x) x = -1;
        else x = 0;

        if (relativePosition.y > chunkBoundsMax.y) y = -1;
        else if (relativePosition.y < chunkBoundsMin.y) y = 1;
        else y = 0;

        return new Vector2(x, y);
    }

    // Tries to Instantiate a chunk, and returns true if it was successful
    private bool CreateChunk(Vector3 position, int chunkId = -1, WorldChunk specificPrefab = null)
    {
        var chunkAtPosition = GetChunkAtPosition(position - new Vector3(0, 0, 1));
        if (chunkAtPosition != null) return false;

        WorldChunk prefab;
        if (chunkId == -1 && specificPrefab != null) prefab = specificPrefab;
        else prefab = WorldChunks[chunkId];
        
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
            ChunksExplored.Push(chunk.ChunkId);
    }
    private Vector3 GetCameraExtent()
    {
        var camHeight = Camera.main.orthographicSize * 2f;
        var camWidth = camHeight * Camera.main.aspect;
        return new Vector3(camWidth / 2, camHeight / 2);
    }
    private Vector3 GetChunkBoundsMax(Vector3 cameraExtent, Vector3 margin)
    {
        var spriteMax = CurrentChunk.Sprite.bounds.max;
        return spriteMax - cameraExtent - margin;
    }
    private Vector3 GetChunkBoundsMin(Vector3 cameraExtent, Vector3 margin)
    {
        var spriteMin = CurrentChunk.Sprite.bounds.min;
        return spriteMin + cameraExtent + margin;
    }

    private Vector3 GetPlayerRelativePosition(Vector3 boundsMax, Vector3 boundsMin)
    {
        var rel = CameraTransform.InverseTransformPoint(CurrentChunk.transform.position);
        if (rel.x > boundsMax.x) rel.x -= 20;
        if (rel.x < boundsMin.x) rel.x += 20;
        if (rel.y > boundsMax.y) rel.y -= 20;
        if (rel.y < boundsMin.y) rel.y += 20;

        return rel;
    }

    private int GetNextChunkId()
    {
        if (Rand.NextDouble() < ProbabilityOfEventChunk())
        {
            print("Using event chunk");
            return -1;
        }

        if (Rand.NextDouble() < ProbOfRepeatChunk && !CurrentChunk.IsFirst)
        {
            print("Using current chunk");
            return CurrentChunk.ChunkId;
        }

        if (Rand.NextDouble() < ProbOfPrevChunk && ChunksExplored.Count > 0)
        {
            print("Using previous chunk");
            return ChunksExplored.Pop();
        }
        
        var nextId = Rand.Next(WorldChunks.Count);
        //print(nextId);
        return nextId;
    }
    
    private WorldChunk GetChunkAtPosition(Vector3 pos)
    {
        RaycastHit[] hits;
        hits = Physics.RaycastAll(pos, Vector3.forward, 1f);
        Debug.DrawRay(pos, Vector3.forward, Color.red, 100);

        if (hits != null && hits.Length > 0 && hits[0].collider.gameObject.layer == LayerMask.NameToLayer("Ground"))
            return hits[0].collider.GetComponent<WorldChunk>();

        return null;
    }

    private double ProbabilityOfEventChunk()
    {
        return 1 / (1 + Mathf.Exp(-EventChunkSlope * (ChunksExplored.Count - EventChunkMidPoint)));
    }

    #endregion
}
