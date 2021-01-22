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

    override protected bool TestBlock(PathNode parentSquare, GameTile goalTile, Enemy owner, Direction dir)
    {
        GameTile nextTile = parentSquare.tile.GetTileByDirection(dir);
        if (nextTile == null)
            return false;

        // 在Close列表
        if (IsTileInCloseBlocks(nextTile))
        {
            return false;
        }

        // 计算fCost
        float gCost = PathDefs.CalcG(parentSquare.gCost, nextTile, dir);
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

        for (Direction dir = Direction.Begin; dir < Direction.End; ++dir)
        {
            // 上下左右
            if(((int)dir & 1) == 0)
            {
                TestBlock(ob, goalPos, owner, dir);
            }
            // 对角线
            else
            {
                GameTile neighbor = tile.GetTileByDirection(dir);
                if (neighbor == null)
                    continue;

                bool isCanPath = true;

                switch (dir)
                {
                    case Direction.UpRight:
                        {
                            if (tile.Up.Content.Type == GameTileContentType.Wall &&
                            tile.Right.Content.Type == GameTileContentType.Wall)
                                isCanPath = false;
                            break;
                        }
                    case Direction.UpLeft:
                        {
                            if (tile.Up.Content.Type == GameTileContentType.Wall &&
                            tile.Left.Content.Type == GameTileContentType.Wall)
                                isCanPath = false;
                            break;
                        }
                    case Direction.DownRight:
                        {
                            if (tile.Down.Content.Type == GameTileContentType.Wall &&
                            tile.Right.Content.Type == GameTileContentType.Wall)
                                isCanPath = false;
                            break;
                        }
                    case Direction.DownLeft:
                        {
                            if (tile.Down.Content.Type == GameTileContentType.Wall &&
                            tile.Left.Content.Type == GameTileContentType.Wall)
                                isCanPath = false;
                            break;
                        }
                }

                if (!isCanPath)
                    continue;

                TestBlock(ob, goalPos, owner, dir);
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
