using UnityEngine;

/// <summary>
/// Each AI will have their own "stats"; being gender, disability, hunger/thirst and so on.
/// They'll act depending on these stats, if toiletNeed reaches a certain threshold they'll leave and go to the toilet.
/// </summary>
public class Profile : MonoBehaviour
{
    //agent info
    public enum Gender { Male, Female, Other } //can add more if necessary
    public Gender gender;
    public bool isDisabled;
    public bool hasTicket;

    //needs
    [Range(0f, 1f)] public float hunger;
    [Range(0f, 1f)] public float thirst;
    [Range(0f, 1f)] public float toiletNeed;
    [Range(0f, 1f)] public float boredom;
    
    public float actionThreshold = 0.7f; //if a need exceeds this the agent will seek a solution

    private void Start()
    {
        gender = (Gender)Random.Range(0f, 3f); //change later to more accurately represent brighton gender statistics if we want
        isDisabled = Random.value < .19f; //19% of being disabled | Src: (https://www.brighton-hove.gov.uk/council-and-democracy/equality/becoming-accessible-city/our-diverse-city)
        hasTicket = Random.value < .8f; //80% of having a ticket

        hunger = Random.Range(0f, .5f);
        thirst = Random.Range(0f, .5f);
        toiletNeed = Random.Range(0f, .5f);
        boredom = Random.Range(0f, .5f);
    }

    public void SatisfyNeed(string type)
    {
        switch (type)
        {
            case "Toilet": toiletNeed = 0; break;
            case "Food": hunger = 0; thirst = 0; break;
            case "Seat": boredom = 0; break;
        }
    }
}
