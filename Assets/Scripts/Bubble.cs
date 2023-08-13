using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Bubble : MonoBehaviour
{
    [SerializeField] private List<Color> colors = new List<Color>
    {
        Color.red,
        Color.blue,
        Color.green
    };

    [SerializeField] private BubbleColor bubbleColor = BubbleColor.Red;
    [SerializeField] private float speed = 20.0f;

    public enum BubbleColor
    {
        Red,
        Blue,
        Green,
    }

    public Vector2Int gridCoordinate;
    public Rigidbody2D rb;
    public CircleCollider2D circleCollider;
    public TrailRenderer trail;
    public SpriteRenderer spriteRenderer;
    public bool currentBubble = false;
    public bool thrown = false;

    private BubbleGrid _parentGrid;
    private Vector2 _velocity;
    private Camera _mainCamera;
    private float _colliderRadius;
    private float _screenHalfWidth;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        trail = GetComponent<TrailRenderer>();
        circleCollider = GetComponent<CircleCollider2D>();
        _colliderRadius = circleCollider.radius * transform.localScale.x;

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
        // If the bubble isn't the currently thrown one, skip
        if (!currentBubble)
        {
            return;
        }

        currentBubble = false;
        // Set the new bubble's parent as the bubble grid and call the function for handling the new bubble
        transform.SetParent(_parentGrid.transform);
        _parentGrid.HandleNewBubble(this, other.transform.GetComponent<Bubble>());
    }

    private void Update()
    {
        if (currentBubble)
        {
            // Check for player's click
            if (Input.GetMouseButtonDown(0) && !thrown)
            {
                ShootBubble();
            }

            // Flip the bubble's x velocity when it touches a wall
            if (transform.position.x <= -_screenHalfWidth + _colliderRadius)
            {
                transform.position = new Vector3(-_screenHalfWidth + _colliderRadius, transform.position.y,
                    transform.position.z);
                _velocity.x = math.abs(_velocity.x);
            }

            if (transform.position.x >= _screenHalfWidth - _colliderRadius)
            {
                transform.position = new Vector3(_screenHalfWidth - _colliderRadius, transform.position.y,
                    transform.position.z);
                _velocity.x = -math.abs(_velocity.x);
            }

            rb.velocity = _velocity;

            // If the bubble goes beyond the first row of the grid, place it there
            if (transform.position.y > _parentGrid.transform.position.y)
            {
                _parentGrid.HandleNewBubble(this, null);
            }
        }
    }

    /**
     * Shoots the bubble in the direction of the mouse.
     */
    private void ShootBubble()
    {
        Vector3 direction = (_mainCamera.ScreenToWorldPoint(Input.mousePosition) - transform.position);
        direction.z = 0.0f;
        direction.y = Mathf.Abs(direction.y);
        direction = direction.normalized;
        _velocity = direction * speed;
        thrown = true;
        trail.enabled = true;
    }

    /**
     * Changes the sprite color in the editor when you change the bubbleColor enum.
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
     * Changes the color of the SpriteRenderer according to the given bubbleColor enum.
     */
    public Bubble SetBubbleColor(BubbleColor color)
    {
        bubbleColor = color;
        SetColors(colors[(int)color]);

        return this;
    }

    /**
     * Sets the sprite and trail's colors to the given color.
     */
    private void SetColors(Color color)
    {
        spriteRenderer.color = color;
        trail.startColor = color;
        trail.endColor = color;
    }

    /**
     * Sets a random color for the bubble.
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