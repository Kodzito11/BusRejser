using BusRejser.Features.Badges.DTOs;
using BusRejser.Features.Progression.Services;
using BusRejserLibrary.Repositories;

namespace BusRejser.Features.Badges.Services
{
	public class BadgeService
	{
		private readonly BadgeRepository _badgeRepository;
		private readonly UserBadgeRepository _userBadgeRepository;
		private readonly ProgressionService _progressionService;

		public BadgeService(
			BadgeRepository badgeRepository,
			UserBadgeRepository userBadgeRepository,
			ProgressionService progressionService)
		{
			_badgeRepository = badgeRepository;
			_userBadgeRepository = userBadgeRepository;
			_progressionService = progressionService;
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
			_progressionService.SyncUserProgress(userId);

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
			_progressionService.SyncUserProgress(userId);
		}
	}
}