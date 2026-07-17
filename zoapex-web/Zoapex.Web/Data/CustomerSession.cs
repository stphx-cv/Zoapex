using Microsoft.AspNetCore.Http;

namespace Zoapex.Web.Data;

public static class CustomerSession
{
    private const string CustomerIdKey = "CustomerId";
    private const string CustomerNameKey = "CustomerName";
    private const string CustomerEmailKey = "CustomerEmail";
    private const string CustomerRoleKey = "CustomerRole";

    public static bool IsLoggedIn(ISession session)
        => session.GetInt32(CustomerIdKey) is > 0;

    public static int? GetCustomerId(ISession session)
        => session.GetInt32(CustomerIdKey);

    public static string? GetCustomerName(ISession session)
        => session.GetString(CustomerNameKey);

    public static string GetRole(ISession session)
        => session.GetString(CustomerRoleKey) ?? "Cliente";

    public static bool IsAdmin(ISession session)
        => string.Equals(GetRole(session), "Admin", StringComparison.OrdinalIgnoreCase);

    public static void SignIn(ISession session, int customerId, string fullName, string email, string role)
    {
        session.SetInt32(CustomerIdKey, customerId);
        session.SetString(CustomerNameKey, fullName);
        session.SetString(CustomerEmailKey, email);
        session.SetString(CustomerRoleKey, role);
    }

    public static void SignOut(ISession session)
    {
        session.Remove(CustomerIdKey);
        session.Remove(CustomerNameKey);
        session.Remove(CustomerEmailKey);
        session.Remove(CustomerRoleKey);
    }
}
