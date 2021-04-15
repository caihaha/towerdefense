using System.Collections;
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
	private enum ProgressState { Done = 0, Active = 1, Failed = 2 };
	private ProgressState progressState;
	private PathManager pathManager;

	private Enemy owner;
	private Vector3 currWayPoint;
	private Vector3 nextWayPoint;
	private Vector3 wayPointDir;
	private Vector3 flatFrontDir;

	private float currentSpeed;
	private float deltaSpeed;

	private float maxSpeed;
	private float accRate;
	private float decRate;

	private bool atGoal;
	private bool atEndOfPath;
	private bool wantRepath;
	private bool pushResistant;

	private bool idling;
	private uint pathID;

	private int numIdlingUpdates;
	private int numIdlingSlowUpdates;

	private Vector3 pos;
	private Vector3 goalPos;
	private Vector3 oldPos;
	private Vector3 currentVelocity;
	
	private float goalRadius;
	private uint posTileIdx;


	public Vector3 CurrWayPoint { get => currWayPoint; set => currWayPoint = value; }
	public float Speed { get => currentSpeed; set => currentSpeed = value; }
	public Vector3 FrontDir { get => flatFrontDir; set => flatFrontDir = value; }
	public uint PosTileIdx { get => posTileIdx; set => posTileIdx = value; }
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

		progressState = ProgressState.Done;
		pos = owner.transform.position;

		goalRadius = 0.0f;
		deltaSpeed = 0.0f;
		numIdlingUpdates = 0;
		numIdlingSlowUpdates = 0;

		wantRepath = false;
		atEndOfPath = true;
		atGoal = true;

		pushResistant = unitDef.isPushResistant;
		flatFrontDir = new Vector3(0, 0, 1);
		posTileIdx = (uint)Common.PosToTileIndex(pos);
	}
	#endregion

	#region 对外接口
	public bool GameUpdate()
	{
		oldPos = pos;
		Vector3 oldDir = flatFrontDir;
		UpdateOwnerSpeedAndHeading();
		UpdateOwnerPos(currentVelocity, flatFrontDir * (currentSpeed + deltaSpeed));
		HandleObjectCollisions();
		this.pos.y = 0.0f;
		OwnerMoved(oldPos, oldDir);
		return true;
	}

	public void OwnerMoved(Vector3 oldPos, Vector3 oldForward)
	{
		if ((oldPos - pos).sqrMagnitude < 0.00001f)
		{
			currentVelocity = Vector3.zero;
			currentSpeed = 0.0f;
			//idling = true;
			idling = (currWayPoint.y != -0.1f && nextWayPoint.y != -0.1f);
			return;
		}
		//Vector3 ffd = flatFrontDir * Common.SqDistance2D(oldPos, pos) * 0.5f;
		//Vector3 wpd = wayPointDir;
		//idling = true;
		idling = Common.SqDistance2D(oldPos, pos) < currentSpeed * 0.5f * currentSpeed * 0.5f;
	}

	public void GameSlowUpdate()
	{
		if (progressState == ProgressState.Active)
		{
			if (pathID != 0)
			{
				if (idling)
				{
					numIdlingSlowUpdates = Mathf.Min(numIdlingSlowUpdates + 1, 16);
				}
				else
				{
					numIdlingSlowUpdates = Mathf.Max(numIdlingSlowUpdates - 1, 0);
				}
				if (numIdlingUpdates > 32768)
				{
					if (numIdlingSlowUpdates < 16)
					{
						ReRequestPath(true);
					}
					else
					{
						Fail();
					}
				}
			}
			else
			{
				ReRequestPath(true);
			}
			if (wantRepath)
			{
				ReRequestPath(true);
			}
		}
	}

	public void StartMoving(Vector3 moveGoalPos, float moveGoalRadius)
	{
		goalPos = new Vector3(moveGoalPos.x, 0, moveGoalPos.z);
		if (currWayPoint == null || goalPos == null)
			return;

		atGoal = Common.SqDistance2D(pos, goalPos) < moveGoalRadius * moveGoalRadius;
		if (atGoal)
        {
			return;
		}

		atEndOfPath = false;
		goalRadius = moveGoalRadius;
		progressState = ProgressState.Active;
		numIdlingUpdates = 0;
		numIdlingSlowUpdates = 0;

		ReRequestPath(true);
	}

	public void SetGoalPos(Vector3 goalPos)
    {
		if(goalPos != currWayPoint)
        {
			this.goalPos = goalPos;
			atGoal = false;
        }
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
		if (newVelocity != Vector3.zero)
		{
			Vector3 newPos = pos + newVelocity;
			pos = newPos;
			posTileIdx = (uint)Common.PosToTileIndex(pos);
		}
		currentVelocity = newVelocity;
		currentSpeed = newVelocity.magnitude;
		deltaSpeed = 0.0f;
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
			Vector3 tmp = pathManager.NextWayPoint(pathID);
			if (tmp == Common.illegalPos)
            {
				Fail();
			}
			else
            {
				nextWayPoint = tmp;
			}
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
			float curGoalDistSq = Common.SqLength2D(pos - goalPos);
			atGoal |= (curGoalDistSq <= goalRadius * goalRadius);

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
		flatFrontDir = wantedForward;
	}
	private void ChangeSpeed(float newWantedSpeed)
	{
		if (newWantedSpeed <= 0.0f && currentSpeed < 0.01f)
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
		if (CanGetNextWayPoint())
        {
			currWayPoint = nextWayPoint;
			nextWayPoint = pathManager.NextWayPoint(pathID);
		}
	}

	private bool CanGetNextWayPoint()
    {
		if (pathID == 0)
		{
			return false;
		}
		if (currWayPoint.y != -1.0f && nextWayPoint.y != -1.0f)
		{
			atEndOfPath = Common.SqDistance2D(currWayPoint, goalPos) <= goalRadius * goalRadius;
			if (atEndOfPath)
			{
				currWayPoint = goalPos;
				nextWayPoint = goalPos;
				atGoal = true;
				return false;
			}
		}
		return true;
	}

	private Vector3 GetRightVector(Vector3 vec)
	{
		return new Vector3(vec.z, 0, -vec.x);
	}
	#endregion

	private void Arrived()
    {
		if (progressState == ProgressState.Active)
		{
			StopEngine(false);
			progressState = ProgressState.Done;
			atGoal = true;
		}
	}

	private void Fail()
    {
		StopEngine(false);
		progressState = ProgressState.Failed;
	}
	#endregion
}
