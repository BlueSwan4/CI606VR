using System.Linq;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent), typeof(Profile))]
public class Agent : MonoBehaviour
{
    private Profile _profile;
    private NavMeshAgent _agent;
    private Destination _currentDestination;

    public float tickRate = 0.005f; //how fast the agents needs will increase overtime (per second)
    public float decisionRate = 2f; //how often the agent will check its needs
    private float _nextDecisionTime;
    
    [Header("Priority Weights")]
    public float bladderWeight = 1.5f;
    public float hungerWeight = 1f;
    public float boredomWeight = .5f;

    public float needFulfillmentRate = 2f; //how fast the need drops when satisfied
    public float arriveDistance = .5f;

    private void Start()
    {
        _profile = GetComponent<Profile>();
        _agent = GetComponent<NavMeshAgent>();
        _nextDecisionTime = Time.time + Random.Range(0f, decisionRate);
        
        var allDestinations = FindObjectsByType<Destination>(FindObjectsSortMode.None);
        DecideAction(allDestinations);
    }

    private void Update()
    {
        Tick();

        if (Time.time >= _nextDecisionTime)
        {
            if (_currentDestination is null)
            {
                var allDestinations = FindObjectsByType<Destination>(FindObjectsSortMode.None);
                DecideAction(allDestinations);
            }
            _nextDecisionTime = Time.time + decisionRate;
        }

        if (_currentDestination is not null && _agent.remainingDistance <= arriveDistance && !_agent.pathPending)
            FulfillCurrentNeed();
    }

    private void Tick()
    {
        //fat ass block of code please find another way to write this later i hate this
        //increase agents needs over time
        _profile.hunger += tickRate * Time.deltaTime;
        _profile.toiletNeed += tickRate * Time.deltaTime;
        _profile.boredom += tickRate * Time.deltaTime;
        
        _profile.hunger = Mathf.Clamp01(_profile.hunger);
        _profile.toiletNeed = Mathf.Clamp01(_profile.toiletNeed);
        _profile.boredom = Mathf.Clamp01(_profile.boredom);
    }

    private void DecideAction(Destination[] allDestinations)
    {
        //determine highest priority based on weights
        var hunger = _profile.hunger * hungerWeight;
        var bladder = _profile.toiletNeed * bladderWeight;
        var boredom = _profile.boredom * boredomWeight;
        
        var highestNeed = Mathf.Max(hunger, bladder, boredom);
        if (highestNeed < Profile.ActionThreshold)
            Wander();
        
        //get destination based on highest priority
        Destination.DestinationType destinationType;
        if (Mathf.Approximately(highestNeed, bladder)) destinationType = Destination.DestinationType.Bladder;
        else if (Mathf.Approximately(highestNeed, hunger)) destinationType = Destination.DestinationType.Hunger;
        else destinationType = Destination.DestinationType.Boredom;
        
        _currentDestination = FindAvailableLocation(allDestinations, destinationType);
        if (_currentDestination is not null)
        {
            _currentDestination.isOccupied = true;
            _agent.SetDestination(_currentDestination.transform.position);
        }
        else
            Wander();
    }

    private Destination FindAvailableLocation(Destination[] allDestinations, Destination.DestinationType targetDestination)
    {
        //filter locations based on needs
        var validDestinations = allDestinations.Where(dest => dest.destinationType == targetDestination).ToList();
        validDestinations = validDestinations.Where(dest => !dest.isOccupied).ToList();

        //disability constraints
        if (targetDestination == Destination.DestinationType.Bladder)
        {
            //if the agent is disabled only go to disabled toilets, else just go to normal toilets
            validDestinations = _profile.isDisabled ? validDestinations.Where(dest => dest.restrictedAccess).ToList() //only place that's restricted is disabled toilets
                : validDestinations.Where(dest => !dest.restrictedAccess).ToList(); //every other toilet is unrestricted
        }

        if (!validDestinations.Any()) return null;
        
        validDestinations.Sort((a, b) => Vector3.Distance(a.transform.position, transform.position).CompareTo(
            Vector3.Distance(b.transform.position, transform.position)));
        
        return validDestinations.FirstOrDefault();
    }
    
    private void FulfillCurrentNeed()
    {
        //stop movement and satisfy needs
        _agent.isStopped = true;
        
        if (_currentDestination.destinationType == Destination.DestinationType.Bladder)
            _profile.toiletNeed = Mathf.MoveTowards(_profile.toiletNeed, 0f, needFulfillmentRate * Time.deltaTime);
        else if (_currentDestination.destinationType == Destination.DestinationType.Hunger)
            _profile.hunger = Mathf.MoveTowards(_profile.hunger, 0f, needFulfillmentRate * Time.deltaTime);
        else if (_currentDestination.destinationType == Destination.DestinationType.Boredom)
            _profile.boredom = Mathf.MoveTowards(_profile.boredom, 0f, needFulfillmentRate * Time.deltaTime);
        
        //once needs are met start looking for next need
        if (_profile.toiletNeed == 0 || _profile.hunger == 0 || _profile.boredom == 0)
        {
            _currentDestination.isOccupied = false;
            _currentDestination = null;
            _agent.isStopped = false;
            Wander();
        }
    }

    private void Wander()
    {
        var randomDir = Random.insideUnitSphere * 100;
        randomDir += transform.position;
        
        //sample point on the mesh within radius
        if(NavMesh.SamplePosition(randomDir, out var hit, 100, NavMesh.AllAreas))
            _agent.SetDestination(hit.position);
    }
}
