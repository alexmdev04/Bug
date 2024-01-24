using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.Events;
using UnityEngine.Rendering;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.ResourceManagement.ResourceProviders;
using UnityEngine.SceneManagement;

public class LevelLoader : MonoBehaviour
{
    public static LevelLoader instance { get; private set; }
    public Level levelCurrent { get; private set; }
    public bool inLevel { get; private set; }
    public GameObject levelRootPrefab;
    public GameObject menuLevel;
    [SerializeField] Vector3 menuLevelStartPosition;
    [SerializeField] bool skipTutorials;
    MenuLevel MenuLevel;
    [HideInInspector] public UnityEvent levelLoaded = new();
    public Volume globalVolume;
    bool waitingForUnload;

    public struct levelLoadData
    {
        public string levelAssetKey;
        public Level.levelDifficultiesEnum levelDifficulty;
        public bool useFade;
        public Vector3 overrideStartRotation;
    }
    void Awake()
    {
        instance = this;
        MenuLevel = menuLevel.GetComponent<MenuLevel>();
        MenuLevel.skipTutorial = skipTutorials;
        levelLoaded.AddListener(delegate { inLevel = true; });
        Addressables.InitializeAsync();
#if !UNITY_EDITOR
        SceneManager.LoadScene("ui", LoadSceneMode.Additive);
#endif
    }   
    void Start()
    {
#if UNITY_EDITOR
        MenuLevelState(true);
        if (unloadAllLevelScenesOnStart) { UnloadAllLevelScenesOnStart(); }
        //int tutorialComplete = PlayerPrefs.GetInt("tutorialComplete", 0);
        //if (tutorialComplete == 0)
        //{
        //    StartCoroutine(MenuLevel.TutorialStart());
        //}
#endif
    }
    void Update()
    {
        inLevel = levelCurrent != null;
    }
    /// <summary>
    /// Loads the given level
    /// </summary>
    /// <param name="levelLoadData"></param>
    public void LoadLevel(levelLoadData levelLoadData)
    {
        // try to load level
        ui.instance.uiFadeToBlack = levelLoadData.useFade;
        AsyncOperationHandle sceneLoad = Addressables.LoadSceneAsync(levelLoadData.levelAssetKey, LoadSceneMode.Additive);
        sceneLoad.Completed += delegate { StartCoroutine(LoadLevelCompleted(levelLoadData, sceneLoad)); };
        Grapple.instance.Disable();
        Grapple.instance.GrapplePointReset();
    }
    /// <summary>
    /// Checks if the level loaded successfully and begins the level startup sequence
    /// </summary>
    /// <param name="levelLoadData"></param>
    /// <param name="sceneLoad"></param>
    IEnumerator LoadLevelCompleted(levelLoadData levelLoadData, AsyncOperationHandle sceneLoad)
    {
        // load level fail
        if (sceneLoad.Status == AsyncOperationStatus.Failed) 
        { 
            uiDebugConsole.instance.InvalidInput(levelLoadData.levelAssetKey, "level");
            ui.instance.uiFadeToBlack = false;
            yield break;
        }
        // load level success
        // unloading previous level
        if (levelCurrent != null) { UnloadLevel(levelCurrent); }
        while (waitingForUnload) { yield return new WaitForEndOfFrame(); }
        // getting new level references
        SceneInstance levelSceneInstance = (SceneInstance)sceneLoad.Result;
        levelCurrent = levelSceneInstance.Scene.GetRootGameObjects()[0].GetComponent<Level>();
        levelCurrent.sceneInstance = levelSceneInstance;
        MenuLevelState(false, true);
        levelCurrent.playerStartRotation = levelLoadData.overrideStartRotation;
        // reset grapple state
        Grapple.instance.Enable();
        Grapple.instance.GrapplePointReset();
        // broadcast the level start
        levelCurrent.InitialStart();
        //uiMessage.instance.New(levelCurrent.assetKey + " - " + levelCurrent.inGameName + " loaded!", uiDebug.str_LevelLoader);
        ui.instance.uiFadeToBlack = false;
        levelLoaded.Invoke();
    }
    /// <summary>
    /// Unloads either the given level or the current level
    /// </summary>
    /// <param name="level"></param>
    /// <param name="returnToMainMenu"></param>
    public void UnloadLevel(Level level, bool returnToMainMenu = false, bool returnToMainMenuNotify = false)
    {
        waitingForUnload = true;
        if (level == null) { uiMessage.instance.New("Already in level area", uiDebug.str_LevelLoader); return; }
        level.Unload();
        string levelAssetKey = level.assetKey;
        AsyncOperation sceneUnload = SceneManager.UnloadSceneAsync(level.gameObject.scene);
        sceneUnload.completed += delegate { waitingForUnload = false; };
        if (returnToMainMenu) { sceneUnload.completed += delegate { MenuLevelState(returnToMainMenu, returnToMainMenuNotify); }; }
        AsyncOperationHandle sceneInstanceUnload = Addressables.UnloadSceneAsync(levelCurrent.sceneInstance, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
    }
    //void UnloadLevelCompleted(AsyncOperation callback)
    //{

    //}
    //void LoadUnloadSuccess()
    //{

    //}
    /// <summary>
    /// Sets the menu level's active state
    /// </summary>
    /// <param name="state"></param>
    public void MenuLevelState(bool state, bool notify = false)
    {
        menuLevel.SetActive(state);
        if (state)
        {
            if (notify) { uiMessage.instance.New("Returned to main menu", uiDebug.str_LevelLoader); }
            Player.instance.TeleportInstant(menuLevelStartPosition, Vector3.zero);
        }
    }
    #region Unload all level scenes on start if in editor
#if UNITY_EDITOR
    [SerializeField] bool unloadAllLevelScenesOnStart = true;
    List<UnityEngine.SceneManagement.Scene> scenes = new List<UnityEngine.SceneManagement.Scene> ();
    void UnloadAllLevelScenesOnStart()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++) { scenes.Add(SceneManager.GetSceneAt(i)); }
        string output = string.Empty;
        foreach (UnityEngine.SceneManagement.Scene scene in scenes)
        {
            if (scene.GetRootGameObjects()[0].GetComponent<Level>() != null)
            {
                output += scene.name + ", ";
#pragma warning disable CS0618 // Type or member is obsolete
                SceneManager.UnloadScene(scene);
#pragma warning restore CS0618 // Type or member is obsolete
            }
        }
        if (output != string.Empty) { Debug.LogWarning("The scenes " + output + "were found on startup and were unloaded."); }
    }
