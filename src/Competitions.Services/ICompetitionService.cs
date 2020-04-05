using System;
using System.Threading.Tasks;
using Competitions.Models;

namespace Competitions.Services
{
    public interface ICompetitionService
    {
        public Task AddCompetitionToDatabase(Competition competition);
        public Task<Competition[]> GetCompetitionsByTitle(string title);
        public Task<int> GetCountCompetitionsByTitle(string title);
        public Task UpdateDatetimeAndNumberOfCompetitionsByTitleAndState(
            string title, 
            string state,
            DateTime newDatetime,
            int newNumberOfCompetitions);

        public Task<Competition[]> GetScheduledCompetitionWithManyCompetitors(int competitors);
    }
}