using UnityEngine;

public static class PathDefs
{
    public static float Heuristic(Vector3 end, Vector3 tile)
    {
        float dx = Mathf.Abs(tile.x - end.x);
        float dz = Mathf.Abs(tile.z - end.z);

        // return Mathf.Sqrt(dx * dx + dz * dz);
        const float c1 = 1f;
        const float c2 = 1.4142f - (2f * c1);

        return ((dx + dz) * c1 + Mathf.Min(dx, dz) * c2);
    }

    public static float CalcG(float parentgCost, Direction direction)
    {
        return parentgCost + (DirectionExtensions.IsDiagonalDirection(direction) ? 1.4142f : 1f);
    }

    public static float CalcG(PathNode parentNode, Vector3 nextPos)
    {
        return CalcG(parentNode.gCost, parentNode.pos, nextPos);
    }

    public static float CalcG(float parentgCost, Vector3 parentPos, Vector3 nextPos)
    {
        return parentgCost + Mathf.Sqrt(DistenceSquare(parentPos, nextPos));
    }

    public static float DistenceSquare(Vector3 start, Vector3 end)
    {
        float dx = start.x - end.x;
        float dz = start.z - end.z;

        return dx * dx + dz * dz;
    }
}

public static class PathConstants
{
    public static uint MAX_SEARCHED_NODES = 655536;

    public static float SQUARE_SPEED_AND_RADIUS = 4f;
}


