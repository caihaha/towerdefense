using UnityEngine;

public class GameTileDefs
{
    
}

public static class Common
{
    private static Vector2Int boardSize = new Vector2Int(11, 11);
    public static Vector2Int BoardSize => boardSize;
    public static int BoardCount => boardSize.x * boardSize.y;

    public static EnemyCollection enemys = new EnemyCollection();
}

