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
    public Toggle FullScreenToggle;
    public Dropdown resolutionDropdown;
    Resolution[] resolutions;

    public void SetVolume (float volume)
    {
        audioMixer.SetFloat("MainVolume", Mathf.Log10(volume) * 20);
        PlayerPrefs.SetFloat("MusicVolume", volume);
    }

    public void SetFullscreen (bool isFullscreen)
    {
        Screen.fullScreen = isFullscreen;
        PlayerPrefs.SetInt("FullScreen", isFullscreen? 1 : 0);
    }

    void startDropdown()
    {
        resolutions = Screen.resolutions;
        resolutionDropdown.ClearOptions();
        List<string> options = new List<string>();
        int currentResolutionIndex = 0;
        for(int i = 0; i < resolutions.Length; i++)
        {
            options.Add(resolutions[i].width + " x " + resolutions[i].height);
            if (resolutions[i].width == Screen.width && resolutions[i].height == Screen.height)
            {
                currentResolutionIndex = i;
            }
        }

        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution (int resolutionIndex)
    {
        Resolution res = resolutions[resolutionIndex];
        Screen.SetResolution(res.width, res.height, Screen.fullScreen);
    }


    void Start()
    {
        startDropdown();
        audioSource.Play();
        VolumeSlider.value = PlayerPrefs.GetFloat("MusicVolume", 1f);
        FullScreenToggle.isOn = PlayerPrefs.GetInt("FullScreen",1) == 1 ? true : false;
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
