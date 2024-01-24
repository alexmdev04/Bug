using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
public class Enemy : MonoBehaviour
{
    public static Enemy instance { get; private set; }
    NavMeshAgent 
        agent;
    Animator 
        animator;
    bool
        wait,
        idle = true,
        patrolRunning;
    public float
        turnSpeed = 100f,
        distanceToStop = 3f,
        decelerationSpeed = 1f,
        moveSpeedLowClamp = 1f;
    public bool
        loopPatrol = true,
        patrolStartComplete;
    public int
        patrolInstructionCurrent,
        patrolInstructionLoopFrom;
    public List<patrolInstruction> 
        patrolInstructions;
    //public Quaternion recentRotation;

    public enum patrolAction
    {
        turnLeft90,
        turnLeft180,
        turnRight90,
        turnRight180,
        walkTo
    }
    [Serializable] public struct patrolInstruction
    {
        public patrolAction action;
        public Vector3 walkTo;
        public float waitAtEnd;
    }
    void Awake()
    {
        instance = this;
        animator = GetComponent<Animator>();
        agent = GetComponent<NavMeshAgent>();
    }
    void Start()
    {
        //recentRotation = transform.rotation;
        patrolStartComplete = false;
    }
    void Update()
    {
        agent.updateRotation = false;
        //if (!wait) { transform.rotation = recentRotation; }
        if (Input.GetKeyDown(KeyCode.F7) && !patrolRunning) { StartCoroutine(Patrol()); };
    }
    IEnumerator Turn(bool turnLeft, bool turn180, float waitAtEnd = 0f)
    {
        // if another patrolaction is running or the player is not idle then prevent this from starting
        if (wait || !idle) { yield break; } wait = true;

        Vector3 enemyEulerAngles = transform.localRotation.eulerAngles;
        enemyEulerAngles.y += (turn180 ? 180f : 90f) * (turnLeft ? -1 : 1);
        enemyEulerAngles = enemyEulerAngles.Round();
        Quaternion targetRotation = Quaternion.Euler(enemyEulerAngles);

        // rotate enemy towards targetRotation
        animator.SetTrigger((turnLeft ? "TurnLeft" : "TurnRight") + (turn180 ? "180" : "90"));
        do { transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRotation, Time.deltaTime * turnSpeed); yield return new WaitForEndOfFrame(); Debug.Log("from: " + transform.localRotation.eulerAngles.y + ", to: " + targetRotation.eulerAngles.y); }
        while (MathF.Round(transform.localRotation.eulerAngles.y, 2) != MathF.Round(targetRotation.eulerAngles.y, 2));

        //recentRotation = targetRotation;
        yield return new WaitForSeconds(waitAtEnd);
        wait = false;
        yield break;
    }
    IEnumerator WalkTo(Vector3 targetPosition, float waitAtEnd = 0f)
    {
        // if another patrolaction is running or the player is not idle already then prevent this from starting
        if (wait || !idle) { yield break; } wait = true; idle = false; 

        // start walking until near destination
        agent.destination = transform.parent.TransformPoint(targetPosition);
        animator.SetTrigger("ToWalking");
        while (Vector3.Distance(transform.localPosition, targetPosition) > distanceToStop)
        { yield return new WaitForEndOfFrame(); }

        // when near start toIdle sequence
        animator.SetTrigger("ToIdle");
        // interpolate the enemy speed according to the distance to the target this is to make the StopWalking animation match better
        float startAgentSpeed = agent.speed;
        float startDistance = Vector3.Distance(transform.localPosition, targetPosition);
        while (transform.localPosition != targetPosition) 
        { agent.speed = Mathf.Lerp(startAgentSpeed, moveSpeedLowClamp, 1 - (Vector3.Distance(transform.localPosition, targetPosition) / startDistance)); yield return new WaitForEndOfFrame(); }

        yield return new WaitForSeconds(waitAtEnd);
        agent.speed = startAgentSpeed;
        wait = false;
        idle = true;
        yield break;
    }
    IEnumerator Patrol()
    {
        patrolRunning = true;
        while (patrolRunning)
        {
            for (int i = 0; i < patrolInstructions.Count; i++)
            {
                patrolInstructionCurrent = i;
                if (i == patrolInstructionLoopFrom) { patrolStartComplete = true; }
                if (patrolStartComplete && i < patrolInstructionLoopFrom) { i = patrolInstructionLoopFrom; }
                PatrolAction(patrolInstructions[i]);
                while (wait) { yield return new WaitForEndOfFrame(); }
            }
            //foreach (patrolInstruction instruction in patrolInstructions)
            //{
            //    patrolInstructionCurrent++;
            //    PatrolAction(instruction);
            //    while (wait) { yield return new WaitForEndOfFrame(); }
            //}
            patrolRunning = loopPatrol;
            patrolInstructionCurrent = 0;
            yield return new WaitForEndOfFrame();
        }
        Debug.Log("Patrol Loop Stopped");
        yield break;
    }
    void PatrolAction(patrolInstruction instruction)
    {
        //Debug.Log(instruction.action.ToString());
        switch (instruction.action)
        {
            case patrolAction.turnLeft90: { StartCoroutine(Turn(true, false, instruction.waitAtEnd)); break; }
            case patrolAction.turnLeft180: { StartCoroutine(Turn(true, true, instruction.waitAtEnd)); break; }
            case patrolAction.turnRight90: { StartCoroutine(Turn(false, false, instruction.waitAtEnd)); break; }
            case patrolAction.turnRight180: { StartCoroutine(Turn(false, true, instruction.waitAtEnd)); break; }
            case patrolAction.walkTo: 
                { 
                    instruction.walkTo.y = transform.localPosition.y; 
                    StartCoroutine(WalkTo(instruction.walkTo, instruction.waitAtEnd)); break;
                }
        }
    }
    public void StartPatrol()
    {
        StartCoroutine(Patrol());
    }
}