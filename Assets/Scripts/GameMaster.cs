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

        if (!GameObject.FindGameObjectWithTag("MM"))
        {
            mManager = Instantiate(musicPrefab, transform.position, Quaternion.identity);
            mManager.name = musicPrefab.name;
            DontDestroyOnLoad(mManager);




        }
        myMusic = Resources.LoadAll("Music", typeof(AudioClip));
        audioThingy2 = mManager.GetComponent<AudioSource>();
        audioThingy2.clip = myMusic[0] as AudioClip;

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
        audioThingy2.mute = !audioThingy2.mute;
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void LoadGame()
    {
               
        MenuController.instance.LoadGame();
        GameHandler.instance.landingPage.SetActive(false);
        GameHandler.instance.landingPageOpen = false;
    }

    public void NewGame()
    {
        GameHandler.instance.landingPage.SetActive(false);
        GameHandler.instance.landingPageOpen = false;
    }
}
