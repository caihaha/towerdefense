﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDef
{
	public int teamID; //enemy can't push
	public float mass; //calc push distance
	public float radius;
	public float maxSpeed;
	public float maxAcc;
	public float maxDec;
	public bool isPushResistant;

	public UnitDef()
    {
		teamID = 1;
		mass = 1.0f;
		radius = 0.6f;
		maxSpeed = 1.0f;
		maxAcc = 1.0f;
		maxDec = 1.0f;
		isPushResistant = true;
    }
}

public class MoveAgent
{
	#region 数据成员
	private PathManager pathManager;

	private Enemy owner;
	private Vector3 currWayPoint;
	private Vector3 nextWayPoint;
	private Vector3 wayPointDir;
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

	private float maxSpeed;
	private float accRate;
	private float decRate;

	private Vector3 pos;
	private Vector3 goalPos;
	private Vector3 oldPos;
	private Vector3 oldSlowUpdatePos;
	private Vector3 currentVelocity;

	public Vector3 GoalPos { get => goalPos; set => goalPos = value; }
	public Vector3 CurrWayPoint { get => currWayPoint; set => currWayPoint = value; }
	public Vector3 NextWayPoint { get => nextWayPoint; set => nextWayPoint = value; }
	public Vector3 FrontDir { get => flatFrontDir; set => flatFrontDir = value; }
	#endregion

	public MoveAgent(Enemy enemy, GameTile startPoint, UnitDef unitDef)
    {
		this.owner = enemy;
		pathManager = new PathManager();
	
		Init(startPoint, unitDef);
	}

	#region 初始化
	void Init(GameTile startPoint, UnitDef unitDef)
	{
		currentSpeed = 0.0f;
		currWayPoint = startPoint.ExitPoint;
		nextWayPoint = currWayPoint;
		goalPos = currWayPoint;
		maxSpeed = unitDef.maxSpeed;
		accRate = Mathf.Max(0.001f, unitDef.maxAcc);
		decRate = Mathf.Max(0.001f, unitDef.maxDec);

		pos = owner.transform.position;
		goalRadius = 0.0f;
		deltaSpeed = 0.0f;
		numIdlingUpdates = 0;
		numIdlingSlowUpdates = 0;

		wantRepath = false;
		idling = true;
		atEndOfPath = true;
		atGoal = true;

		pushResistant = unitDef.isPushResistant;
		flatFrontDir = new Vector3(0, 0, 1);
	}
	#endregion

	#region 对外接口
	public bool GameUpdate()
	{
		currWayPoint = nextWayPoint;
		nextWayPoint += (currentSpeed * flatFrontDir * Time.deltaTime); 

		UpdateOwnerSpeedAndHeading();
		UpdateOwnerPos(currentVelocity, flatFrontDir * (currentSpeed + deltaSpeed));
		HandleObjectCollisions();

		return true;
	}

	public void GameSlowUpdate()
	{
	}

