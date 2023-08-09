using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bubble : MonoBehaviour
{
    public enum BubbleColor
    {
        Red,
        Blue,
        Green
    }

    public Vector2Int gridCoordinate;
    public bool throwingBubble = false;

    [SerializeField] private BubbleColor bubbleColor = BubbleColor.Red;

    private SpriteRenderer _spriteRenderer;
    private BubbleGrid _parentGrid;

    private void Awake()
    {
        _spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
        SetBubbleColor(bubbleColor);
    }

    private void Start()
    {
        _parentGrid = transform.parent.GetComponent<BubbleGrid>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!throwingBubble)
        {
            return;
        }

        _parentGrid.HandleBubble(this, other.transform.GetComponent<Bubble>());
    }

    private void Update()
    {
        if (throwingBubble)
        {
            transform.position = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            transform.position = new Vector3(transform.position.x, transform.position.y, 0.0f);
        }
    }

    // Change the sprite color in the editor when the bubbleColor enum changes
    private void OnValidate()
    {
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        SetBubbleColor(bubbleColor);
    }

    // Change the color of the SpriteRenderer according to the bubbleColor enum
    public Bubble SetBubbleColor(BubbleColor color)
    {
        bubbleColor = color;
        switch (bubbleColor)
        {
            case BubbleColor.Red:
                _spriteRenderer.color = Color.red;
                break;
            case BubbleColor.Green:
                _spriteRenderer.color = Color.green;
                break;
            case BubbleColor.Blue:
                _spriteRenderer.color = Color.blue;
                break;
        }

        return this;
    }

    public BubbleColor GetBubbleColor()
    {
        return bubbleColor;
    }
}