using UnityEngine;
using TMPro;

public class uiObjective : MonoBehaviour
{
    public TextMeshProUGUI uiObjectiveText {  get; private set; }
    [SerializeField] TextMeshProUGUI uiObjectiveMarker;
    public bool objectiveCompleted;
    [SerializeField] [Tooltip("For use with unicode characters")] string
        tickText,
        crossText;
    public LevelObjective objective;
    void Awake()
    {
        uiObjectiveText = GetComponent<TextMeshProUGUI>();
        if (uiObjectiveMarker == null) { uiObjectiveMarker = transform.Find("uiObjectiveMarker").GetComponent<TextMeshProUGUI>(); }
    }
    void Start()
    {
        objective.uiObjectiveRefresh();
    }
    public void Refresh(LevelObjective.objectiveTypes type, double currentValue, double completionValue, bool uiValueCountDown)
    {
        if (LevelLoader.instance.inLevel)
        {
            string text = name;
            string customReturn = string.Empty;
            string unit = string.Empty;
            switch (type)
            {
                case LevelObjective.objectiveTypes.grappleUses:
                    {
                        text = "Grapple Uses = ";
                        break;
                    }
                case LevelObjective.objectiveTypes.grappleDistance:
                    {
                        text = "Grapple Distance = ";
                        unit = "m";
                        break;
                    }
                case LevelObjective.objectiveTypes.timeLimit:
                    {
                        text = "Time = ";
                        customReturn = (completionValue - currentValue).ConvertTime() + " left";
                        break;
                    }
                case LevelObjective.objectiveTypes.collectables:
                    {
                        text = "Key = ";
                        break;
                    }
            }

            uiObjectiveText.text = text + ((customReturn != string.Empty) ? customReturn :
                (uiValueCountDown ? completionValue - currentValue + unit + " left" : currentValue + unit + " / " + completionValue + unit));
            name = "uiObjective - " + objective.type;
            uiObjectiveMarker.text = objective.completed ? tickText : crossText;
            uiObjectiveMarker.color = objective.completed ? Color.green : Color.red;
        }
    }
}