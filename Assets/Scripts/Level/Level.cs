using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.ResourceProviders;

public class Level : MonoBehaviour
{
    public enum levelDifficultiesEnum
    {
        easy,
        normal,
        hard
    }
    [Serializable]
    public struct DifficultySpecificValues
    {
        public float grappleMaxDistance;
        public List<LevelObjective.ObjectiveData> objectiveDataList;
    }
    [Serializable] public struct Section
    {
        public GameObject parent;
        public string startMsg;
        [Tooltip("The player will be teleported here when the section is started")] public GameObject playerStartPos;
        public LevelGoal goal;
        public string levelTip;
        public float levelTipInitialWait;
        public float levelTipRepeatWait;
        public bool torchEnabled;
    }
    [Header("Level Attributes")]
    public string assetKey;
    public string inGameName;
    public levelDifficultiesEnum levelDifficulty = levelDifficultiesEnum.normal;

    [Header("Difficulties")]
    public DifficultySpecificValues easyValues;
    public DifficultySpecificValues normalValues;
    public DifficultySpecificValues hardValues;
    public DifficultySpecificValues currentDifficultyValues { get; private set; }

    [Header("References")]
    [SerializeField] List<Section> sections = new();
    public Section currentSection { get; private set; }
    [HideInInspector] public List<LevelObjective> currentObjectives;
    [HideInInspector] public string levelNumber;
    [HideInInspector] public Vector3 playerStartRotation;
    [HideInInspector] public SceneInstance sceneInstance;
    [HideInInspector] public bool levelRunning;
    [HideInInspector] public UnityEvent collectableCollected = new();
    Coroutine levelTipsLoop;
    bool levelTipsLoopRunning;
    bool levelTipsCondition;

    void Awake()
    {
        // levelNumber is only used for the radar, if the assetKey format is Level1 then it will be 1, otherwise it will be the full asset key
        levelNumber = assetKey[..5].ToLower() == "level" ? (assetKey.ToCharArray()[5..].AllCharsAreDigits() ? assetKey[5..] : assetKey) : assetKey;
    }
    public void InitialStart() // sets the difficulty, sets the section and starts it, activates all objectives and enables grapple movement
    {
        currentDifficultyValues = levelDifficulty switch
        {
            levelDifficultiesEnum.easy => easyValues,
            levelDifficultiesEnum.normal => normalValues,
            levelDifficultiesEnum.hard => hardValues,
            _ => normalValues
        };
        StealthHandler.instance.Reset_();
        //currentSection = sections[0];
        foreach (LevelObjective.ObjectiveData objectiveData in currentDifficultyValues.objectiveDataList) { currentObjectives.Add(new LevelObjective(objectiveData)); }
        SectionStart();
        foreach (LevelObjective objective in currentObjectives) { objective.StartObjective(); }
        Grapple.instance.SetMovementActive(true);
        levelRunning = true;
        if (currentSection.levelTip != string.Empty) { levelTipsLoop = StartCoroutine(LevelTipsLoop()); }
    } 

    void Update() // checks if the goal should be unlocked,
    // at the moment if you fail all required objectives it does not end the level as it would cause levels with collectables to end immediately
    {
        if (levelRunning)
        {
            bool objectivesComplete = ObjectivesValidate();
            if (!objectivesComplete)
            {
                //End(); return;
            }
            currentSection.goal.goalUnlocked = objectivesComplete;
        }
    }
    IEnumerator LevelTipsLoop() // Displays a level tip to the player every few seconds, defined in the section values
    {
        levelTipsLoopRunning = true;
        yield return new WaitForSeconds(currentSection.levelTipInitialWait);
        while (levelRunning)
        {
            if (levelTipsCondition) { yield break; }
            uiMessage.instance.New(currentSection.levelTip, uiDebug.str_Level);
            yield return new WaitForSeconds(currentSection.levelTipRepeatWait);
        }
        levelTipsLoopRunning = false;
    }
    // Validates if all required objectives are completed
    bool ObjectivesValidate()
    {
        List<bool> bools = new();
        foreach (LevelObjective objective in currentObjectives)
        {
            objective.currentValueUpdate();
            if (objective.requiredForCompletion) { bools.Add(objective.isCompleted()); }
        }
        return bools.All(x => x);
    }
    public StringBuilder debugGetStats()
    {
        return new StringBuilder()
            .Append(uiDebug.str_assetKey).Append(assetKey)
            .Append(uiDebug.str_inGameName).Append(inGameName)
            .Append(uiDebug.str_objectives).Append(debugGetObjectives())
            .Append(uiDebug.str_playerStartPos).Append(currentSection.playerStartPos.transform.position.ToStringBuilder());
    }
    StringBuilder debugGetObjectives()
    {
        StringBuilder a = new();
        foreach (LevelObjective objective in currentObjectives) { a.Append(objective.debugGetObjective()); }
        return a;
    }
    // Called by LevelLoader when unloading this level
    public void Unload()
    {
        foreach (LevelObjective objective in currentObjectives) { objective.StopObjective(); }
        ui.instance.uiObjectivesDestroy();
    }
    void End() // ends the level
    {
        levelRunning = false;
        if (levelTipsLoopRunning) { StopCoroutine(levelTipsLoop); }
        uiMessage.instance.New("End of " + assetKey, uiDebug.str_Level);
        LevelLoader.instance.UnloadLevel(this, true);// end of level sequence here
    }
    // logic to end the level if finishing the final section
    public void GoalReached()
    {
        if (SectionCount() == 0) { End(); }
        else { SectionEnd(); }
    }
    // ends the current section and goes to the next
    void SectionEnd()
    {
        if (SectionIndex() == SectionCount() - 1) { End(); }
        else { SectionStart(SectionIndex() + 1); }
    }
    public void SectionStart(int index = 0) // starts the given section
    {
        // sets the current section
        index = Mathf.Clamp(index, 0, SectionCount() - 1);
        currentSection = sections[index];
        ui.instance.uiSectionNumUpdate();
        // enables the section game object
        foreach (Section section in sections) { section.parent.SetActive(false); }
        sections[index].parent.SetActive(true);
        // sets the grapple distance
        Grapple.instance.GrappleDistanceSet(currentDifficultyValues.grappleMaxDistance);
        // allows the level start area to override the start rotation
        if (SectionIndex() > 0 || playerStartRotation == default) { playerStartRotation = currentSection.playerStartPos.transform.rotation.eulerAngles; }
        // activates the torch
        Player.instance.TorchSetActive(currentSection.torchEnabled);
        // starts torch tutorial if it is level 3 section 1
        if (assetKey == "Level3") 
        {
            if (SectionIndex() == 0) { StartCoroutine(WaitForTorch()); }
            if (SectionIndex() == 1)
            {
                uiMessage.instance.New("W.I.P.");
                Enemy.instance.StartPatrol();
            }
        }
        // teleports the player to the start of the section
        Player.instance.TeleportInstant(
            currentSection.playerStartPos != null ? currentSection.playerStartPos.transform.position : transform.TransformPoint(Vector3.zero),
            currentSection.playerStartPos != null ? playerStartRotation : Vector3.zero);
        uiMessage.instance.New(currentSection.startMsg, uiDebug.str_Level);
    }
    // public ways to get the current section index and the amount of sections
    public int SectionIndex() { return sections.IndexOf(currentSection); }
    public int SectionCount() { return sections.Count; }
    IEnumerator WaitForTorch() // waits for the player to enable the torch, as a tutorial
    {
        while (!Input.GetKey(KeyCode.F)) { yield return new WaitForEndOfFrame(); }
        levelTipsCondition = true;
    }
}