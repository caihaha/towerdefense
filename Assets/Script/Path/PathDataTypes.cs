using UnityEngine;

public struct PathNode
{
    public float fCost;
    public float gCose;

    public int nodeNum;
    public GameTile tile;

    public static bool operator < (PathNode lpn, PathNode rpn)
    {
        return lpn.fCost < rpn.fCost;
    }

    public static bool operator > (PathNode lpn, PathNode rpn)
    {
        return lpn.fCost > rpn.fCost;
    }

    public static bool operator == (PathNode lpn, PathNode rpn)
    {
        return lpn.nodeNum == rpn.nodeNum;
    }

    public static bool operator !=(PathNode lpn, PathNode rpn)
    {
        return lpn.nodeNum != rpn.nodeNum;
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

