using RaceDay.Contracts;

namespace RaceDay.Mvc.ViewModels;

public class EventDetailsVm
{
    public EventDto Event { get; set; } = default!;
    public List<CategoryDto> Categories { get; set; } = new();
    public int SelectedCategoryId { get; set; }
}