using UnityEngine;

public class PathNode
{
    public float fCost;
    public float gCost;

    public GameTile tile = new GameTile();

    public static bool operator < (PathNode lpn, PathNode rpn)
    {
        return lpn.fCost < rpn.fCost;
    }

    public static bool operator > (PathNode lpn, PathNode rpn)
    {
        return lpn.fCost > rpn.fCost;
    }
}

public class PathNodeBuffer
{
    public PathNodeBuffer()
    {
        idx = 0;
    }

    void SetSize(uint i)
    {
        idx = i;
    }

    uint GetSize()
    {
        return idx;
    }

    PathNode GetNode(uint i)
    {
        return buffer[i];
    }

    private uint idx;
    private PathNode[] buffer = new PathNode[65535];
}

