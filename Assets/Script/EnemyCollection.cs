using System.Collections.Generic;

[System.Serializable]
public class EnemyCollection
{
	List<Enemy> enemies = new List<Enemy>();

	// 添加敌人
	public void Add(Enemy enemy)
	{
		enemies.Add(enemy);
	}

	// 更新整个集合
	public void GameUpdate()
	{
		for (int i = 0; i < enemies.Count; i++)
		{
			enemies[i].GameUpdate();
		}
	}

	public void SetDestination(GameTile tile)
    {
        for (int i = 0; i < enemies.Count; ++i)
        { 
			enemies[i].GoalPoint = tile;
		}
	}

	public void PathFinder()
    {
		for (int i = 0; i < enemies.Count; i++)
		{
			enemies[i].PathFinder();
		}
	}
}