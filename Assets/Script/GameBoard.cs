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

        ToggleDestination(tiles[tiles.Length / 2]);
        // ToggleSpawnPoint(tiles[0]);
    }

    bool FindPaths()
    {
        // 初始化所有点
        foreach (GameTile tile in tiles)
        {
            if (tile.Content.Type == GameTileContentType.Destination)
            {
                tile.BecomeDestination();
                searchFrontier.Enqueue(tile);
            }
            else
            {
                tile.ClearPath();
            }
        }

        // 没有目标点
        if (searchFrontier.Count == 0)
        {
            return false;
        }

        while (searchFrontier.Count > 0)
        {
            GameTile tile = searchFrontier.Dequeue();
            if(tile != null)
            {
                searchFrontier.Enqueue(tile.GrowPathUp());
                searchFrontier.Enqueue(tile.GrowPathUpRight());
                searchFrontier.Enqueue(tile.GrowPathRight());
                searchFrontier.Enqueue(tile.GrowPathDownRight());
                searchFrontier.Enqueue(tile.GrowPathDown());
                searchFrontier.Enqueue(tile.GrowPathDownLeft());
                searchFrontier.Enqueue(tile.GrowPathLeft());
                searchFrontier.Enqueue(tile.GrowPathUpLeft());
                //if(tile.IsAlternative)
                //{
                //    searchFrontier.Enqueue(tile.GrowPathUp());
                //    searchFrontier.Enqueue(tile.GrowPathDown());
                //    searchFrontier.Enqueue(tile.GrowPathRight());
                //    searchFrontier.Enqueue(tile.GrowPathLeft());
                //}
                //else
                //{
                //    searchFrontier.Enqueue(tile.GrowPathLeft());
                //    searchFrontier.Enqueue(tile.GrowPathRight());
                //    searchFrontier.Enqueue(tile.GrowPathDown());
                //    searchFrontier.Enqueue(tile.GrowPathUp());
                //}
            }
        }

        foreach (GameTile tile in tiles)
        {
            if(!tile.HasPath)
            {
                return false;
            }
        }

        if(showPaths)
        {
            foreach (GameTile tile in tiles)
            {
                tile.ShowPath();
            }
        }

        return true;
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
                destinationPoint.Content = contentFactory.Get(GameTileContentType.Empty);
            }
            destinationPoint = tile;

            tile.Content = contentFactory.Get(GameTileContentType.Destination);
            FindPaths();
        }
    }

    public void ToggleWall (GameTile tile) {
        if (tile.Content.Type == GameTileContentType.Wall)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Empty);
            FindPaths();
        }
        else if (tile.Content.Type == GameTileContentType.Empty)
        {
            tile.Content = contentFactory.Get(GameTileContentType.Wall);
            if (!FindPaths())
            {
                tile.Content = contentFactory.Get(GameTileContentType.Empty);
                FindPaths();
            }
        }
	}

    public GameTile GetDestination()
    {
        return destinationPoint;
    }
}
