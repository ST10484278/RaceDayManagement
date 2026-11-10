using System.ComponentModel.DataAnnotations;

namespace RaceDay.Mvc.ViewModels;

public class CaptureResultVm
{
    [Required]
    public int EnrolmentId { get; set; }

    [Required]
    public TimeSpan FinishTime { get; set; }

    [Required, Range(1, 10000)]
    public int FinishingPosition { get; set; }
}