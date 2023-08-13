using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;

public class Player : MonoBehaviour
{
    [SerializeField] private GameObject bubblePrefab;
    [SerializeField] private BubbleGrid bubbleGrid;

    private Transform _shootingPosition;
    private Bubble _currentBubble;

    private void Awake()
    {
        _shootingPosition = transform.Find("ShootingPosition");
    }

    private void Start()
    {
        bubbleGrid.BubblePlaced += BubbleGridOnBubblePlaced;
        
        ReadyNewBubble();
    }

    private void BubbleGridOnBubblePlaced(object sender, EventArgs e)
    {
        ReadyNewBubble();
    }

    /**
     * Creates a new bubble for player to shoot.
     */
    private void ReadyNewBubble()
    {
        _currentBubble = Instantiate(
            bubblePrefab,
            _shootingPosition.position,
            quaternion.identity).GetComponent<Bubble>();
        _currentBubble.currentBubble = true;
        _currentBubble.SetRandomColor();
    }
}