#endif
#endregion
}




//public void ChangeLevel(string levelAssetKey, Level.levelDifficultiesEnum levelDifficulty = Level.levelDifficultiesEnum.normal, bool overrideFade = true, Vector3 overrideStartRotation = default)
//{
//    inLevel = false;
//    ui.instance.uiFadeToBlack = overrideFade;
//    if (levelCurrent != null)
//    {
//        levelCurrent.Unload();
//        AsyncOperation sceneUnload = SceneManager.UnloadSceneAsync(levelCurrent.gameObject.scene);
//        UnloadSceneInstance(levelCurrentSceneInstance, levelCurrent.assetKey);
//        sceneUnload.completed += delegate { LoadLevel(levelAssetKey, levelDifficulty, overrideStartRotation); };
//        levelCurrent = null;
//    }
//    else
//    {
//        Debug.Log("No level is loaded, unload skipped.");
//        LoadLevel(levelAssetKey, levelDifficulty, overrideStartRotation);
//    }
//    Grapple.instance.Disable();
//    Grapple.instance.GrapplePointReset();
//}


//void LoadLevel(string levelAssetKey, Level.levelDifficultiesEnum levelDifficulty = Level.levelDifficultiesEnum.normal, Vector3 overrideStartRotation = default)
//{
//    AsyncOperationHandle sceneLoad = Addressables.LoadSceneAsync(levelAssetKey, LoadSceneMode.Additive);
//    sceneLoad.Completed += delegate { LoadLevelCompleted(levelAssetKey, sceneLoad, levelDifficulty, overrideStartRotation); };
//}


//void LoadLevelCompleted(string levelAssetKey, AsyncOperationHandle sceneLoad, Level.levelDifficultiesEnum levelDifficulty = Level.levelDifficultiesEnum.normal, Vector3 overrideStartRotation = default)
//{
//    Debug.Log("loadlevelcompleted = " + sceneLoad.Status.ToString());
//    if (sceneLoad.Status == AsyncOperationStatus.Failed || sceneLoad.OperationException is InvalidKeyException invalidKeyException)
//    {
//        uiDebugConsole.instance.InvalidInput(levelAssetKey, "level");
//        return;
//    }
//    levelCurrentSceneInstance = (SceneInstance)sceneLoad.Result;
//    levelCurrent = levelCurrentSceneInstance.Scene.GetRootGameObjects()[0].GetComponent<Level>();
//    MenuLevelState(false);
//    uiMessage.instance.New(levelCurrent.assetKey + " - " + levelCurrent.inGameName + " loaded!", uiDebug.str_LevelLoader);
//    ui.instance.uiFadeToBlack = false;
//    levelCurrent.playerStartRotation = overrideStartRotation;
//    levelCurrent.InitialStart();
//    levelLoaded.Invoke();
//}


//public void UnloadLevelCurrent()
//{
//    inLevel = false;
//    if (levelCurrent != null)
//    {
//        levelCurrent.Unload();
//        string levelAssetKey = levelCurrent.assetKey;
//        AsyncOperation sceneUnload = SceneManager.UnloadSceneAsync(levelCurrent.gameObject.scene);
//        sceneUnload.completed += delegate { MenuLevelState(true, true); };
//        AsyncOperationHandle sceneInstanceUnload = Addressables.UnloadSceneAsync(levelCurrentSceneInstance, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
//        levelCurrent = null;
//    }
//    else
//    {
//        uiMessage.instance.New("No level to unload / Already on main menu");
//        MenuLevelState(true);
//    }
//}


//void UnloadSceneInstance(SceneInstance sceneInstance, string assetKey)
//{
//    AsyncOperationHandle sceneInstanceUnload = Addressables.UnloadSceneAsync(sceneInstance, UnloadSceneOptions.UnloadAllEmbeddedSceneObjects);
//    //sceneInstanceUnload.Completed += delegate { uiMessage.instance.New("Unloaded " + assetKey, uiDebug.str_LevelLoader); };
//}