using System;
using UnityEngine;
using Object = System.Object;

public class SessionMetadata : CustomDataHolder
{
    public string session_id;
    public int session_length;
    private float session_length_float;
    public string session_start_ts;
    public string version;
    public string os;
    public string app_build;

    public int seq_no;
    
    private float context_last_set = 0f;

    public Object last_event;
    
    public SessionMetadata()
    {
        session_id = Guid.NewGuid().ToString();
        session_length = 0;
        session_length_float = 0f;
        session_start_ts = DateTime.Now.ToString();
        version = Application.version;
        os = SystemInfo.operatingSystem;
        seq_no = 1;
    }

    public void Update(float delta_time)
    {
        session_length_float += delta_time;
        session_length = (int)session_length_float;
    }

    public void SetLastEvent(Object last_event_data)
    {
        last_event = last_event_data;
    }

    public void IncrementSequenceNumber()
    {
        seq_no += 1;
    }
}