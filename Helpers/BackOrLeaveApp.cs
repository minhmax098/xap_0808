using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BackOrLeaveApp : MonoBehaviour
{
    private bool clickedBefore = false;
    const float timerTime = 1f;
    private static string EXIT_APP_SYMBOL = "#EXIT";
    private static string CONNECT_SYMBOL = "#CONNECT";
    public List<string> previousScenes = new List<string>();
    public Dictionary<string, string> staticScenePair = new Dictionary<string, string>()
    {
        {SceneConfig.home_user, EXIT_APP_SYMBOL},
        {SceneConfig.home_nosignin, EXIT_APP_SYMBOL},
        {SceneConfig.signIn, SceneConfig.home_nosignin},
        {SceneConfig.forgotPass, SceneConfig.signIn},
        {SceneConfig.resetpass, SceneConfig.forgotPass},
        {SceneConfig.passresetdone, SceneConfig.signIn},
        {SceneConfig.myLesson, SceneConfig.home_user},
        {SceneConfig.lesson_edit, SceneConfig.myLesson},
        {SceneConfig.createLesson_main, SceneConfig.home_user},
        {SceneConfig.storeModel, SceneConfig.createLesson_main},
        {SceneConfig.uploadModel, SceneConfig.createLesson_main},
        {SceneConfig.interactiveModel, SceneConfig.uploadModel},
        {SceneConfig.meetingJoining, SceneConfig.home_user},
        {SceneConfig.update_lesson, SceneConfig.lesson_edit},
        {SceneConfig.buildLesson, SceneConfig.lesson_edit},
    };
    public List<string> flexibleScenePair = new List<string>()
    {
        SceneConfig.signUp + CONNECT_SYMBOL + SceneConfig.home_nosignin,
        SceneConfig.signUp + CONNECT_SYMBOL + SceneConfig.signIn,
        SceneConfig.xrLibrary_edit + CONNECT_SYMBOL + SceneConfig.home_user,
        SceneConfig.xrLibrary_edit + CONNECT_SYMBOL + SceneConfig.home_nosignin,
        SceneConfig.listOrgan_edit + CONNECT_SYMBOL + SceneConfig.home_user,
        SceneConfig.listOrgan_edit + CONNECT_SYMBOL + SceneConfig.home_nosignin,
        SceneConfig.lesson + CONNECT_SYMBOL + SceneConfig.home_user,
        SceneConfig.lesson + CONNECT_SYMBOL + SceneConfig.xrLibrary_edit,
        SceneConfig.lesson + CONNECT_SYMBOL + SceneConfig.listOrgan_edit,
        SceneConfig.lesson_nosignin + CONNECT_SYMBOL + SceneConfig.home_nosignin,
        SceneConfig.lesson_nosignin + CONNECT_SYMBOL + SceneConfig.xrLibrary_edit,
        SceneConfig.lesson_nosignin + CONNECT_SYMBOL + SceneConfig.listOrgan_edit,
        SceneConfig.experience + CONNECT_SYMBOL + SceneConfig.lesson,
        SceneConfig.experience + CONNECT_SYMBOL + SceneConfig.lesson_nosignin,
        SceneConfig.experience + CONNECT_SYMBOL + SceneConfig.lesson_edit,
        SceneConfig.meetingStarting + CONNECT_SYMBOL + SceneConfig.lesson,
        SceneConfig.meetingStarting + CONNECT_SYMBOL + SceneConfig.lesson_edit,
        SceneConfig.createLesson + CONNECT_SYMBOL + SceneConfig.storeModel,       
    };
    private static BackOrLeaveApp instance;
    public static BackOrLeaveApp Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindObjectOfType<BackOrLeaveApp>();
            }
            return instance;
        }
    }

    public void ResetPreviousScene()
    {
        previousScenes.Clear();
    }

    public void AddPreviousScene(string currentSceneName, string nextScecne)
    {
       if (flexibleScenePair.IndexOf(nextScecne + CONNECT_SYMBOL + currentSceneName) != -1)
       {
            if (previousScenes.Count > 0 && (previousScenes[previousScenes.Count - 1] == currentSceneName))
            {
                return;
            }
            previousScenes.Add(currentSceneName);
       }
    }

    public void BackToPreviousScene(string currentScene, bool isLoadAsync = true)
    {
        string previousScene = "";
        if (staticScenePair.ContainsKey(currentScene))
        {
            if (staticScenePair[currentScene] == EXIT_APP_SYMBOL)
            {
                return;
            }
            previousScene = staticScenePair[currentScene];
        }
        else
        {
            if (previousScenes.Count < 1)
            {
                return;
            }
            previousScene = previousScenes[previousScenes.Count - 1];
            previousScenes.RemoveAt(previousScenes.Count - 1);
        }

        if (isLoadAsync)
        {
            StartCoroutine(Helper.LoadAsynchronously(previousScene));
        }
        else
        {
            SceneManager.LoadScene(previousScene);
        }
    }
    void Start()
    {
        DontDestroyOnLoad(gameObject);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) && !clickedBefore)
        { 
            clickedBefore = true;
            StartCoroutine(QuitingTimer());
        }
    }

    IEnumerator QuitingTimer()
    {
        yield return null;
        float counter = 0;
        while (counter < timerTime)
        {
            counter += Time.deltaTime;

            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Quit();
            }
            yield return null;
        }
        clickedBefore = false;
        StopAllCoroutines();
        BackToPreviousScene(SceneManager.GetActiveScene().name);
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }
}
