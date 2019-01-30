using Newtonsoft.Json;

namespace Common
{
    public class DurableResponse
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "statusQueryGetUri")]
        public string StatusQueryGetUri { get; set; }

        [JsonProperty(PropertyName = "sendEventPostUri")]
        public string SendEventPostUri { get; set; }

        [JsonProperty(PropertyName = "terminatePostUri")]
        public string TerminatePostUri { get; set; }

        [JsonProperty(PropertyName = "rewindPostUri")]
        public string RewindPostUri { get; set; }
    }
}
