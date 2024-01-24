using System.Collections;
using System.Text;
using UnityEngine;

public class StealthHandler : MonoBehaviour
{
    public static StealthHandler instance { get; private set; }
    public StealthIndicator indicator { get; private set; }
    public enum stealthLevelEnum
    {
        hidden,
        spotted,
        fullAlert
    }

    public bool playerVisible { get; private set; }
    public stealthLevelEnum stealthLevel { get; private set; }
    public float stealthLevelTimer { get; private set; }

    [Header("Time in seconds to move between stealth levels")]
    public float toLevelSpotted = 1f;
    public float toLevelFullAlert = 3f;
    //[SerializeField] float decreaseRate = 0f;
    [SerializeField] float fromLevelSpottedCooldown = 5f;
    float fromLevelSpottedCooldownTimer;

    [Header("Time until game over when spotted")]
    [SerializeField] float toGameOver;
    float toGameOverTimer;
    bool toGameOverSequenceRunning;

    //[Header("Stats")]
    //public int statEnemiesSeenBy;
    //public int statTimesSpotted;
    //public float statTimeVisibleFor;
    void Awake()
    {
        instance = this;
        indicator = GetComponent<StealthIndicator>();
    }
    void Update()
    {
        StealthLevelUpdate();
    }
    public void SetPlayerVisible(bool state)
    {
        playerVisible = state;
    }
    void StealthLevelUpdate()
    {
        if (playerVisible && !toGameOverSequenceRunning)
        {
            //statTimeVisibleFor += Time.deltaTime;
            switch (stealthLevel)
            {
                case stealthLevelEnum.hidden:
                    {
                        if (stealthLevelTimer < toLevelSpotted)
                        {
                            stealthLevelTimer += Time.deltaTime;
                        }
                        else
                        {
                            stealthLevel = stealthLevelEnum.spotted;
                            fromLevelSpottedCooldownTimer = fromLevelSpottedCooldown;
                            //statTimesSpotted += 1;
                        }
                        break;
                    }
                case stealthLevelEnum.spotted:
                    {
                        if (stealthLevelTimer < toLevelFullAlert)
                        {
                            stealthLevelTimer += Time.deltaTime;
                        }
                        else
                        {
                            stealthLevel = stealthLevelEnum.fullAlert;
                        }
                        break;
                    }
                case stealthLevelEnum.fullAlert:
                    {
                        StartCoroutine(GameOverSequence());
                        break;
                    }
            }
        }
        else // lowering alert level
        {
            switch (stealthLevel)
            {
                case stealthLevelEnum.hidden:
                    {
                        if (stealthLevelTimer > 0)
                        {
                            stealthLevelTimer -= Time.deltaTime;
                        }
                        break;
                    }
                case stealthLevelEnum.spotted:
                    {
                        if (fromLevelSpottedCooldownTimer > 0)
                        {
                            fromLevelSpottedCooldownTimer -= Time.deltaTime;
                            
                        }
                        else
                        {
                            if (stealthLevelTimer > 0)
                            {
                                stealthLevelTimer -= Time.deltaTime;
                            }
                            else 
                            {
                                stealthLevel = stealthLevelEnum.hidden;
                            }
                        }
                        break;
                    }
            }
        }
    }
    IEnumerator GameOverSequence()
    {
        toGameOverSequenceRunning = true;
        toGameOverTimer = toGameOver;
        while (toGameOverTimer > 0)
        {
            if (!toGameOverSequenceRunning) { Debug.Log("stopped gameover"); yield break; }
            toGameOverTimer -= Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        uiMessage.instance.New("Game Over", uiDebug.str_StealthHandler);
        uiDebugConsole.instance.InternalCommandCall("menu");
    }
    public void Reset_()
    {
        toGameOverTimer = 0; 
        toGameOverSequenceRunning = false;
        fromLevelSpottedCooldownTimer = 0;
        stealthLevelTimer = 0;
        stealthLevel = stealthLevelEnum.hidden;
        indicator.VignetteReset();
    }
    public StringBuilder debugGetStats()
    {
        return new StringBuilder()
            .Append(uiDebug.str_stealthTitle)
            .Append(uiDebug.str_stealthLevel).Append(stealthLevel.ToString())
            .Append(uiDebug.str_stealthTimer).Append(stealthLevelTimer).Append(uiDebug.str_divide).Append(toLevelSpotted + toLevelFullAlert)
            .Append(uiDebug.str_playerVisible).Append(playerVisible.ToString())
            .Append(uiDebug.str_toLevelSpotted).Append(toLevelSpotted.ToString())
            .Append(uiDebug.str_toLevelFullAlert).Append(toLevelFullAlert.ToString())
            .Append(uiDebug.str_fromLevelSpotted).Append((fromLevelSpottedCooldown - fromLevelSpottedCooldownTimer).ToString()).Append(uiDebug.str_divide).Append(fromLevelSpottedCooldown.ToString())
            .Append(uiDebug.str_toGameOver).Append((toGameOver - toGameOverTimer).ToString()).Append(uiDebug.str_divide).Append(toGameOver.ToString())
            .Append(uiDebug.str_toGameOverSequnceRunning).Append(toGameOverSequenceRunning.ToString());
    }
}