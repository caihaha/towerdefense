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

	MoveAgent moveAgent;
	public MoveAgent Move => moveAgent;

    #endregion

    #region 初始化
    public void SpawnOn(GameTile tile)
	{
		transform.localRotation = Quaternion.identity;
		transform.localPosition = tile.transform.localPosition;
		moveAgent = new MoveAgent(this, tile);
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
		moveAgent.GoalTile = tile;
    }
    #endregion
}