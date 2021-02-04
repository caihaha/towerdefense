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

	#region 方向改变
	Vector3 flatFrontDir;
	float cosAngle;
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
		directionAngleFrom = directionAngleTo = 0;
		transform.localRotation = Quaternion.identity;

		flatFrontDir = new Vector3(0, 0, 1);
		cosAngle = Common.cosAngleIllegalValue; // 取值范围(-1 ~ 1)
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

		HandleStaticObjectCollision();
	}

	private void HandleUnitObjectCollision()
	{
		
	}

	private void HandleStaticObjectCollision()
	{
		if(nextWayPoint == null || currWayPoint == null)
        {
			return;
        }

		// 下一步可以走
		if (!GameTileDefs.IsBlocked(currWayPoint, nextWayPoint))
		{
			return;
		}

		bool wantRequestPath = false;
		float fCost = float.MaxValue;
		float cos = -1f;

		for(int z = -1; z <= 1; ++z)
        {
			for(int x = -1; x <= 1; ++x)
            {
				if(x == 0 && z == 0)
                {
					continue;
                }

				GameTile nextTile = GameTileDefs.GetGameTileByPos(new Vector2Int((int)currWayPoint.ExitPoint.x + x, (int)currWayPoint.ExitPoint.z + z));

				if(nextTile == null || 
					GameTileDefs.IsBlocked(currWayPoint, nextTile))
                {
					continue;
                }

				float gCost = PathDefs.CalcG(0, currWayPoint, nextTile);
				float hCost = PathDefs.Heuristic(goalPoint, nextTile);
				float tmpCos = Common.Dot((nextTile.ExitPoint - currWayPoint.ExitPoint), flatFrontDir);

				if (gCost + hCost < fCost || 
					(Common.Sign(fCost - (gCost + hCost)) == 0 && tmpCos > cos))
				{
					fCost = gCost + hCost;
					nextWayPoint = nextTile;

					cos = tmpCos;
					wantRequestPath = true;
				}
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
				if (Common.Sign(cosAngle - Common.cosAngleIllegalValue) != 0)
				{
					// float angle = Mathf.LerpUnclamped(directionAngleFrom, directionAngleTo, 1);
					transform.localRotation = Quaternion.Euler(0f, directionAngleTo, 0f);
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

			progress -= 1f;
			nextWayPoint = pathManager.NextWayPoint(pathID);

			// 动态避障(搜索所有的enemy)
			GetObstacleAvoidanceDir();
			PrepareNextState();
		}

		// 调整方向
		if (Common.Sign(cosAngle - Common.cosAngleIllegalValue) != 0)
		{
			// float angle = Mathf.LerpUnclamped(directionAngleFrom, directionAngleTo, progress);
			transform.localRotation = Quaternion.Euler(0f, directionAngleTo, 0f);
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

			// 处于静止状态，直接挤过
			if (enemy.AtGoal || enemy.currWayPoint == enemy.goalPoint)
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

			// 修改currWayPoint
			float dirCos = -1f; // 自身与原来运动的方向夹角
			// float pathCos = Common.Dot((nowPoint.ExitPoint - enemy.nowPoint.ExitPoint), flatFrontDir); // 两个enemy所在点的夹角

			for (DirectionChange i = DirectionChange.TurnUpRight; i < DirectionChange.End; ++i)
            {
				if(i == DirectionChange.TurnAround)
                {
					// 往回走
					continue;
                }

				GameTile tmpPoint = nowPoint.GetNextTileByDegree((int)directionAngleTo + 45 * (int)i);
				if(tmpPoint == null)
                {
					continue;
                }

				float tmpDirCos = Common.Dot((tmpPoint.ExitPoint - currWayPoint.ExitPoint), flatFrontDir);

				if (!GameTileDefs.IsBlocked(nowPoint, tmpPoint) &&
					tmpPoint != currWayPoint &&
					tmpDirCos < dirCos)
                {
					currWayPoint = tmpPoint;
				}
			}
		}

		return;
	}

	#region 改变下一个状态
	void PrepareNextState()
	{
		positionFrom = positionTo;
		positionTo = currWayPoint.ExitPoint;
		cosAngle = Common.cosAngleIllegalValue;

		if (positionFrom != positionTo)
		{
			// 使用向量
			Vector3 nextDir = positionTo - positionFrom;
			nextDir /= Mathf.Sqrt(Common.SqLength(nextDir));

			if(nextDir != flatFrontDir)
            {
				cosAngle = Common.Dot(flatFrontDir, nextDir);
				SetDirectionAngleTo(nextDir);

				flatFrontDir = nextDir;
			}
		}

		directionAngleFrom = directionAngleTo;
	}

	void SetDirectionAngleTo(Vector3 nextDir)
	{
		if (Common.Sign(cosAngle - 1f) == 0)
		{
			return;
		}

		int degree;
		Vector3 dirCross = Common.Cross(flatFrontDir, nextDir);

		if (Common.Sign(cosAngle + 1f) == 0)
        {
			degree = 180;
        }
		else if(Common.Sign(dirCross.y) > 0)
        {
			degree = Common.Rad2Degree(Mathf.Acos(cosAngle));
		}
		else
        {
			degree = -Common.Rad2Degree(Mathf.Acos(cosAngle));
        }

		directionAngleTo = (directionAngleFrom + degree) % 360;
	}

	#endregion

	#endregion

	#endregion
}