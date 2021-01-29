using System.Collections.Generic;

[System.Serializable]
public class EnemyCollection
{
	Dictionary<uint, Enemy> enemies = new Dictionary<uint, Enemy>();
	uint enemyID;

	Enemy selectedEnemy;

	public Dictionary<uint, Enemy> Enemys => enemies;
	// 添加敌人
	public void Add(Enemy enemy)
	{
		if(enemy != null)
        {
			enemies.Add(++enemyID, enemy);
		}
	}

	// 更新整个集合
	public void GameUpdate()
	{
		foreach(var enemy in enemies)
        {
			enemy.Value.GameUpdate();
		}
	}

	// 设置选中Enemy的目标
	public void SetDestination(GameTile tile)
    {
		if(selectedEnemy == null)
        {
			return;
        }

		selectedEnemy.GoalPoint = tile;
		selectedEnemy.StartMoving();
		//foreach (var enemy in enemies)
		//{
		//	enemy.Value.GoalPoint = tile;
		//}
	}

	public bool SelectedEnemy(GameTile tile)
    {
		return (selectedEnemy = GetEnemyBuTile(tile)) != null;
    }

	public bool IsEnemyInThisTile(GameTile tile)
    {
		return GetEnemyBuTile(tile) != null;
	}

	private Enemy GetEnemyBuTile(GameTile tile)
    {
		foreach (var enemy in enemies)
		{
			if (enemy.Value.NowPoint == tile)
			{
				return enemy.Value;
			}
		}

		return null;
	}
}