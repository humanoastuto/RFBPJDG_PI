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

    public double[] getArrayx()
    {
        double[] arrayx = { 713.549, 717.589, 650.932, 560.829, 474.593, 789.967, 878.214, 964.378, 713.711, 676.377, 678.369, 668.569, 758.596, 743.08, 746.985, 698.038, 727.333, 682.286, 745.001, 792.074, 793.91, 739.089, 621.554, 621.554, 680.334 };
        return arrayx;
    }

    public double[] getArrayy()
    {
        double[] arrayy = { 94.5737, 165.122, 165.16, 178.768, 180.826, 165.069, 165.17, 167.014, 378.593, 374.707, 511.872, 647.009, 380.551, 515.78, 646.956, 84.7163, 84.786, 96.5211, 100.351, 674.425, 666.537, 64.823, 668.531, 664.564, 656.804 };
        return arrayy;
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