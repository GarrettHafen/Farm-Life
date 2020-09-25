using UnityEngine;

public class CollisionController : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name != "Grid" && collision.name != "OverlaySprite")
        {
            MenuController.instance.previewObstructed = true;
            Debug.Log("collision: " + collision.name);
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        MenuController.instance.previewObstructed = false;

    }
}
