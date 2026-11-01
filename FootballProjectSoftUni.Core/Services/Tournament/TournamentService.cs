using FootballProjectSoftUni.Core.Contracts.Tournament;
using FootballProjectSoftUni.Core.Extensions;
using FootballProjectSoftUni.Core.Models.City;
using FootballProjectSoftUni.Core.Models.Match;
using FootballProjectSoftUni.Core.Models.Tournament;
using FootballProjectSoftUni.Infrastructure.Data;
using FootballProjectSoftUni.Infrastructure.Data.Enums;
using FootballProjectSoftUni.Infrastructure.Data.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using X.PagedList;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace FootballProjectSoftUni.Core.Services.Tournament
{
    public class TournamentService : ITournamentService
    {
        private readonly ApplicationDbContext data;

        /// <summary>
        /// A started (or finished) tournament keeps its dates: the start, the refunds and the reminders depend on them.
        /// </summary>
        public static bool DatesLocked(FootballProjectSoftUni.Infrastructure.Data.Models.Tournament tournament) =>
            tournament.Status != TournamentStatus.Upcoming || BgTime.Now >= tournament.StartDate;

        public TournamentService(ApplicationDbContext _data)
        {
            data = _data;
        }

        public async Task<bool> DeleteTournamentAsync(int id)
        {
            var tournament = await data.Tournaments
                .Include(t => t.TournamentCities)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
            {
                return false;
            }

            // Payments are kept as financial records: remember what they were for, then unlink them.
            var cityName = await data.TournamentsCities
                .Where(tc => tc.TournamentId == id)
                .Select(tc => tc.City.Name)
                .FirstOrDefaultAsync();

            var payments = await data.TournamentJoinPayments
                .Where(p => p.TournamentId == id)
                .ToListAsync();

            foreach (var payment in payments)
            {
                payment.TournamentCity = cityName;
                payment.TournamentStartDate = tournament.StartDate;
                payment.TournamentId = null;
            }

            if (tournament.RefereeId != null)
            {
                var referee = await data.Referees
                    .FirstOrDefaultAsync(x => x.Id == tournament.RefereeId);

                if (referee != null)
                {
                    referee.TournamentId = null;
                }

                tournament.RefereeId = null;
            }

            var tournamentParticipants = await data.TournamentsParticipants
                .Where(x => x.TournamentId == id)
                .ToListAsync();

            if (tournamentParticipants.Any())
            {
                data.TournamentsParticipants.RemoveRange(tournamentParticipants);
            }

            var matches = await data.Matches
                .Where(m => m.TournamentId == id)
                .ToListAsync();

            if (matches.Any())
            {
                data.Matches.RemoveRange(matches);
            }

            var tournamentTeams = await data.TournamentsTeams
                .Where(x => x.TournamentId == id)
                .ToListAsync();

            if (tournamentTeams.Any())
            {
                data.TournamentsTeams.RemoveRange(tournamentTeams);
            }

            var tournamentCities = await data.TournamentsCities
                .Where(x => x.TournamentId == id)
                .ToListAsync();

            if (tournamentCities.Any())
            {
                data.TournamentsCities.RemoveRange(tournamentCities);
            }

            data.Tournaments.Remove(tournament);

            await data.SaveChangesAsync();

            return true;
        }


        public async Task EditTournamentAsync(EditViewModel model, DateTime start, DateTime end)
        {
            var tournament = await data.Tournaments
         .Include(t => t.TournamentCities)
         .FirstOrDefaultAsync(t => t.Id == model.Id);

            if (tournament == null)
            {
                throw new ArgumentException("Invalid tournament ID");
            }

            if (tournament.StartDate != start)
            {
                // A moved tournament gets its 24-hour reminder and its "too few teams" warning again, for the new date.
                tournament.ReminderSent = false;
                tournament.LowTeamsWarningSent = false;
            }

            // The winner is not edited here: it comes from the result of the final.
            tournament.Description = model.Description;
            tournament.StartDate = start;
            tournament.EndDate = end;
            tournament.ImageUrl = model.ImageUrl;
            tournament.Prize = model.Prize;
            tournament.SecondPlacePrize = model.SecondPlacePrize;
            tournament.ThirdPlacePrize = model.ThirdPlacePrize;

            await data.SaveChangesAsync();
        }

        public async Task<CityViewModel> FindCityAsync(int id)
        {
            var city = await data.Cities.FindAsync(id);

            if (city == null)
            {
                return null;
            }

            return new CityViewModel()
            {
                Id = city.Id,
                Name = city.Name,
                ImageUrl = city.ImageUrl
            };
        }

        public async Task<IEnumerable<CityViewModel>> GetCitiesAsync()
        {
            return await data.Cities.Select(x => new CityViewModel()
            {
                Id = x.Id,
                Name = x.Name
            }).ToListAsync();
        }

        public async Task<IPagedList<TournamentViewModel>> GetCityTournamentsAsync(int id, bool showPast, int? page)
        {
            var pageNumber = Paging.ValidPage(page);
            var pageSize = 9;

            var cityExists = await data.Cities.AnyAsync(x => x.Id == id);
            if (!cityExists) return null;

            var now = BgTime.Now;

            // базов query
            var query = data.TournamentsCities
                .Where(x => x.CityId == id)
                .Select(x => new TournamentViewModel
                {
                    Id = x.TournamentId,
                    StartDate = x.Tournament.StartDate,
                    EndDate = x.Tournament.EndDate,
                    Description = x.Tournament.Description,
                    RefereeId = x.Tournament.RefereeId,
                    NumberOfTeams = x.Tournament.NumberOfTeams,
                    ImageUrl = x.Tournament.ImageUrl,

                    // можеш да оставиш това, но реалният "динамичен" статус ще го сложим после
                    Status = x.Tournament.Status
                });

            // showPast = приключили => EndDate < now
            if (showPast)
                query = query.Where(x => x.EndDate < now)
                             .OrderByDescending(x => x.EndDate);
            else
                query = query.Where(x => x.EndDate >= now)
                             .OrderBy(x => x.StartDate);

            var paged = await query.ToPagedListAsync(pageNumber, pageSize);

            // тук вече имаш реални обекти (материализирани) и можеш да сметнеш динамичния статус
            foreach (var t in paged)
                UpdateTournamentStatus(t, now);

            return paged;
        }

        public async Task<DetailsViewModel> GetTournamentDetailsAsync(int id)
        {
            var needed = await data.Tournaments
                .Include(t => t.Referee)
                .Include(t => t.TournamentCities).ThenInclude(tc => tc.City)
                .Include(t => t.TournamentTeams).ThenInclude(tt => tt.Team)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.Team1)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.Team2)
                .Include(t => t.Matches)
                    .ThenInclude(m => m.WinnerTeam)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (needed == null)
            {
                return null;
            }

            var cityId = needed.TournamentCities.FirstOrDefault().CityId;
            if (cityId == null) return null;

            var model = new DetailsViewModel()
            {
                Id = needed.Id,
                Description = needed.Description,
                StartDate = needed.StartDate,
                EndDate = needed.EndDate,
                RefereeId = needed.RefereeId,
                RefereeName = needed.Referee?.Name,
                CreatedOn = needed.CreatedOn,
                Status = needed.Status.ToString(),
                NumberOfTeams = needed.NumberOfTeams,
                CityId = cityId,
                ParticipantTeams = needed.TournamentTeams
                    .Where(tt => tt.Team != null)
                    .Select(tt => tt.Team.Name)
                    .ToList(),
                Prize = needed.Prize,
                ParticipationFee = needed.ParticipationFee,
                Winner = needed.Winner,
                SecondPlacePrize = needed.SecondPlacePrize,
                ThirdPlacePrize = needed.ThirdPlacePrize,

            };

            model.Matches = needed.Matches
       .OrderBy(m => m.Round)
       .ThenBy(m => m.IndexInRound)
       .Select(m => new MatchViewModel
       {
           Id = m.Id,
           Round = m.Round,
           IndexInRound = m.IndexInRound,
           Team1Id = m.Team1Id,
           Team1Name = m.Team1 != null ? m.Team1.Name : "---",
           Team2Id = m.Team2Id,
           Team2Name = m.Team2 != null ? m.Team2.Name : "---",
           Team1Goals = m.Team1Goals,
           Team2Goals = m.Team2Goals,
           WinnerTeamId = m.WinnerTeamId,
           WinnerTeamName = m.WinnerTeam != null ? m.WinnerTeam.Name : null
       })
       .ToList();

            return model;

        }

        public async Task AddTournamentToCityAsync(AddTournamentFormViewModel model, int cityId, DateTime start, DateTime end)
        {
            var tournament = new FootballProjectSoftUni.Infrastructure.Data.Models.Tournament()
            {
                StartDate = start,
                EndDate = end,
                CreatedOn = BgTime.Now,
                Description = model.Description,
                NumberOfTeams = 0,
                ImageUrl = model.ImageUrl,
                Status = FootballProjectSoftUni.Infrastructure.Data.Enums.TournamentStatus.Upcoming,
                OrganiserId = "600bafb9-a73d-4489-a387-643c2b8ae96c",
                RefereeId = null,
                Prize = model.Prize,
                SecondPlacePrize = model.SecondPlacePrize,
                ThirdPlacePrize = model.ThirdPlacePrize,
                ParticipationFee = model.ParticipationFee

            };

            data.Tournaments.Add(tournament);

            await data.SaveChangesAsync();

            var cityTournament = new TournamentCity()
            {
                CityId = cityId,
                TournamentId = tournament.Id
            };

            data.TournamentsCities.Add(cityTournament);

            var stats = await data.AppStats.FindAsync(1);

            if (stats != null)
            {
                stats.TournamentsCreatedTotal++;
            }

            await data.SaveChangesAsync();
        }


        private void UpdateTournamentStatus(TournamentViewModel tournament, DateTime now)
        {
            if (tournament.StartDate > now)
                tournament.Status = TournamentStatus.Upcoming;
            else if (now >= tournament.StartDate && now < tournament.EndDate)
                tournament.Status = TournamentStatus.Started;
            else
                tournament.Status = TournamentStatus.Finished;
        }

        public async Task<EditViewModel> FindTournamentAsync(int id)
        {
            var tournament = await data.Tournaments
                .Include(t => t.TournamentCities)
                   .ThenInclude(tc => tc.City)
                   .FirstOrDefaultAsync(t => t.Id == id);

            if (tournament == null)
            {
                return null;
            }

            return new EditViewModel()
            {
                Id = tournament.Id,
                Description = tournament.Description,
                // Values for the date pickers of the edit form (the same in every language).
                EndDate = tournament.EndDate.ToDateTimeInput(),
                StartDate = tournament.StartDate.ToDateTimeInput(),
                ImageUrl = tournament.ImageUrl,
                CreatedOn = tournament.CreatedOn.ToString(RequiredDateTimeFormat),
                Winner = tournament.Winner,
                Prize = tournament.Prize,
                SecondPlacePrize = tournament.SecondPlacePrize,
                ThirdPlacePrize = tournament.ThirdPlacePrize,
                DatesLocked = DatesLocked(tournament)
            };
        }

        public async Task<FootballProjectSoftUni.Infrastructure.Data.Models.Tournament> FindTournamentByIdAsync(int id)
        {
            return await data.Tournaments
                .Include(t => t.TournamentCities)
                    .ThenInclude(tc => tc.City)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task GenerateBracketAsync(int tournamentId)
        {
            var tournament = await data.Tournaments
                .Include(t => t.Matches)
                .FirstOrDefaultAsync(t => t.Id == tournamentId);

            if (tournament == null)
            {
                throw new ArgumentException("Invalid tournament id");
            }

            if (tournament.Matches.Any())
            {
                return;
            }

            for (int i = 0; i < 8; i++)
            {
                data.Matches.Add(new Match
                {
                    TournamentId = tournamentId,
                    Round = 1,
                    IndexInRound = i
                });
            }

            for (int i = 0; i < 4; i++)
            {
                data.Matches.Add(new Match
                {
                    TournamentId = tournamentId,
                    Round = 2,
                    IndexInRound = i
                });
            }

            for (int i = 0; i < 2; i++)
            {
                data.Matches.Add(new Match
                {
                    TournamentId = tournamentId,
                    Round = 3,
                    IndexInRound = i
                });
            }

            data.Matches.Add(new Match
            {
                TournamentId = tournamentId,
                Round = 4,
                IndexInRound = 0
            });

            await data.SaveChangesAsync();
        }

        public async Task AssignTeamToBracketAsync(int tournamentId, int teamId)
        {
            var firstRoundMatches = await data.Matches
                .Where(m => m.TournamentId == tournamentId && m.Round == 1)
                .OrderBy(m => m.IndexInRound)
                .ToListAsync();

            // First one team in every first-round match (positions 1, 3, 5 ... 15), then the second places
            // (2, 4 ... 16). With 8-15 teams every team still gets into the second round: the ones without
            // an opponent go through when the tournament starts.
            var freeSlot = firstRoundMatches.FirstOrDefault(m => !m.Team1Id.HasValue);
            if (freeSlot != null)
            {
                freeSlot.Team1Id = teamId;
                await data.SaveChangesAsync();
                return;
            }

            freeSlot = firstRoundMatches.FirstOrDefault(m => !m.Team2Id.HasValue);
            if (freeSlot != null)
            {
                freeSlot.Team2Id = teamId;
                await data.SaveChangesAsync();
                return;
            }

            throw new InvalidOperationException("No free slots in bracket.");
        }

        public async Task PrepareBracketForStartAsync(int tournamentId)
        {
            var firstRound = await data.Matches
                .Where(m => m.TournamentId == tournamentId && m.Round == 1)
                .OrderBy(m => m.IndexInRound)
                .ToListAsync();

            if (!firstRound.Any())
            {
                return;
            }

            var hasResults = await data.Matches.AnyAsync(m =>
                m.TournamentId == tournamentId && (m.WinnerTeamId.HasValue || m.Team1Goals.HasValue || m.Team2Goals.HasValue));

            // Brackets filled the old way (1 v 2, 3 v 4 ...) or after teams left can have first-round matches
            // without any team; that leaves holes in later rounds. Spread the teams again, in the same order.
            if (!hasResults && firstRound.Any(m => !m.Team1Id.HasValue && !m.Team2Id.HasValue))
            {
                var teams = firstRound
                    .SelectMany(m => new[] { m.Team1Id, m.Team2Id })
                    .Where(id => id.HasValue)
                    .Select(id => id!.Value)
                    .ToList();

                foreach (var match in firstRound)
                {
                    match.Team1Id = null;
                    match.Team2Id = null;
                }

                for (var i = 0; i < teams.Count && i < firstRound.Count * 2; i++)
                {
                    if (i < firstRound.Count)
                    {
                        firstRound[i].Team1Id = teams[i];
                    }
                    else
                    {
                        firstRound[i - firstRound.Count].Team2Id = teams[i];
                    }
                }
            }

            // A team without an opponent goes straight to the next round.
            foreach (var match in firstRound.Where(m => !m.WinnerTeamId.HasValue && (m.Team1Id.HasValue != m.Team2Id.HasValue)))
            {
                match.WinnerTeamId = match.Team1Id ?? match.Team2Id;
                await MoveWinnerToNextRoundAsync(match.Id);
            }

            await data.SaveChangesAsync();
        }

        public async Task MoveWinnerToNextRoundAsync(int matchId)
        {
            var match = await data.Matches.FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null || !match.WinnerTeamId.HasValue)
            {
                return;
            }

            if (match.Round == 4)
            {
                var tournament = await data.Tournaments.FirstOrDefaultAsync(t => t.Id == match.TournamentId);
                if (tournament != null)
                {
                    var team = await data.Teams.FindAsync(match.WinnerTeamId.Value);
                    tournament.Winner = team?.Name;
                }

                return;
            }

            int nextRound = match.Round + 1;
            int nextIndex = match.IndexInRound / 2;

            var nextMatch = await data.Matches.FirstOrDefaultAsync(m =>
                m.TournamentId == match.TournamentId &&
                m.Round == nextRound &&
                m.IndexInRound == nextIndex);

            if (nextMatch == null)
            {
                return;
            }

            if (match.IndexInRound % 2 == 0)
            {
                nextMatch.Team1Id = match.WinnerTeamId;
            }
            else
            {
                nextMatch.Team2Id = match.WinnerTeamId;
            }
        }

        public async Task RemoveTeamFromBracketAsync(int tournamentId, int teamId)
        {
            var tournament = await data.Tournaments.FirstOrDefaultAsync(t => t.Id == tournamentId);
            if (tournament == null)
            {
                return;
            }

            if (BgTime.Now >= tournament.StartDate)
            {
                return;
            }

            var matches = await data.Matches
                .Where(m => m.TournamentId == tournamentId &&
                            (m.Team1Id == teamId || m.Team2Id == teamId || m.WinnerTeamId == teamId))
                .ToListAsync();

            foreach (var match in matches)
            {
                if (match.Team1Id == teamId) match.Team1Id = null;
                if (match.Team2Id == teamId) match.Team2Id = null;
                if (match.WinnerTeamId == teamId) match.WinnerTeamId = null;

                match.Team1Goals = null;
                match.Team2Goals = null;
            }

            await data.SaveChangesAsync();
        }

        public async Task<EnterResultViewModel?> GetEnterResultModelAsync(int matchId)
        {
            var match = await data.Matches
                .Include(m => m.Team1)
                .Include(m => m.Team2)
                .FirstOrDefaultAsync(m => m.Id == matchId);

            if (match == null)
            {
                return null;
            }

            return new EnterResultViewModel
            {
                MatchId = match.Id,
                TournamentId = match.TournamentId,

                Team1Id = match.Team1Id,
                Team1Name = match.Team1?.Name,

                Team2Id = match.Team2Id,
                Team2Name = match.Team2?.Name,

                Team1Goals = match.Team1Goals ?? 0,
                Team2Goals = match.Team2Goals ?? 0
            };
        }

            public async Task<(bool ok, string? error, int tournamentId)> EnterMatchResultAsync(EnterResultViewModel model)
        {
            var match = await data.Matches.FirstOrDefaultAsync(m => m.Id == model.MatchId);

            if (match == null)
            {
                return (false, "Match not found.", 0);
            }

            if (model.Team1Goals < 0 || model.Team2Goals < 0)
            {
                return (false, "Головете не могат да са отрицателни.", match.TournamentId);
            }

            if (model.Team1Goals == model.Team2Goals)
            {
                return (false, "Равенство не е позволено. Моля, въведи победител.", match.TournamentId);
            }

            match.Team1Goals = model.Team1Goals;
            match.Team2Goals = model.Team2Goals;

            if (model.Team1Goals > model.Team2Goals)
            {
                match.WinnerTeamId = match.Team1Id;
            }
            else
            {
                match.WinnerTeamId = match.Team2Id;
            }

            await MoveWinnerToNextRoundAsync(match.Id);

            await data.SaveChangesAsync();

            return (true, null, match.TournamentId);
        }
    }
}
