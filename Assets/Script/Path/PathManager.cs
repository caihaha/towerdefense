using System.Collections.Generic;
using UnityEngine;


public class PathManager
{
    #region 数据成员
    class MultiPath
    {
        public IPath.Path path = new IPath.Path();
        public IPath.SearchResult searchResult;

        public Vector3 start;
        public Vector3 finalGoal;
        public MoveAgent caller;

        public Vector3 goalPos;
        float goalRadius;

        public MultiPath()
        {
            caller = null;
        }

        public MultiPath(Vector3 startPos, Vector3 goalPos, float radius)
        {
            this.start = startPos;
            this.goalPos = goalPos;
            this.goalRadius = radius;
            
            this.caller = null;
            this.searchResult = IPath.SearchResult.Error;
        }
    }

    readonly PathFinder pathFinder;
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

    public Vector3 NextWayPoint(uint pathID)
    {
        MultiPath multiPath = GetMultiPath(pathID);
        if (multiPath == null)
        {
            return Common.illegalPos;
        }

        if (multiPath.path != null && multiPath.path.path.Count > 0)
        {
            return multiPath.path.path.Pop();
        }

        return Common.illegalPos;
    }

    public uint RequiredPath(MoveAgent caller, Vector3 startPos, Vector3 goalPos, float goalRadius)
    {
        if(!IsFinalized())
        {
            return 0;
        }

        Common.GetTileXZ(startPos, out var startX, out var startZ);
        Common.GetTileXZ(goalPos, out var goalX, out var goalZ);

        MultiPath newPath = new MultiPath(startPos, goalPos, goalRadius);
        newPath.finalGoal = goalPos;
        newPath.caller = caller;

        IPath.SearchResult result = ArrangePath(newPath, startPos, goalPos, caller);

        FinalizePath(newPath, startPos, goalPos, result == IPath.SearchResult.CantGetCloser);
        newPath.searchResult = result;
        uint pathID = Store(newPath);

        return pathID;
    }
    #endregion
    
    #region 内部函数
    private IPath.SearchResult ArrangePath(MultiPath newPath, Vector3 starePos, Vector3 goalPos, MoveAgent caller)
    {
        IPath.SearchResult result = pathFinder.GetPath(caller, starePos, goalPos, newPath.path);

        return result;
    }

    private uint Store(MultiPath path)
    {
        pathMap.Add(++nextPathID, path);
        return nextPathID;
    }

    private static void FinalizePath(MultiPath path, Vector3 startPos, Vector3 goalPos, bool cantGetCloser)
    {

    }

    private bool IsFinalized()
    {
        return pathFinder != null;
    }

    private MultiPath GetMultiPath(uint pathID)
    {
        if(pathMap.ContainsKey(pathID))
        {
            return pathMap[pathID];
        }

        return null;
    }
    #endregion
}
