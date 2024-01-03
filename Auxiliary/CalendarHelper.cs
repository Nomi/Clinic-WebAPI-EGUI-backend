using EGUI_Stage2.Models;

namespace EGUI_Stage2.Auxiliary
{
    public static class CalendarHelper
    {
        public static DateTime GetDateOfMondayOfWeek(DateTime date)
        {
            int delta = DayOfWeek.Monday - date.DayOfWeek;
            return date.AddDays(delta);
        }
    }
}
