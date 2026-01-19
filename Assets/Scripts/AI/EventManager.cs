using System.Collections;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static bool IsEventActive { get; private set;}

    private float _timer;
    public float eventStarts;
    private bool _eventHasHappened;

    private void Update()
    {
        _timer += Time.deltaTime;
        
        if(!_eventHasHappened && _timer >= eventStarts)
            StartCoroutine(RunEvent());
        
        Debug.Log(IsEventActive);
    }

    private IEnumerator RunEvent()
    {
        _eventHasHappened = true;
        IsEventActive = true;
        Debug.Log("Event Started");

        yield return new WaitForSeconds(180f);
        
        Debug.Log("Event Finished");
        IsEventActive = false;
    }
}
