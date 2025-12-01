using System;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Profile))]
public class AgentAI : MonoBehaviour
{
    
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Profile profile;
    [SerializeField] private Zone currentZone;
    [SerializeField] private float waitTimer;
    
    public enum State { Idle, SeekingDestination, Moving, SatisfyingNeed }
    public State state = State.Idle;

    private void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        profile = GetComponent<Profile>();
    }

    private void Update()
    {
        //needs will increase over time
        profile.thirst += Time.deltaTime * 0.005f;
        profile.hunger += Time.deltaTime * 0.004f;
        profile.toiletNeed += Time.deltaTime * 0.003f;
        profile.boredom += Time.deltaTime * 0.001f;

        switch (state)
        {
            case State.Idle:
                CheckNeeds();
                break;
            case State.SeekingDestination:
                SetNewDestination();
                break;
            case State.Moving:
                CheckIfArrived();
                break;
            case State.SatisfyingNeed:
                SatisfyingNeeds();
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
    
    private void CheckNeeds()
    {
        if (profile.thirst >= profile.actionThreshold)
        {
            state = State.SeekingDestination;
        }
        else if (profile.toiletNeed >= profile.actionThreshold)
        {
            state = State.SeekingDestination;
        }
        else if (profile.boredom >= profile.actionThreshold && profile.hasTicket)
        {
            state = State.SeekingDestination;
        }
        else
        {
            Debug.Log("All is good!");
        }
    }
    
    private void SetNewDestination()
    {
        //get the most important need
        var need = GetMostCriticalNeedType(); 
        currentZone = WaypointManager.Instance.GetDestination(need, profile);
        
        //set target
        if (currentZone?.queueStartingPoint is not null)
        {
            agent.SetDestination(currentZone.queueStartingPoint.position);
            state = State.Moving;
        }
        else
        {
            //if no destination found just go back to idle for now
            //code should hopefully loop back and check again
            state = State.Idle;
        }
    }
    
    void CheckIfArrived()
    {
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            if (!agent.hasPath || agent.velocity.sqrMagnitude == 0f)
            {
                state = State.SatisfyingNeed;
                waitTimer = UnityEngine.Random.Range(5f, 15f); 
            }
        }
    }

    void SatisfyingNeeds()
    {
        waitTimer -= Time.deltaTime;
        
        if (waitTimer <= 0f)
        {
            // Reduce the need and reset
            var needType = GetMostCriticalNeedType();
            profile.SatisfyNeed(needType);
            
            //cycle back and decide what need needs to be filled next.
            state = State.Idle;
        }
    }
    
    private string GetMostCriticalNeedType()
    {
        //idk how to do this so im leaving it for now.
        return "Toilet"; 
    }
}