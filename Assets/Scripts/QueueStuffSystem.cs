using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueStuffSystem
{
    /*found in https://answers.unity.com/questions/1040319/whats-the-proper-way-to-queue-and-space-function-c.html */

    //stats queue variables
    MonoBehaviour stuff_Owner = null;
    Coroutine stuff_InternalCoroutine = null;
    Queue<IEnumerator> stuffActions = new Queue<IEnumerator>();



    //stats queue-----------------------------------------------------------------------------------------------------------------------
    public QueueStuffSystem(MonoBehaviour aCoroutineOwner)
    {
        stuff_Owner = aCoroutineOwner;
    }

    public void StartLoop()
    {
        stuff_InternalCoroutine = stuff_Owner.StartCoroutine(Process());
    }

    public void StopLoop()
    {
        stuff_Owner.StopCoroutine(stuff_InternalCoroutine);
        stuff_InternalCoroutine = null;
    }

    public void EnqueueAction(IEnumerator aAction)
    {
        stuffActions.Enqueue(aAction);
        //Debug.Log("action queued");
    }

    private IEnumerator Process()
    {
        while (true)
        {
            if (stuffActions.Count > 0)
            {
                //Debug.Log("action performed");
                yield return stuff_Owner.StartCoroutine(stuffActions.Dequeue());
            }
            else
                yield return null;
        }
    }

    public void EnqueueWait(float aWaitTime)
    {
        stuffActions.Enqueue(Wait(aWaitTime));
    }

    private IEnumerator Wait(float aWaitTime)
    {
        yield return new WaitForSeconds(aWaitTime);
    }
}
