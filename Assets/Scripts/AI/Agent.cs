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
            //re-decide if we have there is no destination OR if an event just started and the ai isnt headed there yet
            var headedToConcert = _currentDestination is not null && _currentDestination.destinationType == Destination.DestinationType.Boredom;
            if (_currentDestination is null || (EventManager.IsEventActive && !headedToConcert))
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
        Destination.DestinationType destinationType;
        if (EventManager.IsEventActive)
        {
            //force everyone to boredom to meet in concert hall
            destinationType = Destination.DestinationType.Boredom;
        }
        else
        {
            var hunger = _profile.hunger * hungerWeight;
            var bladder = _profile.toiletNeed * bladderWeight;
            var boredom = _profile.boredom * boredomWeight;
        
            var highestNeed = Mathf.Max(hunger, bladder, boredom);
            if (highestNeed < Profile.ActionThreshold)
            {
                Wander();
                return; 
            }

            if (Mathf.Approximately(highestNeed, bladder)) destinationType = Destination.DestinationType.Bladder;
            else if (Mathf.Approximately(highestNeed, hunger)) destinationType = Destination.DestinationType.Hunger;
            else destinationType = Destination.DestinationType.Boredom;
        }
        
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
        var validDestinations = allDestinations.Where(dest => dest.destinationType == targetDestination).ToList();

        //only filter by occupied if there is no event going on
        //lets multiple agents occupy the same destination
        if (!EventManager.IsEventActive)
            validDestinations = validDestinations.Where(dest => !dest.isOccupied).ToList();

        //general agents cannot access restricted areas
        if (!_profile.isDisabled)
            validDestinations = validDestinations.Where(dest => !dest.restrictedAccess).ToList();
        //only disabled agents can access restricted areas for toilet needs
        else if (targetDestination == Destination.DestinationType.Bladder)
            validDestinations = validDestinations.Where(dest => dest.restrictedAccess).ToList();

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
        
        var needIsMet = (_profile.toiletNeed == 0 || _profile.hunger == 0 || _profile.boredom == 0);
        
        if (needIsMet && !EventManager.IsEventActive)
        {
            _currentDestination.isOccupied = false;
            _currentDestination = null;
            _agent.isStopped = false;
            Wander();
        }
        //agent will leave if theyre at the wrong destination and enter the concert hall
        else if (EventManager.IsEventActive && _currentDestination.destinationType != Destination.DestinationType.Boredom)
        {
            _currentDestination.isOccupied = false;
            _currentDestination = null;
            _agent.isStopped = false;
            //next update tick will trigger DecideAction since currentDest is null
        }
    }

    private void Wander()
    {
        var randomDir = Random.insideUnitSphere * 25;
        randomDir += transform.position;
        
        //sample point on the mesh within radius
        if(NavMesh.SamplePosition(randomDir, out var hit, 100, NavMesh.AllAreas))
            _agent.SetDestination(hit.position);
    }
}
