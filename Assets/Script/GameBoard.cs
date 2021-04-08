using System.Collections;
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
    public GameTile[] Tiles => tiles;
    public int TileSize => tiles.Length;

    GameTileContentFactory contentFactory;

    // 目标点
    GameTile destinationPoint;
    public GameTile DestinationPoint => destinationPoint;

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
        for (uint i = 0, y = 0; y < size.y; ++y)
        {
            for(uint x = 0; x < size.x; ++x, ++i)
            {
                GameTile tile = tiles[i] = Instantiate(tilePrefab);
                tile.transform.SetParent(transform, false);
                tile.transform.localPosition = new Vector3(x - offset.x, 0f, y - offset.y);
                tile.ExitPoint = tile.transform.localPosition;
                tile.num = i;

                tile.IsAlternative = (x & 1) == 0;
                if((y & 1) == 0)
                {
                    tile.IsAlternative = !tile.IsAlternative;
                }

                tile.Content = contentFactory.Get(GameTileContentType.Empty);
            }
        }
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
        if (tiles.Length < index || index < 0)
            return null;

        return tiles[index];
    }

    public bool ToggleDestination(GameTile tile)
    {
        if (tile.Content.Type == GameTileContentType.Destination)
        {
            // 不做处理，最少要有一个目标点
            Debug.Log("Destination Repeated");
            return false;
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            if (destinationPoint != null)
            {
                SetGameTileContentType(destinationPoint, GameTileContentType.Empty);
            }
            destinationPoint = tile;

            SetGameTileContentType(tile, GameTileContentType.Destination);
            return true;
        }

        return false;
    }

    public void ToggleWall (GameTile tile) {
        if (tile.Content.Type == GameTileContentType.Wall)
        {
            SetGameTileContentType(tile, GameTileContentType.Empty);
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            SetGameTileContentType(tile, GameTileContentType.Wall);
        }
	}

    public void SetGameTileContentType(GameTile tile, GameTileContentType contentType)
    {
        tile.Content = contentFactory.Get(contentType);
    }
}
