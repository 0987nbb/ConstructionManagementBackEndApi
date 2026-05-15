namespace ConstructionManagement.Domain.Constants;

/// <summary>
/// Role-based access matrix for Modules 1–5 (Auth, Users, Clients, Projects, Sites).
/// </summary>
public static class RolePermissions
{
    // Module 1 — Auth / session (controller-level; registration is client-only in AuthService)
    public static bool CanAccessAdminDashboard(string role) => role == ApplicationRoles.Admin;

    // Module 2 — Users
    public static bool CanManageUsers(string role) => role == ApplicationRoles.Admin;

    // Module 3 — Clients
    public static bool CanManageClients(string role) => role == ApplicationRoles.Admin;
    public static bool CanReadClients(string role) =>
        role == ApplicationRoles.Admin || role == ApplicationRoles.ProjectManager;

    // Module 4 — Projects
    public static bool CanReadProjects(string role) =>
        role == ApplicationRoles.Admin ||
        role == ApplicationRoles.ProjectManager ||
        role == ApplicationRoles.Engineer ||
        role == ApplicationRoles.Accountant ||
        role == ApplicationRoles.Client;

    public static bool CanReadProjectFinancials(string role) =>
        role == ApplicationRoles.Admin ||
        role == ApplicationRoles.ProjectManager ||
        role == ApplicationRoles.Accountant;

    public static bool CanManageProjects(string role) =>
        role == ApplicationRoles.Admin || role == ApplicationRoles.ProjectManager;

    public static bool CanDeleteProjects(string role) => role == ApplicationRoles.Admin;

    // Module 5 — Sites
    public static bool CanReadSites(string role) =>
        role == ApplicationRoles.Admin ||
        role == ApplicationRoles.ProjectManager ||
        role == ApplicationRoles.Engineer ||
        role == ApplicationRoles.Accountant ||
        role == ApplicationRoles.Client;

    public static bool CanManageSites(string role) =>
        role == ApplicationRoles.Admin || role == ApplicationRoles.ProjectManager;

    public static bool CanDeleteSites(string role) => role == ApplicationRoles.Admin;

    public static bool CanAssignSiteEngineer(string role) =>
        role == ApplicationRoles.Admin || role == ApplicationRoles.ProjectManager;

    public static bool CanUpdateSiteStatus(string role) =>
        role == ApplicationRoles.Admin ||
        role == ApplicationRoles.ProjectManager ||
        role == ApplicationRoles.Engineer;

    public static bool CanUpdateSiteProgress(string role) =>
        role == ApplicationRoles.Admin ||
        role == ApplicationRoles.ProjectManager ||
        role == ApplicationRoles.Engineer;

    public static bool IsReadOnlyRole(string role) =>
        role == ApplicationRoles.Accountant || role == ApplicationRoles.Client;
}
