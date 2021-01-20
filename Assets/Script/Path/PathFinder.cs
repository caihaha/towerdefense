using UnityEngine;

public class PathFinder : IPathFinder
{

    override protected IPath.SearchResult DoSearch(Enemy owner)
    {
        return IPath.SearchResult.Error;
    }

    override protected void FinishSearch(IPath.Path path)
    {

    }

    override protected bool TestBlock(PathNode suqare, Enemy owner)
    {
        return true;
    }
}
