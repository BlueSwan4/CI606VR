using UnityEngine;

public class Destination : MonoBehaviour
{
    public enum DestinationType { Hunger, Bladder, Boredom, None }
    public DestinationType destinationType;

    //for the disabled toilets
    public bool restrictedAccess; 
    public bool isOccupied;
}
