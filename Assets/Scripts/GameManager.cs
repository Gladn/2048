using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;


public class GameManager : MonoBehaviour
{
    [SerializeField] private int gameWidth = 4;
    [SerializeField] private int gameHeight = 4;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private SpriteRenderer boardPrefab;
    [SerializeField] private List<BlockType> blockTypes;

    private List<Node> nodes;
    private List<Block> blocks;
    private GameStats stats;
    private int gameRound = 0;

    private BlockType GetBlockTypeValue(int value) => blockTypes.First(t => t.value == value);


    // Start is called before the first frame update
    void Start()
    {
        ChangeStats(GameStats.GenerateLevel);
    }

    private void ChangeStats(GameStats newStats)
    {
        stats = newStats;

        switch (newStats)
        {
            case GameStats.GenerateLevel:
                GenerateGrid();
                break;
            case GameStats.SpawningBlocks:
                SpawnBlocks(UnityEngine.Random.value > 0.3f || gameRound++ == 0 ? 2 : 1);
                break;
            case GameStats.WaitingInput:
                break;
            case GameStats.Moving:
                break;
            case GameStats.Win:
                break;
            case GameStats.Lose:
                break;           
            default:
                throw new ArgumentOutOfRangeException(nameof(newStats), newStats, null);


        }
    }

    void GenerateGrid() {
        nodes = new List<Node>();
        blocks = new List<Block>();
        for (int x = 0; x < gameWidth; x++)
        {
            for (int y = 0; y < gameHeight; y++)
            {
                var node = Instantiate(nodePrefab, new Vector2(x,y), Quaternion.identity);
                nodes.Add(node);
            }
        }

        var centerScreen = new Vector2((float) gameWidth/2 - 0.5f, (float)gameHeight / 2 - 0.5f);

        //var board = Instantiate(boardPrefab, Vector3.zero, Quaternion.identity);
        var board = Instantiate(boardPrefab, centerScreen, Quaternion.identity);
        board.size = new Vector2(gameWidth, gameHeight);

        Camera.main.transform.position = new Vector3(centerScreen.x, centerScreen.y, -10);

        //SpawnBlocks(2);
        ChangeStats(GameStats.SpawningBlocks);
    }

    void SpawnBlocks(int amount)
    {
        var freeNodes = nodes.Where(n => n.ocupiedBlock == null).OrderBy(b => UnityEngine.Random.value);

        foreach (var node in freeNodes.Take(amount))
        {
            var block = Instantiate(blockPrefab, node.Pos, Quaternion.identity);
            block.Init(GetBlockTypeValue(UnityEngine.Random.value > 0.9f ? 4 : 2)); ;
        }

        //for (int i = 0; i<amount; i++)
        //{
        //    var block = Instantiate(blockPrefab);
        //}

        if (freeNodes.Count() == 1) //game lost
        {
            return;
        }
    }

}

[Serializable]
public struct BlockType
{
    public int value;
    public Color Color;
}

public enum GameStats
{
    GenerateLevel,
    SpawningBlocks,
    WaitingInput,
    Moving,
    Win,
    Lose
}