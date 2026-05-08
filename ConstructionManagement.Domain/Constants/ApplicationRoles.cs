namespace ConstructionManagement.Domain.Constants
{
    public static class ApplicationRoles
    {
        public const string Admin = "Admin";
        public const string ProjectManager = "Project Manager";
        public const string Engineer = "Engineer";
        public const string Accountant = "Accountant";
        public const string Client = "Client";

        public static readonly HashSet<string> All = new(StringComparer.OrdinalIgnoreCase)
        {
            Admin,
            ProjectManager,
            Engineer,
            Accountant,
            Client
        };
    }
}
