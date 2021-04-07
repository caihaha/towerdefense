using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveAgent
{
	#region 数据成员
	private PathManager pathManager;

	private Enemy owner;
	private GameTile goalTile;
	private Vector3 currWayPoint;
	private Vector3 nextWayPoint;
	public GameTile GoalTile { get => goalTile; set => goalTile = value; }

	public Vector3 CurrWayPoint { get => currWayPoint; set => currWayPoint = value; }
	public Vector3 NextWayPoint { get => nextWayPoint; set => nextWayPoint = value; }
	public Vector3 FrontDir { get => flatFrontDir; set => flatFrontDir = value; }

	private Vector3 flatFrontDir;
	private float wantedSpeed;
	private float currentSpeed;
	private float deltaSpeed;

	private bool atGoal;
	private bool atEndOfPath;
	private bool wantRepath;

	private float currWayPointDist;
	private float prevWayPointDist;
	private float goalRadius;

	private bool reversing;
	private bool idling;
	private bool pushResistant; 
	private bool canReverses;
	private bool useMainHeading;
	private bool useRawMovement;

	private float skidRotSpeed;
	private float skidRotAccel;

	private uint pathID;
	private uint nextObstacleAvoidanceFrame;

	private int numIdlingUpdates;
	private int numIdlingSlowUpdates;
	#endregion

	public MoveAgent(Enemy enemy, GameTile startPoint)
    {
		owner = enemy;
		Init(startPoint);
		pathManager = new PathManager();
    }

	#region 初始化
	void Init(GameTile startPoint)
	{
		currentSpeed = 0.01f;
		currWayPoint = startPoint.ExitPoint;
		nextWayPoint = startPoint.ExitPoint;

		atGoal = false;

		flatFrontDir = new Vector3(0, 0, 1);
	}
	#endregion

	#region 对外接口
	public bool GameUpdate()
	{
		currWayPoint = nextWayPoint;
		nextWayPoint = currWayPoint + (currentSpeed * flatFrontDir); 

		UpdateOwnerSpeedAndHeading();
		//UpdateOwnerPos();
		//HandleObjectCollisions();

		return true;
	}

	public void GameSlowUpdate()
	{
	}

	public void StartMoving()
	{
		if (currWayPoint == null || goalTile == null)
			return;

		if (atGoal)
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

		// ReRequestPath(nowPoint);
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
		// transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);
	}

	private void HandleObjectCollisions()
	{
		HandleUnitObjectCollision();

		// HandleStaticObjectCollision();
	}

	private void HandleUnitObjectCollision()
	{

	}

	private void HandleStaticObjectCollision()
	{
		//if (nextWayPoint == null || currWayPoint == null)
		//{
		//	return;
		//}

		//// 下一步可以走
		//if (!GameTileDefs.IsBlocked(currWayPoint, nextWayPoint))
		//{
		//	return;
		//}

		//bool wantRequestPath = false;
		//float fCost = float.MaxValue;
		//float cos = -1f;

		//for (int z = -1; z <= 1; ++z)
		//{
		//	for (int x = -1; x <= 1; ++x)
		//	{
		//		if (x == 0 && z == 0)
		//		{
		//			continue;
		//		}

		//		GameTile nextTile = GameTileDefs.GetGameTileByPos(new Vector2Int((int)currWayPoint.ExitPoint.x + x, (int)currWayPoint.ExitPoint.z + z));

		//		if (nextTile == null ||
		//			GameTileDefs.IsBlocked(currWayPoint, nextTile))
		//		{
		//			continue;
		//		}

		//		float gCost = PathDefs.CalcG(0, currWayPoint, nextTile);
		//		float hCost = PathDefs.Heuristic(goalPoint, nextTile);
		//		float tmpCos = Common.Dot((nextTile.ExitPoint - currWayPoint.ExitPoint), flatFrontDir);

		//		if (gCost + hCost < fCost ||
		//			(Common.Sign(fCost - (gCost + hCost)) == 0 && tmpCos > cos))
		//		{
		//			fCost = gCost + hCost;
		//			nextWayPoint = nextTile;

		//			cos = tmpCos;
		//			wantRequestPath = true;
		//		}
		//	}
		//}

		//if (wantRequestPath)
		//{
		//	ReRequestPath(nextWayPoint);
		//}
	}

	private void ReRequestPath(GameTile startPoint)
	{
		StopEngine();
		StartEngine(startPoint);
	}

	private void StopEngine()
	{
		if (pathID != 0)
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
			GetNextWayPoint();

			//if(startPoint == nextWayPoint)
			//         {
			//	return;
			//         }

			//GameTile nextPoint = pathManager.NextWayPoint(pathID);
			//if (nextPoint != null)
			//         {
			//	currWayPoint = nextPoint;
			//	nextWayPoint = pathManager.NextWayPoint(pathID);

			//	PrepareNextState();
			//	if (Common.Sign(cosAngle - Common.cosAngleIllegalValue) != 0)
			//	{
			//		transform.localRotation = Quaternion.Euler(0f, directionAngleTo, 0f);
			//	}

			//	nowPoint = currWayPoint;

			//	UpdateOwnerPos();
			//	HandleObjectCollisions();
			//}
		}
	}

	private uint GetNewPath(GameTile startPoint)
	{
		return pathManager.RequiredPath(this, startPoint, goalTile);
	}

	private bool FollowPath()
	{
		if (WantToStop())
		{
			currWayPoint.y = -1.0f;
			nextWayPoint.y = -1.0f;
			SetMainHeading();
			ChangeSpeed(0);
			return false;
		}
        else
        {

        }

		//progress += Time.deltaTime;
		//while (progress >= 1f)
		//{
		//	// currWayPoint当前位置
		//	if (nextWayPoint != null)
		//	{
		//		currWayPoint = nextWayPoint;
		//	}

		//	GetNextWayPoint();
		//	GetObstacleAvoidanceDir();
		//	PrepareNextState();

		//	progress -= 1f;
		//}

		//// 调整方向
		//if (Common.Sign(cosAngle - Common.cosAngleIllegalValue) != 0)
		//{
		//	// transform.localRotation = Quaternion.Euler(0f, directionAngleTo, 0f);
		//}

		//nowPoint = currWayPoint;

		return true;
	}
	private void SetMainHeading()
    {
		return;
    }
	private void ChangeSpeed(float newWantedSpeed)
	{
		wantedSpeed = newWantedSpeed;
		if (wantedSpeed <= 0.0f && currentSpeed < 0.01f)
        {
			currentSpeed = 0.0f;
			deltaSpeed = 0.0f;

			return;
        }

		if(newWantedSpeed > 0.0f)
        {
			// TODO
			wantedSpeed = newWantedSpeed;
		}
        else
        {
			wantedSpeed = newWantedSpeed;
		}
    }


	private bool WantToStop()
	{
		return pathID == 0;
	}

	private void GetObstacleAvoidanceDir()
	{
        if (atGoal)
        {
            return;
        }

        // 动态避障(搜索所有的enemy)
        foreach (var id2Enemy in Common.enemys.Enemys)
        {
            MoveAgent enemy = id2Enemy.Value.Move;
            if (enemy == this)
            {
                continue;
            }

            // 处于静止状态，直接挤过
            //if (enemy.AtGoal || enemy.currWayPoint == enemy.goalPoint)
            //{
            //    continue;
            //}

            //float distSquare = PathDefs.DistenceSquare(this.currWayPoint, enemy.currWayPoint);
            //if (distSquare > PathConstants.SQUARE_SPEED_AND_RADIUS) // SQUARE_SPEED_AND_RADIUS = Square(speed + enemy.radius + this.radius)
            //{
            //    continue;
            //}

            //if (distSquare >= PathDefs.DistenceSquare(currWayPoint, goalPoint))
            //{
            //    continue;
            //}

            // 修改currWayPoint
        //    Vector3 avoiderRightDir = GetRightVector(flatFrontDir);
        //    Vector3 avoideeRightDir = GetRightVector(enemy.flatFrontDir);
        //    int avoiderTurnSign = Common.Sign(Common.Dot(nowPoint.ExitPoint, avoiderRightDir) - Common.Dot(enemy.nowPoint.ExitPoint, avoiderRightDir));
        //    int avoideeTurnSign = Common.Sign(Common.Dot(enemy.nowPoint.ExitPoint, avoideeRightDir) - Common.Dot(nowPoint.ExitPoint, avoideeRightDir));
        //    if (Common.Dot(flatFrontDir, enemy.flatFrontDir) < 0)
        //    {
        //        avoiderTurnSign = Mathf.Max(avoiderTurnSign, avoideeTurnSign);
        //    }
        //    avoiderTurnSign = avoiderTurnSign >= 0 ? 1 : -1;

        //    int tmp = directionAngleFrom % 45;
        //    if (tmp != 0)
        //    {
        //        tmp = avoiderTurnSign < 0 ? -tmp : 45 - tmp;
        //    }

        //    directionAngleFrom += tmp;

        //    for (int i = (tmp == 0 ? 1 : 0); i < 4; ++i)
        //    {
        //        GameTile tmpPoint = nowPoint.GetNextTileByDegree(directionAngleFrom + 45 * i * avoiderTurnSign);
        //        if (tmpPoint == null)
        //        {
        //            continue;
        //        }

        //        if (!GameTileDefs.IsBlocked(nowPoint, tmpPoint))
        //        {
        //            // currWayPoint = tmpPoint;
        //            break;
        //        }
        //    }
        }

        return;
	}

	#region 改变下一个状态
	void GetNextWayPoint()
	{
		//nextWayPoint = pathManager.NextWayPoint(pathID);

		//if (nextWayPoint != null && currWayPoint != null &&
		//	GameTileDefs.IsBlocked(currWayPoint, nextWayPoint))
		//{
		//	ReRequestPath(currWayPoint);
		//}
	}

	void PrepareNextState()
	{
		//positionFrom = positionTo;
		//positionTo = currWayPoint.ExitPoint;
		//cosAngle = Common.cosAngleIllegalValue;

		//if (positionFrom != positionTo)
		//{
		//	// 使用向量
		//	Vector3 nextDir = positionTo - positionFrom;
		//	nextDir /= Mathf.Sqrt(Common.SqLength(nextDir));

		//	if (nextDir != flatFrontDir)
		//	{
		//		cosAngle = Common.Dot(flatFrontDir, nextDir);
		//		SetDirectionAngleTo(nextDir);

		//		flatFrontDir = nextDir;
		//	}
		//}

		//directionAngleFrom = directionAngleTo;
	}

	void SetDirectionAngleTo(Vector3 nextDir)
	{
		//if (Common.Sign(cosAngle - 1f) == 0)
		//{
		//	return;
		//}

		//int degree;
		//Vector3 dirCross = Common.Cross(flatFrontDir, nextDir);

		//if (Common.Sign(cosAngle + 1f) == 0)
		//{
		//	degree = 180;
		//}
		//else if (Common.Sign(dirCross.y) > 0)
		//{
		//	degree = Common.Rad2Degree(Mathf.Acos(cosAngle));
		//}
		//else
		//{
		//	degree = -Common.Rad2Degree(Mathf.Acos(cosAngle));
		//}

		//// 消除误差，确保在临近网格行走时角度准确(有问题)
		//int tmp = (degree + 180) % 45;
		//if (tmp != 0)
		//{
		//	if (tmp <= 2)
		//	{
		//		degree -= tmp;
		//	}
		//	else if (tmp >= 43)
		//	{
		//		degree += (45 - tmp);
		//	}
		//}

		// directionAngleTo = (directionAngleFrom + degree) % 360;
	}

	// 二维下
	Vector3 GetRightVector(Vector3 vec)
	{
		return new Vector3(vec.z, 0, -vec.x);
	}
	#endregion

	#endregion
}
