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
}
