namespace FootballProjectSoftUni.Core.Services.Gallery
{
    /// <summary>Why an uploaded file can't become a picture of the site.</summary>
    public enum PictureProblem
    {
        None,
        NotAllowedType,
        Empty,
        TooLarge,
        NotAPicture
    }
}
