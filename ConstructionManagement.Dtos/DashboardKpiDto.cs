namespace ConstructionManagement.Dtos;

public class DashboardKpiDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int InactiveUsers { get; set; }
    public int AdminCount { get; set; }
    public int ProjectManagerCount { get; set; }
    public int EngineerCount { get; set; }
    public int AccountantCount { get; set; }
    public int ClientCount { get; set; }
}
