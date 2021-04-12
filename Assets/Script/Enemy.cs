﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    #region 数据成员
    EnemyFactory originFactory;
	public EnemyFactory OriginFactory
	{
		get => originFactory;
		set
		{
			Debug.Assert(originFactory == null, "Redefined origin factory!");
			originFactory = value;
		}
	}
	
	UnitDef unitDef;
	MoveAgent moveAgent;
	float deltaTime;
	public MoveAgent UnitMove => moveAgent;
    #endregion

    #region 初始化
    public void SpawnOn(GameTile tile)
	{
		transform.localRotation = Quaternion.identity;
		transform.localPosition = tile.transform.localPosition;

		unitDef = new UnitDef();
		moveAgent = new MoveAgent(this, tile, unitDef);
		deltaTime = 0.0f;
	}
	#endregion

	#region 对外接口
	public bool GameUpdate()
    {
		deltaTime += Time.deltaTime;
		if (deltaTime < 1)
		{
			return false;
		}
		
		deltaTime -= 1;
		moveAgent.GameUpdate();
		transform.localPosition = moveAgent.CurrWayPoint;
		transform.forward = moveAgent.FrontDir;
		
		return true;
    }

	public void GameSlowUpdate()
    {
		moveAgent.GameSlowUpdate();
    }

	public void StartMoving(GameTile tile)
    {
		moveAgent.StartMoving(tile.ExitPoint, 0.01f);
	}

	public void SetGoalPos(GameTile tile)
    {
		moveAgent.SetGoalPos(tile.ExitPoint);
    }
    #endregion
}