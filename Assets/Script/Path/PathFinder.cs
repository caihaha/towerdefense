using UnityEngine;

public class PathFinder : IPathFinder
{
    override protected IPath.SearchResult DoSearch(MoveAgent owner, Vector3 goalPos)
    {
        bool foundGoal = false;

        while (openBlocks.Count > 0)
        {
            PathNode node = openBlocks.Pop();
            if (node == null)
                continue;

            closeBlocks.Add(node);
            TestNeighborSquares(node, owner, goalPos);
        }

        return foundGoal ? IPath.SearchResult.Ok : IPath.SearchResult.Error;
    }

    override protected void FinishSearch(IPath.Path foundPath, Vector3 startPos, Vector3 goalPos)
    {
        Vector3 tmp = goalPos;
        while (true)
        {
            if (tmp == null || tmp == startPos)
            {
                break;
            }

            foundPath.path.Push(tmp);
            tmp = blockStates.parentTile[Common.PosToTileIndex(tmp)];
        }
    }

    override protected bool TestBlock(PathNode parentSquare, Vector3 goalPos, Vector3 nextPos)
    {
        // 在Close列表
        if (IsTileInCloseBlocks(nextPos))
        {
            return false;
        }

        // 计算fCost
        float gCost = PathDefs.CalcG(parentSquare, nextPos);
        float hCost = PathDefs.Heuristic(goalPos, nextPos);
        float fCost = gCost + hCost;

        PathNode openBlock = GetOpenBlocksByPos(nextPos);
        if (openBlock != null)
        {
            if (gCost >= openBlock.gCost)
            {
                return false;
            }
        }
        else
        {
            openBlock = new PathNode();
            openBlock.pos = nextPos;
            openBlocks.Push(openBlock);
        }

        openBlock.gCost = gCost;
        openBlock.fCost = fCost;

        blockStates.parentTile[Common.PosToTileIndex(nextPos)] = parentSquare.pos;

        return true;
    }

    #region 内部函数
    void TestNeighborSquares(PathNode ob, MoveAgent owner, Vector3 goalPos)
    {
        Vector3 pos = ob.pos;

        if (pos == null)
            return;

        for (int z = 1; z >= -1; --z)
        {
            for (int x = -1; x <= 1; ++x)
            {
                if (x == 0 && z == 0)
                {
                    continue;
                }

                Vector3 nextPos = new Vector3(pos.x + x, 0, pos.z + z);
                if (GameDefs.IsBlocked(pos, nextPos))
                {
                    continue;
                }

                TestBlock(ob, goalPos, nextPos);
            }
        }
    }

    bool IsTileInCloseBlocks(Vector3 pos)
    {
        foreach (var tmp in closeBlocks)
        {
            if (tmp.pos == pos)
            {
                return true;
            }
        }
        return false;
    }

    PathNode GetOpenBlocksByPos(Vector3 pos)
    {
        foreach (var tmp in openBlocks.Elements)
        {
            if (tmp.pos == pos)
            {
                return tmp;
            }
        }
        return null;
    }
    #endregion
}
