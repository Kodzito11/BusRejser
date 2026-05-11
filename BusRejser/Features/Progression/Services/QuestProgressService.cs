using BusRejser.Features.Progression.DTOs;
using BusRejserLibrary.Models;

namespace BusRejser.Features.Progression.Services;

public class QuestProgressService
{
	public List<QuestProgressResponse> BuildQuests(
		List<VisitedLocation> locations,
		List<TerritoryProgressResponse> territories,
		List<MunicipalityProgressResponse> municipalities)
	{
		var completedTrips = locations.Sum(x => x.VisitCount);
		var visitedMunicipalities = municipalities.Count(x => x.VisitCount > 0);

		var quests = new List<QuestProgressResponse>
		{
			CreateQuest(
				key: "first-trip",
				title: "Første rejse",
				description: "Gennemfør din første rejse.",
				current: completedTrips,
				target: 1,
				rewardLabel: "Starter badge"
			),

			CreateQuest(
				key: "three-trips",
				title: "På vej ud i verden",
				description: "Gennemfør 3 rejser.",
				current: completedTrips,
				target: 3,
				rewardLabel: "+ progression"
			),

			CreateQuest(
				key: "visit-three-municipalities",
				title: "Kommunesamler",
				description: "Besøg 3 forskellige kommuner.",
				current: visitedMunicipalities,
				target: 3,
				rewardLabel: "Explorer status"
			),

			CreateQuest(
				key: "master-one-municipality",
				title: "Master én kommune",
				description: "Få én kommune op på mastered.",
				current: municipalities.Count(x => x.Status == "mastered"),
				target: 1,
				rewardLabel: "Mastery reward"
			)
		};

		foreach (var territory in territories)
		{
			if (territory.Status == "locked" && territory.VisitCount <= 0)
			{
				quests.Add(CreateQuest(
					key: $"unlock-{territory.Key}",
					title: $"Unlock {territory.Name}",
					description: $"Tag din første rejse til {territory.Name}.",
					current: territory.VisitCount,
					target: 1,
					rewardLabel: $"{territory.Name} unlocked"
				));
			}
		}

		return quests;
	}

	private static QuestProgressResponse CreateQuest(
		string key,
		string title,
		string description,
		int current,
		int target,
		string rewardLabel)
	{
		var cappedCurrent = Math.Min(current, target);

		var completionPercent = target <= 0
			? 0
			: (int)Math.Round((double)cappedCurrent / target * 100);

		return new QuestProgressResponse
		{
			Key = key,
			Title = title,
			Description = description,
			Current = cappedCurrent,
			Target = target,
			CompletionPercent = completionPercent,
			Status = current >= target ? "completed" : current > 0 ? "active" : "locked",
			RewardLabel = rewardLabel
		};
	}
}