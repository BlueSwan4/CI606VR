using UnityEngine;

/// <summary>
/// Each AI will have their own "stats"; being gender, disability, hunger/thirst and so on.
/// They'll act depending on these stats, if toiletNeed reaches a certain threshold they'll leave and go to the toilet.
/// </summary>
public class Profile : MonoBehaviour
{
    //agent info
    public enum Gender { Male, Female, Other } //added but forget dome has inclusive toilets so dont NEED to use these really
    public Gender gender;
    public bool isDisabled;

    //needs
    [Range(0f, 1f)] public float hunger;
    //removed thirst stat because hunger and thirst put into one
    [Range(0f, 1f)] public float toiletNeed;
    [Range(0f, 1f)] public float boredom;
    
    public const float ActionThreshold = 0.7f; //if a need exceeds this the agent will seek a solution

    private void Start()
    {
        gender = (Gender)Random.Range(0f, 3f); //change later to more accurately represent brighton gender statistics if we want
        isDisabled = Random.value < .19f; //19% of being disabled | Src: (https://www.brighton-hove.gov.uk/council-and-democracy/equality/becoming-accessible-city/our-diverse-city)

        hunger = Random.Range(0f, .7f);
        toiletNeed = Random.Range(0f, .7f);
        boredom = Random.Range(0f, .7f);

        gameObject.GetComponent<Renderer>().material.color = isDisabled ? Color.blue : Color.red;
    }
}
