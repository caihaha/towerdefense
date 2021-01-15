using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameBoard : MonoBehaviour
{
    #region 数据成员
    [SerializeField]
    Transform ground = default;

    [SerializeField]
    GameTile tilePrefab = default;

    [SerializeField]
    Texture2D gridTexture = default;

    Vector2Int size;

    GameTile[] tiles;

    Queue<GameTile> searchFrontier = new Queue<GameTile>();

    GameTileContentFactory contentFactory;

    public bool hasPath;

    // List<GameTile> spawnPoints = new List<GameTile>();
    //public int SpawnPointCount => spawnPoints.Count;

    // 现在的位置
    GameTile nowPoint;
    public GameTile NowPoint
    {
        get => nowPoint;
        set
        {
            nowPoint = value;
        }
    }
    // 目标点
    GameTile destinationPoint;

    PathManager pathManager;

    bool showGrid, showPaths;
    public bool ShowGrid
    {
        get => showGrid;
        set
        {
            showGrid = value;
            Material m = ground.GetComponent<MeshCollider>().GetComponent<MeshRenderer>().material;
            if (showGrid)
            {
                m.mainTexture = gridTexture;
                m.SetTextureScale("_MainTex", size);
            }
            else
            {
                m.mainTexture = null;
            }
        }
    }
    public bool ShowPaths
    {
        get => showPaths;
        set
        {
            showPaths = value;
            if (showPaths)
            {
                foreach (GameTile tile in tiles)
                {
                    tile.ShowPath();
                }
            }
            else
            {
                foreach (GameTile tile in tiles)
                {
                    tile.HidePath();
                }
            }
        }
    }
    #endregion

    public static GameBoard Instance { get; private set; }

    void Awake()
    {
        Instance = this;
    }

    public void Initialize(Vector2Int size, GameTileContentFactory contentFactory)
    {
        this.size = size;
        this.contentFactory = contentFactory;
        ground.localScale = new Vector3(size.x, size.y, 1f);

        Vector2Int offset = new Vector2Int((size.x - 1) >> 1, (size.y - 1) >> 1);
        tiles = new GameTile[size.x * size.y];
        for (int i = 0, y = 0; y < size.y; ++y)
        {
            for(int x = 0; x < size.x; ++x, ++i)
            {
                GameTile tile = tiles[i] = Instantiate(tilePrefab);
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(x - offset.x, 0f, y - offset.y);
                tile.ExitPoint = tile.transform.localPosition;

                if (x > 0)
                {
                    GameTile.MakeRightLeftNightbors(tile, tiles[i - 1]);
                }
                if (y > 0)
                {
                    GameTile.MakeUpDownNightbors(tile, tiles[i - size.x]);
                }

                tile.IsAlternative = (x & 1) == 0;
                if((y & 1) == 0)
                {
                    tile.IsAlternative = !tile.IsAlternative;
                }

                tile.Content = contentFactory.Get(GameTileContentType.Empty);
            }
        }

        for(int i = 0; i < tiles.Length; ++i)
        {
            GameTile.MakeDiagonalNightbors(tiles[i]);
        }

        pathManager = new PathManager();
        // ToggleDestination(tiles[tiles.Length / 2]);
        // ToggleSpawnPoint(tiles[0]);
    }

    public bool PathFinder()
    {
        if (nowPoint == null || destinationPoint == null)
            return false;

        if (nowPoint == destinationPoint)
            return true;

        foreach (GameTile tile in tiles)
        {
            if (tile.Content.Type == GameTileContentType.Destination)
            {
                tile.BecomeDestination();
            }
            else if (tile == nowPoint)
            {
                tile.ClearPath();
                searchFrontier.Enqueue(tile);
            }
            else
            {
                tile.ClearPath();
            }
        }

        // hasPath = pathManager.DFS(nowPoint, destinationPoint);
        hasPath = pathManager.AStart(nowPoint, destinationPoint);
        if (hasPath && showPaths)
        {
            foreach (GameTile tile in tiles)
            {
                tile.ShowPath();
            }
        }

        return hasPath;
    }

    public GameTile GetTile(Ray ray)
    {
        if(Physics.Raycast(ray, out RaycastHit hit))
        {
            int x = (int)(hit.point.x + size.x * 0.5f);
            int y = (int)(hit.point.z + size.y * 0.5f);
            if(x >= 0 && x < size.x && y >=0 && y < size.y)
                return tiles[x + y * size.x];
        }

        return null;
    }

    public GameTile GetTileByIdx(int index)
    {
        if (tiles.Length < index)
            return null;

        return tiles[index];
    }

    public void ToggleDestination(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.Destination)
        {
            // 不做处理，最少要有一个目标点
            Debug.Log("Destination Repeated");
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            if (destinationPoint != null)
            {
                SetGameTileContentType(destinationPoint, GameTileContentType.Empty);
            }
            destinationPoint = tile;

            SetGameTileContentType(tile, GameTileContentType.Destination);
            PathFinder();
        }
    }

    public void ToggleWall (GameTile tile) {
        if (tile.Content.Type == GameTileContentType.Wall)
        {
            SetGameTileContentType(tile, GameTileContentType.Empty);
            PathFinder();
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            SetGameTileContentType(tile, GameTileContentType.Wall);

            if (!PathFinder())
            {
                SetGameTileContentType(tile, GameTileContentType.Empty);
                PathFinder();
            }
        }
	}

    public GameTile DestinationPoint
    {
        get => destinationPoint;
    }

    public void SetGameTileContentType(GameTile tile, GameTileContentType contentType)
    {
        tile.Content = contentFactory.Get(contentType);
    }

    public int GetTilesSize()
    {
        return tiles.Length;
    }
}
