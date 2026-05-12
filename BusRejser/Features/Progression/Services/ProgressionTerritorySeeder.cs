using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;

namespace BusRejser.Features.Progression.Services
{
	public class ProgressionTerritorySeeder
	{
		private readonly ProgressionTerritoryRepository _territoryRepository;

		public ProgressionTerritorySeeder(ProgressionTerritoryRepository territoryRepository)
		{
			_territoryRepository = territoryRepository;
		}

		public Task SeedAsync()
		{
			var defaults = new List<DefaultProgressionTerritory>
			{
				new(
					Key: "dk",
					Name: "Danmark",
					Type: "country",
					MasteryTarget: 10,
					Description: "Standard progression territory for Danmark.",
					Aliases:
					[
						"dk",
						"danmark",
						"denmark",
						"københavn",
						"koebenhavn",
						"copenhagen"
					]
				),

				new(
					Key: "germany",
					Name: "Tyskland",
					Type: "country",
					MasteryTarget: 10,
					Description: "Standard progression territory for Tyskland.",
					Aliases:
					[
						"germany",
						"tyskland",
						"deutschland",
						"berlin"
					]
				),

				new(
					Key: "prague",
					Name: "Prag",
					Type: "city",
					MasteryTarget: 5,
					Description: "Standard progression territory for Prag.",
					Aliases:
					[
						"prague",
						"prag",
						"praha",
						"czechia",
						"tjekkiet",
						"czech republic"
					]
				),

				new(
					Key: "netherlands",
					Name: "Holland",
					Type: "country",
					MasteryTarget: 10,
					Description: "Standard progression territory for Holland.",
					Aliases:
					[
						"netherlands",
						"holland",
						"nederlandene",
						"amsterdam"
					]
				),

				new(
					Key: "sweden",
					Name: "Sverige",
					Type: "country",
					MasteryTarget: 10,
					Description: "Standard progression territory for Sverige.",
					Aliases:
					[
						"sweden",
						"sverige",
						"stockholm",
						"malmö",
						"malmo"
					]
				),

				new(
					Key: "norway",
					Name: "Norge",
					Type: "country",
					MasteryTarget: 10,
					Description: "Standard progression territory for Norge.",
					Aliases:
					[
						"norway",
						"norge",
						"oslo"
					]
				)
			};

			foreach (var item in defaults)
			{
				var territory = _territoryRepository.GetByKey(item.Key);

				if (territory == null)
				{
					var newTerritory = new ProgressionTerritory
					{
						Key = item.Key,
						Name = item.Name,
						Type = item.Type,
						IsActive = true,
						IsVisible = true,
						IsComingSoon = false,
						MasteryTarget = item.MasteryTarget,
						Description = item.Description,
						CreatedAt = DateTime.UtcNow
					};

					var id = _territoryRepository.Create(newTerritory);
					territory = _territoryRepository.GetById(id);
				}

				if (territory == null)
					continue;

				foreach (var alias in item.Aliases)
				{
					_territoryRepository.AddAlias(territory.ProgressionTerritoryId, alias);
				}
			}

			return Task.CompletedTask;
		}

		private record DefaultProgressionTerritory(
			string Key,
			string Name,
			string Type,
			int MasteryTarget,
			string Description,
			List<string> Aliases
		);
	}
}