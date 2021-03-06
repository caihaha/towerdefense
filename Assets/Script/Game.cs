﻿using System.Collections;
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
    EnemyCollection enemies = Common.enemys;

    bool isSelectedEnemy;

    int activeSlowUpdateUnit;
    #endregion

    #region 内部引用
    public void Awake()
    {
        board.Initialize(boardSize, tileContentFactory);
        board.ShowGrid = true;
        // SpawnEnemy(board.GetTileByIdx(0));
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

        if (Input.GetKeyDown(KeyCode.G))
        {
            board.ShowGrid = !board.ShowGrid;
        }

        enemies.GameUpdate();

        ++activeSlowUpdateUnit;
        if (activeSlowUpdateUnit > Common.enemySlowUpdateRate)
        {
            activeSlowUpdateUnit = 0;
            enemies.GameSlowUpdate();
        }
    }
    #endregion

    #region 处理点击
    // 左键
    void HandleTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if(tile != null)
        {
            if(Input.GetKey(KeyCode.LeftShift))
            {
                board.ToggleWall(tile);
            }
            else
            {
                isSelectedEnemy = enemies.SelectedEnemy(tile);
            }
        }
    }

    // 右键
    void HandleAlternativeTouch()
    {
        GameTile tile = board.GetTile(TouchRay);
        if (tile != null)
        {
            if(Input.GetKey(KeyCode.LeftShift))
            {
                // 生成一个enemy
                if (tile.Content.Type != GameTileContentType.Empty ||
                    enemies.IsEnemyInThisTile(tile))
                {
                    return;
                }

                SpawnEnemy(tile);
            }
            else
            {
                // 设置目标点
                if (isSelectedEnemy && board.ToggleDestination(tile))
                {
                    enemies.SetDestination(tile);
                }
            }
        }
    }

    // 产生Enemy
    void SpawnEnemy(GameTile tile)
    {
        Enemy enemy = enemyFactory.Get();
        enemy.SpawnOn(tile);

        enemies.Add(enemy);
    }
    #endregion
}
