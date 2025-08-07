namespace DevHabit.Api.Entities;

public sealed class User
{
    public string Id { get; set; }

    public string Email { get; set; }

    public string Name { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? UpdatedAtUtc { get; set; }

    /// <summary>
    /// We will store the IdentityId of the user from the Identity provider (e.g., Azure Ad, Keycloak,  Okta, Auth0, etc.).
    /// </summary>
    public string IdentityId { get; set; }
}
