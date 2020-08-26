using System;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CameraController : MonoBehaviour
{
    //old code
    /*
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
    */

    //brackeys code perspective camera 
    public float panSpeed = 20f;
    public float panBorderThickness = 10f;
    public Vector2 panLimit;
    public float scrollSpeed = 5f;
    public float minZ = -20f;
    public float maxZ = -1.5f;

    //orthographic code
    public float maxSize = 3.5f;
    public float minSize = 1f;


    /* 
    void Start()
    {
        instance = this;
        halfHeight = Camera.main.orthographicSize;
        halfWidth = halfHeight * Camera.main.aspect;
        bottomLeftLimit = theMap.localBounds.min + new Vector3(halfWidth, halfHeight, 0f);
        topRightLimit = theMap.localBounds.max + new Vector3(-halfWidth, -halfHeight, 0f);
        atTopEdge = false; atRightEdge = false; atLeftEdge = false; atBottomEdge = false;
    }
    */

    private void Update()
    {
        /*
         * need to implement click and drag to move the mouse, maybe border stuff, but its kinda
         * wacky, and needs to limit if the mouse is on the screen??
         */
        Vector3 pos = transform.position;
        if (Input.GetKey("w") /*|| Input.mousePosition.y >= Screen.height - panBorderThickness*/)
        {
            pos.y += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("s") /*|| Input.mousePosition.y <= panBorderThickness*/)
        {
            pos.y -= panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("d") /*|| Input.mousePosition.y >= Screen.width - panBorderThickness*/)
        {
            pos.x += panSpeed * Time.deltaTime;
        }
        if (Input.GetKey("a") /*|| Input.mousePosition.y <= panBorderThickness*/)
        {
            pos.x -= panSpeed * Time.deltaTime;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        //pos.z -= scroll * scrollSpeed * 100f * Time.deltaTime;
        Camera.main.orthographicSize = Mathf.Clamp((Camera.main.orthographicSize + scroll * scrollSpeed * 100f * Time.deltaTime),minSize, maxSize);
        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.y = Mathf.Clamp(pos.y, -panLimit.y, panLimit.y);
        //pos.z = Mathf.Clamp(pos.z, minZ, maxZ);
        transform.position = pos;
    }

    /* old code
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

    private void Update()
    {
        
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
        
    }*/

}
