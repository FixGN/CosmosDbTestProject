namespace Competitions.Models
{
    public class Location
    {
        public string ZipCode { get; set; }
        public string State { get; set; }

        public Location(string zipCode, string state)
        {
            ZipCode = zipCode;
            State = state;
        }

        public Location() { }
    }
}
