using System;
using Newtonsoft.Json;

namespace Platform.Data.WakaTimeProxy.Models
{
    public class Heartbeat
    {
        [JsonProperty("entity")]
        public string Entity { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("time")]
        public double Time { get; set; }

        [JsonProperty("project")]
        public string Project { get; set; }

        [JsonProperty("branch")]
        public string Branch { get; set; }

        [JsonProperty("language")]
        public string Language { get; set; }

        [JsonProperty("dependencies")]
        public string[] Dependencies { get; set; }

        [JsonProperty("lines")]
        public int? Lines { get; set; }

        [JsonProperty("lineno")]
        public int? LineNo { get; set; }

        [JsonProperty("cursorpos")]
        public int? CursorPos { get; set; }

        [JsonProperty("is_write")]
        public bool? IsWrite { get; set; }
    }
}
