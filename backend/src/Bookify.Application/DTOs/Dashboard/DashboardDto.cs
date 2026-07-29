namespace Bookify.Application.DTOs.Dashboard;

public class DashboardSummaryDto
{
    public int UpcomingAppointments { get; set; }
    public int PastAppointments { get; set; }
    public decimal TotalSpent { get; set; }
    public string Currency { get; set; } = "USD";
    public int UnreadNotifications { get; set; }
    public List<Appointments.AppointmentDto> UpcomingAppointmentList { get; set; } = new();
}

public class BusinessDashboardSummaryDto
{
    public int TotalAppointments { get; set; }
    public int UpcomingAppointments { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal MonthlyRevenue { get; set; }
    public string Currency { get; set; } = "USD";
    public double AverageRating { get; set; }
    public int TotalReviews { get; set; }
    public int TotalCustomers { get; set; }
}
