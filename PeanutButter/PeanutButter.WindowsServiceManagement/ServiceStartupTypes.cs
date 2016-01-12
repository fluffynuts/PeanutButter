namespace PeanutButter.Win32ServiceControl
{
    public enum ServiceStartupTypes
    {
        Unknown = -1,
        Boot = 0,
        System = 1,
        Automatic = 2,
        Manual = 3,
        Disabled = 4
    }
}