using UnityEngine;
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
	public MoveAgent UnitMove => moveAgent;
    #endregion

    #region 初始化
    public void SpawnOn(GameTile tile)
	{
		transform.localRotation = Quaternion.identity;
		transform.localPosition = tile.transform.localPosition;

		unitDef = new UnitDef();
		moveAgent = new MoveAgent(this, tile, unitDef);
	}
	#endregion

	#region 对外接口
	public bool GameUpdate()
    {
		moveAgent.GameUpdate();
		transform.localPosition = moveAgent.CurrWayPoint;
		transform.forward = moveAgent.FrontDir;
		
		return true;
    }

	public void GameSlowUpdate()
    {
		moveAgent.GameSlowUpdate();
    }

	public void StartMoving()
    {
		moveAgent.StartMoving();
	}

	public void SetGoalPos(GameTile tile)
    {
		moveAgent.SetGoalPos(tile.ExitPoint);
    }
    #endregion
}