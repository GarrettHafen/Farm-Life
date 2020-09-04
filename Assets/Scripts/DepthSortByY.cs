using System.Collections;
using System.Collections.Generic;
using UnityEngine;


[ExecuteInEditMode]
[RequireComponent(typeof(SpriteRenderer))]
public class DepthSortByY : MonoBehaviour
{
    private const int IsometricRanchPerYUnit = 100;

    // Update is called once per frame
    void Update()
    {
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        renderer.sortingOrder = -(int)(transform.position.y * IsometricRanchPerYUnit);
    }
}
