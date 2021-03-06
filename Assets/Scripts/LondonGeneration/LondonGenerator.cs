using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static LondonSettings;
using static GraphicsUtil;

public partial class LondonGenerator : MonoBehaviour
{

    [System.NonSerialized]
    public Dictionary<Vector2Int, Block> blockMap;
    Transform player;
    Vector2Int[] bounds; // top-left, top-right, bottom-left, bottom-right
    Vector2Int[] loadedBounds; // top-left, top-right, bottom-left, bottom-right
    BlockGenerator blockGenerator;
    RoadGenerator roadGenerator;
    int seed;

    public delegate void LondonGenerationStartEventHandler();
    public delegate void LondonGenerationStartedEventHandler(Vector2Int[] bounds, Vector2Int[] loadedBounds);
    public static LondonGenerationStartEventHandler onLondonGenerationStart;
    public static LondonGenerationStartedEventHandler onLondonGenerationStarted;

    void Start()
    {
        player = GameObject.Find("Player").GetComponent<Transform>();

        blockGenerator = GetComponent<BlockGenerator>();
        roadGenerator = GetComponent<RoadGenerator>();

        seed = GetComponent<LondonSerializer>().Init("default");
        blockMap = GetComponent<LondonSerializer>().savedMap;

        Random.InitState(seed);
        perlinOffset = new Vector2Int
        (
            Random.Range(0, 10000),
            Random.Range(0, 10000)
        );

        CalculateBounds();

        onLondonGenerationStart();

        blockGenerator.LoadBlocks(bounds);
        roadGenerator.LoadRoads(bounds);

        loadedBounds = bounds;

        if(onLondonGenerationStarted != null && onLondonGenerationStarted.GetInvocationList().Length > 0)
            onLondonGenerationStarted(bounds, loadedBounds);
    }
    
    void Update()
    {
        CalculateBounds();
        if(BoundsChanged())
        {
            blockGenerator.LoadBlocks(bounds, loadedBounds);
            blockGenerator.UnloadBlocks(bounds, loadedBounds);
            roadGenerator.LoadRoads(bounds, loadedBounds);
            roadGenerator.UnloadRoads(bounds, loadedBounds);
            loadedBounds = bounds;
        }
    }

    void CalculateBounds()
    {
        bounds = new Vector2Int[4];

        Vector2Int currentBlock = new Vector2Int((int)(player.position.x / blockSize), (int)(player.position.z / blockSize));
        //print(currentBlock);
        
        bounds[0] = new Vector2Int(currentBlock.x - renderDistance, currentBlock.y + renderDistance);
        bounds[1] = new Vector2Int(currentBlock.x + renderDistance, currentBlock.y + renderDistance);
        bounds[2] = new Vector2Int(currentBlock.x - renderDistance, currentBlock.y - renderDistance);
        bounds[3] = new Vector2Int(currentBlock.x + renderDistance, currentBlock.y - renderDistance);
    }

    bool IsInBounds(Vector2Int pos, Vector2Int[] bounds)
    {
        return 
        pos.x >= bounds[2].x && pos.y >= bounds[2].y &&
        pos.x <= bounds[1].x && pos.y <= bounds[1].y;
    }

    bool BoundsChanged()
    {
        return 
        !bounds[0].Equals(loadedBounds[0]) ||
        !bounds[1].Equals(loadedBounds[1]) ||
        !bounds[2].Equals(loadedBounds[2]) ||
        !bounds[3].Equals(loadedBounds[3]);
    }
}
