using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Linq;
using System.IO;

public static class ButtonExtension
{
    public static void AddEventListener<T1,T2>(this Button button, T1 param, T2 param2, Action<T1,T2> OnClick)
    {
        button.onClick.AddListener(delegate () {
            OnClick(param,param2);
        });
    }
}


public class LevelData
{
    public string artist;
    public string name;
    public string movement;
    public string charter;
}

public class ListGeneration : MonoBehaviour
{
    public AudioSource AudioSource;
    public GameObject buttonTemplate;
    public Text selectedName;
    public Text selectedArtist;
    public Text selectedDance;
    public Text selectedCharter;
    public Image selectedImage;
    private float musicVolume = 1f;
    private List<LevelData> levelList;

    void Start()
    {
        GameObject g;
        levelList = new List<LevelData>();
        DirectoryInfo root = new DirectoryInfo("./Assets/Custom/");
        LevelData lvldat;
        foreach (DirectoryInfo dir in root.GetDirectories())
        {
            FileInfo[] info = (dir.GetFiles("data.json"));
            string filein = File.ReadAllText(info[0].FullName);
            lvldat = JsonUtility.FromJson<LevelData>(filein);
            levelList.Add(lvldat);

            g = Instantiate(buttonTemplate, transform);
            g.transform.GetChild(0).GetComponent<Text>().text = lvldat.name;
            g.transform.GetChild(1).GetComponent<Text>().text = lvldat.artist;

            Texture2D tex = null;
            byte[] fileData;
            Sprite mySprite;
            if (File.Exists((info[0].Directory + "/icon.png")))
            {
                fileData = File.ReadAllBytes((info[0].Directory + "/icon.png"));
                tex = new Texture2D(2, 2);
                tex.LoadImage(fileData);
                mySprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), new Vector2(0.5f, 0.5f), 100.0f);
                g.transform.GetChild(2).GetComponent<Image>().sprite = mySprite;
            }
           
            g.GetComponent<Button>().AddEventListener(lvldat, g.transform.GetChild(2).GetComponent<Image>().sprite, ItemClicked);
        }


        Destroy(buttonTemplate);
        transform.GetChild(1).GetComponent<Button>().onClick.Invoke();
    }
    void ItemClicked(LevelData lvldat, Sprite sprite)
    {
        //  Debug.Log("------------item  clicked---------------");
        // Debug.Log(lvldat.name);
        selectedName.text = lvldat.name;
        selectedArtist.text = lvldat.artist;
        selectedCharter.text = lvldat.charter;
        selectedDance.text = lvldat.movement;
        selectedImage.sprite = sprite;

    }
    public void PlayGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
    public void UpdateVolume(float volume)
    {
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
