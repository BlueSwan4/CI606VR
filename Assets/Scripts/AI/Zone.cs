using UnityEngine;

public class Zone : MonoBehaviour
{
    public enum ZoneType { Toilet, Cafe, Seating, Entrance, DisabledToilet }
    public ZoneType zoneType;
    public string zoneID;
    public Transform queueStartingPoint;

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<AgentAI>() is not null)
        {
            //idk what to debug
        }
    }
}
