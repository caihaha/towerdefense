using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour
{
    [SerializeField]
    Transform arrow = default;

    GameTile left, right, up, down, upRight, downRight, upLeft, downLeft, nextOnPath, lastOnPath;
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

    public GameTile NextTileOnPath => nextOnPath;
    public GameTile LastTileOnPath => lastOnPath;

    public Vector3 ExitPoint{ get; set; }

    // lastDirection父节点过来的方向
    private Direction pathDirection, lastDirection;
    public Direction PathDirection 
    { 
        get => pathDirection; 
        set
        {
            pathDirection = value;
        }
    }

    public Direction LastDirection
    {
        get => lastDirection;
        set
        {
            lastDirection = value;
        }
    }

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
        if(tile.up != null && tile.up.right != null)
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
        ExitPoint = transform.localPosition;
    }

    public void BecomeDestination()
    {
        distance = 0;
        nextOnPath = null;
        ExitPoint = transform.localPosition;
    }

    public bool HasPath => distance != float.MaxValue;
    public bool IsDiatance => Mathf.Abs(distance) <= 0.00001f;

    GameTile GrowPathTo(GameTile neighbor, Direction direction)
    {
        if (neighbor == null || neighbor.HasPath)
        {
            return null;
        }

        neighbor.lastOnPath = this;

        neighbor.LastDirection = direction;
        return neighbor.Content.Type != GameTileContentType.Wall ? neighbor : null;
    }

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

    public GameTile GrowPathUp() => GrowPathTo(up, Direction.Up);
    public GameTile GrowPathRight() => GrowPathTo(right, Direction.Right);
    public GameTile GrowPathLeft() => GrowPathTo(left, Direction.Left);
    public GameTile GrowPathDown() => GrowPathTo(down, Direction.Down);
    public GameTile GrowPathUpRight() => GrowPathTo(upRight, Direction.UpRight);
    public GameTile GrowPathUpLeft() => GrowPathTo(upLeft, Direction.UpLeft);
    public GameTile GrowPathDownRight() => GrowPathTo(downRight, Direction.DownRight);
    public GameTile GrowPathDownLeft() => GrowPathTo(downLeft, Direction.DownLeft);

    public void ShowPath()
    {
        if(IsDiatance)
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
}
