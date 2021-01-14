using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Game : MonoBehaviour
{
    #region 数据成员
    [SerializeField]
    Vector2Int boardSize = Common.BoardSize;

    [SerializeField]
    GameBoard board = default;

    [SerializeField]
    GameTileContentFactory tileContentFactory = default;

    [SerializeField]
    EnemyFactory enemyFactory = default;

    [SerializeField, Range(0.1f, 10f)]
    EnemyCollection enemies = new EnemyCollection();
    #endregion

    #region 内部引用
    public void Awake()
    {
        board.Initialize(boardSize, tileContentFactory);
        board.ShowGrid = true;
        SpawnEnemy(board.GetTileByIdx(0));
    }

    void OnValidate()
    {
        if(boardSize.x < 2)
        {
            boardSize.x = 2;
        }
        if(boardSize.y < 2)
        {
            boardSize.y = 2;
        }
    }

    Ray TouchRay => Camera.main.ScreenPointToRay(Input.mousePosition);

    void Update()
    {
        if(Input.GetMouseButtonDown(0))
        {
            HandleTouch();
        }
        else if (Input.GetMouseButtonDown(1))
        {
            HandleAlternativeTouch();
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            board.ShowPaths = !board.ShowPaths;
        }

        if (Input.GetKeyDown(KeyCode.G))
        {
            board.ShowGrid = !board.ShowGrid;
        }

            enemies.GameUpdate();
    }
    #endregion

    #region 处理点击
    void HandleTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if(tile != null)
        {
            board.ToggleWall(tile);
        }
    }

    void HandleAlternativeTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if (tile != null)
        {
            // 目标点
            board.ToggleDestination(tile);
        }
    }

    // 产生Enemy
    void SpawnEnemy(GameTile tile)
    {
        board.NowPoint = tile;
        board.SetGameTileContentType(tile, GameTileContentType.SpawnPoint);
        Enemy enemy = enemyFactory.Get();
        
        enemy.SpawnOn(tile);
        enemies.Add(enemy);

        board.PathFinder();
    }
    #endregion
}
