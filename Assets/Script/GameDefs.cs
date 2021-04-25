using UnityEngine;

public static class Common
{
    public static Vector2Int boardSize = new Vector2Int(11, 11);
    public static Vector2Int boardRangeX = new Vector2Int(-boardSize.x >> 1, (boardSize.x - 1) >> 1);
    public static Vector2Int boardRangeZ = new Vector2Int(-boardSize.y >> 1, (boardSize.y - 1) >> 1);
    public static Vector3 boardMin = new Vector3(boardRangeX.x - 0.5f, 0, boardRangeZ.x - 0.5f);
    public static Vector3 boardMax = new Vector3(boardRangeX.y + 0.5f, 0, boardRangeZ.y + 0.5f);
    public static int BoardCount => boardSize.x * boardSize.y;

    public static int centerCount = (boardSize.x >> 1) + ((boardSize.y >> 1) * boardSize.x);

    public static EnemyCollection enemys = new EnemyCollection();

    public static bool isUseDirection = false;

    public static float cosAngleIllegalValue = 2f;

    public static int enemySlowUpdateRate = 15;

    // 根据网格索引获取网格X,Z
    public static Vector2Int BlockIndex2Pos(int index)
    {
        if(index < 0 || index > BoardCount)
        {
            return new Vector2Int(int.MaxValue, int.MaxValue);
        }

        return new Vector2Int(index % boardSize.x - (boardSize.x >> 1), index / boardSize.x - (boardSize.y >> 1));
    }

    // 根据网格x,z获取网格索引
    public static int BlockPos2Index(Vector2Int pos)
    {
        if(pos.x < boardRangeX.x || pos.x > boardRangeX.y || 
            pos.y < boardRangeZ.x || pos.y > boardRangeZ.y)
        {
            return -1;
        }

        return pos.x + boardSize.x * pos.y + centerCount;
    }

    public static int Sign(float num)
    {
        if(Mathf.Abs(num) < 0.00001)
        {
            return 0;
        }
        else
        {
            return num > 0 ? 1 : -1;
        }
    }

    public static int Sign2(float num)
    {
        return ((num > 0) ? 1 : -1);
    }

    public static float SqLength2D(Vector3 vec)
    {
        return vec.x * vec.x + vec.z * vec.z;
    }

    public static float SqDistance2D(Vector3 vec1, Vector3 vec2)
    {
        float dx = vec1.x - vec2.x;
        float dz = vec1.z - vec2.z;

        return dx * dx + dz * dz;
    }

    public static float Distance2D(Vector3 vec1, Vector3 vec2)
    {
        return Mathf.Sqrt(SqDistance2D(vec1, vec2));
    }

    public static float Dot(Vector3 a, Vector3 b)
    {
        return a.x * b.x + a.y * b.y + a.z * b.z;
    }
    public static Vector3 Cross(Vector3 a, Vector3 b)
    {
        // 目前a.y,b.y都为0，叉乘的结果y值大于0，a在b左侧,等于0为共线
        return new Vector3(a.y * b.z - a.z * b.y,
                            a.z * b.x - a.x * b.z,
                            a.x * b.y - a.y * b.x);
    }
    public static int Rad2Degree(float rad)
    {
        return (int)(rad / Mathf.PI * 180);
    }
    public static float Degree2Rad(int degree)
    {
        return (float)(degree * 1.0 / 180.0 * Mathf.PI);
    }

    public static bool GetTileXZUnclamped(Vector3 pos, out int x, out int z)
    {
        x = ((int)(pos.x + Sign(pos.x) * 0.5));
        z = ((int)(pos.z + Sign(pos.z) * 0.5));
        return x >= boardMin.x && x <= boardMax.x && z >= boardMin.z && z <= boardMax.z;
    }
    // 位置获得网格的X,Z
    public static void GetTileXZ(Vector3 pos, out int x, out int z)
    {
        GetTileXZUnclamped(pos, out x, out z);
        x = Mathf.Clamp(x, boardRangeX.x, boardRangeX.y);
        z = Mathf.Clamp(z, boardRangeZ.x, boardRangeZ.y);
    }

    public static int PosToTileIndex(Vector3 pos)
    {
        if (GetTileXZUnclamped(pos, out int x, out int z))
        {
            return BlockPos2Index(new Vector2Int(x, z));
        }
        return -1;
    }

    public static Vector3 illegalPos = new Vector3(float.MinValue, 0, float.MinValue);

    public static bool IsIllegalPos(Vector3 pos)
    {
        return pos.x < boardMin.x || pos.z < boardMin.z || pos.x > boardMax.x || pos.z > boardMax.z;
    }

    public static GameTile GetGameTileByIndex(int index)
    {
        if (index < 0 || index > BoardCount)
        {
            return null;
        }

        return GameBoard.Instance.Tiles[index];
    }

    public static GameTile GetGameTileByBlock(Vector2Int vec)
    {
        return GetGameTileByIndex(BlockPos2Index(vec));
    }

    public static bool IsDiagonalBlocked(Vector2Int currPos, Vector2Int nextPos)
    {
        if (IsBlocked(nextPos))
        {
            return true;
        }

        var currTile = GetGameTileByBlock(currPos);
        var nextTile = GetGameTileByBlock(nextPos);
        Vector3 diff = nextTile.ExitPoint - currTile.ExitPoint;
        if (diff.x == 0 || diff.z == 0)
        {
            return false;
        }

        GameTile upTile = GetGameTileByBlock(new Vector2Int((int)currTile.ExitPoint.x + Sign(diff.x), (int)currTile.ExitPoint.z));
        GameTile rightTile = GetGameTileByBlock(new Vector2Int((int)currTile.ExitPoint.x, (int)currTile.ExitPoint.z + Sign(diff.z)));

        if (upTile.Content.Type == GameTileContentType.Wall && rightTile.Content.Type == GameTileContentType.Wall)
        {
            return true;
        }

        return false;
    }

    public static bool IsBlocked(Vector2Int nextPos)
    {
        if (IsIllegalPos(new Vector3(nextPos.x, 0, nextPos.y)))
        {
            return true;
        }

        var nextTile = GetGameTileByBlock(nextPos);
        if (nextTile == null || nextTile.Content.Type == GameTileContentType.Wall)
        {
            return true;
        }

        return false;
    }

    public static float FOOTPRINT_RADIUS = 0.5f;
    public static bool AllowUnitCollisionOverlap = false;
}

public static class MoveMath
{
    public static bool IsNonBlocking(Enemy avoider, Enemy avoidee)
    {
        if (avoidee == avoider)
        {
            return true;
        }
        return false;
    }
}


