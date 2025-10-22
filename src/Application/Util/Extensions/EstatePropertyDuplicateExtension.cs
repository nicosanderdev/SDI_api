using SDI_Api.Domain.Entities;

namespace SDI_Api.Application.Util;

public static class EstatePropertyDuplicateExtension
{
    /// <summary>
    /// Copies scalar values from the source EstateProperty into the target EstateProperty,
    /// excluding Id and auditable fields (Created, CreatedBy, LastModified, LastModifiedBy),
    /// and excluding navigation/collection properties.
    /// </summary>
    /// <param name="source">The original property to copy from.</param>
    /// <param name="target">The new property to copy to.</param>
    public static void duplicateScalarValues(this EstateProperty source, EstateProperty target)
    {
        // Address
        target.StreetName = source.StreetName;
        target.HouseNumber = source.HouseNumber;
        target.Neighborhood = source.Neighborhood;
        target.City = source.City;
        target.State = source.State;
        target.ZipCode = source.ZipCode;
        target.Country = source.Country;
        target.LocationLatitude = source.LocationLatitude;
        target.LocationLongitude = source.LocationLongitude;

        // Description
        target.Title = source.Title + " (Copia)";
        target.Type = source.Type;
        target.AreaValue = source.AreaValue;
        target.AreaUnit = source.AreaUnit;
        target.Bedrooms = source.Bedrooms;
        target.Bathrooms = source.Bathrooms;
        target.HasGarage = source.HasGarage;
        target.GarageSpaces = source.GarageSpaces;
        target.Visits = source.Visits;

        // Relationships
        target.OwnerId = source.OwnerId;
    }
}
