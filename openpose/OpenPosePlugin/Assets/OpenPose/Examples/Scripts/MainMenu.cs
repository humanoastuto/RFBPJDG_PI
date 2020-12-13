using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public AudioSource AudioSource;
    private float musicVolume =1f;
    // Start is called before the first frame update
    void Start()
    {
        AudioSource.Play();
    }

  //  public void PlayGame() {
  //     SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex+1);
 //   }
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
        AudioSource.volume = musicVolume;
    }
    public float getVolume()
    {
        return this.musicVolume;
    }
}
