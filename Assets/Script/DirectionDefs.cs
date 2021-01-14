using UnityEngine;

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
        return dir == Direction.DownLeft ||
            dir == Direction.DownRight ||
            dir == Direction.UpLeft ||
            dir == Direction.UpRight;
    }
}