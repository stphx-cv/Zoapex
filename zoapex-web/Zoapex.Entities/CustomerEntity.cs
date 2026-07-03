namespace Zoapex.Entities;

public class CustomerEntity
{
    public int    CustomerId { get; set; }
    public string FirstName  { get; set; } = string.Empty;
    public string LastName   { get; set; } = string.Empty;
    public string Email      { get; set; } = string.Empty;
    public string Phone      { get; set; } = string.Empty;
    public string Address    { get; set; } = string.Empty;
    public int    Status     { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
