using UnityEngine;
using System.Collections;

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
                mGoalFCost = 0.0f;
                fundGoal = true;
                break;
            }

            TestNeighborSquares(openSquare, owner, goalPos);
        }

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
        Vector2Int square = Common.BlockIndex2Pos(mGoalBlockIdx);
        int blockIdx = mGoalBlockIdx;

        while (true)
        {
            foundPath.squares.Add(square);
            foundPath.path.Add(new Vector3(square.x, 0, square.y));

            if (blockIdx == mStartBlockIdx)
            {
                break;
            }

            square -= DirectionDefs.PF_DIRECTION_VECTORS_2D[blockStates.nodeMask[blockIdx] & (int)DirectionDefs.PATHOPT_CARDINALS];
            blockIdx = Common.BlockPos2Index(square);
        }

        if (foundPath.path.Count > 0)
        {
            foundPath.pathGoal = foundPath.path[0];
        }

        foundPath.pathCost = blockStates.fCost[mGoalBlockIdx];
    }

    override protected bool TestBlock(PathNode parentSquare, MoveAgent owner,uint pathOptDir, uint blockStatus, Vector2Int square, int sqrIdx, Vector3 goalPos)
    {
        if ((blockStatus & (uint)BlockTypes.BLOCK_STRUCTURE) != 0)
        {
            return false;
        }

        Vector3 nextPos = new Vector3(square.x, 0, square.y);

        // 计算fCost
        float gCost = PathDefs.CalcG(parentSquare, nextPos);
        float hCost = PathDefs.Heuristic(goalPos, nextPos);
        float fCost = gCost + hCost;

        if ((blockStates.nodeMask[sqrIdx] & (uint)PATHOPT.OPEN) != 0)
        {
            if (blockStates.fCost[sqrIdx] <= fCost)
                return true;

            blockStates.nodeMask[sqrIdx] &= ~((int)DirectionDefs.PATHOPT_CARDINALS); // 除了上下左右置0, 其他位置为1
        }

        // fCost更接近
        if(fCost < mGoalFCost)
        {
            mGoalBlockIdx = sqrIdx;
            mGoalFCost = fCost;
        }

        openBlockBuffer.SetSize(openBlockBuffer.GetSize() + 1);
        var openBlock = openBlockBuffer.GetNode(openBlockBuffer.GetSize());
        openBlock.gCost = gCost;
        openBlock.fCost = fCost;
        openBlock.pos = nextPos;
        openBlock.nodePos = square;
        openBlock.nodeNum = sqrIdx;
        openBlocks.Push(openBlock); // 相同节点会重复push ??

        blockStates.fCost[sqrIdx] = openBlock.fCost;
        blockStates.gCost[sqrIdx] = openBlock.gCost;
        blockStates.nodeMask[sqrIdx] |= (int)((uint)PATHOPT.OPEN | pathOptDir);

        dirtyBlocks.Add(sqrIdx);
        return true;
    }

    #region 内部函数

    public class SquareState
    {
        public BlockTypes blockMask;
        public float speedMod;
        public bool inSearch;

        public SquareState()
        {
            speedMod = 0.0f;
            inSearch = false;
            blockMask = BlockTypes.BLOCK_IMPASSABLE;
        }
    };

    void TestNeighborSquares(PathNode square, MoveAgent owner, Vector3 goalPos)
    {
        SquareState []ngbStates = new SquareState[(int)PATHDIR.DIRECTIONS];

        for(uint dir = 0; dir < (uint)PATHDIR.DIRECTIONS; ++dir)
        {
            uint optDir = DirectionDefs.PathDir2PathOpt(dir);
            Vector2Int ngbSquareCoors = square.nodePos + DirectionDefs.PF_DIRECTION_VECTORS_2D[optDir];
            int ngbSquareIdx = Common.BlockPos2Index(ngbSquareCoors);
            if (ngbSquareIdx < 0) // 非法位置
            {
                continue;
            }

            if ((blockStates.nodeMask[ngbSquareIdx] & (int)(PATHOPT.CLOSED | PATHOPT.BLOCKED)) != 0) // 阻塞或者已经在路径中
            {
                continue;
            }

            TestBlock(square, owner, DirectionDefs.PathDir2PathOpt(dir), (uint)ngbStates[dir].blockMask, ngbSquareCoors, ngbSquareIdx, goalPos);
        }

        blockStates.nodeMask[square.nodeNum] |= (int)PATHOPT.CLOSED;
        dirtyBlocks.Add(square.nodeNum);
    }
    #endregion
}
