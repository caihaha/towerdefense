using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UnitDef
{
	public int allyteam; //enemy can't push
	public float mass; //calc push distance
	public float radius;
	public float maxSpeed;
	public float maxAcc;
	public float maxDec;
	public bool isPushResistant;

	public UnitDef()
    {
		allyteam = 1;
		mass = 1.0f;
		radius = Common.FOOTPRINT_RADIUS;
		maxSpeed = 0.2f;
		maxAcc = 0.2f;
		maxDec = 0.2f;
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

	private float currWayPointDist;
	private float prevWayPointDist;
	private Vector3 lastAvoidanceDir;

	public Vector3 Pos { get => pos; }
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
		flatFrontDir = Vector3.forward;
		posTileIdx = (uint)Common.PosToTileIndex(pos);

		currWayPointDist = 0.0f;
		prevWayPointDist = 0.0f;
		lastAvoidanceDir = Vector3.zero;
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

	public void TerrainChange()
    {
		if(pathID == 0 || atGoal)
        {
			return;
        }

		ReRequestPath(true);
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
		if (currWayPoint == goalPos)
        {
			return;
		}

		atGoal = Common.SqDistance2D(pos, goalPos) < moveGoalRadius * moveGoalRadius;
		if (atGoal)
        {
			return;
		}
		
		goalRadius = moveGoalRadius;
		atEndOfPath = false;
		progressState = ProgressState.Active;

		numIdlingUpdates = 0;
		numIdlingSlowUpdates = 0;

		currWayPointDist = 0.0f;
		prevWayPointDist = 0.0f;

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

	public Vector3 GetRightDir()
    {
		return new Vector3(flatFrontDir.z, 0, -flatFrontDir.x);
	}
	#endregion

	#region 内部函数
	private void UpdateOwnerSpeedAndHeading()
	{
		FollowPath();
	}

	private bool FollowPath()
	{
		if (WantToStop())
		{
			currWayPoint.y = -0.1f;
			nextWayPoint.y = -0.1f;
			SetMainHeading();
			ChangeSpeed(0.0f);
		}
		else
		{
			prevWayPointDist = currWayPointDist;
			currWayPointDist = Common.Distance2D(Pos, currWayPoint);

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
		return false;
	}

	// 更新位置
	private void UpdateOwnerPos(Vector3 oldVelocity, Vector3 newVelocity)
	{
		if (newVelocity != Vector3.zero)
		{
			pos += newVelocity;
			posTileIdx = (uint)Common.PosToTileIndex(pos);
		}
		currentVelocity = newVelocity;
		currentSpeed = newVelocity.magnitude;
		deltaSpeed = 0.0f;
	}

	private void HandleObjectCollisions()
	{
		HandleUnitCollisions(owner, currentSpeed, owner.unitDef.radius);
		HandleFeatureCollisions(owner, currentSpeed, owner.unitDef.radius);

		bool squareChange = (Common.PosToTileIndex(pos + currentVelocity) != Common.PosToTileIndex(pos));
		if(squareChange)
        {
			HandleStaticObjectCollision();
		}
	}

	private void HandleUnitCollisions(Enemy collider, float colliderSpeed, float colliderRadius)
	{
		float searchRadius = colliderSpeed + (colliderRadius * 2.0f);
		var colliderMove = collider.UnitMove;

		foreach (var id2Enemy in Common.enemys.Enemys)
		{
			var collidee = id2Enemy.Value;
			if(collidee == collider)
            {
				continue;
            }

			var collideeMove = collidee.UnitMove;
			float collideeSpeed = 0.2f;
			float collideeRadius = collidee.unitDef.radius;

			Vector3 separationVector = colliderMove.pos - collideeMove.pos;
			float separationMinDistSq = (colliderRadius + collideeRadius) * (colliderRadius + collideeRadius);

			if ((Common.SqLength2D(separationVector) - separationMinDistSq) > 0.01f)
				continue; // 距离大于半径的距离,不会碰撞

			bool pushCollider = true;
			bool pushCollidee = true;

			float colliderRelRadius = colliderRadius / (colliderRadius + collideeRadius);
			float collideeRelRadius = collideeRadius / (colliderRadius + collideeRadius);
			float collisionRadiusSum = Common.AllowUnitCollisionOverlap ?
			(colliderRadius * colliderRelRadius + collideeRadius * collideeRelRadius) :
			(colliderRadius + collideeRadius);

			float sepDistance = Common.SqLength2D(separationVector) + 0.1f;
			float penDistance = Mathf.Max(collisionRadiusSum - sepDistance, 1.0f);
			float sepResponse = Mathf.Min(1.0f, penDistance * 0.5f);

			Vector3 sepDirection = (separationVector / sepDistance);
			Vector3 tmp = sepDirection * sepResponse;
			Vector3 colResponseVec = new Vector3(tmp.x, 0, tmp.z);

			float
			m1 = 1.0f,
			m2 = 1.0f,
			v1 = Mathf.Max(0.2f, colliderSpeed),
			v2 = Mathf.Max(0.2f, collideeSpeed),
			c1 = 1.0f + (1.0f - Mathf.Abs(Vector3.Dot(colliderMove.flatFrontDir, -sepDirection))) * 5.0f,
			c2 = 1.0f + (1.0f - Mathf.Abs(Vector3.Dot(collideeMove.flatFrontDir, sepDirection))) * 5.0f,
			s1 = m1 * v1 * c1,
			s2 = m2 * v2 * c2,
 			r1 = s1 / (s1 + s2 + 1.0f),
 			r2 = s2 / (s1 + s2 + 1.0f);

			float colliderMassScale = Mathf.Clamp(0.2f - r1, 0.01f, 0.99f) * (Common.AllowUnitCollisionOverlap ? (1.0f / colliderRelRadius) : 1.0f);
			float collideeMassScale = Mathf.Clamp(0.2f - r2, 0.01f, 0.99f) * (Common.AllowUnitCollisionOverlap ? (1.0f / collideeRelRadius) : 1.0f );

			float colliderSlideSign = Common.Sign2(Vector3.Dot(separationVector, colliderMove.GetRightDir()));
			float collideeSlideSign = Common.Sign2(Vector3.Dot(- separationVector,collideeMove.GetRightDir()));

			Vector3 colliderPushVec = colResponseVec * colliderMassScale;
			Vector3 collideePushVec = -colResponseVec * collideeMassScale;
			Vector3 colliderSlideVec = colliderMove.GetRightDir() * colliderSlideSign * (1.0f / penDistance) * r2;
			Vector3 collideeSlideVec = collideeMove.GetRightDir() * collideeSlideSign * (1.0f / penDistance) * r1;
			Vector3 colliderMoveVec = colliderPushVec + colliderSlideVec;
			Vector3 collideeMoveVec = collideePushVec + collideeSlideVec;

			if (pushCollider || !pushCollidee)
			{
				if (colliderMove.TestMoveSquare(colliderMove.pos += colliderMoveVec))
				{
					colliderMove.pos += colliderMoveVec;
				}
			}

			if (pushCollidee || !pushCollider)
			{
				if (collideeMove.TestMoveSquare(collideeMove.pos + collideeMoveVec))
				{
					collideeMove.pos += collideeMoveVec;
				}
			}
		}
	}

	private void HandleStaticObjectCollision()
	{
		
	}

	private void HandleFeatureCollisions(Enemy collider, float colliderSpeed, float colliderRadius)
	{
		float searchRadius = colliderSpeed + (colliderRadius * 2);
		HandleStaticObjectCollision();
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
			Vector3 tmp1 = pathManager.NextWayPoint(pathID);
			if (tmp1 == Common.illegalPos)
            {
				Fail();
			}
			else
            {
				currWayPoint = tmp1;
			}
			Vector3 tmp2 = pathManager.NextWayPoint(pathID);
			if (tmp2 != Common.illegalPos)
            {
				nextWayPoint = tmp2;
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

	private void OwnerMoved(Vector3 oldPos, Vector3 oldForward)
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

	private Vector3 GetObstacleAvoidanceDir(Vector3 desiredDir)
	{
        if (WantToStop())
        {
            return flatFrontDir;
        }

		Vector3 avoidanceVec = Vector3.zero;
		Vector3 avoidanceDir = desiredDir;
		lastAvoidanceDir = desiredDir;
		var avoider = owner;
		var avoiderMoveAgent = this;

		if(Vector3.Dot(flatFrontDir, desiredDir) < 0.0f)
        {
			return lastAvoidanceDir;
        }

		float AVOIDER_DIR_WEIGHT = 1.0f;
		float DESIRED_DIR_WEIGHT = 0.5f;
		float MAX_AVOIDEE_COSINE = Mathf.Cos(120.0f * 0.017453292519943295f); // cos(2π/3) = -1/2
		float LAST_DIR_MIX_ALPHA = 0.7f;

		float avoidanceRadius = Mathf.Max(currentSpeed, 1.0f) * (avoider.unitDef.radius * 2.0f);
		float avoiderRadius = Common.FOOTPRINT_RADIUS;

		// 动态避障(搜索所有的enemy)
		foreach (var id2Enemy in Common.enemys.Enemys)
        {
			var avoidee = id2Enemy.Value;
			MoveAgent avoideeMoveAgent = id2Enemy.Value.UnitMove;
            if (avoidee == avoider)
            {
                continue;
            }

			if (MoveMath.IsNonBlocking(avoider, avoidee))
            {
				continue;
            }

			// avoidee不在移动并且是友方直接continue, avoider会将avoidee推开
			if (avoideeMoveAgent.atGoal && avoidee.unitDef.allyteam == avoider.unitDef.allyteam)
			{
				continue;
			}

			bool avoideeMobile = true;
			bool avoideeMovable = true;
			Vector3 avoideeVector = (avoiderMoveAgent.pos + avoiderMoveAgent.currentVelocity) - (avoideeMoveAgent.pos + avoideeMoveAgent.currentVelocity);
			float avoideeRadius = Common.FOOTPRINT_RADIUS;
			float avoidanceRadiusSum = avoiderRadius + avoideeRadius;
			float avoidanceMassSum = avoider.unitDef.mass + avoidee.unitDef.mass;
			float avoideeMassScale = avoideeMobile ? (avoidee.unitDef.mass / avoidanceMassSum) : 1.0f;
			float avoideeDistSq = Common.SqLength2D(avoideeVector);
			float avoideeDist = Mathf.Sqrt(avoideeDistSq) + 0.01f;

			if (Vector3.Dot(avoideeMoveAgent.flatFrontDir, -(avoideeVector / avoideeDist)) < MAX_AVOIDEE_COSINE)
            {
				continue;
            }

			if (avoideeDistSq >= Common.SqDistance2D(avoiderMoveAgent.pos, goalPos))
            {
				continue;
            }

			// 确定符号
			Vector3 avoiderRight = avoiderMoveAgent.GetRightDir();
			Vector3 avoideeRight = avoideeMoveAgent.GetRightDir();
			float avoiderTurnSign = -Common.Sign2(Vector3.Dot(avoideeMoveAgent.pos, avoiderRight) - Vector3.Dot(avoiderMoveAgent.pos, avoiderRight));
			float avoideeTurnSign = -Common.Sign2(Vector3.Dot(avoiderMoveAgent.pos, avoideeRight) - Vector3.Dot(avoideeMoveAgent.pos, avoideeRight));

			float avoidanceCosAngle = Mathf.Clamp(Vector3.Dot(avoiderMoveAgent.flatFrontDir, avoideeMoveAgent.flatFrontDir), -1.0f, 1.0f); // cos() 相对运动时为-1
			float avoidanceResponse = (1.0f - avoidanceCosAngle * (avoideeMobile ? 1 : 0)) + 0.1f;       // 此时为2.1
			float avoidanceFallOff = (1.0f - Mathf.Min(1.0f, avoideeDist / (5.0f * avoidanceRadiusSum)));

			if (avoidanceCosAngle < 0.0f)
            {
				avoiderTurnSign = Mathf.Max(avoiderTurnSign, avoideeTurnSign);
			}

			avoidanceDir = avoiderRight * AVOIDER_DIR_WEIGHT * avoiderTurnSign;
			avoidanceVec += (avoidanceDir * avoidanceResponse * avoidanceFallOff * avoideeMassScale);
		}

		avoidanceDir = (Vector3.Lerp(desiredDir, avoidanceVec, DESIRED_DIR_WEIGHT)).normalized;
		avoidanceDir = (Vector3.Lerp(avoidanceDir, lastAvoidanceDir, LAST_DIR_MIX_ALPHA)).normalized;

		return (lastAvoidanceDir = avoidanceDir);
	}

	private bool TestMoveSquare(Vector3 nextPos)
    {
		if (Common.IsIllegalPos(nextPos))
        {
			return false;
        }
		return true;
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

		if (Common.SqDistance2D(currWayPoint, pos) > maxSpeed)
        {
			return false;
        }

		atEndOfPath = Common.SqDistance2D(currWayPoint, goalPos) <= goalRadius * goalRadius;
		if (atEndOfPath)
		{
			currWayPoint = goalPos;
			nextWayPoint = goalPos;
			atGoal = true;
			return false;
		}

		return true;
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
