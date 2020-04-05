using System;
using System.Threading.Tasks;
using Competitions.Models;
using Competitions.Repository.CosmosDb;

namespace Competitions.Services
{
    public class CompetitionService : ICompetitionService
    {
        private readonly ICompetitionRepository _repository;
        
        public CompetitionService(
            ICompetitionRepository repository)
        {
            _repository = repository;
        }
        
        public async Task AddCompetitionToDatabase(Competition competition)
        {
            await _repository.InsertCompetition(competition);
        }

        public async Task<Competition[]> GetCompetitionsByTitle(string title)
        {
            return await _repository.GetCompetitionsByTitle(title);
        }

        public async Task<int> GetCountCompetitionsByTitle(string title)
        {
            return await _repository.GetCountCompetitionsByTitle(title);
        }

        public async Task UpdateDatetimeAndNumberOfCompetitionsByTitleAndState(
            string title,
            string state,
            DateTime newDatetime,
            int newNumberOfCompetitions)
        {
            await _repository.UpdateDatetimeAndNumberOfCompetitionsByTitleAndState(
                title,
                state,
                newDatetime,
                newNumberOfCompetitions);
        }

        public async Task<Competition[]> GetScheduledCompetitionWithManyCompetitors(int competitors)
        {
            return await _repository.GetScheduledCompetitionWithManyCompetitors(competitors);
        }
    }
}