using UnityEngine;

public class PathFinder : IPathFinder
{
    override protected IPath.SearchResult DoSearch(Enemy owner, GameTile goalPos)
    {
        bool isFinded = false;

        while (openBlocks.Count > 0)
        {
            PathNode node = GetLeastFCostNode();
            if (node == null)
                continue;


            closeBlocks.Add(node);
            if (node.tile.Content.Type == GameTileContentType.Destination)
            {
                isFinded = true;
                break;
            }

            TestNeighborSquares(node, owner, goalPos);
        }

        return isFinded ? IPath.SearchResult.Ok : IPath.SearchResult.Error;
    }

    override protected void FinishSearch(IPath.Path foundPath, GameTile startPos, GameTile goalPos)
    {
        GameTile tile = goalPos;
        while (true)
        {
            if (tile == null || tile == startPos)
            {
                break;
            }

            foundPath.path.Push(tile);
            tile = blockStates.parentTile[(int)tile.num];
        }
    }

    override protected bool TestBlock(PathNode parentSquare, GameTile goalTile, GameTile nextTile)
    {
        // 在Close列表
        if (IsTileInCloseBlocks(nextTile))
        {
            return false;
        }

        // 计算fCost
        float gCost = PathDefs.CalcG(parentSquare, nextTile);
        float hCost = PathDefs.Heuristic(goalTile, nextTile);
        float fCost = gCost + hCost;

        PathNode openBlock = GetOpenBlocksByTiles(nextTile);
        if (openBlock != null)
        {
            if (gCost >= openBlock.gCost)
            {
                return false;
            }
        }
        else
        {
            openBlock = new PathNode();
            openBlock.tile = nextTile;
            openBlocks.Add(openBlock);
        }

        openBlock.gCost = gCost;
        openBlock.fCost = fCost;

        blockStates.parentTile[(int)nextTile.num] = parentSquare.tile;

        return true;
    }

    #region 内部函数
    PathNode GetLeastFCostNode()
    {
        if (openBlocks == null)
            return null;

        PathNode resTile = openBlocks[0];

        for (int i = 1; i < openBlocks.Count; ++i)
        {
            if (resTile.fCost > openBlocks[i].fCost)
                resTile = openBlocks[i];
        }

        openBlocks.Remove(resTile);
        return resTile;
    }

    void TestNeighborSquares(PathNode ob, Enemy owner, GameTile goalPos)
    {
        GameTile tile = ob.tile;

        if (tile == null)
            return;

        for (int z = -1; z <= 1; ++z)
        {
            for (int x = -1; x <= 1; ++x)
            {
                if (x == 0 && z == 0)
                {
                    continue;
                }

                GameTile nextTile = GameTileDefs.GetGameTileByIndex(
                    Common.BlockPos2Index(new Vector2Int((int)tile.ExitPoint.x + x, (int)tile.ExitPoint.z + z)));

                if (nextTile == null ||
                    GameTileDefs.IsBlocked(tile, nextTile))
                {
                    continue;
                }

                TestBlock(ob, goalPos, nextTile);
            }
        }
    }

    bool IsTileInCloseBlocks(GameTile tile)
    {
        foreach (var tmp in closeBlocks)
        {
            if (tmp.tile == tile)
            {
                return true;
            }
        }

        return false;
    }

    PathNode GetOpenBlocksByTiles(GameTile tile)
    {
        foreach (var tmp in openBlocks)
        {
            if (tmp.tile == tile)
            {
                return tmp;
            }
        }

        return null;
    }
    #endregion
}
