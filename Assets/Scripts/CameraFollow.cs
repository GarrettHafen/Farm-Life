using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private Camera myCamera;
    private Func<Vector3> GetCameraFollowPositionFunc;
    private Func<float> GetCameraZoomFunc;




    private void Start()
    {
        myCamera = transform.GetComponent<Camera>();
    }

    public void Setup(Func<Vector3> GetCameraFollowPositionFunc, Func<float> GetCameraZoomFunc)
    {
        this.GetCameraFollowPositionFunc = GetCameraFollowPositionFunc;
        this.GetCameraZoomFunc = GetCameraZoomFunc;
    }

    public void SetGetCameraFollowPositionFunc(Func<Vector3> GetCameraFollowPositionFunc)
    {
        {
            this.GetCameraFollowPositionFunc = GetCameraFollowPositionFunc;
        }
    }

    // Update is called once per frame
    void Update()
    {
        HandleMovement();
        HandleZoom();
        
    }

    private void HandleMovement()
    {
        Vector3 cameraFollowPosition = GetCameraFollowPositionFunc();
        cameraFollowPosition.z = transform.position.z;

        Vector3 cameraMovDir = (cameraFollowPosition - transform.position).normalized;
        float distance = Vector3.Distance(cameraFollowPosition, transform.position);
        float cameraMoveSpeed = 5f;

        if (distance > 0)
        {
            Vector3 newCameraPosition = transform.position + cameraMovDir * cameraMoveSpeed * Time.deltaTime;

            float distanceAfterMoving = Vector3.Distance(newCameraPosition, cameraFollowPosition);

            if (distanceAfterMoving > distance)
            {
                //overshot the target
                newCameraPosition = cameraFollowPosition;
            }
            transform.position = newCameraPosition;
        }
    }

    private void HandleZoom()
    {
        float cameraZoom = GetCameraZoomFunc();

        float cameraZoomDifference = cameraZoom - myCamera.orthographicSize;
        float cameraZoomSpeed = 2f;

        myCamera.orthographicSize += cameraZoomDifference * cameraZoomSpeed * Time.deltaTime;

        if(cameraZoomDifference > 0)
        {
            if(myCamera.orthographicSize > cameraZoom)
            {
                myCamera.orthographicSize = cameraZoom;
            }
        }
        else
        {
            if (myCamera.orthographicSize < cameraZoom)
            {
                myCamera.orthographicSize = cameraZoom;
            }
        }
    }

}
