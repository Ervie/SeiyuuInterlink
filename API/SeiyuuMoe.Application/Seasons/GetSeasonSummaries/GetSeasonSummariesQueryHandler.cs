﻿using SeiyuuMoe.Application.Seasons.Extensions;
using SeiyuuMoe.Domain.ComparisonEntities;
using SeiyuuMoe.Domain.Entities;
using SeiyuuMoe.Domain.Repositories;
using SeiyuuMoe.Domain.WebEssentials;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SeiyuuMoe.Application.Seasons.GetSeasonSummaries
{
	public class GetSeasonSummariesQueryHandler
	{
		private readonly ISeasonRepository _seasonRepository;
		private readonly IAnimeRepository _animeRepository;
		private readonly ISeasonRoleRepository _seasonRoleRepository;
		private readonly ISeasonSearchCriteriaService _seasonSearchCriteriaService;

		public GetSeasonSummariesQueryHandler(
			ISeasonRepository seasonRepository,
			IAnimeRepository animeRepository,
			ISeasonRoleRepository roleRepository,
			ISeasonSearchCriteriaService seasonSearchCriteriaService
			)
		{
			_seasonRepository = seasonRepository;
			_animeRepository = animeRepository;
			_seasonRoleRepository = roleRepository;
			_seasonSearchCriteriaService = seasonSearchCriteriaService;
		}

		public async Task<PagedResult<SeasonSummaryEntryDto>> HandleAsync(GetSeasonSummariesQuery query)
		{
			var seasonPredicate = _seasonSearchCriteriaService.BuildExpression(query);

			var foundSeason = await _seasonRepository.GetAsync(seasonPredicate);

			if (foundSeason != null)
			{
				var animeTypeId = query.TVSeriesOnly
					? AnimeTypeId.TV
					: AnimeTypeId.AllTypes;

				var animeInSeason = await _animeRepository.GetAllBySeasonAndTypeAsync(foundSeason.Id, animeTypeId);

				if (animeInSeason.Any())
				{
					var animeInSeasonIds = animeInSeason.Select(x => x.Id).ToList();

					IReadOnlyCollection<AnimeRole> allRolesInSeason = await _seasonRoleRepository.GetAllRolesInSeason(
						animeInSeasonIds,
						query.MainRolesOnly
					);

					var groupedRoles = GroupRoles(allRolesInSeason, query);

					var mappedRoles = groupedRoles.Map<SeasonSummaryEntry, SeasonSummaryEntryDto>(groupedRoles.Results.Select(x => x.ToSeasonSummaryEntryDto()));

					return mappedRoles;
				}
			}

			return null;
		}

		private PagedResult<SeasonSummaryEntry> GroupRoles(IReadOnlyCollection<AnimeRole> roles, GetSeasonSummariesQuery query)
		{
			ICollection<SeasonSummaryEntry> groupedEntities = new List<SeasonSummaryEntry>();

			foreach (AnimeRole role in roles)
			{
				if (!groupedEntities.Select(x => x.Seiyuu.Id).Contains(role.SeiyuuId.Value))
				{
					groupedEntities.Add(new SeasonSummaryEntry(role.Seiyuu, role.Anime, role.Character));
				}
				else
				{
					groupedEntities
						.Single(x => x.Seiyuu.Id.Equals(role.SeiyuuId.Value))
						.AnimeCharacterPairs.Add((role.Anime, role.Character));
				}
			}

			return new PagedResult<SeasonSummaryEntry>()
			{
				Results = groupedEntities
					.OrderByDescending(x => x.AnimeCharacterPairs.Count)
					.ThenByDescending(x => x.GetTotalSignificanceValue())
					.Skip(query.Page * query.PageSize)
					.Take(query.PageSize)
					.ToList(),
				Page = query.Page,
				PageSize = query.PageSize,
				TotalCount = groupedEntities.Count()
			};
		}
	}
}