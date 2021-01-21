using System.Collections.Generic;
using UnityEngine;


public class PathManager
{
    #region 数据成员
    class MultiPath
    {
        public IPath.Path path = new IPath.Path();
        public IPath.SearchResult searchResult;

        public GameTile start;
        public GameTile finalGoal;

        public Enemy caller;

        public MultiPath(GameTile startPos)
        {
            searchResult = IPath.SearchResult.Error;
            start = startPos;
            caller = null;
        }
    }

    PathFinder pathFinder;
    Dictionary<uint, MultiPath> pathMap = new Dictionary<uint, MultiPath>();
    uint nextPathID;
    #endregion

    #region 对外接口
    public PathManager()
    {
        pathFinder = new PathFinder();
    }

    public void DeletePath(uint pathID)
    {
        if (pathID == 0)
        {
            return;
        }

        if (pathMap.ContainsKey(pathID))
        {
            pathMap.Remove(pathID);
        }
    }

    public GameTile NextWayPoint(uint pathID)
    {
        MultiPath multiPath = GetMultiPath(pathID);
        if (multiPath == null)
            return null;

        GameTile tile = null;
        if (multiPath.path != null && multiPath.path.path.Count > 0)
        {
            tile = multiPath.path.path[0];
            multiPath.path.path.RemoveAt(0);
        }

        return tile;
    }

    public uint RequiredPath(Enemy caller, GameTile startPos, GameTile goalPos)
    {
        if(!IsFinalized())
        {
            return 0;
        }

        MultiPath newPath = new MultiPath(startPos);
        newPath.finalGoal = goalPos;
        newPath.caller = caller;

        IPath.SearchResult result = ArrangePath(newPath, startPos, goalPos, caller);

        uint pathID;
        FinalizePath(newPath, startPos, goalPos, result == IPath.SearchResult.CantGetCloser);
        newPath.searchResult = result;
        pathID = Store(newPath);

        return pathID;
    }
    #endregion
    
    #region 内部函数
    IPath.SearchResult ArrangePath(MultiPath newPath, GameTile starePos, GameTile goalPos, Enemy caller)
    {
        IPath.SearchResult result = pathFinder.GetPath(caller, starePos, goalPos, newPath.path);

        return result;
    }

    uint Store(MultiPath path)
    {
        pathMap.Add(++nextPathID, path);
        return nextPathID;
    }

    static void FinalizePath(MultiPath path, GameTile startPos, GameTile goalPos, bool cantGetCloser)
    {

    }

    bool IsFinalized()
    {
        return pathFinder != null;
    }

    MultiPath GetMultiPath(uint pathID)
    {
        if(pathMap.ContainsKey(pathID))
        {
            return pathMap[pathID];
        }

        return null;
    }
    #endregion
}
