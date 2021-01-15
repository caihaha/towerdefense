﻿using UnityEngine;

public enum PathSearchResult
{
    Ok,
    CantGetCloser
}

public enum PathFindType
{
    DFS,
    AStart
}

public static class PathDefs
{
    public static float Heuristic(GameTile end, GameTile tile)
    {
        float dx = Mathf.Abs(tile.transform.position.x - end.transform.position.x);
        float dz = Mathf.Abs(tile.transform.position.z - end.transform.position.z);

        // return Mathf.Sqrt(dx * dx + dz * dz);
        const float c1 = 1f;
        const float c2 = 1.4142f - (2f * c1);

        return ((dx + dz) * c1 + Mathf.Min(dx, dz) * c2);
    }

    public static float CalcG(GameTile parentTile, GameTile tile, Direction direction)
    {
        return tile.gCost = parentTile.gCost + (DirectionExtensions.IsDiagonalDirection(direction) ? 1.4142f : 1f);
    }
}


