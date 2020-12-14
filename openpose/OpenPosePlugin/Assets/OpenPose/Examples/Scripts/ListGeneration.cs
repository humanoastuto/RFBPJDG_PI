using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System;
using System.Linq;
using System.IO;
using UnityEngine.Networking;


public static class ButtonExtension
{
    public static void AddEventListener<T1,T2,T3>(this Button button, T1 param, T2 param2, T3 param3, Action<T1,T2,T3> OnClick)
    {
        button.onClick.AddListener(delegate () {
            OnClick(param,param2,param3);
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
    public AudioSource audioSource;
    public GameObject buttonTemplate;
    public Text selectedName;
    public Text selectedArtist;
    public Text selectedDance;
    public Text selectedCharter;
    public Image selectedImage;
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

            if (info.Length != 0)
            {
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

                g.GetComponent<Button>().AddEventListener(lvldat, g.transform.GetChild(2).GetComponent<Image>().sprite, info[0].Directory, ItemClicked);
            }
        }


        Destroy(buttonTemplate);
        transform.GetChild(1).GetComponent<Button>().onClick.Invoke();
    }
    void ItemClicked(LevelData lvldat, Sprite sprite, DirectoryInfo location)
    {
        selectedName.text = lvldat.name;
        selectedArtist.text = lvldat.artist;
        selectedCharter.text = "Charted by: "+lvldat.charter;
        selectedDance.text = "Moves by: "+lvldat.movement;
        if (sprite != null)
        {
            selectedImage.sprite = sprite;
        }

        if (File.Exists(location + "/preview.wav"))
        {
            Debug.Log("File exists");

            StartCoroutine(GetAudioClip(location + "/preview.wav"));
        }
        else
        {
            Debug.Log("NOt exists");
            StartCoroutine(FadeOut(0.7f));
        }
    }

    IEnumerator GetAudioClip(String url)
    {
        WWW audio = new WWW(url);
        yield return audio;
        audioSource.clip = audio.GetAudioClip(false, true);
        audioSource.Play();
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
