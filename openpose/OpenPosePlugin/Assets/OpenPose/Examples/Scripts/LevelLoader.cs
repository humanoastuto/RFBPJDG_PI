using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelLoader : MonoBehaviour
{
    public Animator transition;
    public int levelIndex;
    public float transitionTime = 1f;
    public Button button;
    static public string dancename;

    void Start()
    {
        if(button != null)
        {
            Button btn = button.GetComponent<Button>();
            btn.onClick.AddListener(TaskOnClick);
        }
    }

    void TaskOnClick()
    {
        StartCoroutine(LoadLevel(levelIndex));
        dancename = "video123";
    }

    IEnumerator LoadLevel(int levelIndex)
    {
        transition.SetTrigger("Start");
        yield return new WaitForSeconds(transitionTime);
        SceneManager.LoadScene(levelIndex);
    }
}
