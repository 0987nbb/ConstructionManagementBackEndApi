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

        public static bool IsStaffAssignableRole(string? normalizedRole)
        {
            return normalizedRole is ProjectManager or Engineer or Accountant;
        }

        public static string? Normalize(string? role)
        {
            if (string.IsNullOrWhiteSpace(role))
            {
                return null;
            }

            var trimmed = role.Trim();
            var canonical = new string(trimmed.Where(char.IsLetterOrDigit).ToArray()).ToLowerInvariant();

            return canonical switch
            {
                "admin" => Admin,
                "projectmanager" => ProjectManager,
                "engineer" => Engineer,
                "accountant" => Accountant,
                "client" => Client,
                _ => null
            };
        }
    }
}
