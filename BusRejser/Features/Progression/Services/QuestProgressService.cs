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

		var dk = territories.FirstOrDefault(x => x.Key == "dk");
		var germany = territories.FirstOrDefault(x => x.Key == "germany");

		return new List<QuestProgressResponse>
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
				key: "unlock-denmark",
				title: "Unlock Danmark",
				description: "Tag en rejse i Danmark.",
				current: dk?.VisitCount ?? 0,
				target: 1,
				rewardLabel: "Danmark unlocked"
			),

			CreateQuest(
				key: "unlock-germany",
				title: "Unlock Tyskland",
				description: "Tag din første rejse til Tyskland.",
				current: germany?.VisitCount ?? 0,
				target: 1,
				rewardLabel: "Tyskland unlocked"
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