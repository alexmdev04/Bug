using System.Collections;
using UnityEngine;

public class MenuLevel : MonoBehaviour
{
    [HideInInspector] public bool skipTutorial;
    int tutorialPlayerMoved;
    [SerializeField] float tutorialReminderClock = 5f;
    [SerializeField] float tutorialTextWaitTime = 3.25f;
    [SerializeField] GameObject levelSelectAreaBlockade;
    WaitForSeconds tutorialWait;
    [SerializeField] LevelStartCube[] levelStartCubes;
    void Awake()
    {
        tutorialWait = new(tutorialTextWaitTime);
    }
    void Start()
    {
        if (skipTutorial) { StartCoroutine(TutorialFinished()); }
        else { StartCoroutine(TutorialStart()); }
    }
    void Update()
    {
        if ((Input.GetKeyDown(KeyCode.End) || Input.GetKeyDown(KeyCode.Backspace))  && !skipTutorial && InputHandler.instance.active) { SkipTutorials(); }
    }

    public IEnumerator TutorialStart()
    {
        if (skipTutorial) { yield break; }
        uiMessage.instance.New("Tutorial Start", uiDebug.str_MenuLevel);
        Grapple.instance.SetMovementActive(false);

        yield return tutorialWait;
        uiMessage.instance.New("Press End or Backspace to skip this tutorial", uiDebug.str_MenuLevel);

        yield return tutorialWait;
        uiMessage.instance.New("Welcome to Project Bug!", uiDebug.str_MenuLevel);

        yield return tutorialWait;
        uiMessage.instance.New("A game about grappling and sneaking towards a goal.", uiDebug.str_MenuLevel);

        yield return tutorialWait;
        StartCoroutine(TutorialLook());
    }
    IEnumerator TutorialLook()
    {
        if (skipTutorial) { yield break; }
        uiMessage.instance.New("First, let's learn how to look.", uiDebug.str_MenuLevel);

        yield return tutorialWait;
        uiMessage.instance.New("Move your mouse to look around", uiDebug.str_MenuLevel);

        float mouseRotationThreshhold = 1000f;
        float reminderClockMouse = 0f;
        do
        {
            if (reminderClockMouse > tutorialReminderClock)
            {
                uiMessage.instance.New("Move your mouse to look around", uiDebug.str_MenuLevel);
                reminderClockMouse = 0f;
            }
            float mouseRotationMean = Mathf.Abs((Player.instance.mouseRotation.x + Player.instance.mouseRotation.y) / 2);
            mouseRotationThreshhold -= mouseRotationMean;
            reminderClockMouse += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        while (mouseRotationThreshhold > 0);
        uiMessage.instance.New("Nice, now you know how to look!", uiDebug.str_MenuLevel);

        yield return tutorialWait;
        uiMessage.instance.New("You can change your sensitivity by hitting ESC", uiDebug.str_MenuLevel);

        yield return tutorialWait;
        StartCoroutine(TutorialGrappleDestination());
    }
    IEnumerator TutorialGrappleDestination()
    {
        if (skipTutorial) { yield break; }
        uiMessage.instance.New("Next, lets learn how to grapple.", uiDebug.str_MenuLevel);

        yield return tutorialWait;
        uiMessage.instance.New("To choose your destination, look at a wall and hold left click.", uiDebug.str_MenuLevel);
        float grappleHoldThreshhold = 0f;
        float reminderClockGrappleDestination = 0f;
        do
        {
            if (reminderClockGrappleDestination > tutorialReminderClock)
            {
                uiMessage.instance.New("Look at a wall and hold left click.", uiDebug.str_MenuLevel);
                reminderClockGrappleDestination = 0f;
            }
            if (InputHandler.instance.input.Player.Grapple.IsPressed())
            {
                grappleHoldThreshhold += Time.deltaTime;
            }
            reminderClockGrappleDestination += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        while (grappleHoldThreshhold < 2f);
        uiMessage.instance.New("See the sphere that appears?", uiDebug.str_MenuLevel);

        yield return tutorialWait;
        uiMessage.instance.New("That is where you will go when you release the grapple button.", uiDebug.str_MenuLevel);

        yield return tutorialWait;
        uiMessage.instance.New("It's colour indicates if the destination is valid.", uiDebug.str_MenuLevel);

        yield return tutorialWait;
        StartCoroutine(TutorialGrappleMovement());
    }

    IEnumerator TutorialGrappleMovement()
    {
        if (skipTutorial) { yield break; }
        uiMessage.instance.New("Now let's try and move around the room!", uiDebug.str_MenuLevel);

        yield return tutorialWait;
        uiMessage.instance.New("Use what you've learned to move 3 times.", uiDebug.str_MenuLevel);

        Grapple.instance.SetMovementActive(true);
        Grapple.instance.finished.AddListener(TutorialPlayerMoved);

        float reminderClockMovement = 0f;
        do
        {
            if (reminderClockMovement > tutorialReminderClock)
            {
                uiMessage.instance.New("Look at a wall, hold left click, then release to move.", uiDebug.str_MenuLevel);
                reminderClockMovement = 0f;
            }
            reminderClockMovement += Time.deltaTime;
            yield return new WaitForEndOfFrame();
        }
        while (tutorialPlayerMoved < 3);
        Grapple.instance.finished.RemoveListener(TutorialPlayerMoved);
        uiMessage.instance.New("Good job!", uiDebug.str_MenuLevel);

        yield return tutorialWait;
        uiMessage.instance.New("Now you can freely move around the game!", uiDebug.str_MenuLevel);

        yield return tutorialWait;
        StartCoroutine(TutorialRadar());
    }
    void TutorialPlayerMoved()
    {
        tutorialPlayerMoved++;
    }
    IEnumerator TutorialRadar()
    {
        if (skipTutorial) { yield break; }
        yield return new WaitForEndOfFrame();
        StartCoroutine(TutorialFinished());

    }
    IEnumerator TutorialFinished()
    {
        yield return new WaitForEndOfFrame();
        uiMessage.instance.New("The level select area has opened!", uiDebug.str_MenuLevel);
        levelSelectAreaBlockade.GetComponent<Animator>().Play("levelSelectAreaBlockadeOpen");
        Grapple.instance.SetMovementActive(true);
    }
    public void ForceRestartTutorial()
    {
        StartCoroutine(TutorialStart());
    }
    void SkipTutorials()
    {
        skipTutorial = true;
        StopAllCoroutines();
        Grapple.instance.finished.RemoveListener(TutorialPlayerMoved);
        StartCoroutine(TutorialFinished());
        uiMessage.instance.New("Tutorials Skipped!", uiDebug.str_MenuLevel);
    }
}