using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuController : MonoBehaviour
{
    public static MenuController instance;
    public GameObject menuToAnimate;
    public GameObject notificationBar;
    public Text notifcationText;
    public Texture2D plow;
    public CursorMode cursorMode = CursorMode.Auto;
    public Vector2 hotSpot = Vector2.zero;
    public Color errorRed = new Color(255, 255, 255); 

    public SpriteRenderer overlay;

    public GameObject settingsMenu;
    public GameObject saveOrQuitPanel;


    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        //ResetTool();
        Cursor.SetCursor(GameHandler.instance.defaultPointer, hotSpot, cursorMode);
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

    public void AnimateNotifcation(string notifyText, Color color)
    {
        notifcationText.text = notifyText;
        notifcationText.color = color;
        notificationBar.SetActive(true);

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
            if (PlayerInteraction.instance.GetTool().toolType == ToolType.Market)
            {
                //this excellent but inelegant code is to prevent buying different seeds one after another from
                //reseting the market tool causing every other s
            }
            else { 
                ResetTool(); 
            }

        }
        else
        {
            PlayerInteraction.instance.SetTool(t);
        }
    }
    public void SetSeed(SeedBarrel c)
    {
        Crop tempCrop = c.crop;
        PlayerInteraction.instance.SetCrop(new Crop(tempCrop.asset));
    }

    public void OpenSettings()
    {
        settingsMenu.SetActive(true);
    }
    public void CloseSettings()
    {
        settingsMenu.SetActive(false);
        saveOrQuitPanel.SetActive(false);
    }
    public void SaveGame()
    {
        SaveSystem.SavePlayer();
        Debug.Log("Save Complete");
        notificationBar.SetActive(false);
        AnimateNotifcation("Save Complete", Color.white);
    }
    public void LoadGame()
    {
        GameHandler.instance.LoadData(SaveSystem.LoadPlayer());
    }

    public void SaveOrExitGame()
    {
        //save and quit or exit without saving
        CloseSettings();
        saveOrQuitPanel.SetActive(true);
    }
    public void QuitGame()
    {
        //actually quit the game
        Application.Quit();
    }
    public void SaveAndExit()
    {
        StartCoroutine(ExecuteAfterTime(1));
    }
    //do we want to save on quit just yet?
    /*void OnApplicationQuit()
    {
        SaveSystem.SavePlayer();
        Debug.Log("Save Complete");
    }*/

    IEnumerator ExecuteAfterTime(float time)
    {
        yield return new WaitForSeconds(time);
        QuitGame();
    }
}
