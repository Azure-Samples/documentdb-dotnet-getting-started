using Newtonsoft.Json;

namespace DocumentDB.DbSchema
{
    /// <summary>
    /// A Family class, e.g. storing census data about families within the United States. We use this data model throughout the 
    /// sample to show how you can store objects within your application logic directly as JSON within Azure DocumentDB. 
    /// </summary>
    public class Family
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        public string LastName { get; set; }

        public Parent[] Parents { get; set; }

        public Child[] Children { get; set; }

        public Address Address { get; set; }

        public bool IsRegistered { get; set; }

        public override string ToString()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
