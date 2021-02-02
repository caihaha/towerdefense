using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    #region 数据成员
    #region 移动
    EnemyFactory originFactory;

	GameTile currWayPoint, nextWayPoint;
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
    #endregion

    #region 寻路
    PathManager pathManager;

	// 现在的位置
	GameTile nowPoint;
	public GameTile NowPoint => nowPoint;

	// 目标点
	GameTile goalPoint;
	public GameTile GoalPoint
	{
		get => goalPoint;
        set
        {
			goalPoint = value;
        }
	}

	bool AtGoal => nowPoint == goalPoint || goalPoint == null;

	uint pathID;
	#endregion

	#endregion

	#region 初始化
	public void SpawnOn(GameTile tile)
	{
		nowPoint = tile;
		currWayPoint = tile;
		nextWayPoint = null;

		progress = 0f;
		PrepareIntro();

		pathManager = new PathManager();
	}

    // 初始化状态
    void PrepareIntro()
	{
		positionFrom = currWayPoint.transform.localPosition;
		positionTo = currWayPoint.ExitPoint;
		direction = Direction.Up;
		directionChange = DirectionChange.None;
		directionAngleFrom = directionAngleTo = direction.GetAngle();
		transform.localRotation = direction.GetRotation();
	}
	#endregion

    #region Zero-K
    #region 对外接口
    public bool GameUpdate()
    {
		UpdateOwnerSpeedAndHeading();
		UpdateOwnerPos();
		HandleObjectCollisions();

		return true;
    }

	public void StartMoving()
    {
		if (nowPoint == null || goalPoint == null)
			return;

		if (AtGoal)
			return;

		foreach (GameTile tile in GameBoard.Instance.Tiles)
		{
			if (tile.Content.Type == GameTileContentType.Destination)
			{
				tile.BecomeDestination();
			}
			else
			{
				tile.ClearPath();
			}
		}

		ReRequestPath(nowPoint);
	}
    #endregion

    #region 内部函数
    private void UpdateOwnerSpeedAndHeading()
    {
		FollowPath();
	}

	// 更新位置
	private void UpdateOwnerPos()
    {
		transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);
	}

	private void HandleObjectCollisions()
    {
		HandleUnitObjectCollision();

		HandleStaticObjectCollision(this);
	}

	private void HandleUnitObjectCollision()
	{
		
	}

	private void HandleStaticObjectCollision(Enemy collider)
	{
		if(nextWayPoint == null || currWayPoint == null)
        {
			return;
        }

		Direction nextDir = currWayPoint.GetDirectionByTile(nextWayPoint);
		// 下一步可以走
		if ((nextWayPoint.Content.Type != GameTileContentType.Wall && 
			!DirectionExtensions.IsBlocked(currWayPoint, nextWayPoint, nextDir)))
		{
			return;
		}

		bool wantRequestPath = false;
		float fCost = float.MaxValue;

		// 从现在方向余弦为正(<180度)的开始
		Direction dir = DirectionExtensions.GetDirection(direction, -2);
		for (int i = 0; i < (int)Direction.End; ++i)
        {
			Direction tmpDir = DirectionExtensions.GetDirection(dir, i);
			GameTile nextTile = currWayPoint.GetTileByDirection(tmpDir);

			if(nextTile == null || 
			   nextTile.Content.Type == GameTileContentType.Wall || 
			   DirectionExtensions.IsBlocked(currWayPoint, nextTile, tmpDir))
            {
				continue;
            }

			float gCost = PathDefs.CalcG(0, tmpDir);
			float hCost = PathDefs.Heuristic(goalPoint, nextTile);

			if(gCost + hCost < fCost)
            {
				fCost = gCost + hCost;
				nextWayPoint = nextTile;
				wantRequestPath = true;
			}
		}

		if (wantRequestPath)
		{
			ReRequestPath(nextWayPoint);
		}
	}

	private void ReRequestPath(GameTile startPoint)
    {
		StopEngine();
		StartEngine(startPoint);
    }

	private void StopEngine()
    {
		if(pathID != 0)
        {
			pathManager.DeletePath(pathID);
			pathID = 0;
        }
    }

	private void StartEngine(GameTile startPoint)
    {
		if (pathID == 0)
        {
			pathID = GetNewPath(startPoint);

			if(startPoint == nextWayPoint)
            {
				return;
            }

			GameTile nextPoint = pathManager.NextWayPoint(pathID);
			if (nextPoint != null)
            {
				currWayPoint = nextPoint;
				nextWayPoint = pathManager.NextWayPoint(pathID);

				PrepareNextState();

				// 调整方向
				if (directionChange != DirectionChange.None)
				{
					float angle = Mathf.LerpUnclamped(directionAngleFrom, directionAngleTo, progress);
					transform.localRotation = Quaternion.Euler(0f, angle, 0f);
				}

				nowPoint = currWayPoint;

				UpdateOwnerPos();
				HandleObjectCollisions();
			}
		}
	}

	private uint GetNewPath(GameTile startPoint)
    {
		return pathManager.RequiredPath(this, startPoint, goalPoint);
    }

	private bool FollowPath()
	{
		if(WantToStop())
        {
			return true;
        }

		progress += Time.deltaTime;
		while (progress >= 1f)
		{
			// currWayPoint当前位置
			if (nextWayPoint != null)
			{
				currWayPoint = nextWayPoint;
			}

			nextWayPoint = pathManager.NextWayPoint(pathID);

			progress -= 1f;
			PrepareNextState();
		}

		// 动态避障(搜索所有的enemy)
		GetObstacleAvoidanceDir();

		// 调整方向
		if (directionChange != DirectionChange.None)
		{
			float angle = Mathf.LerpUnclamped(directionAngleFrom, directionAngleTo, progress);
			transform.localRotation = Quaternion.Euler(0f, angle, 0f);
		}

		nowPoint = currWayPoint;

		return true;
	}

	private bool WantToStop()
	{
		return false;
	}

	private void GetObstacleAvoidanceDir()
	{
		if(AtGoal)
        {
			return;
        }			

		foreach(var id2Enemy in Common.enemys.Enemys)
        {
			Enemy enemy = id2Enemy.Value;
			if(enemy == this)
            {
				continue;
            }

			if (enemy.AtGoal)
			{
				continue;
			}

			float distSquare = PathDefs.DistenceSquare(this.currWayPoint, enemy.currWayPoint);
			if (distSquare >= PathConstants.SQUARE_SPEED_AND_RADIUS) // SQUARE_SPEED_AND_RADIUS = Square(speed + enemy.radius + this.radius)
			{
				continue;
            }

			if (distSquare >= PathDefs.DistenceSquare(currWayPoint, goalPoint))
            {
				continue;
            }

			// TODO 下一个位置修改

        }

		return;
	}

	#region 改变下一个状态
	void PrepareNextState()
	{
		positionFrom = positionTo;
		positionTo = currWayPoint.ExitPoint;

		if (currWayPoint != null && currWayPoint != nowPoint)
		{
			direction = nowPoint.GetDirectionByTile(currWayPoint);
			directionChange = direction.GetDirectionChangeTo(direction);
		}

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

	#endregion

	#endregion
}