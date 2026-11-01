using FootballProjectSoftUni.Core.Models.Tournament;
using System.ComponentModel.DataAnnotations;

namespace FootballProjectSoftUni.Tests.UnitTests
{
    /// <summary>Every tournament is paid: with a fee below 1 € nobody could join, so it can't be saved.</summary>
    public class AddTournamentFormViewModelTests
    {
        [TestCase(0, false)]
        [TestCase(0.5, false)]
        [TestCase(1, true)]
        [TestCase(25.5, true)]
        public void ParticipationFee_MustBeAtLeastOneEuro(double fee, bool valid)
        {
            var model = new AddTournamentFormViewModel { ParticipationFee = (decimal)fee };
            var errors = new List<ValidationResult>();

            Validator.TryValidateProperty(model.ParticipationFee, new ValidationContext(model) { MemberName = nameof(model.ParticipationFee) }, errors);

            Assert.That(errors.Count == 0, Is.EqualTo(valid));
        }
    }
}
