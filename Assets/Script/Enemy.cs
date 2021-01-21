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

	bool AtGoal => nowPoint == goalPoint;

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
		direction = currWayPoint.PathDirection;
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
			// currWayPoint当前位置
			if (nextWayPoint != null)
            {
				currWayPoint = nextWayPoint;
			}

			nextWayPoint = currWayPoint.NextTileOnPath;
			if (nextWayPoint != null && nextWayPoint.IsDiatance)
            {
				nextWayPoint.PathDirection = currWayPoint.PathDirection;
			}

			progress -= 1f;
			PrepareNextState();
		}

		// 更新位置
		transform.localPosition = Vector3.LerpUnclamped(positionFrom, positionTo, progress);

		// 调整方向
		if (directionChange != DirectionChange.None)
		{
			float angle = Mathf.LerpUnclamped(directionAngleFrom, directionAngleTo, progress);
			transform.localRotation = Quaternion.Euler(0f, angle, 0f);
		}

		if (currWayPoint != null)
			nowPoint = currWayPoint;

		return true;
	}
    #endregion

    #region 改变下一个状态
    void PrepareNextState()
	{
		positionFrom = positionTo;
		positionTo = currWayPoint.ExitPoint;
		directionChange = direction.GetDirectionChangeTo(currWayPoint.PathDirection);
		direction = currWayPoint.PathDirection;
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

    #region Zero-K
    #region 对外接口
	public void PathFinder()
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

		nextWayPoint = null;
		bool hasPath = pathManager.AStart(nowPoint, goalPoint);
		if (hasPath && GameBoard.Instance.ShowPaths)
		{
			foreach (GameTile tile in GameBoard.Instance.Tiles)
			{
				tile.ShowPath();
			}
		}
	}

    public bool Update()
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

		ReRequestPath();
	}
    #endregion

    #region 内部函数
    private void UpdateOwnerSpeedAndHeading()
    {
		FollowPath();
	}

    private void UpdateOwnerPos()
    {
        
    }

	private void HandleObjectCollisions()
    {

    }

    private bool FollowPath()
    {
		return true;
    }

	private void ReRequestPath()
    {
		StopEngine();
		StartEngine();
    }

	private void StopEngine()
    {
		if(pathID != 0)
        {
			pathManager.DeletePath(pathID);
			pathID = 0;
        }

		nextWayPoint = null;
    }

	private void StartEngine()
    {
		if (pathID == 0)
        {
			pathID = GetNewPath();
		}
	}

	private uint GetNewPath()
    {
		uint newPathID = 0;
		newPathID = pathManager.RequiredPath(this, nowPoint, goalPoint);


		return newPathID;
    }

	private void ChangeHeading()
    {

    }

	void GetnextWayPoint()
	{
		positionFrom = positionTo;
		positionTo = currWayPoint.ExitPoint;
		directionChange = direction.GetDirectionChangeTo(currWayPoint.PathDirection);
		direction = currWayPoint.PathDirection;
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
	#endregion

	#endregion
}