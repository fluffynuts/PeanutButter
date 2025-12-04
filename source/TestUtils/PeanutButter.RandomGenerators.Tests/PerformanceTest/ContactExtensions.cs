namespace PeanutButter.RandomGenerators.Tests.PerformanceTest;

public static class ContactExtensions
{
    public static string AsFullName(this Contact contact)
    {
        return contact == null ? "" : $"{contact.FirstNames ?? contact.Initials} {contact.Surname}".Trim();
    }

    public static string AsInitialAndSurname(this Contact contact)
    {
        return contact == null ? "" : $"{contact.Initials} {contact.Surname}".Trim();
    }
}