using System.ComponentModel.DataAnnotations;

namespace FootballProjectSoftUni.Extensions
{
    /// <summary>A checkbox that has to be ticked, e.g. accepting the Terms and Conditions.</summary>
    public class MustBeTrueAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value) => value is true;
    }
}
