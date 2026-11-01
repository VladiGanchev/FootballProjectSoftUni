namespace FootballProjectSoftUni.Models
{
    /// <summary>The picture field of a form (_PicturePicker).</summary>
    public class PicturePickerViewModel
    {
        /// <summary>A new record needs a picture; when editing, an empty field keeps the current one.</summary>
        public bool Required { get; set; }

        public string? Hint { get; set; }
    }

    /// <summary>The page where the admin gives a city a new picture, or a partner a new logo.</summary>
    public class ChangePictureViewModel
    {
        /// <summary>"City" or "Partner": where the form goes.</summary>
        public string Controller { get; set; } = string.Empty;

        public bool IsLogo { get; set; }

        public int? SelectedId { get; set; }

        public List<PictureOwner> Items { get; set; } = new();
    }

    public record PictureOwner(int Id, string Name, string ImageUrl);
}
