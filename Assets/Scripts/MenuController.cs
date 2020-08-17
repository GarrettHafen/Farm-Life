using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

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
        
        //SceneManager.LoadScene("LandingPage");
        
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
                FindObjectOfType<AudioManager>().PlaySound("Click");
            }

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
}
