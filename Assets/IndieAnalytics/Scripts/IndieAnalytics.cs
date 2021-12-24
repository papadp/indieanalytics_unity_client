using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Object = System.Object;

namespace IndieAnalytics
{
    public class IndieAnalytics : MonoBehaviour
    {
        // private string event_endpoint = "https://analytics.jimjum.io:8080";
        private string event_endpoint = "http://127.0.0.1:8080";
        private SessionMetadata session_meta;
        private ClientMetadata client_meta;

        private float session_start_time;

        private string auth_header_value;

        private static DataSerializer<ClientMetadata> client_data_serializer;

        public string client_key;

        private string data_dir_path = "";

        private bool initialized = false;

        private string relative_data_dir_path = "/indieanalytics";

        private string game_name = "test_game";

        // Start is called before the first frame update
        private void Awake()
        {
            if (FindObjectsOfType<IndieAnalytics>().Length > 1)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);

            CreateDataDirIfNeeded();

            client_data_serializer = new DataSerializer<ClientMetadata>(relative_data_dir_path + "/ia_client_data");
            client_meta = GetClientMetadata();

            CreateNewSessionData();

            BuildAuthHeaderValue();

            initialized = true;

            SendEvent(IndieAnalyticsEventType.StartSession.ToString());
        }

        private void CreateDataDirIfNeeded()
        {
            data_dir_path = Application.persistentDataPath + relative_data_dir_path;

            if (!Directory.Exists(data_dir_path))
            {
                Directory.CreateDirectory(data_dir_path);
            }
        }

        private void BuildAuthHeaderValue()
        {
            string auth = String.Format("indie_analytics_user:{0}", client_key);
            auth = Convert.ToBase64String(System.Text.Encoding.GetEncoding("ISO-8859-1").GetBytes(auth));
            auth = "Basic " + auth;
            auth_header_value = auth;
        }

        private ClientMetadata CreateNewClientData()
        {
            ClientMetadata client_meta = new ClientMetadata
            {
                identifier = Application.identifier,
                language = Application.systemLanguage.ToString(),
                locale = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName,
                device_name = SystemInfo.deviceName,
                model = SystemInfo.deviceModel,
                session_num = 1,
            };

            SaveMessageClientMetadata(client_meta);

            return client_meta;
        }

        private void SaveMessageClientMetadata(ClientMetadata client_meta)
        {
            client_data_serializer.Save(client_meta);
        }

        private ClientMetadata GetClientMetadata()
        {
            ClientMetadata client_meta = client_data_serializer.Load();

            if (client_meta == null)
            {
                return CreateNewClientData();
            }

            client_meta.IncrementSessionNum();

            SaveMessageClientMetadata(client_meta);

            return client_meta;
        }

        private void CreateNewSessionData()
        {
            session_meta = new SessionMetadata();
        }

        public void SendEvent(string event_type, Object custom_event_data = null)
        {
            EventMessage event_message = new EventMessage { event_name = event_type };

            StartCoroutine(PostMessageRoutine(event_message, custom_event_data));
        }

        private void Update()
        {
            session_meta.Update(Time.deltaTime);
        }

        IEnumerator PostMessageRoutine(EventMessage event_message, Object custom_event_data = null)
        {

            if (custom_event_data == null)
            {
                custom_event_data = new { };
            }

            IndieAnalyticsMessage ia_message = new IndieAnalyticsMessage
            {
                event_data = custom_event_data,
                data = event_message,
                session_meta = session_meta,
                client_meta = client_meta
            };

            var request = SendRequest(ia_message, out var async_request, "Analytics");

            yield return async_request;

            if (request.error != null)
            {
                Debug.Log("Error: " + request.error);
                Debug.Log("code: " + request.responseCode);
            }

            session_meta.SetLastEvent(new
            {
                data = custom_event_data,
                event_message = event_message
            });

            session_meta.IncrementSequenceNumber();
        }

        private UnityWebRequest SendRequest(Object object_json, out UnityWebRequestAsyncOperation req,
            string message_type)
        {
            var json = JsonConvert.SerializeObject(object_json);

            var request = new UnityWebRequest(event_endpoint, "POST");
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);
            request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.SetRequestHeader("Authorization", auth_header_value);
            request.SetRequestHeader("IndieAnalytics-Message-Type", message_type);
            request.SetRequestHeader("IndieAnalytics-Game", game_name);

            req = request.SendWebRequest();
            return request;
        }

        public void SendProgressionEvent(string progression_name)
        {
            SendEvent(IndieAnalyticsEventType.Progression.ToString(), new { progression_name = progression_name });
        }
        
        void OnApplicationQuit()
        {
            SendEvent(IndieAnalyticsEventType.EndSession.ToString());
        }

        void OnApplicationPause(bool pauseStatus)
        {
            if (!initialized)
            {
                return;
            }

            SendEvent(IndieAnalyticsEventType.PauseSession.ToString(), new { pause = pauseStatus });
        }
    }
}