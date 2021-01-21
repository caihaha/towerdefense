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

    // lastOnPath广度优先搜索使用
    GameTile nextOnPath, lastOnPath;

    float distance;

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

    public GameTile NextTileOnPath => nextOnPath;

    // lastDirection记录父节点过来的方向，回溯的时候用
    public GameTile LastTileOnPath
    {
        get => lastOnPath;
        set
        {
            lastOnPath = value;
        }
    }

    private Direction lastDirection;
    public Direction LastDirection
    {
        get => lastDirection;
        set
        {
            lastDirection = value;
        }
    }

    private Direction pathDirection;
    public Direction PathDirection
    {
        get => pathDirection;
        set
        {
            pathDirection = value;
        }
    }

    // A*寻路
    public float fCost, gCost;
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
        distance = float.MaxValue;
        nextOnPath = null;
        lastOnPath = null;

        gCost = 0f;
        fCost = float.MaxValue;

        ExitPoint = transform.localPosition;
    }

    public void BecomeDestination()
    {
        distance = 0;
        nextOnPath = null;
        lastOnPath = null;

        gCost = 0f;
        fCost = float.MaxValue;

        ExitPoint = transform.localPosition;
    }

    public bool HasPath => distance != float.MaxValue;
    public bool IsDiatance => Mathf.Abs(distance) <= 0.00001f;

    public GameTile BackPathTo(GameTile neighbor, Direction direction)
    {
        if (neighbor == null || neighbor.HasPath)
        {
            return null;
        }

        neighbor.distance += DirectionExtensions.IsDiagonalDirection(direction) ? distance + 1.4142f : distance + 1f;

        neighbor.nextOnPath = this;
        neighbor.ExitPoint = neighbor.transform.localPosition; // (neighbor.transform.localPosition + transform.localPosition) * 0.5f;

        neighbor.PathDirection = direction;
        return neighbor.Content.Type != GameTileContentType.Wall ? neighbor : null;
    }

    public void ShowPath()
    {
        if (IsDiatance || nextOnPath == null)
        {
            arrow.gameObject.SetActive(false);
            return;
        }
        arrow.gameObject.SetActive(true);
        arrow.localRotation =
            nextOnPath == up ? upRotation :
            nextOnPath == upRight ? upRightRotation :
            nextOnPath == right ? rightRotation :
            nextOnPath == downRight ? downRightRotation :
            nextOnPath == down ? downRotation :
            nextOnPath == downLeft ? downLeftRotation :
            nextOnPath == left ? leftRotation :
            upLeftRotation;
    }

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
}
