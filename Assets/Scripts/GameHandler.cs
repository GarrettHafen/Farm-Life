using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameHandler : MonoBehaviour
{
    public static GameHandler instance;

    public CameraFollow cameraFollow;
    public Transform playerTransform;
    public Transform manualMovementTransform;

    private Vector3 cameraFollowPostion;

    private float zoom = 1.25f;

    private bool edgeScrolling;

    public bool overMenu = false;

    public Tool PlowTool;

    //plow pointer will be a hoe, default will be a gloved hand, harvest will be a scythe
    //the planting pointer will be dependant on which seed is selected
    public Texture2D plowPointer, defaultPointer, harvestPointer, plantingPointer;
    //might not need all of these

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        cameraFollowPostion = new Vector3(cameraFollowPostion.x + 4.5f, cameraFollowPostion.y + .5f, cameraFollowPostion.z);
        cameraFollow.Setup(() => cameraFollowPostion, () => zoom);

    }

    // Update is called once per frame
    void Update()
    {
        float moveAmount = 5f;
        float edgeSize = 10f;

        HandleManualMovement(moveAmount);
        HandleScreenEdges(edgeSize, moveAmount);
        HandleZoom();

    }

    private void HandleZoom()
    {
        float zoomChangeAmount = .25f;
        if (Input.GetKey(KeyCode.KeypadPlus))
        {
            zoom -= zoomChangeAmount;
        }
        if (Input.GetKey(KeyCode.KeypadMinus))
        {
            zoom += zoomChangeAmount;
        }

        if(Input.mouseScrollDelta.y > 0){
            zoom -= zoomChangeAmount;
        }
        if(Input.mouseScrollDelta.y < 0)
        {
            zoom += zoomChangeAmount;
        }
        zoom = Mathf.Clamp(zoom, .5f, 5f);
    }

    private void HandleScreenEdges(float edgeSize, float moveAmount)
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            edgeScrolling = !edgeScrolling;
            Debug.Log("Edge Scroll enabled: " + edgeScrolling);
        }
        if (edgeScrolling)
        {
            if (Input.mousePosition.x > Screen.width - edgeSize)
            {
                cameraFollowPostion.x += moveAmount * Time.deltaTime;
            }
            if (Input.mousePosition.x < edgeSize)
            {
                cameraFollowPostion.x -= moveAmount * Time.deltaTime;
            }
            if (Input.mousePosition.y > Screen.height - edgeSize)
            {
                cameraFollowPostion.y += moveAmount * Time.deltaTime;
            }
            if (Input.mousePosition.y < edgeSize)
            {
                cameraFollowPostion.y -= moveAmount * Time.deltaTime;
            }
        }
    }

    private void HandleManualMovement(float moveAmount)
    {
        if (Input.GetKey(KeyCode.W))
        {
            if (!CameraController.instance.atTopEdge) { 
            cameraFollowPostion.y += moveAmount * Time.deltaTime;
            }
        }
        if (Input.GetKey(KeyCode.S))
        {
            if (!CameraController.instance.atBottomEdge)
            {
                cameraFollowPostion.y -= moveAmount * Time.deltaTime;
            }
        }
        if (Input.GetKey(KeyCode.D))
        {
            if (!CameraController.instance.atRightEdge)
            {
                cameraFollowPostion.x += moveAmount * Time.deltaTime;
            }
        }
        if (Input.GetKey(KeyCode.A))
        {
            if (!CameraController.instance.atLeftEdge)
            {
                cameraFollowPostion.x -= moveAmount * Time.deltaTime;
            }
        }
    }

    // for zoom buttons (obsolete)
    public void ZoomIn()
    {
        zoom -= 1f;
        if (zoom < .5f)
        {
            zoom = .5f;
        }
    }

    public void ZoomOut()
    {
        zoom += 1f;
        if (zoom > 5f)
        {
            zoom = 5f;
        }
    }

    //disable and enable gameplay if mouse is hovered over menu
    public void DisableTool()
    {
        overMenu = true;
    }
    public void EnableTool()
    {
        overMenu = false;
    }

    

    
}


