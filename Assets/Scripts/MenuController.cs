using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;
    public GameObject menuToAnimate;
    public Texture2D plow;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        ResetCursor();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void AnimateMenu()
    {
        if (menuToAnimate.activeInHierarchy)
        {
            menuToAnimate.SetActive(false);
            
        }
        else
        {
            menuToAnimate.SetActive(true);
        }
    }

    public void ActivatePointer(int pointer)
    {
        switch (pointer)
        {
            case 1:
                if (TileSelector.instance.plowActive)
                {
                    ResetCursor();
                }
                else
                {
                    TileSelector.instance.plowActive = true;
                    //change cursor to plow
                    SetCursor(GameHandler.instance.plowPointer);
                    PlayerInteraction.instance.SetTool(GameHandler.instance.PlowTool);
                }
                break;
            case 2:
                //harvest
                break;
            case 3:
                //planting
                break;
        }
        
    }

    public void SetCursor(Texture2D cursorToBe)
    {
        Cursor.SetCursor(cursorToBe, hotSpot, cursorMode);
    }

    public void ResetCursor()
    {
        Cursor.SetCursor(GameHandler.instance.defaultPointer, hotSpot, cursorMode);
        TileSelector.instance.plowActive = false;
    }
}
