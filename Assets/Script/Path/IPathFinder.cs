using System;
using System.Collections.Generic;

public class IPathFinder
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
    private IPath.SearchResult DoSearch()
    {

        return IPath.SearchResult.Ok;
    }

    private void TestNeighborTiles(PathNode suqare, Enemy owner)
    {

    }

    private void TestBolock(PathNode suqare, Enemy owner)
    {

    }
    #endregion
}
