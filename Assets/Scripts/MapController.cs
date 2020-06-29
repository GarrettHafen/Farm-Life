using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapController : MonoBehaviour
{
    public static MapController instance;
    public bool overMap = false;

    private void Start()
    {
        instance = this;
    }

    private void OnMouseOver()
    {
        overMap = true;
    }
    private void OnMouseExit()
    {
        overMap = false;
    }
}
