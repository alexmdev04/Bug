using System;
using System.Text;
using UnityEngine;

[Serializable]
public class LevelObjective
{
    public LevelObjective(ObjectiveData objectiveData)
    {
        type = objectiveData.type;
        completionType = objectiveData.completionType;
        requiredForCompletion = objectiveData.requiredForCompletion;
        uiValueCountDown = objectiveData.uiValueCountDown;
        completionValue = objectiveData.completionValue;
        scoreAwarded = objectiveData.scoreAwarded;
    }
    [Serializable] public struct ObjectiveData
    {
        public objectiveTypes type;
        public objectiveCompletionTypes completionType;
        public bool requiredForCompletion;
        public bool uiValueCountDown;
        public double completionValue;
        public float scoreAwarded;
    }
    public enum objectiveTypes
    {
        grappleUses,
        grappleDistance,
        timeLimit,
        alertLevel,
        collectables
    }
    public enum objectiveCompletionTypes
    {
        lessThanCompletionValue,
        lessThanEqualToCompletionValue,
        greaterThanCompletetionValue,
        greaterThanEqualToCompletetionValue
    }
    public objectiveTypes type { get; private set; }
    objectiveCompletionTypes completionType;
    public bool requiredForCompletion { get; private set; }
    bool uiValueCountDown;
    double completionValue;
    public bool completed { get; private set; }
    double currentValue
    {
        get
        {
            return _currentValue;
        }
        set
        {
            _currentValue = value;
            completed = isCompleted();
            if (LevelLoader.instance.inLevel)
            {
                uiObjectiveRefresh();
            }
        }
    }
    double _currentValue = 0d;
    double scoreAwarded;
    public uiObjective uiObjective;

    public void StartObjective()
    {
        currentValue = 0;
        switch (type)
        {
            case objectiveTypes.grappleUses:
                {
                    completionValue = (int)completionValue;
                    Grapple.instance.finished.AddListener(currentValueEditAuto);
                    break;
                }
            case objectiveTypes.grappleDistance:
                {
                    Grapple.instance.finished.AddListener(currentValueEditAuto);
                    break;
                }
            case objectiveTypes.timeLimit:
                {
                    break;
                }
            case objectiveTypes.alertLevel:
                {
                    break;
                }
            case objectiveTypes.collectables:
                {
                    LevelLoader.instance.levelCurrent.collectableCollected.AddListener(currentValueEditAuto);
                    break;
                }
        }
    }
    public void StopObjective()
    {
        Grapple.instance.fired.RemoveListener(currentValueEditAuto);
        Grapple.instance.finished.RemoveListener(currentValueEditAuto);
    }
    void currentValueSet(double value)
    {
        currentValue = value;
    }
    void currentValueEdit(double value)
    {

        if (currentValue >= completionValue)
        {
            //Debug.Log(name + " completion value reached (" + completionValue + ")");
            currentValue = completionValue;
            return;
        }
        currentValue += value;
    }
    void currentValueEditAuto() // remove listener does not work when taking an argument
    {
        switch (type)
        {
            case objectiveTypes.grappleUses:
                {
                    currentValueEdit(1);
                    break;
                }
            case objectiveTypes.grappleDistance:
                {
                    currentValueEdit(Grapple.instance.distanceTravelled);
                    break;
                }
            case objectiveTypes.timeLimit:
                {
                    break;
                }
            case objectiveTypes.alertLevel:
                {
                    break;
                }
            case objectiveTypes.collectables:
                {
                    currentValueEdit(1);
                    break;
                }

        }
    }
    public void currentValueUpdate()
    {
        if (type == objectiveTypes.timeLimit && currentValue < completionValue)
        {
            currentValue += Time.deltaTime;
        }
    }
    public bool isCompleted()
    {
        bool value;
        switch (completionType)
        {
            case objectiveCompletionTypes.lessThanCompletionValue:
                {
                    value = currentValue < completionValue;
                    break;
                }
            case objectiveCompletionTypes.lessThanEqualToCompletionValue:
                {
                    value = currentValue <= completionValue;
                    break;
                }
            case objectiveCompletionTypes.greaterThanCompletetionValue:
                {
                    value = currentValue > completionValue;
                    break;
                }
            case objectiveCompletionTypes.greaterThanEqualToCompletetionValue:
                {
                    value = currentValue >= completionValue;
                    break;
                }
            default:
                {
                    value = false;
                    break;
                }
        }
        return value;
    }
    public void uiObjectiveRefresh()
    {
        uiObjective.Refresh(type, currentValue, completionValue, uiValueCountDown);
    }
    public StringBuilder debugGetObjective()
    {
        return new StringBuilder(uiDebug.str_NewLine).Append(uiDebug.str_dash).Append(type).Append(uiDebug.str_equals).Append(currentValue).Append(uiDebug.str_divide).Append(completionValue);
    }

} 