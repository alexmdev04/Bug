using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ui : MonoBehaviour
{
    public static ui instance { get; private set; }
    public uiGrapple grapple { get; private set; }
    public uiRadar radar { get; private set; }
    public uiCrosshair crosshair { get; private set; }
    public uiSettings settings { get; private set; }
    public List<uiObjective> 
        uiObjectives;
    public bool 
        uiFadeToBlack;
    [SerializeField] TextMeshProUGUI 
        uiLevelNum,
        uiSectionNum;
    [SerializeField] GameObject 
        uiObjectivePrefab,
        uiObjectivesParent;
    [SerializeField] Image 
        uiFade;
    [SerializeField] float 
        uiFadeInSpeed,
        uiFadeOutSpeed;
    public float uiFadeAlpha;
    const string
        uiLevelNumText = "Level ",
        uiSectionNumText = "Section ",
        uiLevelNumDashText = "-";
    void Awake()
    {
        instance = this;
        grapple = GetComponentInChildren<uiGrapple>();
        radar = GetComponentInChildren<uiRadar>();
        crosshair = GetComponentInChildren<uiCrosshair>();
        settings = GetComponentInChildren<uiSettings>();
        settings.gameObject.SetActive(false);
        LevelLoader.instance.levelLoaded.AddListener(Refresh);
    }
    void Start()
    {
        InvokeRepeating(nameof(uiObjectivesRefresh), 1f, 1f);
    }
    void Update()
    {
        radar.gameObject.SetActive(LevelLoader.instance.inLevel);
        if (Input.GetKeyDown(KeyCode.Escape)) { settings.gameObject.SetActive(!settings.gameObject.activeSelf); }
        uiFadeUpdate();
    }
    /// <summary>
    /// Updates all ui elements currently needed
    /// </summary>
    void Refresh()
    {
        uiObjectivesBuild();
        uiObjectivesRefresh();
        uiLevelNumUpdate();
        uiSectionNumUpdate();
    }
    /// <summary>
    /// Creates ui representations for all active objectives and deletes any currently present.
    /// </summary>
    public void uiObjectivesBuild()
    {
        foreach (uiObjective uiObjective in uiObjectives) { Destroy(uiObjective.gameObject); }
        uiObjectives.Clear();
        foreach (LevelObjective objective in LevelLoader.instance.levelCurrent.currentObjectives)
        {
            uiObjective uiObjectiveNew = Instantiate(uiObjectivePrefab, uiObjectivesParent.transform).GetComponent<uiObjective>();
            objective.uiObjective = uiObjectiveNew;
            uiObjectiveNew.objective = objective;
            uiObjectives.Add(uiObjectiveNew);
            uiObjectiveNew.gameObject.SetActive(true);
        }
    }
    /// <summary>
    /// Destorys all uiObjective objects
    /// </summary>
    public void uiObjectivesDestroy()
    {
        foreach (uiObjective uiObjective in uiObjectives) { Destroy(uiObjective.gameObject); }
        uiObjectives.Clear();
    }
    /// <summary>
    /// Keeps the text displayed on all uiObjectives up to date with current values
    /// </summary>
    void uiObjectivesRefresh()
    {
        if (LevelLoader.instance.inLevel)
        {
            for (int i = 0; i < LevelLoader.instance.levelCurrent.currentObjectives.Count; i++)
            {
                LevelLoader.instance.levelCurrent.currentObjectives[i].uiObjectiveRefresh();
            }
        }
    }
    /// <summary>
    /// Updates the level number text to the current levels number
    /// </summary>
    void uiLevelNumUpdate()
    {
        uiLevelNum.text = LevelLoader.instance.inLevel ? uiLevelNumText + LevelLoader.instance.levelCurrent.levelNumber + uiLevelNumDashText + LevelLoader.instance.levelCurrent.levelDifficulty.ToString().ToUpper().ToCharArray()[0] : string.Empty;
    }
    /// <summary>
    /// Updates the section number text to the section the current level is in
    /// </summary>
    public void uiSectionNumUpdate()
    {
        if (LevelLoader.instance.inLevel)
        {
            uiSectionNum.text = (LevelLoader.instance.levelCurrent.SectionCount() > 1) ? uiSectionNumText + (LevelLoader.instance.levelCurrent.SectionIndex() + 1) : string.Empty;
        }
    }
    /// <summary>
    /// Updates the color of the ui fade element on screen used to hide the screen, uiFadeToBlack controls the direction of the fade
    /// </summary>
    void uiFadeUpdate()
    {
        uiFade.color = new Color(0, 0, 0, Mathf.Clamp(uiFade.color.a + (uiFadeToBlack ? Time.deltaTime * uiFadeInSpeed : -Time.deltaTime * uiFadeOutSpeed), 0f, 1f));
        uiFadeAlpha = uiFade.color.a;
    }
}