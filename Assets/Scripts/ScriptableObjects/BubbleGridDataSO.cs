using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu()]
public class BubbleGridDataSO : ScriptableObject
{
    public List<List<Bubble.BubbleColor>> Grid;
}
