using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour
{
    [SerializeField]
    Transform arrow = default;

    GameTile left, right, up, down, upRight, downRight, upLeft, downLeft, nextOnPath;
    float distance;

    static Quaternion upRotation = Quaternion.Euler(90f, 0f, 0f);
    static Quaternion rightRotation = Quaternion.Euler(90f, 90f, 0f);
    static Quaternion downRotation = Quaternion.Euler(90f, 180f, 0f);
    static Quaternion leftRotation = Quaternion.Euler(90f, 270f, 0f);

    GameTileContent content;

    public GameTile NextTileOnPath => nextOnPath;

    public Vector3 ExitPoint{ get; private set; }

    public Direction PathDirection { get; private set; }

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
    }

    public void BecomeDestination()
    {
        distance = 0;
        nextOnPath = null;
        ExitPoint = transform.localPosition;
    }

    public bool HasPath => distance != float.MaxValue;

    GameTile GrowPathTo(GameTile neighbor, Direction direction)
    {
        if (neighbor == null || neighbor.HasPath)
        {
            return null;
        }
        neighbor.distance = distance + 1;
        neighbor.nextOnPath = this;

        neighbor.ExitPoint = (neighbor.transform.localPosition + transform.localPosition) * 0.5f;

        neighbor.PathDirection = direction;
        return neighbor.Content.Type != GameTileContentType.Wall ? neighbor : null;
    }
    GameTile GrowPathToDiagonal(GameTile neighbor, Direction direction)
    {
        if (neighbor == null || neighbor.HasPath)
        {
            return null;
        }
        neighbor.distance = distance + 1.414f;
        neighbor.nextOnPath = this;

        neighbor.ExitPoint = (neighbor.transform.localPosition + transform.localPosition) * 0.5f;

        neighbor.PathDirection = direction;
        return neighbor.Content.Type != GameTileContentType.Wall ? neighbor : null;
    }

    public GameTile GrowPathUp() => GrowPathTo(up, Direction.Down);
    public GameTile GrowPathRight() => GrowPathTo(right, Direction.Left);
    public GameTile GrowPathLeft() => GrowPathTo(left, Direction.Right);
    public GameTile GrowPathDown() => GrowPathTo(down, Direction.Up);
    public GameTile GrowPathUpRight() => GrowPathToDiagonal(upRight, Direction.DownLeft);
    public GameTile GrowPathUpLeft() => GrowPathToDiagonal(upLeft, Direction.DownRight);
    public GameTile GrowPathDownRight() => GrowPathToDiagonal(downRight, Direction.UpLeft);
    public GameTile GrowPathDownLeft() => GrowPathToDiagonal(downLeft, Direction.UpRight);

    public void ShowPath()
    {
        if(Mathf.Abs(distance - 0.00001f) <= 0)
        {
            arrow.gameObject.SetActive(false);
            return;
        }
        arrow.gameObject.SetActive(true);
        arrow.localRotation =
            nextOnPath == up ? upRotation :
            nextOnPath == right ? rightRotation :
            nextOnPath == down ? downRotation :
            leftRotation;
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
