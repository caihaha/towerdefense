using System.Collections.Generic;
using UnityEngine;

abstract public class IPathFinder
{
    #region 数据成员
    public PriorityQueue<PathNode> openBlocks = new PriorityQueue<PathNode>(new PathNodeComparer());
    public HashSet<PathNode> closeBlocks = new HashSet<PathNode>();

    public PathNodeStateBuffer blockStates = new PathNodeStateBuffer();
    #endregion

    #region 对外接口
    public IPath.SearchResult GetPath(MoveAgent owner, Vector3 startPos, Vector3 goalPos,IPath.Path path)
    {
        IPath.SearchResult result = InitSearch(owner, startPos, goalPos);
        if(result == IPath.SearchResult.Ok || result == IPath.SearchResult.GoalOutOfRange)
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
        PathNode ob = new PathNode()
        {
            fCost = 0f,
            gCost = 0f,
            pos = startPos
        };
        openBlocks.Push(ob);

        IPath.SearchResult search = DoSearch(owner, goalPos);

        return search;
    }

    protected void ResetSearch()
    {
        closeBlocks.Clear();
        openBlocks.Clear();
    }

    abstract protected IPath.SearchResult DoSearch(MoveAgent owner, Vector3 goalPos);

    abstract protected void FinishSearch(IPath.Path path, Vector3 startPos, Vector3 goalPos);

    abstract protected bool TestBlock(PathNode parentSquare, Vector3 goalTile, Vector3 nextTile);
    #endregion
}
