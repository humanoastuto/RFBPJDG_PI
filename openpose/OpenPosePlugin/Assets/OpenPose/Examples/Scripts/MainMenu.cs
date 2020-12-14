using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Audio;


public class MainMenu : MonoBehaviour
{
    public AudioSource audioSource;
    public GameObject OptionMenu;
    public GameObject ButtonMenu;
    public AudioMixer audioMixer;
    public Slider VolumeSlider;

    public void SetVolume (float volume)
    {
        audioMixer.SetFloat("MainVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }
    
    void Start()
    {
        audioSource.Play();
        VolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
    }

    public void QuitGame()
    {
        StartCoroutine(FadeOut(0.7f));
        Debug.Log("Quit");
        Application.Quit();
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
