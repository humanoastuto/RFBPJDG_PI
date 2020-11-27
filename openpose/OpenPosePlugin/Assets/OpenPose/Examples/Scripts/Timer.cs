using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;

public class Timer : MonoBehaviour
{
    public float timeRemaining = 10;
    public bool timerIsRunning = false;
    public Text timeText;
    public VideoPlayer videoPlay;

    public void ToggleTimerStart()
    {
        timeRemaining = 10;
        Start();
        timerIsRunning = true;
        videoPlay.Play();
    }

    private void Start()
    {
        Debug.Log("Lets get it started");
        timeText.text = "00:00";
        timeRemaining = (float) videoPlay.clip.length;
    }

    void Update()
    {
     
        if (timerIsRunning)
        {
           
            if (timeRemaining > 0)
            {
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
            }
            else
            {
                Debug.Log("Time has run out!");
                timeRemaining = 0;
                timerIsRunning = false;
                timeText.text = "00:00";
            }
            
        }
    }
    private void DisplayTime(float timeToDisplay)
    {
        float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay % 60);

        timeText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}   