using System.ComponentModel.DataAnnotations;

namespace WebApp.UI.ViewModels
{
    public class RegexDemoViewModel
    {
        [Required(ErrorMessage = "Input Text is required.")]
        [Display(Name = "Input Text")]
        public string InputText { get; set; } = string.Empty;

        [Display(Name = "Find Pattern")]
        public string? FindPattern { get; set; }

        [Display(Name = "Replace With")]
        public string? ReplaceWith { get; set; }

        [Display(Name = "Split Pattern")]
        public string? SplitPattern { get; set; }

        [Display(Name = "Parse Pattern")]
        public string? ParsePattern { get; set; }

        // Results
        public bool IsValidEmail { get; set; }
        public string? ReplaceResult { get; set; }
        public string[]? SplitResult { get; set; }
        public List<ParsedGroup> ParsedGroups { get; set; } = [];
    }

    public class ParsedGroup
    {
        public string? GroupName { get; set; }
        public string? Value { get; set; }
    }
}
