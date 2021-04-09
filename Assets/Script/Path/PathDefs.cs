using UnityEngine;

public static class PathDefs
{
    public static float Heuristic(Vector3 goalPos, Vector3 pos)
    {
        float dx = Mathf.Abs(pos.x - goalPos.x);
        float dz = Mathf.Abs(pos.z - goalPos.z);

        const float c1 = 1f;
        const float c2 = 1.4142f - (2f * c1);

        return ((dx + dz) * c1 + Mathf.Min(dx, dz) * c2);
    }

    public static float CalcG(PathNode parentNode, Vector3 nextPos)
    {
        return CalcG(parentNode.gCost, parentNode.pos, nextPos);
    }

    public static float CalcG(float parentgCost, Vector3 parentPos, Vector3 nextPos)
    {
        return parentgCost + Mathf.Sqrt(Common.SqDistance2D(parentPos, nextPos));
    }

    public static bool IsGoal(Vector3 pos, Vector3 goalPos)
    {
        return false;
    }
}

public static class PathConstants
{
    public static uint MAX_SEARCHED_NODES = 655536;

    public static float SQUARE_SPEED_AND_RADIUS = 4f;
}


