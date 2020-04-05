using System;

namespace Competitions.Models
{
    public class Competition : Document
    {
        public string Title { get; set; }
        public Location Location { get; set; }
        public GamingPlatform[] Platforms { get; set; }
        public string[] Games { get; set; }
        public int NumberOfRegisteredCompetitors { get; set; }
        public int NumberOfCompetitors { get; set; }
        public int NumberOfViewers { get; set; }
        public CompetitionStatus Status { get; set; }
        public DateTime DateTime { get; set; }
        public Winner[] Winners { get; set; }
        
        public Competition(
            string title,
            Location location,
            GamingPlatform[] platforms,
            string[] games,
            int numberOfRegisteredCompetitors,
            int numberOfCompetitors,
            int numberOfViewers,
            CompetitionStatus status,
            DateTime dateTime,
            Winner[] winners)
        {
            Id = title;
            Title = title;
            Location = location;
            Platforms = platforms;
            Games = games;
            NumberOfRegisteredCompetitors = numberOfRegisteredCompetitors;
            NumberOfCompetitors = numberOfCompetitors;
            NumberOfViewers = numberOfViewers;
            Status = status;
            DateTime = dateTime;
            Winners = winners;
        }

        public Competition() { }
    }
}