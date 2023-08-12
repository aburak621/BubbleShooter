using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
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
    [SerializeField] private float speed = 20.0f;

    public Vector2Int gridCoordinate;
    public bool currentBubble = false;
    public bool thrown = false;
    public Rigidbody2D rb;
    public CircleCollider2D circleCollider;
    public TrailRenderer trail;
    public SpriteRenderer spriteRenderer;

    private BubbleGrid _parentGrid;
    private Vector2 _velocity;
    private float _colliderRadius;
    private float _screenHalfWidth;
    private Camera _mainCamera;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        _colliderRadius = GetComponent<CircleCollider2D>().radius * transform.localScale.x;
        rb = GetComponent<Rigidbody2D>();
        circleCollider = GetComponent<CircleCollider2D>();
        trail = GetComponent<TrailRenderer>();
        
        SetBubbleColor(bubbleColor);
    }

    private void Start()
    {
        _parentGrid = GameObject.FindWithTag(nameof(BubbleGrid)).GetComponent<BubbleGrid>();
        _mainCamera = Camera.main;
        _screenHalfWidth = _mainCamera.orthographicSize * _mainCamera.aspect;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!currentBubble)
        {
            return;
        }

        transform.SetParent(_parentGrid.transform);
        _parentGrid.HandleNewBubble(this, other.transform.GetComponent<Bubble>());
    }

    private void Update()
    {
        if (currentBubble)
        {
            if (Input.GetMouseButtonDown(0) && !thrown)
            {
                ShootBubble();
            }

            if (transform.position.x <= -_screenHalfWidth + _colliderRadius)
            {
                _velocity.x = math.abs(_velocity.x);
            }
            if (transform.position.x >= _screenHalfWidth - _colliderRadius)
            {
                _velocity.x = -math.abs(_velocity.x);
            }

            rb.velocity = _velocity;

            if (transform.position.y > _parentGrid.transform.position.y)
            {
                _parentGrid.HandleNewBubble(this, null);
            }
        }
    }

    /**
     a Shoots the bubble in the direction of the mouse.
     */
    private void ShootBubble()
    {
        Vector3 direction = (_mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position);
        direction.z = 0.0f;
        direction = direction.normalized;
        _velocity = direction * speed;
        thrown = true;
        trail.enabled = true;
    }

    /**
     * Change the sprite color in the editor when the bubbleColor enum changes.
     */
    private void OnValidate()
    {
        if (spriteRenderer == null)
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
        }

        if (trail == null)
        {
            trail = GetComponent<TrailRenderer>();
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
                SetColors(Color.red);
                break;
            case BubbleColor.Green:
                SetColors(Color.green);
                break;
            case BubbleColor.Blue:
                SetColors(Color.blue);
                break;
        }

        return this;
    }

    private void SetColors(Color color)
    {
        spriteRenderer.color = color;
        trail.startColor = color;
        trail.endColor = color;
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