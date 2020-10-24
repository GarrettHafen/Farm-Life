using System.Collections.Generic;
using UnityEngine;

public class PreviewCollisionController : MonoBehaviour
{
    private List<GameObject> listOfCollisions = new List<GameObject>();
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.name != "Grid" && collision.name != "OverlaySprite")
        {
            //MenuController.instance.previewObstructed = true;
            //Debug.Log("collision: " + collision.name);
            listOfCollisions.Add(collision.gameObject);



            /*
             * second attempt
             * onTriggerEnter add object to the list
             * onTriggerExit remove object from the list,
             * if list is empty, previewObsturcted = false
             */
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        //MenuController.instance.previewObstructed = false;
        listOfCollisions.Remove(collision.gameObject);

    }

    private void Update()
    {
        //Debug.Log(listOfCollisions.Count);
        if (listOfCollisions.Count == 0)
        {
            MenuController.instance.previewObstructed = false;
        }
        else
        {
            MenuController.instance.previewObstructed = true;
            //Debug.Log("Collision in list: " + listOfCollisions[0].name);
        }
    }
}
