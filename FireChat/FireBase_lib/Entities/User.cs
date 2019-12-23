using Newtonsoft.Json;

namespace FireBase_lib.Entities
{
    public class User : ISerializableObject
    {
        [JsonProperty("UserName")]
        public string Name { get; set; }
        [JsonProperty("ID")]
        public string Value { get; set; }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
