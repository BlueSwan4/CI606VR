using UnityEngine;
using System.Collections.Generic;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance;
    public List<Zone> zones;

    private void Awake()
    {
        if(Instance is null)
            Instance = this;
        else Destroy(this.gameObject);

        zones = new List<Zone>(FindObjectsByType<Zone>(FindObjectsSortMode.None));
    }

    public Zone GetDestination(string need, Profile profile)
    {
        //finish later, definitely not complete
        
        if (need == "Toilet")
        {
            if (profile.isDisabled)
                return zones.Find(z => z.zoneType == Zone.ZoneType.DisabledToilet);

            if (profile.gender == Profile.Gender.Male)
                return zones.Find(z => z.zoneType == Zone.ZoneType.Toilet && z.zoneID.Contains("Male"));

            if (profile.gender == Profile.Gender.Female)
                return zones.Find(z => z.zoneType == Zone.ZoneType.Toilet && z.zoneID.Contains("Female"));
        }

        return zones.Find(z => z.zoneType == Zone.ZoneType.Cafe);
    }
}