using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Linq;
using System.IO;

public class SelectMenu : MonoBehaviour
{
   
    public AudioSource AudioSource;
    private float musicVolume = 1f;
    void Start()
    {
      
    }

    public void PlayGame() {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
    }
    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
    public void UpdateVolume(float volume) {
        this.musicVolume = volume;
    }

    // Update is called once per frame
    void Update()
    {
        //AudioSource.volume = musicVolume;
    }
    public float getVolume()
    {
        return this.musicVolume;
    }
}
