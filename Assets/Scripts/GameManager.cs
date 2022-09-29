using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;


public class GameManager : MonoBehaviour
{
    [SerializeField] private int gameWidth = 4;
    [SerializeField] private int gameHeight = 4;
    [SerializeField] private Node nodePrefab;
    [SerializeField] private Block blockPrefab;
    [SerializeField] private SpriteRenderer boardPrefab;
    [SerializeField] private List<BlockType> blockTypes;
    [SerializeField] private float travel = 0.2f;
    [SerializeField] private int winCondition = 2048;

    [SerializeField] private GameObject winScreen;
    [SerializeField] private GameObject loseScreen;

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

    void Update()
    {
        if (stats != GameStats.WaitingInput) return;
        if (Input.GetKeyDown(KeyCode.LeftArrow)) Shift(Vector2.left);
        if (Input.GetKeyDown(KeyCode.RightArrow)) Shift(Vector2.right);
        if (Input.GetKeyDown(KeyCode.UpArrow)) Shift(Vector2.up);
        if (Input.GetKeyDown(KeyCode.DownArrow)) Shift(Vector2.down);

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
                //SpawnBlocks(UnityEngine.Random.value > 0.8f || gameRound++ == 0 ? 2 : 1);
                SpawnBlocks(gameRound++ == 0 ? 2 : 1);
                break;
            case GameStats.WaitingInput:
                break;
            case GameStats.Moving:
                break;
            case GameStats.Win:
                winScreen.SetActive(true);
                break;
            case GameStats.Lose:
                loseScreen.SetActive(true);
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
            SpawnBlocks(node, UnityEngine.Random.value > 0.9f ? 4 : 2);
        }

        //for (int i = 0; i<amount; i++)
        //{
        //    var block = Instantiate(blockPrefab);
        //}

        if (freeNodes.Count() == 0) 
        {
            ChangeStats(GameStats.Lose);
            return;
        }

        ChangeStats(blocks.Any(b => b.Value == winCondition) ? GameStats.Win : GameStats.WaitingInput);
    }

    void SpawnBlocks(Node node, int value)
    {
        var block = Instantiate(blockPrefab, node.Pos, Quaternion.identity);
        block.Init(GetBlockTypeValue(value));
        block.setBlock(node);
        blocks.Add(block);
    }

    void Shift(Vector2 dir)
    {
        ChangeStats(GameStats.Moving);
        var orderedBlocks = blocks.OrderBy(b => b.Pos.x).ThenBy(b => b.Pos.y).ToList();
        if (dir == Vector2.right || dir == Vector2.up) orderedBlocks.Reverse();

        foreach (var block in orderedBlocks)
        {
            var next = block.Node;
            do
            {
                block.setBlock(next);
                var possibleNode = GetNodePosition(next.Pos + dir);
                if (possibleNode != null)
                {
                    // node is present
                    if (possibleNode.ocupiedBlock != null && possibleNode.ocupiedBlock.CanMerge(block.Value))
                    {
                        //block.MergingBlock = possibleNode.ocupiedBlock;
                        block.MergeBlock(possibleNode.ocupiedBlock);
                    }
                    else if (possibleNode.ocupiedBlock == null) next = possibleNode;
                }               
            } while (next != block.Node);

            //block.transform.position = block.Node.Pos;
        }

        var sequence = DOTween.Sequence();

        foreach (var block in orderedBlocks)
        {
            var movePoint = block.MergingBlock != null ? block.MergingBlock.Node.Pos : block.Node.Pos;

            sequence.Insert(0, block.transform.DOMove(movePoint, travel).SetEase(Ease.InQuad));
        }

        sequence.OnComplete(() =>
        {
            foreach (var block in orderedBlocks.Where(b => b.MergingBlock != null))
            {
                MergeBlocks(block.MergingBlock, block);
            }
        });

        ChangeStats(GameStats.SpawningBlocks);
    }


    void MergeBlocks(Block baseBlock, Block mergingBlock)
    {
        SpawnBlocks(baseBlock.Node, baseBlock.Value * 2);
        RemoveBlock(mergingBlock);
        RemoveBlock(baseBlock);
    }

    void RemoveBlock(Block block)
    {
        blocks.Remove(block);
        Destroy(block.gameObject);
    }
    
    
    Node GetNodePosition(Vector2 pos)
    {
        return nodes.FirstOrDefault(n => n.Pos == pos);
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