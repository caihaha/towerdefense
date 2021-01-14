using UnityEngine;

public class Enemy : MonoBehaviour
{
	#region 数据成员
	EnemyFactory originFactory;

	GameTile tileFrom, tileTo;
	Vector3 positionFrom, positionTo;
	float progress;

	Direction direction;
	DirectionChange directionChange;
	float directionAngleFrom, directionAngleTo;

	public EnemyFactory OriginFactory
	{
		get => originFactory;
		set
		{
			Debug.Assert(originFactory == null, "Redefined origin factory!");
			originFactory = value;
		}
	}

	public GameTile TileFrom
    {
		get => tileFrom;
        set
        {
			tileFrom = value;
        }
    }

	public GameTile TileTo
	{
		get => tileTo;
		set
		{
			tileTo = value;
		}
	}
	#endregion

	#region 初始化
	public void SpawnOn(GameTile tile)
	{
		// Debug.Assert(tile.NextTileOnPath != null, "Nowhere to go!", this);
		tileFrom = tile;
		tileTo = tile.NextTileOnPath;

		progress = 0f;
		PrepareIntro();
	}

    // 初始化状态
    void PrepareIntro()
	{
		positionFrom = tileFrom.transform.localPosition;
		positionTo = tileFrom.ExitPoint;
		direction = tileFrom.PathDirection;
		directionChange = DirectionChange.None;
		directionAngleFrom = directionAngleTo = direction.GetAngle();
		transform.localRotation = direction.GetRotation();
	}
	#endregion

	#region 更新敌人的状态
	public bool GameUpdate()
	{
		progress += Time.deltaTime;
		while (progress >= 1f)
		{
			// tileFrom当前位置
			if (tileTo != null)
            {
				tileFrom = tileTo;
			}

			tileTo = tileFrom.NextTileOnPath;
			if (tileTo != null && tileTo.IsDiatance)
            {
				tileTo.PathDirection = tileFrom.PathDirection;
			}

			progress -= 1f;
			PrepareNextState();
		}

		// 更新位置
		transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);
		GameBoard.Instance.NowPoint = tileFrom;

		// 调整方向
		if (directionChange != DirectionChange.None)
		{
			float angle = Mathf.LerpUnclamped(directionAngleFrom, directionAngleTo, progress);
			transform.localRotation = Quaternion.Euler(0f, angle, 0f);
		}
		return true;
	}
    #endregion

    #region 改变下一个状态
    void PrepareNextState()
	{
		positionFrom = positionTo;
		positionTo = tileFrom.ExitPoint;
		directionChange = direction.GetDirectionChangeTo(tileFrom.PathDirection);
		direction = tileFrom.PathDirection;
		directionAngleFrom = directionAngleTo;

		switch (directionChange)
		{
			case DirectionChange.None: PrepareForward(); break;
			case DirectionChange.TurnUpRight: PrepareTurnUpRight(); break;
			case DirectionChange.TurnRight: PrepareTurnRight(); break;
			case DirectionChange.TurnAroundRight: PrepareTurnAroundRight(); break;
			case DirectionChange.TurnAround: PrepareTurnAround(); break;
			case DirectionChange.TurnAroundLeft: PrepareTurnAroundLeft(); break;
			case DirectionChange.TurnLeft: PrepareTurnLeft(); break;
			default: PrepareTurnUpLeft(); break;
		}
	}

	void PrepareForward()
	{
		transform.localRotation = direction.GetRotation();
		directionAngleTo = direction.GetAngle();
	}
	void PrepareTurnUpRight()
	{
		directionAngleTo = directionAngleFrom + 45f;
	}
	void PrepareTurnRight()
	{
		directionAngleTo = directionAngleFrom + 90f;
	}
	void PrepareTurnAroundRight()
	{
		directionAngleTo = directionAngleFrom + 135f;
	}
	void PrepareTurnAround()
	{
		directionAngleTo = directionAngleFrom + 180f;
	}
	void PrepareTurnUpLeft()
	{
		directionAngleTo = directionAngleFrom - 45f;
	}
	void PrepareTurnLeft()
	{
		directionAngleTo = directionAngleFrom - 90f;
	}
	void PrepareTurnAroundLeft()
	{
		directionAngleTo = directionAngleFrom - 135f;
	}

	#endregion
}