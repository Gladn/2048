using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int Value;
    public Node Node;
    public Block MergingBlock;
    public bool Merging;
    public Vector2 Pos => transform.position;
    [SerializeField] private SpriteRenderer blockRenderer;
    [SerializeField] private TextMeshPro blocktexts;

    public void Init(BlockType type)
    {
        Value = type.value;
        blockRenderer.color = type.Color;
        blocktexts.text = type.value.ToString();
    }

    public void setBlock (Node node)
    {
        if (Node != null) Node.ocupiedBlock = null;
        Node = node;
        Node.ocupiedBlock = this;
    }
    public void MergeBlock(Block BlockMergeWith)
    {
        MergingBlock = BlockMergeWith;
        Node.ocupiedBlock = null;
        BlockMergeWith.Merging = true;
    }

    public bool CanMerge(int value) => value == Value && !Merging && MergingBlock == null;
}
