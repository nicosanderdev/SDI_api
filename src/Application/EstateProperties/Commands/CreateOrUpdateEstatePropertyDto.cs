﻿using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SDI_Api.Application.Dtos;
using SDI_Api.Application.DTOs.EstateProperties;
using SDI_Api.Domain.Enums;

namespace SDI_Api.Application.EstateProperties.Commands;

public class CreateOrUpdateEstatePropertyDto
{
      /// /////////////////
    // Estate Property //
    /// /////////////////
    public Guid Id { get; set; }
    
    // Address
    
    /// <summary>
    /// Maps to 'streetName'.
    /// </summary>
    [Required]
    [MinLength(5)]
    public string StreetName { get; set; } = string.Empty;
    /// <summary>
    /// Maps to 'houseNumber'.
    /// </summary>
    [Required]
    public string HouseNumber { get; set; } = string.Empty;
    /// <summary>
    /// Maps to optional 'neighborhood'.
    /// </summary>
    public string? Neighborhood { get; set; }
    /// <summary>
    /// Maps to 'city'.
    /// </summary>
    [Required]
    public string City { get; set; } = string.Empty;
    /// <summary>
    /// Maps to 'state'.
    /// </summary>
    [Required]
    public string State { get; set; } = string.Empty;
    /// <summary>
    /// Maps to 'zipCode'.
    /// </summary>
    [Required]
    public string ZipCode { get; set; } = string.Empty;
    /// <summary>
    /// Maps to 'country'.
    /// </summary>
    [Required]
    public string Country { get; set; } = string.Empty;
    /// <summary>
    /// Maps to 'location' object.
    /// Note: The Zod 'refine' validation (checking for default coords)
    /// should be handled in the API controller or a service layer.
    /// </summary>
    [FromForm(Name = "Location")]
    public string? LocationString { get; set; }

    // This property will hold the deserialized object.
    // We ignore it for model binding purposes as we will populate it manually.
    [JsonIgnore]
    public LocationDto? Location { get; set; }

    // Description
    
    /// <summary>
    /// Maps to 'title'.
    /// </summary>
    [Required]
    public string Title { get; set; } = "";
    /// <summary>
    /// Maps to 'type'.
    /// </summary>
    [Required]
    public string? Type { get; set; }
    /// <summary>
    /// Maps to 'areaValue'.
    /// </summary>
    public int AreaValue { get; set; }
    /// <summary>
    /// Maps to 'areaUnit'.
    /// </summary>
    public int AreaUnit { get; set; }
    /// <summary>
    /// Maps to 'bedrooms'.
    /// </summary>
    public int Bedrooms { get; set; }
    /// <summary>
    /// Maps to 'bathrooms'.
    /// </summary>
    public int Bathrooms { get; set; }
    /// <summary>
    /// Maps to 'hasGarage'.
    /// </summary>
    [Required]
    public bool HasGarage { get; set; }
    /// <summary>
    /// Maps to 'garageSpaces'.
    /// </summary>
    [Required]
    public int GarageSpaces { get; set; } = 0;
    // other info
    /// <summary>
    /// Maps to 'visits'.
    /// </summary>
    public int Visits { get; set; } = 0;
    /// <summary>
    /// Maps to 'createdOnUtc'.
    /// </summary>
    public DateTime CreatedOnUtc { get; set; } = DateTime.UtcNow;
    
    // Relationships
    public List<IFormFile> Documents { get; set; } = new List<IFormFile>();
    /// <summary>
    /// Maps to 'mainImage'. This is the primary image file to be uploaded.
    /// </summary>
    public string? MainImageUrl { get; set; }
    /// <summary>
    /// A list of DTOs representing the processed images, including their URLs.
    /// This is for reading data, not for uploading.
    /// </summary>
    public List<IFormFile> Images { get; set; } = new List<IFormFile>();
    /// <summary>
    /// A string representing the owner ID.
    /// </summary>
    public string? OwnerId { get; set; }
    
    ///////////////////////////
    // Estate property values//
    ///////////////////////////
    
    /// <summary>
    /// Maps to 'description' enum.
    /// </summary>
    [MaxLength(1000)]
    public string? Description { get; set; }
    /// <summary>
    /// Maps to 'availableFrom' enum.
    /// </summary>
    public DateTime AvailableFrom { get; set; }
    /// <summary>
    /// Maps to 'arePetsAllowed' enum.
    /// </summary>
    public bool ArePetsAllowed { get; set; }
    /// <summary>
    /// Maps to 'capacity' enum.
    /// </summary>
    public int Capacity { get; set; }
    
    // Price and status
    
    /// <summary>
    /// Maps to 'currency' enum.
    /// </summary>
    public Currency Currency { get; set; }
    /// <summary>
    /// Maps to optional 'salePrice'. Using decimal for financial values.
    /// </summary>
    public decimal? SalePrice { get; set; }
    /// <summary>
    /// Maps to optional 'rentPrice'. Using decimal for financial values.
    /// </summary>
    public decimal? RentPrice { get; set; }
    /// <summary>
    /// Maps to 'hasCommonExpenses'.
    /// </summary>
    public bool HasCommonExpenses { get; set; }
    /// <summary>
    /// Maps to 'commonExpensesAmount'
    /// </summary>
    public decimal? CommonExpensesAmount { get; set; }
    /// <summary>
    /// Maps to 'isElectricityIncluded'.
    /// </summary>
    public bool IsElectricityIncluded { get; set; }
    /// <summary>
    /// Maps to 'isWaterIncluded'.
    /// </summary>
    public bool IsWaterIncluded { get; set; }
    /// <summary>
    /// Maps to 'isPriceVisible'.
    /// </summary>
    public bool IsPriceVisible { get; set; }
    /// <summary>
    /// Maps to 'status'.
    /// </summary>
    public PropertyStatus Status { get; set; }
    /// <summary>
    /// Maps to 'isActive'.
    /// </summary>
    public bool IsActive { get; set; }
    /// <summary>
    /// Maps to 'isPropertyVisible'.
    /// </summary>
    public bool IsPropertyVisible { get; set; }
}
