using UnityEngine;

public class PathFinder : IPathFinder
{
    override protected IPath.SearchResult DoSearch(MoveAgent owner, Vector3 goalPos)
    {
        bool fundGoal = false;

        while(openBlocks.Count > 0)
        {
            PathNode openSquare = openBlocks.Pop();
            if (blockStates.fCost[openSquare.nodeNum] != openSquare.fCost)
            {
                continue;
            }

            if (PathDefs.IsGoal(new Vector3(mStartBlock.x, 0, mStartBlock.y), goalPos))
            {
                mGoalBlockIdx = openSquare.nodeNum;
                mGoalHeuristic = 0.0f;
                fundGoal = true;
                break;
            }

            TestNeighborSquares(openSquare, owner, goalPos);
        }

        //while (openBlocks.Count > 0)
        //{
        //    PathNode node = openBlocks.Pop();
        //    if (node == null)
        //        continue;

        //    closeBlocks.Add(node);
        //    if (node.pos == goalPos)
        //    {
        //        foundGoal = true;
        //        break;
        //    }

        //    TestNeighborSquares(node, owner, goalPos);
        //}

        if (fundGoal)
        {
            return IPath.SearchResult.Ok;
        }

        if(openBlocks.Count <= 0)
        {
            return IPath.SearchResult.GoalOutOfRange;
        }

        return IPath.SearchResult.Error;
    }

    override protected void FinishSearch(IPath.Path foundPath, Vector3 startPos, Vector3 goalPos)
    {
        Vector3 tmp = goalPos;
        while (true)
        {
            foundPath.path.Push(tmp);
            if (tmp == startPos)
            {
                break;
            }
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

            openBlock.gCost = gCost;
            openBlock.fCost = fCost;
        }
        else
        {
            openBlock = new PathNode();
            openBlock.gCost = gCost;
            openBlock.fCost = fCost;
            openBlock.pos = nextPos;
            openBlocks.Push(openBlock);
        }

        blockStates.parentTile[Common.PosToTileIndex(nextPos)] = parentSquare.pos;
        return true;
    }

    #region 内部函数

    struct SquareState
    {
    };

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
            if (tmp != null && tmp.pos == pos)
            {
                return tmp;
            }
        }
        return null;
    }
    #endregion
}
