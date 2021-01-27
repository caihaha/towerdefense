using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum Direction
{
    Begin,

    Up = Begin,
    UpRight,
    Right,
    DownRight,
    Down,
    DownLeft,
    Left,
    UpLeft,

    End
}

public enum DirectionChange
{
    Begin,

    None = Begin,
    TurnUpRight,
    TurnRight,
    TurnAroundRight,
    TurnAround,
    TurnAroundLeft,
    TurnLeft,
    TurnUpLeft,

    End
}

public enum PATHOPT
{
    LEFT = 1, // +x
    RIGHT = 2, // -x
    UP = 4, // +z
    DOWN = 8, // -z
    OPEN = 16,
    CLOSED = 32,
    BLOCKED = 64,
    OBSOLETE = 128, // 过时的

    SIZE = 255, // size of PATHOPT bitmask
}

public static class DirectionExtensions
{
    static Quaternion[] rotations = {
        Quaternion.identity,
        Quaternion.Euler(0f, 45f, 0f),
        Quaternion.Euler(0f, 90f, 0f),
        Quaternion.Euler(0f, 135f, 0f),
        Quaternion.Euler(0f, 180f, 0f),
        Quaternion.Euler(0f, 225f, 0f),
        Quaternion.Euler(0f, 270f, 0f),
        Quaternion.Euler(0f, 315f, 0f)
    };

    public static Quaternion GetRotation(this Direction direction)
    {
        return rotations[(int)direction];
    }

    public static DirectionChange GetDirectionChangeTo(this Direction current, Direction next)
    {
        if (current == next)
        {
            return DirectionChange.None;
        }
        else if (current + (int)Direction.UpRight == next || current - (Direction.End - Direction.UpRight) == next)
        {
            return DirectionChange.TurnUpRight;
        }
        else if (current + (int)Direction.Right == next || current - (Direction.End - Direction.Right) == next)
        {
            return DirectionChange.TurnRight;
        }
        else if (current + (int)Direction.DownRight == next || current - (Direction.End - Direction.DownRight) == next)
        {
            return DirectionChange.TurnAroundRight;
        }
        else if (current + (int)Direction.Down == next || current - (Direction.End - Direction.Down) == next)
        {
            return DirectionChange.TurnAround;
        }
        else if (current + (int)Direction.DownLeft == next || current - (Direction.End - Direction.DownLeft) == next)
        {
            return DirectionChange.TurnAroundLeft;
        }
        else if (current + (int)Direction.Left == next || current - (Direction.End - Direction.Left) == next)
        {
            return DirectionChange.TurnLeft;
        }
        return DirectionChange.TurnUpLeft;
    }

    public static float GetAngle(this Direction direction)
    {
        return (float)direction * 45f;
    }

    public static bool IsDiagonalDirection(Direction dir)
    {
        //return dir == Direction.DownLeft ||
        //    dir == Direction.DownRight ||
        //    dir == Direction.UpLeft ||
        //    dir == Direction.UpRight;

        return (((int)dir & 1) != 0);
    }

    public static uint[] GetPathDir2PathOpt()
    {
        uint []a = new uint[(uint)Direction.End];

        a[(uint)Direction.Up] = (uint)PATHOPT.UP;
        a[(uint)Direction.Right] = (uint)PATHOPT.RIGHT;
        a[(uint)Direction.Down] = (uint)PATHOPT.DOWN;
        a[(uint)Direction.Left] = (uint)PATHOPT.LEFT;
        a[(uint)Direction.UpRight] = (uint)(PATHOPT.RIGHT | PATHOPT.UP);
        a[(uint)Direction.UpLeft] = (uint)(PATHOPT.LEFT | PATHOPT.UP);
        a[(uint)Direction.DownRight] = (uint)(PATHOPT.RIGHT | PATHOPT.DOWN);
        a[(uint)Direction.DownLeft] = (uint)(PATHOPT.LEFT | PATHOPT.DOWN);

        return a;
    }

    public static uint[] GetPathOpt2PathDir()
    {
        uint[] a = new uint[15];

        a[(uint)PATHOPT.LEFT] = (uint)Direction.Left;
        a[(uint)PATHOPT.RIGHT] = (uint)Direction.Right;
        a[(uint)PATHOPT.UP] = (uint)Direction.Up;
        a[(uint)PATHOPT.DOWN] = (uint)Direction.Down;

        a[(uint)(PATHOPT.LEFT | PATHOPT.UP)] = (uint)Direction.UpLeft;
        a[(uint)(PATHOPT.RIGHT | PATHOPT.UP)] = (uint)Direction.UpRight;
        a[(uint)(PATHOPT.RIGHT | PATHOPT.DOWN)] = (uint)Direction.DownRight;
        a[(uint)(PATHOPT.LEFT | PATHOPT.DOWN)] = (uint)Direction.DownLeft;
        return a;
    }

    public static uint[] DIR2OPT = GetPathDir2PathOpt();
    public static uint[] OPT2DIR = GetPathOpt2PathDir();

    public static uint PathDir2PathOpt(uint pathDir)
    {
        return DIR2OPT[pathDir];
    }

    public static uint PathOpt2PathDir(uint pathOpt)
    {
        return OPT2DIR[pathOpt];
    }

    public static bool IsBlocked(GameTile tile, GameTile neighbor, Direction direction)
    {
        if(tile == null || neighbor == null)
        {
            return true;
        }    

        switch (direction)
        {
            case Direction.UpRight:
                {
                    if (tile.Up.Content.Type == GameTileContentType.Wall &&
                    tile.Right.Content.Type == GameTileContentType.Wall)
                        return true;
                    break;
                }
            case Direction.UpLeft:
                {
                    if (tile.Up.Content.Type == GameTileContentType.Wall &&
                    tile.Left.Content.Type == GameTileContentType.Wall)
                        return true;
                    break;
                }
            case Direction.DownRight:
                {
                    if (tile.Down.Content.Type == GameTileContentType.Wall &&
                    tile.Right.Content.Type == GameTileContentType.Wall)
                        return true;
                    break;
                }
            case Direction.DownLeft:
                {
                    if (tile.Down.Content.Type == GameTileContentType.Wall &&
                    tile.Left.Content.Type == GameTileContentType.Wall)
                        return true;
                    break;
                }
        }

        return false;
    }

    public static Direction GetDirection(this Direction current, int num)
    {
        Direction dir = current + num;
        if(dir < Direction.Begin)
        {
            dir += (int)Direction.End;
        }
        else if(dir > Direction.End)
        {
            dir -= (int)Direction.End;
        }

        return dir;
    }
}