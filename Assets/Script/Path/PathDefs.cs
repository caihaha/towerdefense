using UnityEngine;

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

    public static float CalcG(float parentgCost, Direction direction)
    {
        return parentgCost + (DirectionExtensions.IsDiagonalDirection(direction) ? 1.4142f : 1f);
    }
}

public static class PathConstants
{
    public static uint MAX_SEARCHED_NODES = 655536;
}


