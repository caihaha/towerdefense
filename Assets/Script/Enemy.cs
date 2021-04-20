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

	int id;
	float deltaTime;
	public UnitDef unitDef;
	MoveAgent moveAgent;
	public MoveAgent UnitMove => moveAgent;
	public int ID => id;
    #endregion

    #region 初始化
    public void SpawnOn(GameTile tile, int id)
	{
		this.id = id;
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
		if (deltaTime > 1)
		{
			moveAgent.GameUpdate();
			deltaTime -= 1;
		}
		
		transform.localPosition = Vector3.Lerp(transform.localPosition, moveAgent.Pos, deltaTime);
		transform.localPosition = new Vector3(transform.localPosition.x, 0, transform.localPosition.z);
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