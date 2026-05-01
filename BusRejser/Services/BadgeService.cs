using BusRejser.DTOs;
using BusRejserLibrary.Repositories;

namespace BusRejser.Services
{
	public class BadgeService
	{
		private readonly BadgeRepository _badgeRepository;
		private readonly UserBadgeRepository _userBadgeRepository;
		private readonly BadgeEngine _badgeEngine;

		public BadgeService(
			BadgeRepository badgeRepository,
			UserBadgeRepository userBadgeRepository,
			BadgeEngine badgeEngine)
		{
			_badgeRepository = badgeRepository;
			_userBadgeRepository = userBadgeRepository;
			_badgeEngine = badgeEngine;
		}

		public List<BadgeResponse> GetAllActive()
		{
			var badges = _badgeRepository.GetAllActive();

			return badges.Select(b => new BadgeResponse
			{
				BadgeId = b.BadgeId,
				Slug = b.Slug,
				Name = b.BadgeName,
				Description = b.Description,
				IconUrl = b.IconUrl,
				RuleType = b.RuleType,
				RuleValue = b.RuleValue,
				RequiredValue = b.RequiredValue,
				RuleWindowValue = b.RuleWindowValue,
				Tier = b.Tier.ToString()
			}).ToList();
		}

		public List<UserBadgeResponse> GetByUserId(int userId)
		{
			var userBadges = _userBadgeRepository.GetByUserIdWithBadge(userId);

			return userBadges.Select(ub => new UserBadgeResponse
			{
				BadgeId = ub.BadgeId,
				Slug = ub.Badge?.Slug ?? "",
				Name = ub.Badge?.BadgeName ?? "",
				Description = ub.Badge?.Description ?? "",
				IconUrl = ub.Badge?.IconUrl ?? "",
				Tier = ub.Badge?.Tier.ToString() ?? "",
				EarnedAt = ub.EarnedAt
			}).ToList();
		}

		public void EvaluateUserBadges(int userId)
		{
			_badgeEngine.EvaluateUserBadges(userId);
		}
	}
}