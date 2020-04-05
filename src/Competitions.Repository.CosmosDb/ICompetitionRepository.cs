using System;
using System.Threading.Tasks;
using Competitions.Models;

namespace Competitions.Repository.CosmosDb
{
    public interface ICompetitionRepository
    {
        public Task InsertCompetition(Competition competition);
        public Task<Competition[]> GetCompetitionsByTitle(string title);
        public Task<int> GetCountCompetitionsByTitle(string title);
        public Task UpdateDatetimeAndNumberOfCompetitionsByTitleAndState(
            string title, 
            string state,
            DateTime newDatetime,
            int newNumberOfCompetitors);
        public Task<Competition[]> GetScheduledCompetitionWithManyCompetitors(int competitors);
    }
}