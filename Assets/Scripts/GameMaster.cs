using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameMaster : MonoBehaviour
{
    public static GameMaster instance;
    public GameObject musicPrefab;
    public Object[] myMusic;
    //public AudioSource audioThingy;
    public AudioSource audioThingy2;
    private GameObject mManager;



    // Start is called before the first frame update
    void Start()
    {
        instance = this;

        mManager = GameObject.FindGameObjectWithTag("MM");
        if (mManager == null)
        {
            mManager = Instantiate(musicPrefab, transform.position, Quaternion.identity);
            mManager.name = musicPrefab.name;
            DontDestroyOnLoad(mManager);
        }
        myMusic = Resources.LoadAll("Music", typeof(AudioClip));
        audioThingy2 = mManager.GetComponent<AudioSource>();
        audioThingy2.clip = myMusic[0] as AudioClip;

#if UNITY_EDITOR
        audioThingy2.mute = true;
        AudioManager.instance.Mute();
        muteIndicator?.SetActive(true);
#endif
    }

    // Update is called once per frame
    void Update()
    {
        if (audioThingy2 != null)
        {
            if (!audioThingy2.isPlaying)
            {
                PlayRandomMusic();
            }
        }
    }

    void PlayRandomMusic()
    {
        audioThingy2.clip = myMusic[Random.Range(0, myMusic.Length)] as AudioClip;
        if (audioThingy2 != null)
        {
            audioThingy2.Play();
        }
    }

    public void MuteAudio()
    {
        AudioManager.instance.PlaySound("Click");
        audioThingy2.mute = !audioThingy2.mute;
        AudioManager.instance.Mute();
        muteIndicator?.SetActive(audioThingy2.mute);
    }

    public void ExitGame()
    {
        AudioManager.instance.PlaySound("Click");
#if UNITY_WEBGL
        MenuController.instance.AnimateNotifcation("Close the tab to exit", Color.white, "Null");
#elif UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public void LoadGame()
    {
               
        MenuController.instance.LoadGame();
        GameHandler.instance.landingPage.SetActive(false);
        GameHandler.instance.landingPageOpen = false;
        AudioManager.instance.PlaySound("Click");
    }

    public GameObject newGameConfirmPanel;
    public GameObject muteIndicator;

    public void NewGame()
    {
        AudioManager.instance.PlaySound("Click");
        if (SaveSystem.HasSave())
            newGameConfirmPanel.SetActive(true);
        else
        {
            GameHandler.instance.NewGame();
            StartFreshGame();
        }
    }
    public void ConfirmNewGame()
    {
        newGameConfirmPanel.SetActive(false);
        GameHandler.instance.NewGame();
        StartFreshGame();
        AudioManager.instance.PlaySound("Click");
    }
    public void CancelNewGame()
    {
        newGameConfirmPanel.SetActive(false);
        AudioManager.instance.PlaySound("Click");
    }
    private void StartFreshGame()
    {
        GameHandler.instance.landingPage.SetActive(false);
        GameHandler.instance.landingPageOpen = false;
    }
}
