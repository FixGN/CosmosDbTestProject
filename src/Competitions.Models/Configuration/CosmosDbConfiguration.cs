namespace Competitions.Models.Configuration
{
    public class CosmosDbConfiguration
    {
        public string Uri { get; set; }
        public string MasterKey { get; set; }
        public string DatabaseId { get; set; }
        public int DatabaseThroughput { get; set; }
        public string ContainerId { get; set; }
    }
}