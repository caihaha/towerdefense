using System;
using System.Collections.Generic;

public class PathNode
{
    public float fCost;
    public float gCost;

    public GameTile tile;

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
    private PathNode[] buffer = new PathNode[65536];
}

public class PathNodeStateBuffer
{
    public List<float> fCost = new List<float>(Common.BoardCount);
    public List<float> gCost = new List<float>(Common.BoardCount);

    public List<GameTile> parentTile = new List<GameTile>(Common.BoardCount);

    public PathNodeStateBuffer()
    {
        for(int i = 0; i < parentTile.Capacity; ++i)
        {
            parentTile.Add(null);
        }
    }
}

