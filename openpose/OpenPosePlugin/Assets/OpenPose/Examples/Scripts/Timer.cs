using System.Collections;
using System;
using System.Linq;
using System.IO;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;


public class Timer : MonoBehaviour
{
    public float timeRemaining = 10;
    private float totalTime = 0;
    public bool timerIsRunning = false;
    public Text timeText;
    public VideoPlayer videoPlay;
    public Button playButton;
    private double[][] arrayx;
    private double[][] arrayy;
    public int startingFrame = 0;
    private int videoLength = 0;
    public int currentFrame = 0;

    public void ToggleTimerStart()
    {
       //etArrays((startingFrame - Time.frameCount));
        setArrays();
        totalTime = (float)videoPlay.clip.length;
        timeRemaining = (float)videoPlay.clip.length;
        timerIsRunning = true;
        videoPlay.Play();
        startingFrame = Time.frameCount;
        playButton.gameObject.SetActive(false);
    }

    private void Start()
    {
        timeText.text = "00:00";
        videoPlay.url = "./Assets/OpenPose/Examples/Media/HanSoloLevel/video.mp4";
        videoLength = Mathf.FloorToInt((float)videoPlay.clip.length);
        arrayx = new double[videoLength][];//Body [0-25]
        arrayy = new double[videoLength][];
    }

    private void setArrays()
    {
        DirectoryInfo dir = new DirectoryInfo("./Assets/OpenPose/Examples/Media/HanSoloLevel/output");
        FileInfo[] info = dir.GetFiles("*keypoints.txt");
        string[] videoCoord=new string[2];
        int fcount = 0;
        int vcount = 0;
        foreach (FileInfo f in info)
        {
            vcount = 0;
            using (StreamReader sr = f.OpenText())
            {
                arrayx[fcount] = new double[25];
                arrayy[fcount] = new double[25];
                var s = "";
                while ( ! String.IsNullOrWhiteSpace((s = sr.ReadLine())))
                {
                    videoCoord = s.Split(',');
                    arrayx[fcount][vcount] = double.Parse(videoCoord[0]);
                    arrayy[fcount][vcount] = double.Parse(videoCoord[1]);
                    vcount++;
                }
            }
            fcount++;
        }
        
    }

    
    //   return arrayx.GetRow(startingFrame - Time.frameCount);
    public double[] getArrayx()
    {
        double[] arrayTest = arrayx[currentFrame];
        return arrayTest;
    }

    public double[] getArrayy()
    {
        double[] arrayTest = arrayy[currentFrame];
        return arrayTest;
    }

    void Update()
    {
     
        if (timerIsRunning)
        {
            if (timeRemaining > 0)
            {
                currentFrame = Time.frameCount - startingFrame;
                Debug.Log(currentFrame);
                timeRemaining -= Time.deltaTime;
                DisplayTime(timeRemaining);
                
            }
            else
            {
                Debug.Log("Time has run out!");
                timeRemaining = 0;
                timerIsRunning = false;
                timeText.text = "00:00";
                playButton.gameObject.SetActive(true);

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