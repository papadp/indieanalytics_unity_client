using System;
using Newtonsoft.Json.Linq;
using UnityEngine;

namespace IndieAnalyticsInternal
{
    [Serializable]
    public class ClientMetadata : CustomDataHolder
    {
        public string platform;
        public string client_id;
        public string client_install_ts;
        public string identifier;
        public string language;
        public string locale;
        public string device_name;
        public string model;
        public int session_num;

        public ClientMetadata()
        {
            platform = Application.platform.ToString();
            client_id = Guid.NewGuid().ToString();
            client_install_ts = DateTime.Now.ToString();
        }

        public void IncrementSessionNum()
        {
            session_num += 1;
        }

    }
}