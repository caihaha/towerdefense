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
        return CalcG(parentNode.gCost, new Vector3(parentNode.nodePos.x, 0, parentNode.nodePos.y), nextPos);
    }

    public static float CalcG(float parentgCost, Vector3 parentPos, Vector3 nextPos)
    {
        return parentgCost + Mathf.Sqrt(Common.SqDistance2D(parentPos, nextPos));
    }

    public static bool IsGoal(Vector3 pos, Vector3 goalPos)
    {
        return Common.SqDistance2D(pos, goalPos) < 0.0001;
    }
}

public static class PathConstants
{
    public static uint MAX_SEARCHED_NODES = 655536;

    public static float SQUARE_SPEED_AND_RADIUS = 4f;
}

public enum BlockTypes
{
    BLOCK_NONE = 0,
    BLOCK_MOVING = 1,
    BLOCK_MOBILE = 2,
    BLOCK_MOBILE_BUSY = 4,
    BLOCK_STRUCTURE = 8,
    BLOCK_IMPASSABLE = 24 // := 16 | BLOCK_STRUCTURE; 不可逾越
};

