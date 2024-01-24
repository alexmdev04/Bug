using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class uiDebug : MonoBehaviour
{
    public static uiDebug instance { get; private set; }
    [SerializeField] TextMeshProUGUI
        uiFPSText,
        uiRes,
        uiNotes,
        //uiStats,
        uiStatsPlayer,
        uiStatsStealth,
        uiStatsGrapple,
        uiStatsMiscellaneous,
        uiStatsLevel,
        uiVersion;
    [SerializeField] GameObject 
        uiDebugGroup,
        uiFPS;
    [SerializeField] bool showNotes;
    public bool debugMode { get; private set; }
    public bool debugLines { get; private set; }
    public bool uiFPSEnabled;
    public float noclipSpeed = 10f;
    [Tooltip("Lower is faster")] public float statsRepeatRate = 0.02f;
    bool noclipEnabled, godEnabled;

    public const string
        // player
        str_playerTitle = "<u>Player/Game;</u>",
        str_targetFPS = "\ntargetFPS = ",
        str_vSync = ", vSync = ",
        str_mouseRotation = "\nmouseRotation = ",
        str_lookSensitivity = "\nlookSensitivity = ",
        str_playerDimensions = "\nplayerRadius = ",
        str_multiply = " * ",
        // grapple
        str_grappleTitle = "<u>Grapple;</u>",
        str_maxDistance = "\nmaxDistance = ",
        str_currentDistance = "\ncurrentDistance = ",
        str_playerMoving = "\nplayerMoving = ",
        str_grapplePointValid = "\ngrapplePointValid = ",
        str_grapplePointCheck = "\ngrapplePointCheck;\n  collisions = ",
        str_grapplePointCheckNames = ", names = ",
        str_targetPosition = "\ntargetPosition = ",
        str_targetRotation = "\ntargetRotation = ",
        str_distanceToTargetPosition = "\ndistanceToTargetPosition = ",
        str_notApplicable = "n/a",
        // level
        str_levelTitle = "<u>Level;</u>\ninLevel = ",
        str_assetKey = "\nassetKey = ",
        str_inGameName = "\ninGameName = ",
        str_objectives = "\nobjectives; ",
        str_playerStartPos = "\nplayerStartPos = ",
        // stealth
        str_stealthTitle = "<u>Stealth;</u>",
        str_stealthLevel = "\nstealthLevel = ",
        str_stealthTimer = "\nstealthTimer = ",
        str_playerVisible = "\nplayerVisible = ",
        str_toLevelSpotted = "\ntoLevelSpotted = ",
        str_toLevelFullAlert = "\ntoLevelFullAlert = ",
        str_fromLevelSpotted = "\nfromLevelSpotted = ",
        str_toGameOver = "\ntoGameOver = ",
        str_toGameOverSequnceRunning = "\ntoGameOverSequnceRunning = ",
        //other
        str_x = "x",
        str_v = "v",
        str_dash = " - ",
        str_equals = " = ",
        str_divide = " / ",
        str_NewLine = "\n",
        str_NewLineDash = "\n - ",
        str_Hz = "Hz",
        str_ResFormat = "{0}:{1}",
        str_noclip = "noclip ",
        str_god = "god mode ",
        str_enabled = "enabled",
        str_disabled = "disabled",
        str_notesTitle = "<u>Notes:</u>",
        str_todoTitle = "<u>\nTo do:</u>",
        // class names
        str_uiDebug = "uiDebug",
        str_uiDebugConsole = "uiDebugConsole",
        str_StealthHandler = "StealthHandler",
        str_Level = "Level",
        str_LevelGoal = "LevelGoal",
        str_LevelLoader = "LevelLoader",
        str_MenuLevel = "MenuLevel";

    void Awake()
    { 
        instance = this;
        uiVersion.text = str_v + Application.version;
        uiFPSText = uiFPS.GetComponent<TextMeshProUGUI>();
        #if UNITY_EDITOR
        debugMode = true;
        #endif
    }
    void Start()
    {
        StartRepeating();
        InvokeRepeating(nameof(GetFPS), 0f, statsRepeatRate);
    }
    void StartRepeating()
    {
        InvokeRepeating(nameof(GetRes), 0f, 1f);
        InvokeRepeating(nameof(GetDebugNotes), 0f, 1f);
        InvokeRepeating(nameof(GetStats), 0f, statsRepeatRate);
    }
    void StopRepeating()
    {
        CancelInvoke(nameof(GetRes));
        CancelInvoke(nameof(GetDebugNotes));
        CancelInvoke(nameof(GetStats));
    }
    public void RefreshRepeating()
    {
        CancelInvoke();
        StartRepeating();
        InvokeRepeating(nameof(GetFPS), 0f, statsRepeatRate);
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F3)) 
        {
            debugMode = !debugMode; 
            if (debugMode) { StartRepeating(); }
            else { StopRepeating();  }
            if (!uiFPSEnabled) uiFPS.SetActive(debugMode);
        }
        if (Input.GetKeyDown(KeyCode.F4)) { debugLines = debugMode && !debugLines; }
        uiDebugGroup.SetActive(debugMode);
        if (debugMode) { Controls(); }
        if (noclipEnabled) { Noclip(); }
    }
    void GetRes() // gets the current resolution, refresh rate and aspect ratio
    {
        float gcd = Extensions.CalcGCD(Screen.width, Screen.height);
        uiRes.text = Screen.width.ToString() + str_x + Screen.height.ToString() + str_NewLine
            + Screen.currentResolution.refreshRateRatio + str_Hz + str_NewLine +
            (string.Format(str_ResFormat, Screen.width / gcd, Screen.height / gcd));
    }
    void GetFPS() // fps counter
    {
        uiFPSText.text = ((int)(1 / Time.unscaledDeltaTime)).ToString();
    }
    void GetStats() // contructs all stats for the debug overlay, uses stringbuilder & append to slightly improve performance
    {
        if (!debugMode)
        {
            uiStatsPlayer.text = string.Empty;
            uiStatsGrapple.text = string.Empty;
            uiStatsLevel.text = string.Empty;
            uiStatsStealth.text = string.Empty;
            uiStatsMiscellaneous.text = string.Empty;
            return;
        }
        uiStatsPlayer.text = Player.instance.debugGetStats().ToString();
        uiStatsGrapple.text = Grapple.instance.debugGetStats().ToString();
        uiStatsLevel.text = new StringBuilder(str_levelTitle).Append(LevelLoader.instance.inLevel).Append(LevelLoader.instance.levelCurrent != null ? LevelLoader.instance.levelCurrent.debugGetStats().ToString() : string.Empty).ToString();
        uiStatsStealth.text = StealthHandler.instance.debugGetStats().ToString();
        uiStatsMiscellaneous.text = new StringBuilder("<u>Miscellaneous;</u>")
            .Append("\nuiFadeAlpha = ").Append(ui.instance.uiFadeAlpha)
            .Append("\nplayerVisible = ").Append(StealthHandler.instance.playerVisible).ToString();
    }
    void GetDebugNotes() // scans all loaded scenes and their root game objects for uiDebugNote components and combines them all
    {
        if (!showNotes) { return; }
        if (!debugMode) { return; }
        List<uiDebugNote> noteComponents = new();
        List<Scene> scenes = new List<Scene>();
        StringBuilder notesText = new(str_notesTitle), todosText = new (str_todoTitle);

        for (int i = 0; i < SceneManager.sceneCount; i++) { scenes.Add(SceneManager.GetSceneAt(i)); }
        foreach (Scene scene in scenes) 
        { 
            foreach (GameObject rootObject in scene.GetRootGameObjects()) 
            {
                uiDebugNote noteComponent = rootObject.GetComponent<uiDebugNote>();
                if (noteComponent != null) { noteComponents.Add(noteComponent); }
            }
        }
        foreach (uiDebugNote noteComponent in noteComponents)
        {
            if (noteComponent.notes.Length > 0)
            {
                notesText.Append(str_NewLine).Append(noteComponent.gameObject.name);
                foreach (string note in noteComponent.notes) { notesText.Append(str_NewLineDash).Append(note); }
            }

            if (noteComponent.toDos.Length > 0)
            {
                todosText.Append("\n").Append(noteComponent.gameObject.name);
                foreach (string todo in noteComponent.toDos) { todosText.Append(str_NewLineDash).Append(todo); }
            }
        }
        uiNotes.text = notesText.Append(todosText).ToString();
    }
    void Controls() // allows for WASD movement control and scroll to change the grapple distance
    {
        // scroll to change hook distance
        float scrollY = Input.mouseScrollDelta.y;
        if (scrollY != 0) { Grapple.instance.debugMaxDistanceEdit(scrollY); }

        // enables debug notes on screen
        if (Input.GetKeyDown(KeyCode.Insert)) { showNotes = !showNotes; }

        // toggles debug console
        if (Input.GetKeyDown(KeyCode.BackQuote) || Input.GetKeyDown(KeyCode.Tilde)) 
        {
            uiDebugConsole.instance.gameObject.SetActive(!uiDebugConsole.instance.gameObject.activeSelf);
        }
    }
    void Noclip()
    {
        // wasd movement
        if (uiDebugConsole.instance.gameObject.activeSelf) { return; }
        Player.instance.transform.position += noclipSpeed * Time.deltaTime * Player.instance.transform.TransformDirection(new(Extensions.FloatFromAxis(Input.GetKey(KeyCode.D), Input.GetKey(KeyCode.A)), 0, Extensions.FloatFromAxis(Input.GetKey(KeyCode.W), Input.GetKey(KeyCode.S))));
    }
    public void ToggleNoclip()
    {
        noclipEnabled = !noclipEnabled;
        uiMessage.instance.New(str_noclip + (noclipEnabled ? str_enabled : str_disabled), str_uiDebug); 
    }
    public void ToggleGod()
    {
        godEnabled = !godEnabled;
        uiMessage.instance.New(str_god + (godEnabled ? str_enabled : str_disabled), str_uiDebug);
    }
    public void ToggleFPS()
    {
        uiFPSEnabled = !uiFPSEnabled;
        if (!uiFPSEnabled && debugMode) { return; }
        uiFPS.SetActive(uiFPSEnabled);
    }
}