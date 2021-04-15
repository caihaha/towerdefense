using System.Collections.Generic;
using UnityEngine;

abstract public class IPathFinder
{
    #region 数据成员
    public PriorityQueue<PathNode> openBlocks = new PriorityQueue<PathNode>(new PathNodeComparer());
    // 上次搜索中更改的块列表

    public PathNodeStateBuffer blockStates = new PathNodeStateBuffer();
    public PathNodeBuffer openBlockBuffer = new PathNodeBuffer();
    public List<int> dirtyBlocks = new List<int>();

    public Vector2Int mStartBlock;
    public int mStartBlockIdx;
    public int mGoalBlockIdx;
    public float mGoalFCost;
    #endregion

    #region 对外接口
    public IPath.SearchResult GetPath(MoveAgent owner, Vector3 startPos, Vector3 goalPos, Vector2Int startBlock, IPath.Path path)
    {
        mStartBlock = startBlock;
        mStartBlockIdx = Common.BlockPos2Index(startBlock);

        // start up a new search
        IPath.SearchResult result = InitSearch(owner, startPos, goalPos);
        // if search was successful, generate new path
        if (result == IPath.SearchResult.Ok || result == IPath.SearchResult.GoalOutOfRange)
        {
            FinishSearch(path, startPos, goalPos);
        }

        return result;
    }
    #endregion

    #region 内部函数
    protected IPath.SearchResult InitSearch(MoveAgent owner, Vector3 startPos, Vector3 goalPos)
    {
        Vector2Int square = mStartBlock;

        bool isStartGoal = PathDefs.IsGoal(startPos, goalPos);
        bool startInGoal = false;// pfDef.startInGoalRadius;
        // 尽管我们的起始正方形可能在目标半径内，但起始坐标可能在目标半径外。 在这种情况下，我们不想返回CantGetCloser，而是返回到我们的起始正方形的路径。
        // although our starting square may be inside the goal radius, the starting coordinate may be outside.
        // in this case we do not want to return CantGetCloser, but instead a path to our starting square.
        if (isStartGoal && startInGoal)
            return IPath.SearchResult.CantGetCloser;

        ResetSearch();
        blockStates.nodeMask[mStartBlockIdx] &= (int)PATHOPT.OBSOLETE;
        blockStates.nodeMask[mStartBlockIdx] |= (int)PATHOPT.OPEN;
        blockStates.fCost[mStartBlockIdx] = 0.0f;
        blockStates.gCost[mStartBlockIdx] = 0.0f;
        dirtyBlocks.Add(mStartBlockIdx);

        openBlockBuffer.SetSize(0);
        PathNode ob = openBlockBuffer.GetNode(openBlockBuffer.GetSize());
        ob.fCost = 0.0f;
        ob.gCost = 0.0f;
        ob.nodeNum = mStartBlockIdx;
        ob.nodePos = mStartBlock;
        openBlocks.Push(ob);

        // 将起点标记为最佳位置
        mGoalBlockIdx = mStartBlockIdx;
        mGoalFCost = PathDefs.Heuristic(startPos, goalPos);
        IPath.SearchResult ipfResult = DoSearch(owner, goalPos);

        if (ipfResult == IPath.SearchResult.Ok)
            return ipfResult;

        if (mGoalBlockIdx != mStartBlockIdx)
            return ipfResult;

        // if start and goal are within the same block, but distinct squares
        // or considered a single point for search purposes, then we probably
        // can not get closer 如果开始和目标在同一块内，但有不同的正方形或出于搜索目的被视为单个点，那么我们可能无法接近
        return (!isStartGoal || startInGoal) ? IPath.SearchResult.CantGetCloser : ipfResult;
    }

    protected void ResetSearch()
    {
        var i = dirtyBlocks.Count;
        while (i > 0)
        {
            --i;
            blockStates.ClearSquare(dirtyBlocks[i]);
        }
        dirtyBlocks.Clear();
        openBlocks.Clear();
    }

    abstract protected IPath.SearchResult DoSearch(MoveAgent owner, Vector3 goalPos);

    abstract protected void FinishSearch(IPath.Path path, Vector3 startPos, Vector3 goalPos);

    abstract protected bool TestBlock(PathNode parentSquare, MoveAgent owner, uint pathOptDir, uint blockStatus, Vector2Int square, int sqrIdx, Vector3 goalPos);
    #endregion
}
