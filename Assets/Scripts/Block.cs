using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Block : MonoBehaviour
{
    public int Value;
    [SerializeField] private SpriteRenderer blockRenderer;
    [SerializeField] private TextMeshPro blocktexts;

    public void Init(BlockType type)
    {
        Value = type.value;
        blockRenderer.color = type.Color;
        blocktexts.text = type.value.ToString();
    }
}
