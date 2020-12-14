using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public AudioSource audioSource;
    public GameObject OptionMenu;
    public GameObject ButtonMenu;

    private float musicVolume =1f;
    // Start is called before the first frame update
    void Start()
    {
       audioSource.Play();
    }

    public void QuitGame()
    {
        StartCoroutine(FadeOut(0.7f));
        Debug.Log("Quit");
        Application.Quit();
    }
    public void UpdateVolume(float volume) {
        this.musicVolume = volume;
    }

    // Update is called once per frame
    void Update()
    {
        audioSource.volume = musicVolume;
    }

    public void ShowOptions()
    {
        OptionMenu.active = false; ;
    }

    public float getVolume()
    {
        return this.musicVolume;
    }

    IEnumerator FadeOut(float FadeTime)
    {
        float startVolume = audioSource.volume;

        while (audioSource.volume > 0)
        {
            audioSource.volume -= startVolume * Time.deltaTime / FadeTime;

            yield return null;
        }

        audioSource.Stop();
        audioSource.volume = startVolume;
    }

}
