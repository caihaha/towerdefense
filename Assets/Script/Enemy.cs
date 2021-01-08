using UnityEngine;

public class Enemy : MonoBehaviour
{
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

	public void SpawnOn(GameTile tile)
	{
		Debug.Assert(tile.NextTileOnPath != null, "Nowhere to go!", this);
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

	// 更新敌人的状态(live/dead)
	public bool GameUpdate()
	{
		progress += Time.deltaTime;
		while (progress >= 1f)
		{
			tileFrom = tileTo;
			tileTo = tileTo.NextTileOnPath;
			if(tileTo == null)
            {
				OriginFactory.Reclaim(this);
				return false;
            }

			progress -= 1f;
			PrepareNextState();
		}

		transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);

		if (directionChange != DirectionChange.None)
		{
			float angle = Mathf.LerpUnclamped(directionAngleFrom, directionAngleTo, progress);
			transform.localRotation = Quaternion.Euler(0f, angle, 0f);
		}
		return true;
	}

	// 改变下一个状态
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
			case DirectionChange.TurnRight: PrepareTurnRight(); break;
			case DirectionChange.TurnLeft: PrepareTurnLeft(); break;
			default: PrepareTurnAround(); break;
		}
	}

	void PrepareForward()
	{
		transform.localRotation = direction.GetRotation();
		directionAngleTo = direction.GetAngle();
	}

	void PrepareTurnRight()
	{
		directionAngleTo = directionAngleFrom + 90f;
	}

	void PrepareTurnLeft()
	{
		directionAngleTo = directionAngleFrom - 90f;
	}

	void PrepareTurnAround()
	{
		directionAngleTo = directionAngleFrom + 180f;
	}
}