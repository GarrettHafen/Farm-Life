using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;
    public GameObject menuToAnimate;
    public Texture2D plow;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;

    public SpriteRenderer overlay;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        ResetTool();
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


    public void SetCursor(Texture2D cursorToBe)
    {
        Cursor.SetCursor(cursorToBe, hotSpot, cursorMode);
    }

    public void ResetTool()
    {
        Cursor.SetCursor(GameHandler.instance.defaultPointer, hotSpot, cursorMode);
        PlayerInteraction.instance.SetTool(null);
    }

    public void SetTool(Tool t)
    {
        if (PlayerInteraction.instance.GetTool() == t)
        {
            ResetTool();
        }
        else
        {
            PlayerInteraction.instance.SetTool(t);
        }
    }
    public void SetSeed(SeedBarrel c)
    {
        PlayerInteraction.instance.SetCrop(c.crop);
    }
}
