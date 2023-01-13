using System;
using System.Collections.Generic;
using UnityEngine;


public class Speaker : MonoBehaviour
{
    public Vector3 position
    {
        get
        {
            return base.transform.position;
        }
    }

    private void OnEnable()
    {
        this.host = Speaker.GetHost(this);
        if (this.host != null)
        {
            List<Speaker> list;
            if (!Speaker.hosts.TryGetValue(this.host, out list))
            {
                list = new List<Speaker>();
                Speaker.hosts.Add(this.host, list);
            }
            list.Add(this);
            return;
        }
        Speaker.defaultSpeakers.Add(this);
    }

    private void OnDisable()
    {
        if (this.host != null)
        {
            List<Speaker> list;
            if (Speaker.hosts.TryGetValue(this.host, out list))
            {
                list.Remove(this);
                if (list.Count == 0)
                {
                    Speaker.hosts.Remove(this.host);
                    return;
                }
            }
        }
        else
        {
            Speaker.defaultSpeakers.Remove(this);
        }
    }


    public static void GetSpeakers(ISpeakerHost host, Vector3 position, float radius, List<Speaker> results)
    {
        results.Clear();
        float num = radius * radius;
        List<Speaker> list = null;
        
        if (host != null)
        {
            Speaker.hosts.TryGetValue(host, out list);
        }
        if (list == null)
        {
            list = Speaker.defaultSpeakers;
        }
        for (int j = 0; j < list.Count; j++)
        {
            Speaker speaker = list[j];
            if ((speaker.position - position).sqrMagnitude <= num)
            {
                results.Add(speaker);
            }
        }
    }


    public static ISpeakerHost GetHost(MonoBehaviour target)
    {
        if (!(target != null))
        {
            return null;
        }
        return target.GetComponentInParent<ISpeakerHost>();
    }

    public static bool IsSameHost(ISpeakerHost a, ISpeakerHost b)
    {
        if (a == null || b == null)
        {
            return false;
        }
       
        return a == b;
    }

    private static readonly Dictionary<ISpeakerHost, List<Speaker>> hosts = new Dictionary<ISpeakerHost, List<Speaker>>();
    private static readonly List<Speaker> defaultSpeakers = new List<Speaker>();
    private ISpeakerHost host;
}
