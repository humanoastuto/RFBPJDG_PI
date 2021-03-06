using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using System.IO;

public static class SelectedChart {
    public static DirectoryInfo getLocation { get; set; }
}
public static class ButtonExtension
{
    public static void AddEventListener<T1, T2, T3>(this Button button, T1 param, T2 param2, T3 param3, Action<T1, T2, T3> OnClick)
    {
        button.onClick.AddListener(delegate ()
        {
            OnClick(param, param2, param3);
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
    public Button playButton;
    private List<LevelData> levelList;
    private LevelData selectedItem;

    void Start()
    {
        GameObject g;
        levelList = new List<LevelData>();
        DirectoryInfo root = new DirectoryInfo("./Custom/");
        LevelData lvldat;
        if (Directory.Exists("./Custom") && root.GetDirectories().Length != 0)
        {
            playButton.enabled = true;
            foreach (DirectoryInfo dir in root.GetDirectories())
            {
                FileInfo[] info = (dir.GetFiles("data.json"));

                if (info.Length != 0 && File.Exists((info[0].Directory + "/movement.txt")))
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
            transform.GetChild(1).GetComponent<Button>().onClick.Invoke();
        }
        else
        {
            playButton.enabled = false;
        }
        Destroy(buttonTemplate);

    }
    void ItemClicked(LevelData lvldat, Sprite sprite, DirectoryInfo location)
    {
        if (selectedItem != lvldat)
        {
            SelectedChart.getLocation = location;
            selectedItem = lvldat;
            selectedName.text = lvldat.name;
            selectedArtist.text = lvldat.artist;
            selectedCharter.text = "Charted by: " + lvldat.charter;
            selectedDance.text = "Moves by: " + lvldat.movement;
            if (sprite != null)
            {
                selectedImage.sprite = sprite;
            }
            if (File.Exists(location + "/preview.ogg"))
            {
                StartCoroutine(GetAudioClip(location + "/preview.ogg"));
            }
            else
            {
                StartCoroutine(FadeOut(0.7f));
            }
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