	public void StartMoving()
	{
		if (currWayPoint == null || goalPos == null)
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
	private void UpdateOwnerPos(Vector3 oldVelocity, Vector3 newVelocity)
	{
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
		
	}

	private void ReRequestPath(bool forceRequest)
	{
		if(forceRequest)
        {
			StopEngine();
			StartEngine();
		}

		wantRepath = !forceRequest;
	}

	private void StopEngine(bool hardStop = false)
	{
		if (pathID != 0)
		{
			pathManager.DeletePath(pathID);
			pathID = 0;
		}

		if (hardStop)
        {
			currentVelocity = Vector3.zero;
			currentSpeed = 0;
		}
		wantedSpeed = 0.0f;
	}

	private void StartEngine()
	{
		if (pathID == 0)
		{
			pathID = GetNewPath();
		}

		if (pathID != 0)
		{
			atGoal = false;
			atEndOfPath = false;
			// currWayPoint = pathManager.NextWayPoint(this, ref this.path, pos, Mathf.Max(currentSpeed * 1.05f, 1.25f * manager.TileSize));
			// nextWayPoint = pathManager.NextWayPoint(this, ref this.path, currWayPoint, Mathf.Max(currentSpeed * 1.05f, 1.25f * manager.TileSize));
		}
		else
		{
			Fail();
		}
	}

	private uint GetNewPath()
	{
		return pathManager.RequiredPath(this, pos, goalPos, goalRadius);
	}

	private bool FollowPath()
	{
		if (WantToStop())
		{
			currWayPoint.y = -0.1f;
			nextWayPoint.y = -0.1f;
			SetMainHeading();
			ChangeSpeed(0.0f);
			return false;
		}
        else
        {
			float curGoalDistSq = (owner.transform.position - goalPos).sqrMagnitude;
			atGoal |= curGoalDistSq <= goalRadius * goalRadius;

			if (!atGoal)
			{
				if (idling)
				{
					numIdlingUpdates = Mathf.Min(numIdlingUpdates + 1, 32768);
				}
				else
				{
					numIdlingUpdates = Mathf.Max(numIdlingUpdates - 1, 0);
				}
			}
			if (!atEndOfPath)
			{
				GetNextWayPoint();
			}
			else
			{
				if (atGoal)
				{
					Arrived();
				}
				else
				{
					ReRequestPath(true);
				}
			}
			if (Mathf.Abs(currWayPoint.x - pos.x) > 0.00001f || Mathf.Abs(currWayPoint.z - pos.z) > 0.00001f)
			{
				wayPointDir = new Vector3(currWayPoint.x - pos.x, 0.0f, currWayPoint.z - pos.z).normalized;
			}
			Vector3 wantedForward = !atGoal ? wayPointDir : flatFrontDir;
			wantedForward = GetObstacleAvoidanceDir(wantedForward);
			ChangeHeading(wantedForward);
			ChangeSpeed(maxSpeed);
		}
		return true;
	}
	private void SetMainHeading()
    {
		return;
    }
	private void ChangeHeading(Vector3 wantedForward)
	{
		return;
	}
	private void ChangeSpeed(float newWantedSpeed)
	{
		if (wantedSpeed <= 0.0f && currentSpeed < 0.01f)
        {
			currentSpeed = 0.0f;
			deltaSpeed = 0.0f;
			return;
        }

		float targetSpeed = maxSpeed;
		if (currWayPoint.y < 0 && nextWayPoint.y < 0)
        {
			targetSpeed = 0.0f;
        }
		else
        {
			if (newWantedSpeed > 0.0f)
			{
				if (!WantToStop())
                {

				}
				else
				{
					targetSpeed = 0.0f;
				}
			}
			else
			{
				targetSpeed = 0.0f;
			}
		}

		targetSpeed = Mathf.Min(targetSpeed, newWantedSpeed);
		float speedDiff = targetSpeed - currentSpeed;
		if (speedDiff > 0.0f)
		{
			deltaSpeed = Mathf.Min(speedDiff, accRate);
		}
		else
		{
			deltaSpeed = Mathf.Max(speedDiff, -decRate);
		}
	}


	private bool WantToStop()
	{
		return pathID == 0;
	}

	private Vector3 GetObstacleAvoidanceDir(Vector3 wantedForward)
	{
        if (atGoal)
        {
            return wantedForward;
        }

        // 动态避障(搜索所有的enemy)
        foreach (var id2Enemy in Common.enemys.Enemys)
        {
            MoveAgent moveAgent = id2Enemy.Value.UnitMove;
            if (moveAgent == this)
            {
                continue;
            }

        }

        return wantedForward;
	}

	#region 改变下一个状态
	private void GetNextWayPoint()
	{
		
	}

	private void PrepareNextState()
	{
		
	}

	private void SetDirectionAngleTo(Vector3 nextDir)
	{
		
	}

	private Vector3 GetRightVector(Vector3 vec)
	{
		return new Vector3(vec.z, 0, -vec.x);
	}
	#endregion

	private void Arrived()
    {

    }

	private void Fail()
    {

    }
	#endregion
}
