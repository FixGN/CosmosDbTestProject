using System;
using System.Text.Json;
using System.Threading.Tasks;
using Competitions.Models;
using Competitions.Models.Configuration;
using Competitions.Repository.CosmosDb;
using Competitions.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Competitions.ConsoleApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var services = new ServiceCollection();
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder.AddJsonFile(
                path: "configuration.json", 
                optional: false,
                reloadOnChange: false);
            var configuration = configurationBuilder.Build();
            
            ConfigureServices(services, configuration);
            var serviceProvider = services.BuildServiceProvider();

            var firstCompetition = new Competition(
                title: "First Blood",
                location: new Location(
                    "112112",
                    "Russia"),
                platforms: new []{GamingPlatform.PC},
                games: new []{"Counter Strike: Source"},
                numberOfRegisteredCompetitors: 500,
                numberOfCompetitors: 0,
                numberOfViewers: 0,
                status: CompetitionStatus.Scheduled,
                dateTime: new DateTime(2020, 5, 1),
                winners: new Winner[0]
            );
            
            var service = serviceProvider.GetService<ICompetitionService>();
            var logger = serviceProvider.GetService<ILogger<Program>>();
            
            // Try add competition to database
            await service.AddCompetitionToDatabase(firstCompetition);

            // Try get competition by title
            var items = await service.GetCompetitionsByTitle("First Blood");
            if (items.Length > 0)
            {
                logger.LogInformation("Returned items after select by title:");
                foreach (var item in items)
                {
                    logger.LogInformation($"{JsonSerializer.Serialize(item)}");
                }
            }
            
            // Try to get count of competition with specific title
            var count = await service.GetCountCompetitionsByTitle("First Blood");
            logger.LogInformation($"Found {count} with title {firstCompetition.Title}");

            // Try to update competition
            await service.UpdateDatetimeAndNumberOfCompetitionsByTitleAndState(
                firstCompetition.Title,
                firstCompetition.Location.State,
                new DateTime(2020, 5, 10),
                600);
            
            // Get All competition with competitors > 200
            var manyCompetitors = await service.GetScheduledCompetitionWithManyCompetitors(200);
            if (manyCompetitors.Length > 0)
            {
                logger.LogInformation("Returned items after select by title:");
                foreach (var item in items)
                {
                    logger.LogInformation($"{JsonSerializer.Serialize(item)}");
                }
            }
            
            Console.WriteLine("Enter any key...");
            Console.ReadKey();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services
                .AddOptions()
                .Configure<CosmosDbConfiguration>(configuration.GetSection("CosmosDb"))
                .AddLogging(configure => configure.AddConsole())
                .AddSingleton<ICompetitionService, CompetitionService>()
                .AddSingleton<ICompetitionRepository, CompetitionRepository>();
        }
    }
}