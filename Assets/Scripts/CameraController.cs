using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float panSpeed = 20f;
    public float panBorderThickness = 10f;
    public Vector2 panLimit;
    public float scrollSpeed = 5f;
    public float minZ = -20f;
    public float maxZ = -1.5f;

    public float maxSize = 50f;
    public float minSize = 1f;

    private Vector3 dragOrigin;
    private bool isDragging;

    private void Update()
    {
        Vector3 pos = transform.position;

        if (Input.GetKey("w"))
            pos.y += panSpeed * Time.deltaTime;
        if (Input.GetKey("s"))
            pos.y -= panSpeed * Time.deltaTime;
        if (Input.GetKey("d"))
            pos.x += panSpeed * Time.deltaTime;
        if (Input.GetKey("a"))
            pos.x -= panSpeed * Time.deltaTime;

        if (Input.GetMouseButtonDown(2))
        {
            dragOrigin = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            isDragging = true;
        }

        if (Input.GetMouseButtonUp(2))
            isDragging = false;

        if (isDragging)
        {
            Vector3 delta = dragOrigin - Camera.main.ScreenToWorldPoint(Input.mousePosition);
            pos += delta;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - scroll * scrollSpeed * 100f * Time.deltaTime, minSize, maxSize);
        pos.x = Mathf.Clamp(pos.x, -panLimit.x, panLimit.x);
        pos.y = Mathf.Clamp(pos.y, -panLimit.y, panLimit.y);
        transform.position = pos;
    }
}
