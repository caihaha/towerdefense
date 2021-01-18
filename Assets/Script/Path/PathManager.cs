using System.Collections.Generic;
using UnityEngine;

public class PathManager
{
    public PathManager()
    {
    }

    Queue<GameTile> searchFrontier = new Queue<GameTile>();

    List<GameTile> openList = new List<GameTile>();
    HashSet<GameTile> clostList = new HashSet<GameTile>();

    #region AStare
    public bool AStart(GameTile start, GameTile end)
    {
        isFinded = false;
        openList.Clear();
        clostList.Clear();
        openList.Add(start);

        while(openList.Count > 0)
        {
            GameTile tile = GetLeastFTile();
            if (tile == null)
                continue;

            clostList.Add(tile);
            if (tile.Content.Type == GameTileContentType.Destination)
            {
                isFinded = true;
                break;
            }

            for (Direction dir = Direction.Begin; dir < Direction.End; ++ dir)
            {
                GameTile nextTile = GrowPathTo(tile, end, dir);
                if (nextTile == null)
                    continue;

                openList.Add(nextTile);
            }
        }

        if(isFinded)
        {
            FinishPath(start, end);
        }

        return isFinded;
    }

    // 可以维护一个最大堆
    GameTile GetLeastFTile()
    {
        if (openList == null)
            return null;

        GameTile resTile = openList[0];

        for(int i = 1; i < openList.Count; ++i)
        {
            if (resTile.fCost > openList[i].fCost)
                resTile = openList[i];
        }

        openList.Remove(resTile);
        return resTile;
    }

    public bool IsTileInOpenList(GameTile tile)
    {
        foreach (var tmp in openList)
        {
            if(tmp == tile)
            {
                return true;
            }
        }

        return false;
    }

    public bool IsTileInCloseList(GameTile tile)
    {
        return clostList.Contains(tile);
    }

    GameTile GrowPathTo(GameTile tile, GameTile end, Direction direction)
    {
        if (tile == null)
            return null;

        GameTile neighbor = tile.GetTileByDirection(direction);
        if (neighbor == null)
            return null;

        if (DirectionExtensions.IsDiagonalDirection(direction))
        {
            switch (direction)
            {
                case Direction.UpRight:
                    {
                        if (tile.Up.Content.Type == GameTileContentType.Wall &&
                        tile.Right.Content.Type == GameTileContentType.Wall)
                            return null;
                        break;
                    }
                case Direction.UpLeft:
                    {
                        if (tile.Up.Content.Type == GameTileContentType.Wall &&
                        tile.Left.Content.Type == GameTileContentType.Wall)
                            return null;
                        break;
                    }
                case Direction.DownRight:
                    {
                        if (tile.Down.Content.Type == GameTileContentType.Wall &&
                        tile.Right.Content.Type == GameTileContentType.Wall)
                            return null;
                        break;
                    }
                case Direction.DownLeft:
                    {
                        if (tile.Down.Content.Type == GameTileContentType.Wall &&
                        tile.Left.Content.Type == GameTileContentType.Wall)
                            return null;
                        break;
                    }
            }
        }

        // 在Close列表
        if (IsTileInCloseList(neighbor))
        {
            return null;
        }

        // 计算fCost
        float gCost = PathDefs.CalcG(tile, neighbor, direction);
        float hCost = PathDefs.Heuristic(end, neighbor);
        float fCost = gCost + hCost;

        if (IsTileInOpenList(neighbor))
        {
            if (fCost >= neighbor.fCost)
            {
                return null;
            }
        }
        neighbor.gCost = gCost;
        neighbor.fCost = fCost;

        neighbor.LastDirection = direction;
        neighbor.LastTileOnPath = tile;

        return neighbor.Content.Type != GameTileContentType.Wall ? neighbor : null;
    }
    #endregion

    #region DFS
    public bool DFS(GameTile start, GameTile end)
    {
        isFinded = false;
        searchFrontier.Clear();
        searchFrontier.Enqueue(start);

        while (searchFrontier.Count > 0)
        {
            GameTile tile = searchFrontier.Dequeue();
            if (tile == null)
                continue;

            if (tile.Content.Type == GameTileContentType.Destination)
            {
                isFinded = true;
                break;
            }

            // 可以打乱方向
            for (Direction dir = Direction.Begin; dir < Direction.End; ++dir)
            {
                if(IsDestination(GrowPathTo(tile, dir)))
                {
                    break;
                }
            }
        }

        if (isFinded)
            FinishPath(start, end);

        return isFinded;
    }

    void FinishPath(GameTile start, GameTile end)
    {
        GameTile tmp = end;
        while (tmp != null && tmp != start)
        {
            tmp = tmp.BackPathTo(tmp.LastTileOnPath, tmp.LastDirection);
        }
    }

    bool IsDestination(GameTile tile)
    {
        if(tile != null)
        {
            searchFrontier.Enqueue(tile);

            if(tile.Content.Type == GameTileContentType.Destination)
            {
                isFinded = true;
                return true;
            }
        }

        return false;
    }

    GameTile GrowPathTo(GameTile tile, Direction direction)
    {
        GameTile neighbor = tile.GetTileByDirection(direction);

        if (neighbor == null || neighbor.LastTileOnPath != null)
        {
            return null;
        }

        neighbor.LastTileOnPath = tile;
        neighbor.LastDirection = direction;

        return neighbor.Content.Type != GameTileContentType.Wall ? neighbor : null;
    }
    #endregion

    // 到达目标点
    bool isFinded;
}
