using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour
{
    [SerializeField]
    Transform arrow = default;

    GameTile west, east, north, south, nextOnPath;
    int distance;

    static Quaternion northRotation = Quaternion.Euler(90f, 0f, 0f);
    static Quaternion eastRotation = Quaternion.Euler(90f, 90f, 0f);
    static Quaternion southRotation = Quaternion.Euler(90f, 180f, 0f);
    static Quaternion westRotation = Quaternion.Euler(90f, 270f, 0f);

    GameTileContent content;

    public GameTile NextTileOnPath => nextOnPath;

    public Vector3 ExitPoint{ get; private set; }

    public Direction PathDirection { get; private set; }

    public static void MakeEastWeastNightbors(GameTile east, GameTile west)
    {
        Debug.Assert(west.east == null && east.west == null, "Redefined neighbors");
        west.east = east;
        east.west = west;
    }

    public static void MakeNorthSouthNightbors(GameTile north, GameTile south)
    {
        Debug.Assert(north.south == null && south.north == null, "Redefined neighbors");
        north.south = south;
        south.north = north;
    }

    public void ClearPath()
    {
        distance = int.MaxValue;
        nextOnPath = null;
    }

    public void BecomeDestination()
    {
        distance = 0;
        nextOnPath = null;
        ExitPoint = transform.localPosition;
    }

    public bool HasPath => distance != int.MaxValue;

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
        return neighbor.Content.Type != GameTileContentType.Wall ? neighbor : null; ;
    }

    public GameTile GrowPathNorth() => GrowPathTo(north, Direction.South);
    public GameTile GrowPathEast() => GrowPathTo(east, Direction.West);
    public GameTile GrowPathWest() => GrowPathTo(west, Direction.East);
    public GameTile GrowPathSouth() => GrowPathTo(south, Direction.North);

    public void ShowPath()
    {
        if(distance == 0)
        {
            arrow.gameObject.SetActive(false);
            return;
        }
        arrow.gameObject.SetActive(true);
        arrow.localRotation =
            nextOnPath == north ? northRotation :
            nextOnPath == east ? eastRotation :
            nextOnPath == south ? southRotation :
            westRotation;
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
