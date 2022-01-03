using Object = System.Object;

namespace IndieAnalytics
{
    public class IndieAnalyticsMessage
    {
        public Object event_data;
        public EventMessage data;
        public ClientMetadata client_meta;
        public SessionMetadata session_meta;
        public Object context;
        public Object previous_context;
    }
}