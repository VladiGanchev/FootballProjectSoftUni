using Microsoft.Extensions.Localization;
using static FootballProjectSoftUni.Infrastructure.Data.Constants.DataConstants;

namespace FootballProjectSoftUni.Core.Extensions
{
    /// <summary>
    /// The services return their error messages in English (the unit tests check those exact texts).
    /// Before a message is shown to the user it is translated here to the current site language.
    /// </summary>
    public static class ServiceMessageLocalizer
    {
        private static readonly Dictionary<string, string> Keys = new()
        {
            // Joining a tournament with a team
            ["You need to become a coach to join a team."] = "SMBecomeCoachFirst",
            ["BadRequest Message"] = "SMTournamentNotFound",
            ["Tournament not found."] = "SMTournamentNotFound",
            ["You cannot join a tournament that has already finished."] = "SMTournamentFinished",
            ["You cannot join a tournament that has already started."] = "SMTournamentStarted",
            ["You cannot apply for second tournament. You have to leave the current one in order to join a new one."] = "SMAlreadyInTournament",
            ["You cannot apply for this tournament because the maximum limit of teams has been reached."] = "SMTournamentFull",
            ["You cannot apply second time for this tournament because you already participate as a coach in it."] = "SMAlreadyCoachInTournament",
            ["You cannot apply as a coach for this tournament because you are already a referee in it."] = "SMRefereeCannotBeCoach",
            ["A Team with the same name already exists"] = "SMTeamNameTaken",
            ["You already have a team."] = "SMAlreadyHaveTeam",
            ["You must enter at least 6 players."] = "SMMinPlayers",
            ["Please enter at least 6 players."] = "SMMinPlayers",
            ["Coach not found."] = "SMCoachNotFound",

            // Paying the participation fee
            ["You have already paid for this tournament."] = "SMAlreadyPaid",
            ["Your payment for this tournament is already being processed."] = "SMPaymentProcessing",
            ["Your previous payment for this tournament is still being refunded."] = "SMRefundInProgress",
            ["Liability declaration must be accepted before payment."] = "SMDeclarationRequired",
            ["Participation fee is not configured."] = "SMFeeMissing",
            ["Invalid fee amount."] = "SMFeeMissing",
            ["Team not found."] = "SMTeamNotFound",
            ["The tournament is already full."] = "SMTournamentFull",
            ["Tournament already started."] = "SMTournamentStarted",
            ["Tournament already finished."] = "SMTournamentFinished",
            ["The user already participates in another active tournament."] = "SMAlreadyInTournament",
            ["The user is a referee in this tournament."] = "SMRefereeCannotBeCoach",
            ["This team does not belong to the paying user."] = "SMTeamNotYours",

            // Referee
            ["You cannot become a referee for a tournament that has already started."] = "SMRefereeTournamentStarted",
            ["You cannot apply as a referee for this tournament because there is already registered referee."] = "SMRefereeSlotTaken",
            ["You cannot apply as a referee for this tournament because you already participate as a coach in it."] = "SMCoachCannotBeReferee",
            ["You cannot apply as a referee for this tournament because you already participate in another one as a referee. You have to leave the current tournament you are in to become a referee to another one."] = "SMRefereeElsewhere",
            ["You cannot become a referee because you are already registered as a coach in the system."] = "SMCoachCannotBecomeReferee",
            ["Rating must be between 1 and 5."] = "SMRatingRange",
            ["You cannot rate yourself."] = "RateSelfNotAllowed",
            ["You cannot stop being a referee while your tournament is in progress."] = "SMRefereeStopRunning",
            ["You cannot stop being a referee while you are assigned to an upcoming tournament. Leave it first."] = "SMRefereeStopUpcoming",

            // Coach
            ["You have already registered as a coach."] = "SMAlreadyCoach",
            ["You cannot become a coach because you are already registered as a referee in the system."] = "SMRefereeCannotBecomeCoach",
            ["You cannot stop being a coach while still participating in a tournament."] = "SMCoachStillPlaying",

            // Refunds (admin)
            ["Order not found."] = "SMOrderNotFound",
            ["Only PAID orders can be refunded."] = "SMRefundOnlyPaid",
            ["This order is already refunded (or refund is started)."] = "SMAlreadyRefunded",
            ["Missing Stripe PaymentIntentId for this order."] = "SMRefundNoPaymentIntent",
            ["Refunds are possible only before the tournament starts."] = "RefundTournamentStarted",
            ["Refund amount must be > 0."] = "SMRefundAmountPositive",
            ["Refund amount cannot exceed paid amount."] = "SMRefundAmountTooHigh",
        };

        // Messages with a number in them: the translation gets the same number.
        private static readonly Dictionary<string, (string Key, int Number)> KeysWithNumber = new()
        {
            [$"A team can have at most {MaxPlayersPerTeam} players."] = ("SMTooManyPlayers", MaxPlayersPerTeam),
            [$"Player names can be at most {PlayerNameMaxLength} characters."] = ("SMPlayerNameTooLong", PlayerNameMaxLength),
        };

        /// <summary>
        /// The translation of one of the services' own messages, or null for any other text - for example
        /// the technical message of an exception from EF or .NET, which must never reach the screen.
        /// </summary>
        public static string? KnownServiceMessage(this IStringLocalizer localizer, string? message)
        {
            if (message == null)
            {
                return null;
            }

            if (Keys.TryGetValue(message, out var key))
            {
                return localizer[key].Value;
            }

            if (KeysWithNumber.TryGetValue(message, out var withNumber))
            {
                return localizer[withNumber.Key, withNumber.Number].Value;
            }

            return null;
        }

        /// <summary>For the messages of the services' error results (always their own texts): unknown ones are shown as they are.</summary>
        public static string ServiceMessage(this IStringLocalizer localizer, string? message) =>
            localizer.KnownServiceMessage(message) ?? message ?? string.Empty;
    }
}
