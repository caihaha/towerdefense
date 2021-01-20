using System;
using System.Collections.Generic;

abstract public class IPathFinder
{
    #region 数据成员
    public List<PathNode> openBlocks = new List<PathNode>();
    public List<uint> clockBlocks;
    #endregion

    #region 对外接口
    public IPath.SearchResult GetPath(Enemy owner, GameTile startPos, IPath.Path path)
    {
        return IPath.SearchResult.Ok;
    }
    #endregion

    #region 内部函数
    abstract protected IPath.SearchResult DoSearch(Enemy owner);

    abstract protected void FinishSearch(IPath.Path path);

    abstract protected bool TestBlock(PathNode suqare, Enemy owner);
    #endregion
}
