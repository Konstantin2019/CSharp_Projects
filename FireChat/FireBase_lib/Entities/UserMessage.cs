using Newtonsoft.Json;

namespace FireBase_lib.Entities
{
    public class UserMessage : ISerializableObject
    {
        [JsonProperty("UserName")]
        public string UserName { get; set; }
        [JsonProperty("Message")]
        public string Name { get; set; }
        [JsonProperty("TimeStamp")]
        public string Value { get; set; }

        public override string ToString()
        {
            return $"{UserName}({Value}): {Name}";
        }
    }
}
