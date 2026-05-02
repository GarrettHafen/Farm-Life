using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
    public GameObject helpMenu;

    //variables to handle preview
    private Transform previewPlacementLocation = null;
    private SpriteRenderer previewPlacementLocationSprite;
    public bool previewObstructed = false;
    public bool previewActive;
    private Grid grid;
    public GameObject preview4x4;
    public GameObject preview1x1;
    public GameObject preview2x1;
    public GameObject previewParent;
    public GameObject placeablePreview;
    public float xOffset, yOffset;
    private Vector3 placementPosition;

    public Tool plow;
    public PlayerToolState toolState = new PlayerToolState();

    public Sprite fireToolSprite;

    public Image handIndicator;
    public GameObject handIndicatorParent;

    public GameObject fireMenu;

    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        //ResetTool();
        //Cursor.SetCursor(GameHandler.instance.defaultPointer, hotSpot, cursorMode);

        grid = FindFirstObjectByType<Grid>();
       

    }

    // Update is called once per frame
    void Update()
    {
        if (previewPlacementLocation != null)
        {
            if (previewPlacementLocation.gameObject.activeInHierarchy)
            {
                //using grid to provide for isometric design 
                Vector3 screenToWorld = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 5));
                Vector3Int worldToCell = grid.WorldToCell(screenToWorld);
                Vector3 cellCenter = grid.GetCellCenterWorld(worldToCell);
                placementPosition = new Vector3(cellCenter.x, cellCenter.y, 9f);
                previewPlacementLocation.position = new Vector3(cellCenter.x + xOffset, cellCenter.y + yOffset, 9f);
                
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
            AudioManager.instance.PlaySound("Click");
        }
    }

    public void AnimateNotifcation(string notifyText, Color color, string sound)
    {
        notifcationText.text = notifyText;
        notifcationText.color = color;
        notificationBar.SetActive(true);
        AudioManager.instance.PlaySound(sound);

    }

    public void SetCursor(Texture2D cursorToBe)
    {
        Cursor.SetCursor(cursorToBe, hotSpot, cursorMode);
    }

    public void OpenSettings()
    {
        settingsMenu.SetActive(true);
        AudioManager.instance.PlaySound("Click");
    }
    public void CloseSettings()
    {
        settingsMenu.SetActive(false);
        saveOrQuitPanel.SetActive(false);
        AudioManager.instance.PlaySound("Click");
    }
    public void SaveGame()
    {
        try
        {
            SaveSystem.SavePlayer();
            Debug.Log("Save Complete");
            notificationBar.SetActive(false);
            AnimateNotifcation("Save Complete", Color.white, "Manual Save");
        }
        catch (Exception e)
        {
            Debug.LogError("Save failed: " + e.Message);
            notificationBar.SetActive(false);
            AnimateNotifcation("Save Failed!", Color.red, "Error");
        }
    }
    public void LoadGame()
    {
        AudioManager.instance.PlaySound("Click");
        try
        {
            PlayerData data = SaveSystem.LoadPlayer();
            if (data == null)
            {
                notificationBar.SetActive(false);
                AnimateNotifcation("No save found", Color.red, "Error");
                return;
            }
            GameHandler.instance.LoadData(data);
        }
        catch (Exception e)
        {
            Debug.LogError("Load failed: " + e.Message);
            notificationBar.SetActive(false);
            AnimateNotifcation("Load Failed!", Color.red, "Error");
        }
    }

    public void SaveOrExitGame()
    {
        //save and quit or exit without saving
        CloseSettings();
        saveOrQuitPanel.SetActive(true);
        AudioManager.instance.PlaySound("Click");
    }
    public void QuitGame()
    {
        AudioManager.instance.PlaySound("Click");
        saveOrQuitPanel.SetActive(false);
        settingsMenu.SetActive(false);
        GameHandler.instance.landingPage.SetActive(true);
        GameHandler.instance.landingPageOpen = true;
    }
    public void SaveAndExit()
    {
        AudioManager.instance.PlaySound("Click");
        StartCoroutine(ExecuteAfterTime(1));
    }

    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        QuitGame();
    }

    public void MuteAudio()
    {
        AudioManager.instance.PlaySound("Click");
        GameMaster.instance.MuteAudio();
        //mute sounds
    }

    public void ActivatePreview(GameObject preview)
    {
        
        previewPlacementLocation = ((GameObject)preview).transform;
        previewPlacementLocation.gameObject.SetActive(true);
        preview.SetActive(true);
        previewActive = true;
        previewParent.SetActive(true);
    }
    public void DestroyPreview()
    {
        if (previewPlacementLocation)
        {
            previewPlacementLocation.gameObject.SetActive(false);
            previewActive = false;
            previewParent.SetActive(false);
        }
    }

    public bool GetpreviewPlacementLocation()
    {
        if (previewPlacementLocation != null)
        {
            if (previewPlacementLocation.gameObject.activeInHierarchy)
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
    public Vector3 GetpreviewPlacementLocationPosition()
    {
        return previewPlacementLocation.position;
    }

    public Vector3 GetPlacementPosition()
    {
        return placementPosition;
    }
    public void SetPreviewColor(Sprite color, GameObject preview)
    {
        previewPlacementLocationSprite = preview.GetComponent<SpriteRenderer>();

        previewPlacementLocationSprite.sprite = color;
    }

    public void ActivatePlow()
    {
        toolState.TogglePlow();
        previewObstructed = false;
    }
    public void DeactivatePlow()
    {
        if (toolState.plowActive) toolState.Clear();
    }
    public void DisplayInventory()
    {
        handIndicator.sprite = null;
        handIndicatorParent.SetActive(true);

        if (toolState.plowActive)
            handIndicator.sprite = plow.sprite;
        else if (toolState.hasSeed)
            handIndicator.sprite = PlayerInteraction.instance.GetCrop().asset.iconSprite;
        else if (toolState.fireTool)
            handIndicator.sprite = fireToolSprite;
        else if (toolState.hasTree)
            handIndicator.sprite = PlayerInteraction.instance.GetTree().asset.treeIconSprite;
        else if (toolState.hasAnimal)
            handIndicator.sprite = PlayerInteraction.instance.GetAnimal().asset.animalIconSprite;
    }

    public void ClearHand()
    {
        handIndicatorParent.SetActive(false);
        toolState.Clear();
    }

    public void SetFireTool()
    {
        toolState.ToggleFire();
    }

    public void OpenFireMenu()
    {
        fireMenu.gameObject.SetActive(true);
        PlayerInteraction.instance.SetTempTarget();
    }
    public void CloseFireMenu()
    {
        fireMenu.gameObject.SetActive(false);
        PlayerInteraction.instance.ClearTemptTarget();
    }

    public void ForceDeselect()
    {
        PlayerInteraction.instance.Deselect();
    }

    public void OpenHelpMenu()
    {
        //open the help menu, don't want it to close the settings, they can do that manually. 
        helpMenu.SetActive(true);
        AudioManager.instance.PlaySound("Click");
    }

    public void CloseHelpMenu()
    {
        helpMenu.SetActive(false);
        AudioManager.instance.PlaySound("Click");
    }
}
