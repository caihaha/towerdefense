using UnityEngine;

public static class Common
{
    private static Vector2Int boardSize = new Vector2Int(11, 11);
    public static Vector2Int BoardSize => boardSize;
    public static int BoardCount => boardSize.x * boardSize.y;

    public static int c = (boardSize.x / 2) + (boardSize.y / 2 * boardSize.x);

    public static EnemyCollection enemys = new EnemyCollection();

    public static bool isUseDirection = false;

    public static float cosAngleIllegalValue = 2f;

    public static int enemySlowUpdateRate = 15;

    public static Vector2Int BlockIndex2Pos(int index)
    {
        if(index < 0 || index > Common.BoardCount)
        {
            return new Vector2Int(int.MaxValue, int.MaxValue);
        }

        return new Vector2Int(index % boardSize.x - boardSize.x / 2, index / boardSize.x - boardSize.y / 2);
    }

    public static int BlockPos2Index(Vector2Int pos)
    {
        if(pos.x < -boardSize.x / 2 || pos.x > (boardSize.x - 1) / 2 || 
            pos.y < -boardSize.y / 2 || pos.y > (boardSize.y - 1) / 2)
        {
            return -1;
        }

        return pos.x + boardSize.x * pos.y + c;
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

    public static float SqLength(Vector3 vec)
    {
        return vec.x * vec.x + vec.y * vec.y + vec.z * vec.z;
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

    public static bool SquareIsBlocked()
    {
        return false;
    }
}

public static class GameTileDefs
{
    public static GameTile GetGameTileByIndex(int index)
    {
        if(index < 0 || index > Common.BoardCount)
        {
            return null;
        }

        return GameBoard.Instance.Tiles[index];
    }

    public static GameTile GetGameTileByPos(Vector2Int vec)
    {
        return GetGameTileByIndex(Common.BlockPos2Index(vec));
    }

    public static bool IsBlocked(GameTile currTile, GameTile nextTile)
    {
        if(nextTile == null || nextTile.Content.Type == GameTileContentType.Wall)
        {
            return true;
        }

        Vector3 diff = nextTile.ExitPoint - currTile.ExitPoint;
        if(diff.x == 0 || diff.z == 0)
        {
            return false;
        }

        GameTile upTile = GetGameTileByPos(new Vector2Int((int)currTile.ExitPoint.x + Common.Sign(diff.x), (int)currTile.ExitPoint.z));
        GameTile rightTile = GetGameTileByPos(new Vector2Int((int)currTile.ExitPoint.x , (int)currTile.ExitPoint.z + Common.Sign(diff.z)));

        if(upTile.Content.Type == GameTileContentType.Wall && rightTile.Content.Type == GameTileContentType.Wall)
        {
            return true;
        }

        return false;
    }

}

