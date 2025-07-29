namespace SDI_Api.Domain.Enums;

/// <summary>
/// Represents the type of a property.
/// </summary>
public enum PropertyType
{
    Apartment,
    House,
    Commercial,
    Land,
    Other
}

/// <summary>
/// Extension and utility methods for the <see cref="PropertyType"/> enum.
/// </summary>
public static class PropertyTypeExtensions
{
    /// <summary>
    /// Converts a <see cref="PropertyType"/> enum value to its display string representation.
    /// </summary>
    /// <param name="propertyType">The property type enum value.</param>
    /// <returns>The string representation of the enum value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the enum value is not defined.</exception>
    public static string ToDisplayString(this PropertyType propertyType)
    {
        return propertyType switch
        {
            PropertyType.Apartment => "Apartment",
            PropertyType.House => "House",
            PropertyType.Commercial => "Commercial",
            PropertyType.Land => "Land",
            PropertyType.Other => "Other",
            _ => throw new ArgumentOutOfRangeException(nameof(propertyType), propertyType, "Unsupported property type.")
        };
    }

    /// <summary>
    /// Converts a display string to its corresponding <see cref="PropertyType"/> enum value.
    /// This is the reverse of <see cref="ToDisplayString"/>.
    /// </summary>
    /// <param name="displayString">The string representation of the property type.</param>
    /// <returns>The corresponding <see cref="PropertyType"/> enum value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if the string does not match any known property type.</exception>
    public static PropertyType FromDisplayString(string displayString)
    {
        return displayString switch
        {
            "Apartment" => PropertyType.Apartment,
            "House" => PropertyType.House,
            "Commercial" => PropertyType.Commercial,
            "Land" => PropertyType.Land,
            "Other" => PropertyType.Other,
            _ => throw new ArgumentOutOfRangeException(nameof(displayString), displayString, "Invalid property type string.")
        };
    }
}
