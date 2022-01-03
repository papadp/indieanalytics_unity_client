using System;
using System.Collections;
using System.Globalization;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Object = System.Object;

namespace IndieAnalytics
{
    public class IndieAnalytics : MonoBehaviour
    {
        private string event_endpoint = "https://es.indie-analytics.com:7070";
        private SessionMetadata session_meta;
        private ClientMetadata client_meta;

        private float session_start_time;

        private string auth_header_value;
        private string game_header_value;

        private static DataSerializer<ClientMetadata> client_data_serializer;

        public string client_key;

        private string data_dir_path = "";

        private bool initialized = false;

        private string relative_data_dir_path = "/indieanalytics";

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

            BuildIndieAnalyticsHeaderValues();

            initialized = true;

            OnInitialized();
        }

        private void OnInitialized()
        {
            DataSerializer<bool> played_before_serializer = new DataSerializer<bool>(relative_data_dir_path + "/ia_firstplay");
            bool played_before = played_before_serializer.Load();

            if (!played_before)
            {
                SendEvent(IndieAnalyticsEventType.FirstPlay.ToString());
                played_before_serializer.Save(true);
            }

            SendEvent(IndieAnalyticsEventType.StartSession.ToString());
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void CreateDataDirIfNeeded()
        {
            data_dir_path = Application.persistentDataPath + relative_data_dir_path;

            if (!Directory.Exists(data_dir_path))
            {
                Directory.CreateDirectory(data_dir_path);
            }
        }

        private void BuildIndieAnalyticsHeaderValues()
        {
            string auth = client_key;

            auth_header_value = auth;
            game_header_value = auth.Substring(0, 16);
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
            if (!initialized)
            {
                Debug.LogWarning("Cant send event, not initialized yet");
            }
            
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

            // Race condition on increment here
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
            
            request.SetRequestHeader("IA-Key-Authorization", auth_header_value);
            request.SetRequestHeader("IA-Message-Type", message_type);
            request.SetRequestHeader("IA-Game", game_header_value);

            req = request.SendWebRequest();
            return request;
        }

        public void SendProgressionEvent(string name)
        {
            SendEvent(IndieAnalyticsEventType.Progression.ToString(), new { name = name });
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            SendEvent(IndieAnalyticsEventType.SceneLoaded.ToString(), new { scene_name = scene.name });
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