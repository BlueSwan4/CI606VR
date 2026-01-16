using System.Collections;
using UnityEngine;

public class EventManager : MonoBehaviour
{
    public static bool IsEventActive { get; private set; }

    public float _timer;
    public float eventStarts;
    private bool _eventHasHappened = false;

    private void Update()
    {
        _timer += Time.deltaTime;
        
        if(!_eventHasHappened && _timer >= eventStarts)
            StartCoroutine(RunEvent());
    }

    private IEnumerator RunEvent()
    {
        _eventHasHappened = true;
        IsEventActive = true;
        Debug.Log("staart");

        yield return new WaitForSeconds(60);
        
        Debug.Log("done");
        IsEventActive = false;
    }
}
