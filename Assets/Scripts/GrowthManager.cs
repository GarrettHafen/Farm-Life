using System.Collections.Generic;
using UnityEngine;

public class GrowthManager : MonoBehaviour
{
    private static GrowthManager _instance;
    public static GrowthManager instance
    {
        get
        {
            if (_instance == null)
                _instance = new GameObject("GrowthManager").AddComponent<GrowthManager>();
            return _instance;
        }
    }

    private struct Entry
    {
        public float completionTime;
        public System.Action callback;
    }

    private readonly List<Entry> pending = new List<Entry>();

    void Awake()
    {
        _instance = this;
    }

    public void Register(float completionTime, System.Action callback)
    {
        var entry = new Entry { completionTime = completionTime, callback = callback };
        int i = 0;
        while (i < pending.Count && pending[i].completionTime <= completionTime)
            i++;
        pending.Insert(i, entry);
    }

    public void Cancel(System.Action callback)
    {
        for (int i = pending.Count - 1; i >= 0; i--)
            if (pending[i].callback == callback)
                pending.RemoveAt(i);
    }

    void Update()
    {
        while (pending.Count > 0 && pending[0].completionTime <= Time.time)
        {
            var cb = pending[0].callback;
            pending.RemoveAt(0);
            cb();
        }
    }
}
