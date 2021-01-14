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
			//if (!enemies[i].GameUpdate())
			//{
			//	int lastIndex = enemies.Count - 1;
			//	enemies[i] = enemies[lastIndex];
			//	enemies.RemoveAt(lastIndex);
			//	i -= 1;
			//}
		}
	}
}