using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Bubble : MonoBehaviour
{
    public enum BubbleColor
    {
        Red,
        Blue,
        Green
    }

    [SerializeField] private BubbleColor bubbleColor = BubbleColor.Red;
    [SerializeField] private float speed = 30.0f;

    public Vector2Int gridCoordinate;
    public bool currentBubble = false;
    public bool thrown = false;

    private SpriteRenderer _spriteRenderer;
    private BubbleGrid _parentGrid;
    private Vector2 _velocity;
    private float _colliderRadius;
    private float _screenHalfWidth;
    private Camera _mainCamera;
    private Rigidbody2D _rb;

    private void Awake()
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        SetBubbleColor(bubbleColor);
        _colliderRadius = GetComponent<CircleCollider2D>().radius;
        _rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        _mainCamera = Camera.main;
        _screenHalfWidth = _mainCamera.orthographicSize * _mainCamera.aspect;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!currentBubble)
        {
            return;
        }

        _parentGrid = GameObject.FindWithTag(nameof(BubbleGrid)).GetComponent<BubbleGrid>();
        transform.SetParent(_parentGrid.transform);
        _parentGrid.HandleNewBubble(this, other.transform.GetComponent<Bubble>());
    }

    private void Update()
    {
        if (currentBubble)
        {
            if (Input.GetMouseButtonDown(0) && !thrown)
            {
                Debug.Log("Bruh");
                ShootBubble();
            }

            if (transform.position.x <= -_screenHalfWidth + _colliderRadius ||
                transform.position.x >= _screenHalfWidth - _colliderRadius)
            {
                _velocity.x = -_velocity.x;
            }

            _rb.velocity = _velocity;
        }
    }

    /**
     a Shoots the bubble in the direction of the mouse.
     */
    public void ShootBubble()
    {
        Vector3 direction = (_mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized;
        _velocity = direction * speed;
        thrown = true;
    }

    /**
     * Change the sprite color in the editor when the bubbleColor enum changes.
     */
    private void OnValidate()
    {
        if (_spriteRenderer == null)
        {
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        SetBubbleColor(bubbleColor);
    }

    /**
     * Change the color of the SpriteRenderer according to the bubbleColor enum.
     */
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

    /**
     * Sets a random color.
     */
    public void SetRandomColor()
    {
        SetBubbleColor((BubbleColor)Random.Range(0, Enum.GetValues(typeof(Bubble.BubbleColor)).Length));
    }

    /**
     * Returns the bubbleColor.
     */
    public BubbleColor GetBubbleColor()
    {
        return bubbleColor;
    }
}