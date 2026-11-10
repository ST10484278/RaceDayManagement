using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels;

public class EventFormVm
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Description { get; set; } = string.Empty;

    [Required, DataType(DataType.Date)]
    public DateTime Date { get; set; } = DateTime.Today.AddDays(7);

    [Required]
    public string Location { get; set; } = string.Empty;
}