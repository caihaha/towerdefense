using System.Collections.Generic;
using UnityEngine;

public class PathManager
{
    public PathManager()
    {
    }

    Queue<GameTile> searchFrontier = new Queue<GameTile>();

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
                break;

            if (!IsDestination(tile.GrowPathUp()))
                break;
            if (!IsDestination(tile.GrowPathDown()))
                break;
            if (!IsDestination(tile.GrowPathRight()))
                break;
            if (!IsDestination(tile.GrowPathLeft()))
                break;
            if (!IsDestination(tile.GrowPathUpRight()))
                break;
            if (!IsDestination(tile.GrowPathDownLeft()))
                break;
            if (!IsDestination(tile.GrowPathDownRight()))
                break;
            if (!IsDestination(tile.GrowPathUpLeft()))
                break;
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
                return false;
            }
        }

        return true;
    }

    PathFinder pathFinder;
    
    uint nextPathId;

    // 到达目标点
    bool isFinded;
}
