using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameTile : MonoBehaviour
{
    #region 数据成员
    [SerializeField]
    Transform arrow = default;

    public uint num;

    GameTileContent content;
    public Vector3 ExitPoint { get; set; }

    #endregion

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

    public GameTile GetNextTileByDegree(int degree)
    {
        GameTile nextTile = null;

        while(degree < 0)
        {
            degree += 360;
        }

        switch(degree % 360)
        {
            case 0: nextTile = GameTileDefs.GetGameTileByPos(new Vector2Int((int)ExitPoint.x, (int)ExitPoint.y + 1)); break;
            case 45: nextTile = GameTileDefs.GetGameTileByPos(new Vector2Int((int)ExitPoint.x + 1, (int)ExitPoint.y + 1)); break;
            case 90: nextTile = GameTileDefs.GetGameTileByPos(new Vector2Int((int)ExitPoint.x + 1, (int)ExitPoint.y)); break;
            case 135: nextTile = GameTileDefs.GetGameTileByPos(new Vector2Int((int)ExitPoint.x + 1, (int)ExitPoint.y - 1)); break;
            case 180: nextTile = GameTileDefs.GetGameTileByPos(new Vector2Int((int)ExitPoint.x, (int)ExitPoint.y - 1)); break;
            case 225: nextTile = GameTileDefs.GetGameTileByPos(new Vector2Int((int)ExitPoint.x - 1, (int)ExitPoint.y - 1)); break;
            case 270: nextTile = GameTileDefs.GetGameTileByPos(new Vector2Int((int)ExitPoint.x - 1, (int)ExitPoint.y)); break;
            case 315: nextTile = GameTileDefs.GetGameTileByPos(new Vector2Int((int)ExitPoint.x - 1, (int)ExitPoint.y + 1)); break;
        }

        return nextTile;
    }
}
