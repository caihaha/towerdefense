using System;
using System.Collections.Generic;

abstract public class IPathFinder
{
    #region 数据成员
    public List<PathNode> openBlocks = new List<PathNode>();
    public HashSet<PathNode> closeBlocks = new HashSet<PathNode>();

    public PathNodeStateBuffer blockStates = new PathNodeStateBuffer();
    #endregion

    #region 对外接口
    public IPath.SearchResult GetPath(MoveAgent owner, GameTile startPos, GameTile goalPos,IPath.Path path)
    {
        IPath.SearchResult result = InitSeatch(owner, startPos, goalPos);
        if(result == IPath.SearchResult.Ok || result == IPath.SearchResult.GoalOutOfRange)
        {
            FinishSearch(path, startPos, goalPos);
        }

        return result;
    }
    #endregion

    #region 内部函数
    protected IPath.SearchResult InitSeatch(MoveAgent owner, GameTile startPos, GameTile goalPos)
    {
        ResetSearch();
        PathNode ob = new PathNode();
        ob.fCost = 0f;
        ob.gCost = 0f;
        ob.tile = startPos;
        openBlocks.Add(ob);

        IPath.SearchResult search = DoSearch(owner, goalPos);

        return search;
    }

    protected void ResetSearch()
    {
        closeBlocks.Clear();
        openBlocks.Clear();
    }

    abstract protected IPath.SearchResult DoSearch(MoveAgent owner, GameTile goalPos);

    abstract protected void FinishSearch(IPath.Path path, GameTile startPos, GameTile goalPos);

    abstract protected bool TestBlock(PathNode parentSquare, GameTile goalTile, GameTile nextTile);
    #endregion
}
