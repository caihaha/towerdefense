using System;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    public float fCost;
    public float gCost;

    public Vector3 pos;
    public int nodeNum;
    public Vector2Int nodePos;

    public static bool operator < (PathNode lpn, PathNode rpn)
    {
        return lpn.fCost < rpn.fCost;
    }

    public static bool operator > (PathNode lpn, PathNode rpn)
    {
        return lpn.fCost > rpn.fCost;
    }

    //public static bool operator ==(PathNode lpn, PathNode rpn)
    //{
    //    if (rpn == null)
    //    {
    //        return false;
    //    }
    //    return lpn.pos == rpn.pos;
    //}


    //public static bool operator !=(PathNode lpn, PathNode rpn)
    //{
    //    if (rpn == null)
    //    {
    //        return true;
    //    }
    //    return lpn.pos != rpn.pos;
    //}
}

sealed class PathNodeComparer : IComparer<PathNode>
{
    public int Compare(PathNode lpn, PathNode rpn)
    {
        return lpn.fCost > rpn.fCost ? -1 : (lpn.fCost == rpn.fCost ? 0 : 1);
    }
}

public class PathNodeBuffer
{
    public PathNodeBuffer()
    {
        idx = 0;
    }

    public void SetSize(uint i)
    {
        idx = i;
    }

    public uint GetSize()
    {
        return idx;
    }

    public PathNode GetNode(uint i)
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

    public List<Vector3> parentTile = new List<Vector3>(Common.BoardCount);
    public int []nodeMask = new int[Common.BoardCount];

    public PathNodeStateBuffer()
    {
        for(int i = 0; i < parentTile.Capacity; ++i)
        {
            parentTile.Add(Common.illegalPos);
        }
    }

    public void ClearSquare(int idx)
    {
        // assert(idx >= 0 && idx < fCost.size());
        fCost[idx] = float.MaxValue;
        gCost[idx] = float.MaxValue;
        // clear all bits except PATHOPT_OBSOLETE 清除除PATHOPT_OBSOLETE以外的所有位
        nodeMask[idx] &= (int)PATHOPT.OBSOLETE;
    }
}

