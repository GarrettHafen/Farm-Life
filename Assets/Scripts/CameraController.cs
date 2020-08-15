using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    public static CameraController instance;
    public Tilemap theMap;
    private Vector3 bottomLeftLimit;
    private Vector3 topRightLimit;
    public bool atTopEdge;
    public bool atRightEdge;
    public bool atLeftEdge;
    public bool atBottomEdge;

    private float halfHeight;
    private float halfWidth;

    public int musicToPlay;
    private bool musicStarted;

   

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        halfHeight = Camera.main.orthographicSize;
        halfWidth = halfHeight * Camera.main.aspect;
        bottomLeftLimit = theMap.localBounds.min + new Vector3(halfWidth, halfHeight, 0f);
        topRightLimit = theMap.localBounds.max + new Vector3(-halfWidth, -halfHeight, 0f);
        atTopEdge = false; atRightEdge = false; atLeftEdge = false; atBottomEdge = false;

    }

    // LateUpdate is called once per frame, after Update
    void LateUpdate()
    {
        //pass map size when increasing map
        CheckMapBounds();

    }

    public void CheckMapBounds()
    {
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, bottomLeftLimit.x, topRightLimit.x), Mathf.Clamp(transform.position.y, bottomLeftLimit.y, topRightLimit.y), transform.position.z);
        if (Camera.main.transform.position.y == topRightLimit.y) atTopEdge = true; else atTopEdge = false;
        
        if (Camera.main.transform.position.x == topRightLimit.x) atRightEdge = true; else atRightEdge = false;
        
        if (Camera.main.transform.position.x == bottomLeftLimit.x) atLeftEdge = true; else atLeftEdge = false;
        
        if (Camera.main.transform.position.y == bottomLeftLimit.y) atBottomEdge = true; else atBottomEdge = false;
        
    }
}
