using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum PATHDIR {
    LEFT = 0, // +x (LEFT *TO* RIGHT)
    LEFT_UP = 1, // +x+z
    UP = 2, // +z (UP *TO* DOWN)
    RIGHT_UP = 3, // -x+z

    RIGHT = 4, // -x (RIGHT *TO* LEFT)
    RIGHT_DOWN = 5, // -x-z
    DOWN = 6, // -z (DOWN *TO* UP)
    LEFT_DOWN = 7, // +x-z

    DIRECTIONS = 8,
};

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

static public class DirectionDefs
{
    static public uint PATHOPT_CARDINALS = (uint)(PATHOPT.RIGHT | PATHOPT.LEFT | PATHOPT.UP | PATHOPT.DOWN);
    static public uint [] PATHDIR_CARDINALS = {(uint)PATHDIR.LEFT, (uint)PATHDIR.RIGHT, (uint)PATHDIR.UP, (uint)PATHDIR.DOWN };
    static public uint PATH_DIRECTION_VERTICES = (int)PATHDIR.DIRECTIONS >> 1;
    public static int PATH_NODE_SPACING = 2;

    static public uint[] GetPathDir2PathOpt()
    {
        uint []a = new uint[(int)PATHDIR.DIRECTIONS];

        a[(int)PATHDIR.LEFT] = (uint)PATHOPT.LEFT;
        a[(int)PATHDIR.RIGHT] = (uint)PATHOPT.RIGHT;
        a[(int)PATHDIR.UP] = (uint)PATHOPT.UP;
        a[(int)PATHDIR.DOWN] = (uint)PATHOPT.DOWN;
        a[(int)PATHDIR.LEFT_UP] = (uint)(PATHOPT.LEFT | PATHOPT.UP);
        a[(int)PATHDIR.RIGHT_UP] = (uint)(PATHOPT.RIGHT | PATHOPT.UP);
        a[(int)PATHDIR.RIGHT_DOWN] = (uint)(PATHOPT.RIGHT | PATHOPT.DOWN);
        a[(int)PATHDIR.LEFT_DOWN] = (uint)(PATHOPT.LEFT | PATHOPT.DOWN);

        return a;
    }

    static public uint[] GetPathOpt2PathDir()
    {
        uint[] a = new uint[15];

        a[(int)PATHOPT.LEFT] = (uint)PATHDIR.LEFT;
        a[(int)PATHOPT.RIGHT] = (uint)PATHDIR.RIGHT;
        a[(int)PATHOPT.UP] = (uint)PATHDIR.UP;
        a[(int)PATHOPT.DOWN] = (uint)PATHDIR.DOWN;

        a[(int)(PATHOPT.LEFT | PATHOPT.UP)] = (uint)PATHDIR.LEFT_UP;
        a[(int)(PATHOPT.RIGHT | PATHOPT.UP)] = (uint)PATHDIR.RIGHT_UP;
        a[(int)(PATHOPT.RIGHT | PATHOPT.DOWN)] = (uint)PATHDIR.RIGHT_DOWN;
        a[(int)(PATHOPT.LEFT | PATHOPT.DOWN)] = (uint)PATHDIR.LEFT_DOWN;
        return a;
    }

    static public uint[] DIR2OPT = GetPathDir2PathOpt();
    static public uint[] OPT2DIR = GetPathOpt2PathDir();

    static public uint PathDir2PathOpt(uint pathDir) { return DIR2OPT[pathDir]; }
    static public uint PathOpt2PathDir(uint pathOptDir) { return OPT2DIR[pathOptDir]; }

    static public int GetBlockVertexOffset(uint pathDir, uint numBlocks)
    {
        int bvo = (int)pathDir;
        int tmp = (int)PATH_DIRECTION_VERTICES;

        switch ((PATHDIR)pathDir)
        {
            case PATHDIR.RIGHT: { bvo = (int)(PATHDIR.LEFT) - tmp; } break;
            case PATHDIR.RIGHT_DOWN: { bvo = (int)(PATHDIR.LEFT_UP) - (int)(numBlocks * tmp) - tmp; } break;
            case PATHDIR.DOWN: { bvo = (int)(PATHDIR.UP) - (int)(numBlocks * tmp); } break;
            case PATHDIR.LEFT_DOWN: { bvo = (int)(PATHDIR.RIGHT_UP) - (int)(numBlocks * tmp) + tmp; } break;
            default: { } break;
        }

        return bvo;
    }

    public static Vector2Int[] PF_DIRECTION_VECTORS_2D = new Vector2Int[(int)PATHDIR.DIRECTIONS << 1]
    {
    new Vector2Int(0, 0),
    new Vector2Int(+1 * PATH_NODE_SPACING, 0 * PATH_NODE_SPACING), // PATHOPT_LEFT
    new Vector2Int(-1 * PATH_NODE_SPACING, 0 * PATH_NODE_SPACING), // PATHOPT_RIGHT
    new Vector2Int(0, 0), // PATHOPT_LEFT | PATHOPT_RIGHT
    new Vector2Int(0 * PATH_NODE_SPACING, +1 * PATH_NODE_SPACING), // PATHOPT_UP
    new Vector2Int(+1 * PATH_NODE_SPACING, +1 * PATH_NODE_SPACING), // PATHOPT_LEFT | PATHOPT_UP
    new Vector2Int(-1 * PATH_NODE_SPACING, +1 * PATH_NODE_SPACING), // PATHOPT_RIGHT | PATHOPT_UP
    new Vector2Int(0, 0), // PATHOPT_LEFT | PATHOPT_RIGHT | PATHOPT_UP
    new Vector2Int(0 * PATH_NODE_SPACING, -1 * PATH_NODE_SPACING), // PATHOPT_DOWN
    new Vector2Int(+1 * PATH_NODE_SPACING, -1 * PATH_NODE_SPACING), // PATHOPT_LEFT | PATHOPT_DOWN
    new Vector2Int(-1 * PATH_NODE_SPACING, -1 * PATH_NODE_SPACING), // PATHOPT_RIGHT | PATHOPT_DOWN
    new Vector2Int(0, 0),
    new Vector2Int(0, 0),
    new Vector2Int(0, 0),
    new Vector2Int(0, 0),
    new Vector2Int(0, 0),
    };
}

public enum NODE_COST
{
    F = 0,
    G = 1,
    H = 2,
}