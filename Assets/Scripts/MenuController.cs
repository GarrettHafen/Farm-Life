using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEditor.Experimental.UIElements.GraphView;
using System.Runtime.InteropServices.WindowsRuntime;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;
    public GameObject menuToAnimate;
    public GameObject notificationBar;
    public Text notifcationText;
    //public Texture2D plow;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;
    public Color errorRed = new Color(255, 255, 255); 

    public SpriteRenderer overlay;

    public GameObject settingsMenu;
    public GameObject saveOrQuitPanel;

    //variables to handle preview
    private Transform mouseyThingy = null;
    private SpriteRenderer mouseyThingySprite;
    public bool previewObstructed = false;
    public bool previewActive;
    private Grid grid;
    public GameObject preview;
    public GameObject placeablePreview;
    public float xOffset, yOffset;

    public Tool plow;
    public bool plowActive = false;
    public bool hasSeed = false;

    public bool fireTool = false;
    public Sprite fireToolSprite;

    public Image handIndicator;
    public GameObject handIndicatorParent;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        //ResetTool();
        Cursor.SetCursor(GameHandler.instance.defaultPointer, hotSpot, cursorMode);

        grid = Grid.FindObjectOfType<Grid>();
       

    }

    // Update is called once per frame
    void Update()
    {
        if (mouseyThingy != null)
        {
            if (mouseyThingy.gameObject.activeInHierarchy)
            {
                //using grid to provide for isometric design 
                Vector3 screenToWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5));
                Vector3Int worldToCell = grid.WorldToCell(screenToWorld);
                mouseyThingy.position = new Vector3(grid.GetCellCenterWorld(worldToCell).x + xOffset, grid.GetCellCenterWorld(worldToCell).y + yOffset, 9f);
                //placeablePreview for when we need to place animals, trees or decorations. 
            }
        }

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
            FindObjectOfType<AudioManager>().PlaySound("Click");
        }
    }

    public void AnimateNotifcation(string notifyText, Color color, string sound)
    {
        notifcationText.text = notifyText;
        notifcationText.color = color;
        notificationBar.SetActive(true);
        FindObjectOfType<AudioManager>().PlaySound(sound);

    }

    public void SetCursor(Texture2D cursorToBe)
    {
        Cursor.SetCursor(cursorToBe, hotSpot, cursorMode);
    }

    /*
    public void ResetTool()
    {
        Cursor.SetCursor(GameHandler.instance.defaultPointer, hotSpot, cursorMode);
        PlayerInteraction.instance.SetTool(null);
        MarketController.instance.hasSeed = false;
    }

    
    public void SetTool(Tool t)
    {
        if (PlayerInteraction.instance.GetTool() == t)
        {
            ResetTool();
            FindObjectOfType<AudioManager>().PlaySound("Click");
        }
        else
        {
            PlayerInteraction.instance.SetTool(t);
            FindObjectOfType<AudioManager>().PlaySound("Click");
        }
    }
   
    public void SetSeed(SeedBarrel c)//not used?
    {
        Crop tempCrop = c.crop;
        PlayerInteraction.instance.SetCrop(new Crop(tempCrop.asset));
        FindObjectOfType<AudioManager>().PlaySound("Seed");
    }
    */

    public void OpenSettings()
    {
        settingsMenu.SetActive(true);
        FindObjectOfType<AudioManager>().PlaySound("Click");
    }
    public void CloseSettings()
    {
        settingsMenu.SetActive(false);
        saveOrQuitPanel.SetActive(false);
        FindObjectOfType<AudioManager>().PlaySound("Click");
    }
    public void SaveGame()
    {
        SaveSystem.SavePlayer();
        Debug.Log("Save Complete");
        notificationBar.SetActive(false);
        AnimateNotifcation("Save Complete", Color.white, "Manual Save");
    }
    public void LoadGame()
    {
        FindObjectOfType<AudioManager>().PlaySound("Click");
        GameHandler.instance.LoadData(SaveSystem.LoadPlayer());
    }

    public void SaveOrExitGame()
    {
        //save and quit or exit without saving
        CloseSettings();
        saveOrQuitPanel.SetActive(true);
        FindObjectOfType<AudioManager>().PlaySound("Click");
    }
    public void QuitGame()
    {
        FindObjectOfType<AudioManager>().PlaySound("Click");
        //actually quit the game
        Application.Quit();
    }
    public void SaveAndExit()
    {
        FindObjectOfType<AudioManager>().PlaySound("Click");
        StartCoroutine(ExecuteAfterTime(1));
    }

    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        QuitGame();
    }

    public void MuteAudio()
    {
        FindObjectOfType<AudioManager>().PlaySound("Click");
        GameMaster.instance.MuteAudio();
        //mute sounds
    }

    public void ActivatePreview()
    {
        
        mouseyThingy = ((GameObject)preview).transform;
        mouseyThingy.gameObject.SetActive(true);
        preview.SetActive(true);
        previewActive = true;
    }
    public void DestroyPreview()
    {
        if (mouseyThingy)
        {
            mouseyThingy.gameObject.SetActive(false);
            previewActive = false;
            preview.SetActive(false);
        }
    }

    public bool GetMouseyThingy()
    {
        if (mouseyThingy != null)
        {
            if (mouseyThingy.gameObject.activeInHierarchy)
            {
                return true;
            }
            else return false;
        }
        else
        {
            return false;
        }
    }
    public Vector3 GetMouseyThingyPosition()
    {
        return mouseyThingy.position;
    }
    public void SetPreviewColor(Sprite color)
    {
        mouseyThingySprite = preview.GetComponent<SpriteRenderer>();

        mouseyThingySprite.sprite = color;
    }

    public void ActivatePlow()
    {
        plowActive = !plowActive;
    }
    public void DeactivatePlow()
    {
        plowActive = false;

    }
    public void DisplayInventory()
    {
        handIndicatorParent.SetActive(true);

        if (MenuController.instance.plowActive)
        {
            handIndicator.sprite = MenuController.instance.plow.sprite;
        }
        else if (hasSeed)
        {
            handIndicator.sprite = PlayerInteraction.instance.GetCrop().asset.iconSprite;
        }
        else if (fireTool)
        {
            handIndicator.sprite = fireToolSprite;
        }
    }

    public void ClearHand()
    {
        handIndicatorParent.SetActive(false);
        hasSeed = false;
        plowActive = false;
        fireTool = false;
    }

    public void SetFireTool()
    {
        fireTool = !fireTool;
    }

}
