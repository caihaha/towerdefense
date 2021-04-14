using System.Collections.Generic;
using UnityEngine;

abstract public class IPathFinder
{
    #region 数据成员
    public PriorityQueue<PathNode> openBlocks = new PriorityQueue<PathNode>(new PathNodeComparer());
    // 上次搜索中更改的块列表

    public PathNodeStateBuffer blockStates;
    public PathNodeBuffer openBlockBuffer;
    public List<int> dirtyBlocks;

    public Vector2Int mStartBlock;
    public int mStartBlockIdx;
    public int mGoalBlockIdx;
    public float mGoalHeuristic;
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
        ResetSearch();
        blockStates.nodeMask[mStartBlockIdx] &= (int)PATHOPT.OBSOLETE;
        blockStates.nodeMask[mStartBlockIdx] |= (int)PATHOPT.OBSOLETE;
        blockStates.fCost[mStartBlockIdx] = 0.0f;
        blockStates.gCost[mStartBlockIdx] = 0.0f;
        openBlockBuffer.SetSize(0);

        PathNode ob = openBlockBuffer.GetNode(openBlockBuffer.GetSize());
        ob.fCost = 0f;
        ob.gCost = 0f;
        ob.pos = startPos;
        ob.nodeNum = mStartBlockIdx;
        ob.nodePos = mStartBlock;
        openBlocks.Push(ob);

        // 将起点标记为最佳位置
        mGoalBlockIdx = mStartBlockIdx;
        IPath.SearchResult search = DoSearch(owner, goalPos);

        return search;
    }

    protected void ResetSearch()
    {
        int i = dirtyBlocks.Count;
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
