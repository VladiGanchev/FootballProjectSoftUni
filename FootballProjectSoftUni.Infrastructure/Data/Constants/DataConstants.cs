using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FootballProjectSoftUni.Infrastructure.Data.Constants
{
    public static class DataConstants
    {
        public const int TournamentDescriptionMinLength = 10;
        public const int TournamentDescriptionMaxLength = 150;

        public const int TournamentNumnerOfTeamsMinLength = 0;
        public const int TournamentNumnerOfTeamsMaxLength = 16;

        public const int TeamNameMinLength = 3;
        public const int TeamNameMaxLength = 15;

        public const int RefereeExperienceMinYears = 2;

        public const int CoachExperienceMinYears = 1;
        public const string CoachExperienceErrorMessage = "The coach experience must be at least {1} year.";

        public const string RequireErrorMessage = "The field {0} is required.";

        public const string StringLengthErrorMessage = "The field {0} must be between {2} and {1} symbols.";

        public const string RequiredDateTimeFormat = "dd/MM/yyyy HH:mm";


    }
}
