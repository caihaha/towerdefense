using System.Collections.Generic;
using UnityEngine;

public class PathManager
{
    public PathManager()
    {
    }

    Queue<GameTile> searchFrontier = new Queue<GameTile>();

    public bool DFS(GameTile start, GameTile end, GameTile[] tiles)
    {
        isFinded = false;
        if (start == null || end == null)
            return false;

        foreach (GameTile tile in tiles)
        {
            if (tile.Content.Type == GameTileContentType.Destination)
            {
                tile.BecomeDestination();
            }
            else if (tile.Content.Type == GameTileContentType.SpawnPoint)
            {
                searchFrontier.Enqueue(tile);
            }
            else
            {
                tile.ClearPath();
            }
        }

        if (searchFrontier.Count == 0)
        {
            return false;
        }

        while (searchFrontier.Count > 0)
        {
            GameTile tile = searchFrontier.Dequeue();
            if (tile == null)
                continue;

            if (tile.Content.Type == GameTileContentType.Destination)
                break;

            if (!GrowPathUp(tile))
                break;
            if (!GrowPathDown(tile))
                break;
            if (!GrowPathRight(tile))
                break;
            if (!GrowPathLeft(tile))
                break;
            if (!GrowPathUpRight(tile))
                break;
            if (!GrowPathDownLeft(tile))
                break;
            if (!GrowPathDownRight(tile))
                break;
            if (!GrowPathUpLeft(tile))
                break;
        }

        if (isFinded)
            FinishPath(start, end);

        return true;
    }

    void FinishPath(GameTile start, GameTile end)
    {
        GameTile tmp = end;
        while (tmp != null && tmp.Content.Type != GameTileContentType.SpawnPoint)
        {
            tmp = tmp.BackPathTo(tmp.LastTileOnPath, tmp.LastDirection);
        }
    }

    bool GrowPathUp(GameTile tile)
    {
        return IsDestination(tile.GrowPathUp());
    }

    bool GrowPathDown(GameTile tile)
    {
        return IsDestination(tile.GrowPathDown());
    }

    bool GrowPathRight(GameTile tile)
    {
        return IsDestination(tile.GrowPathRight());
    }

    bool GrowPathLeft(GameTile tile)
    {
        return IsDestination(tile.GrowPathLeft());
    }

    bool GrowPathUpRight(GameTile tile)
    {
        return IsDestination(tile.GrowPathUpRight());
    }

    bool GrowPathDownLeft(GameTile tile)
    {
        return IsDestination(tile.GrowPathDownLeft());
    }

    bool GrowPathDownRight(GameTile tile)
    {
        return IsDestination(tile.GrowPathDownRight());
    }

    bool GrowPathUpLeft(GameTile tile)
    {
        return IsDestination(tile.GrowPathUpLeft());
    }

    bool IsDestination(GameTile tile)
    {
        searchFrontier.Enqueue(tile);
        if (tile != null && tile.Content.Type == GameTileContentType.Destination)
        {
            isFinded = true;
            return false;
        }

        return true;
    }

    PathFinder pathFinder;
    
    uint nextPathId;

    // 到达目标点
    bool isFinded;
}
