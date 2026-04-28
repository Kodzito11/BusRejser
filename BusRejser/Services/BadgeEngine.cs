using BusRejserLibrary.Models;
using BusRejserLibrary.Repositories;

namespace BusRejser.Services
{
	public class BadgeEngine
	{
		private readonly BadgeRepository _badgeRepository;
		private readonly UserBadgeRepository _userBadgeRepository;
		private readonly TravelHistoryRepository _travelHistoryRepository;

		public BadgeEngine(
			BadgeRepository badgeRepository,
			UserBadgeRepository userBadgeRepository,
			TravelHistoryRepository travelHistoryRepository)
		{
			_badgeRepository = badgeRepository;
			_userBadgeRepository = userBadgeRepository;
			_travelHistoryRepository = travelHistoryRepository;
		}

		public void EvaluateUserBadges(int userId)
		{
			var badges = _badgeRepository.GetAllActive();
			var userBadges = _userBadgeRepository.GetByUserId(userId);
			var history = _travelHistoryRepository.GetByUserId(userId);

			foreach (var badge in badges)
			{
				if (userBadges.Any(x => x.BadgeId == badge.BadgeId))
					continue;

				if (IsBadgeUnlocked(badge, history))
				{
					_userBadgeRepository.Create(new UserBadge
					{
						UserId = userId,
						BadgeId = badge.BadgeId,
						EarnedAt = DateTime.UtcNow
					});
				}
			}
		}

		private bool IsBadgeUnlocked(Badge badge, List<TravelHistory> history)
		{
			return badge.RuleType switch
			{
				"CompletedTrips" => HasCompletedTrips(badge, history),
				"EarlyBooking" => HasEarlyBooking(badge, history),
				"LastMinute" => HasLastMinuteBooking(badge, history),
				"UniqueDestinations" => HasUniqueDestinations(badge, history),
				"BackToBack" => HasBackToBackTrips(badge, history),
				"FirstBlood" => HasFirstBlood(badge,history),
				"DoubleTrouble" => HasDoubleTrouble(badge, history),
				"NightRider" => HasNightRider(badge, history),

				_ => false
			};
		}

		private bool HasCompletedTrips(Badge badge, List<TravelHistory> history)
		{
			return history.Count >= badge.RequiredValue;
		}

		private bool HasEarlyBooking(Badge badge, List<TravelHistory> history)
		{
			// RequiredValue = antal dage før afgang, fx 14
			return history.Any(x =>
				x.Booking != null &&
				x.Rejse != null &&
				(x.Rejse.StartAt - x.Booking.CreatedAt).TotalDays >= badge.RequiredValue);
		}

		private bool HasLastMinuteBooking(Badge badge, List<TravelHistory> history)
		{
			// RequiredValue = antal timer før afgang, fx 48
			return history.Any(x =>
				x.Booking != null &&
				x.Rejse != null &&
				(x.Rejse.StartAt - x.Booking.CreatedAt).TotalHours <= badge.RequiredValue);
		}

		private bool HasUniqueDestinations(Badge badge, List<TravelHistory> history)
		{
			return history
				.Where(x => !string.IsNullOrWhiteSpace(x.Destination))
				.Select(x => x.Destination.Trim().ToLowerInvariant())
				.Distinct()
				.Count() >= badge.RequiredValue;
		}

		private bool HasBackToBackTrips(Badge badge, List<TravelHistory> history)
		{
			// RequiredValue = antal timer mellem to rejser, fx 48
			var ordered = history
				.Where(x => x.Rejse != null)
				.OrderBy(x => x.Rejse!.StartAt)
				.ToList();

			for (int i = 1; i < ordered.Count; i++)
			{
				var previous = ordered[i - 1].Rejse!;
				var current = ordered[i].Rejse!;

				var hoursBetween = (current.StartAt - previous.EndAt).TotalHours;

				if (hoursBetween >= 0 && hoursBetween <= badge.RequiredValue)
					return true;
			}

			return false;
		}

		private bool HasFirstBlood(Badge badge, List<TravelHistory> history)
		{
			// First Blood: Vær den første til at booke en rejse
			return history.Any(x => x.Booking != null && x.Rejse != null) &&
				   history.OrderBy(x => x.Booking!.CreatedAt).First().UserId == history.First().UserId;
		}

		private bool HasDoubleTrouble(Badge badge, List<TravelHistory> history)
		{
			// Double Trouble: Book to rejser samme måned
			if (badge.RuleWindowValue == null)
				return false;

			var dates = history
				.Where(x => x.Rejse != null)
				.Select(x => x.Rejse!.StartAt)
				.OrderBy(x => x)
				.ToList();

			for (int i = 0; i < dates.Count; i++)
			{
				var windowEnd = dates[i].AddDays(badge.RuleWindowValue.Value);

				var count = dates.Count(x => x >= dates[i] && x <= windowEnd);

				if (count >= badge.RequiredValue)
					return true;
			}

			return false;
		}

		private bool HasNightRider(Badge badge, List<TravelHistory> history)
		{
			// Night Rider: Book en rejse der starter mellem kl. 00:00 og 05:00
			return history.Any(x =>
				x.Rejse != null &&
				x.Rejse.StartAt.TimeOfDay >= TimeSpan.FromHours(0) &&
				x.Rejse.StartAt.TimeOfDay < TimeSpan.FromHours(5));
		}


	}
}