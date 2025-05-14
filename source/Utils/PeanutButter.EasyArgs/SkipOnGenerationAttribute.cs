using System;

/// <summary>
/// Properties decorated with this are not considered
/// when generating arguments from an options object
/// </summary>
[AttributeUsage(AttributeTargets.Property)]
public class SkipOnGenerationAttribute : Attribute
{
}