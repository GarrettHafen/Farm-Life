using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QueueSystem
{ 

    /*found in https://answers.unity.com/questions/1040319/whats-the-proper-way-to-queue-and-space-function-c.html */

    MonoBehaviour m_Owner = null;
    Coroutine m_InternalCoroutine = null;
    Queue<IEnumerator> actions = new Queue<IEnumerator>();

    public QueueSystem(MonoBehaviour aCoroutineOwner)
    {
        m_Owner = aCoroutineOwner;
    }

    public void StartLoop()
    {
        m_InternalCoroutine = m_Owner.StartCoroutine(Process());
    }

    public void StopLoop()
    {
        m_Owner.StopCoroutine(m_InternalCoroutine);
        m_InternalCoroutine = null;
    }

    public void EnqueueAction(IEnumerator aAction)
    {
        actions.Enqueue(aAction);
        //Debug.Log("action queued");
    }

    private IEnumerator Process()
    {
        while (true)
        {
            if (actions.Count > 0)
            {
                //Debug.Log("action performed");
                yield return m_Owner.StartCoroutine(actions.Dequeue());
            }
            else
                yield return null;
        }
    }

    public void EnqueueWait(float aWaitTime)
    {
        actions.Enqueue(Wait(aWaitTime));
    }

    private IEnumerator Wait(float aWaitTime)
    {
        Debug.Log("wait");
        yield return new WaitForSeconds(aWaitTime);
    }

  
    public int GetQueueCount()
    {
        return actions.Count;
    }

}
