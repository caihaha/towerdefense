using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour
{
    #region 数据成员
    [SerializeField]
    Transform arrow = default;

    GameTile left, right, up, down, upRight, downRight, upLeft, downLeft;
    public GameTile Left => left;
    public GameTile Up => up;
    public GameTile Right => right;
    public GameTile Down => down;
    public GameTile UpRight => upRight;
    public GameTile DownRight => downRight;
    public GameTile UpLeft => upLeft;
    public GameTile DownLeft => downLeft;

    public uint num;

    static Quaternion upRotation = Quaternion.Euler(90f, 0f, 0f);
    static Quaternion upRightRotation = Quaternion.Euler(90f, 45f, 0f);
    static Quaternion rightRotation = Quaternion.Euler(90f, 90f, 0f);
    static Quaternion downRightRotation = Quaternion.Euler(90f, 135f, 0f);
    static Quaternion downRotation = Quaternion.Euler(90f, 180f, 0f);
    static Quaternion downLeftRotation = Quaternion.Euler(90f, 225f, 0f);
    static Quaternion leftRotation = Quaternion.Euler(90f, 270f, 0f);
    static Quaternion upLeftRotation = Quaternion.Euler(90f, 315f, 0f);

    GameTileContent content;
    public Vector3 ExitPoint { get; set; }

    #endregion

    public static void MakeRightLeftNightbors(GameTile right, GameTile left)
    {
        Debug.Assert(left.right == null && right.left == null, "Redefined neighbors");
        left.right = right;
        right.left = left;
    }

    public static void MakeUpDownNightbors(GameTile up, GameTile down)
    {
        Debug.Assert(up.down == null && down.up == null, "Redefined neighbors");
        up.down = down;
        down.up = up;
    }

    public static void MakeDiagonalNightbors(GameTile tile)
    {
        if (tile.up != null && tile.up.right != null)
        {
            tile.upRight = tile.up.right;
        }

        if (tile.up != null && tile.up.left != null)
        {
            tile.upLeft = tile.up.left;
        }

        if (tile.down != null && tile.down.left != null)
        {
            tile.downLeft = tile.down.left;
        }

        if (tile.down != null && tile.down.right != null)
        {
            tile.downRight = tile.down.right;
        }
    }

    public void ClearPath()
    {
        ExitPoint = transform.localPosition;
    }

    public void BecomeDestination()
    {
        ExitPoint = transform.localPosition;
    }

    public bool IsDestination => Content.Type == GameTileContentType.Destination;

    // 用于交替搜索优先级
    public bool IsAlternative { get; set; }

    public GameTileContent Content
    {
        get => content;
        set
        {
            Debug.Assert(value != null, "Null assigned to content!");
            if (content != null)
            {
                content.Recycle();
            }
            content = value;
            content.transform.localPosition = transform.localPosition;
        }
    }

    public void HidePath()
    {
        arrow.gameObject.SetActive(false);
    }

    public GameTile GetTileByDirection(Direction direction)
    {
        return direction == Direction.Up ? up :
            direction == Direction.UpLeft ? upLeft :
            direction == Direction.UpRight ? upRight :
            direction == Direction.Left ? left :
            direction == Direction.Right ? right :
            direction == Direction.DownLeft ? downLeft :
            direction == Direction.DownRight ? downRight :
            down;
    }

    public Direction GetDirectionByTile(GameTile tile)
    {
        if(tile == null)
        {
            Debug.LogError("GetDirectionByTile, tile is null");
            return Direction.Up;
        }

        return tile == up ? Direction.Up :
            tile == upLeft ? Direction.UpLeft :
            tile == upRight ? Direction.UpRight :
            tile == left ? Direction.Left :
            tile == right ? Direction.Right :
            tile == downLeft ? Direction.DownLeft :
            tile == downRight ? Direction.DownRight :
            Direction.Down;
    }
}
