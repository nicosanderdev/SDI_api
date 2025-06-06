using Microsoft.EntityFrameworkCore.Migrations;

namespace SDI_Api.Infrastructure.Data.Migrations;

public partial class migration_save : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // It's good practice to define your seed data constants and IDs at a scope where they can be reused if needed,
        // but for direct inclusion in a migration, you can hardcode them.
        // For clarity in generation, I'm listing the GUIDs that will be used.

        // --- Fixed values for seeding ---
        var seedDate = new DateTimeOffset(2024, 07, 15, 10, 0, 0, TimeSpan.Zero);
        var seedUser = "system";

        // --- EstateProperty IDs (10) ---
        var epId1 = Guid.NewGuid().ToString();
        var epId2 = Guid.NewGuid().ToString();
        var epId3 = Guid.NewGuid().ToString();
        var epId4 = Guid.NewGuid().ToString();
        var epId5 = Guid.NewGuid().ToString();
        var epId6 = Guid.NewGuid().ToString();
        var epId7 = Guid.NewGuid().ToString();
        var epId8 = Guid.NewGuid().ToString();
        var epId9 = Guid.NewGuid().ToString();
        var epId10 = Guid.NewGuid().ToString();

        // --- PropertyImage IDs (2 per EstateProperty = 20) ---
        // For epId1
        var imgId1_1 = Guid.NewGuid().ToString(); // Main
        var imgId1_2 = Guid.NewGuid().ToString();
        // For epId2
        var imgId2_1 = Guid.NewGuid().ToString(); // Main
        var imgId2_2 = Guid.NewGuid().ToString();
        // ... and so on for all 10 properties
        var imgId3_1 = Guid.NewGuid().ToString();
        var imgId3_2 = Guid.NewGuid().ToString();
        var imgId4_1 = Guid.NewGuid().ToString();
        var imgId4_2 = Guid.NewGuid().ToString();
        var imgId5_1 = Guid.NewGuid().ToString();
        var imgId5_2 = Guid.NewGuid().ToString();
        var imgId6_1 = Guid.NewGuid().ToString();
        var imgId6_2 = Guid.NewGuid().ToString();
        var imgId7_1 = Guid.NewGuid().ToString();
        var imgId7_2 = Guid.NewGuid().ToString();
        var imgId8_1 = Guid.NewGuid().ToString();
        var imgId8_2 = Guid.NewGuid().ToString();
        var imgId9_1 = Guid.NewGuid().ToString();
        var imgId9_2 = Guid.NewGuid().ToString();
        var imgId10_1 = Guid.NewGuid().ToString();
        var imgId10_2 = Guid.NewGuid().ToString();


        // --- EstatePropertyDescription IDs (2 per EstateProperty = 20) ---
        // For epId1
        var descId1_EN = Guid.NewGuid().ToString(); // Featured (EN)
        var descId1_ES = Guid.NewGuid().ToString();
        // For epId2
        var descId2_EN = Guid.NewGuid().ToString(); // Featured (EN)
        var descId2_ES = Guid.NewGuid().ToString();
        // ... and so on for all 10 properties
        var descId3_EN = Guid.NewGuid().ToString();
        var descId3_ES = Guid.NewGuid().ToString();
        var descId4_EN = Guid.NewGuid().ToString();
        var descId4_ES = Guid.NewGuid().ToString();
        var descId5_EN = Guid.NewGuid().ToString();
        var descId5_ES = Guid.NewGuid().ToString();
        var descId6_EN = Guid.NewGuid().ToString();
        var descId6_ES = Guid.NewGuid().ToString();
        var descId7_EN = Guid.NewGuid().ToString();
        var descId7_ES = Guid.NewGuid().ToString();
        var descId8_EN = Guid.NewGuid().ToString();
        var descId8_ES = Guid.NewGuid().ToString();
        var descId9_EN = Guid.NewGuid().ToString();
        var descId9_ES = Guid.NewGuid().ToString();
        var descId10_EN = Guid.NewGuid().ToString();
        var descId10_ES = Guid.NewGuid().ToString();
        
        // --- PropertyMessageLog IDs (10) ---
        var pmlId1 = Guid.NewGuid().ToString();
        var pmlId2 = Guid.NewGuid().ToString();
        var pmlId3 = Guid.NewGuid().ToString();
        var pmlId4 = Guid.NewGuid().ToString();
        var pmlId5 = Guid.NewGuid().ToString();
        var pmlId6 = Guid.NewGuid().ToString();
        var pmlId7 = Guid.NewGuid().ToString();
        var pmlId8 = Guid.NewGuid().ToString();
        var pmlId9 = Guid.NewGuid().ToString();
        var pmlId10 = Guid.NewGuid().ToString();

        // --- PropertyVisitLog IDs (10) ---
        var pvlId1 = Guid.NewGuid().ToString();
        var pvlId2 = Guid.NewGuid().ToString();
        var pvlId3 = Guid.NewGuid().ToString();
        var pvlId4 = Guid.NewGuid().ToString();
        var pvlId5 = Guid.NewGuid().ToString();
        var pvlId6 = Guid.NewGuid().ToString();
        var pvlId7 = Guid.NewGuid().ToString();
        var pvlId8 = Guid.NewGuid().ToString();
        var pvlId9 = Guid.NewGuid().ToString();
        var pvlId10 = Guid.NewGuid().ToString();

        migrationBuilder.InsertData(
            table: "EstateProperties",
            columns: new[]
            {
                "Id", "Address", "Address2", "City", "State", "ZipCode", "Country", "IsPublic", "Title", "Price",
                "Status", "Type", "AreaValue", "AreaUnit", "Bedrooms", "Bathrooms", "CreatedOnUtc", "Visits",
                "MainImageId", "FeaturedDescriptionId", "IsDeleted", "Created", "CreatedBy", "LastModified",
                "LastModifiedBy"
            },
            values: new object[,]
            {
                {
                    epId1, "123 Sunshine Ave", "Apt 1A", "Miami", "FL", "33101", "USA", true, "Sunny Beachside Condo",
                    350000.00m, 0, "Condo", 85.5m, "m²", 2, 1, seedDate.UtcDateTime, 150, imgId1_1, descId1_EN, false,
                    seedDate, seedUser, seedDate, seedUser
                },
                {
                    epId2, "456 Mountain Rd", "Apt 1A", "Denver", "CO", "80202", "USA", true, "Cozy Mountain Chalet",
                    480000.00m, 0, "Chalet", 120.0m, "m²", 3, 2, seedDate.UtcDateTime, 80, imgId2_1, descId2_EN, false,
                    seedDate, seedUser, seedDate, seedUser
                },
                {
                    epId3, "789 Urban St", "Unit 5B", "New York", "NY", "10001", "USA", true, "Modern Downtown Loft",
                    1200000.00m, 1, "Loft", 110.75m, "m²", 1, 1, seedDate.UtcDateTime, 220, imgId3_1, descId3_EN, false,
                    seedDate, seedUser, seedDate, seedUser
                },
                {
                    epId4, "101 Suburbia Ln", "Apt 1A", "Austin", "TX", "78701", "USA", true, "Spacious Family House",
                    650000.00m, 0, "House", 200.0m, "m²", 4, 3, seedDate.UtcDateTime, 120, imgId4_1, descId4_EN, false,
                    seedDate, seedUser, seedDate, seedUser
                },
                {
                    epId5, "202 Countryside Dr", "Farmhouse", "Nashville", "TN", "37201", "USA", true,
                    "Charming Country Farmhouse", 550000.00m, 2, "Farmhouse", 180.0m, "m²", 3, 2, seedDate.UtcDateTime,
                    220, imgId5_1, descId5_EN, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    epId6, "303 Lakeside VW", "Farmhouse", "Chicago", "IL", "60601", "USA", true, "Apartment with Lake View",
                    750000.00m, 1, "Apartment", 95.0m, "m²", 2, 2, seedDate.UtcDateTime, 95, imgId6_1, descId6_EN,
                    false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    epId7, "404 Historic Sq", "Old Town", "Boston", "MA", "02109", "USA", true, "Historic Townhouse",
                    890000.00m, 0, "Townhouse", 150.25m, "m²", 3, 2, seedDate.UtcDateTime, 60, imgId7_1, descId7_EN,
                    false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    epId8, "505 Pacific Hwy", "Suite 1000", "San Francisco", "CA", "94105", "USA", true,
                    "Luxury Penthouse", 2500000.00m, 0, "Penthouse", 300.0m, "m²", 4, 4, seedDate.UtcDateTime, 300,
                    imgId8_1, descId8_EN, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    epId9, "606 Forest Path", "Cabin 3", "Asheville", "NC", "28801", "USA", true,
                    "Secluded Forest Cabin", 280000.00m, 1, "Cabin", 70.0m, "m²", 2, 1, seedDate.UtcDateTime, 45,
                    imgId9_1, descId9_EN, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    epId10, "707 Desert Oasis", "Farmhouse", "Phoenix", "AZ", "85001", "USA", true, "Modern Desert Home",
                    620000.00m, 0, "House", 220.5m, "m²", 3, 3, seedDate.UtcDateTime, 70, imgId10_1, descId10_EN, false,
                    seedDate, seedUser, seedDate, seedUser
                }
            });

        migrationBuilder.InsertData(
            table: "PropertyImages",
            columns: new[]
            {
                "Id", "Url", "AltText", "IsMain", "EstatePropertyId", "IsDeleted", "Created", "CreatedBy",
                "LastModified", "LastModifiedBy"
            },
            values: new object[,]
            {
                // Images for epId1
                {
                    imgId1_1, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "Main view of Sunny Beachside Condo", true, epId1, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    imgId1_2, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "Living room of Sunny Beachside Condo", false, epId1, false, seedDate, seedUser, seedDate, seedUser
                },
                // Images for epId2
                {
                    imgId2_1, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "Exterior of Cozy Mountain Chalet", true, epId2, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    imgId2_2, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "Kitchen of Cozy Mountain Chalet", false, epId2, false, seedDate, seedUser, seedDate, seedUser
                },
                // Images for epId3
                {
                    imgId3_1, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "City view from Modern Downtown Loft", true, epId3, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    imgId3_2, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "Interior of Modern Downtown Loft", false, epId3, false, seedDate, seedUser, seedDate, seedUser
                },
                // Images for epId4
                {
                    imgId4_1, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "Front yard of Spacious Family House", true, epId4, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    imgId4_2, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "Backyard of Spacious Family House", false, epId4, false, seedDate, seedUser, seedDate, seedUser
                },
                // Images for epId5
                {
                    imgId5_1, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "Charming Country Farmhouse exterior", true, epId5, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    imgId5_2, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "Kitchen in Country Farmhouse", false, epId5, false, seedDate, seedUser, seedDate, seedUser
                },
                // Images for epId6
                {
                    imgId6_1, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "Lake view from Apartment", true, epId6, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    imgId6_2, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "Bedroom in Apartment with Lake View", false, epId6, false, seedDate, seedUser, seedDate, seedUser
                },
                // Images for epId7
                {
                    imgId7_1, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "Historic Townhouse facade", true, epId7, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    imgId7_2, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "Living area of Historic Townhouse", false, epId7, false, seedDate, seedUser, seedDate, seedUser
                },
                // Images for epId8
                {
                    imgId8_1, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "Panoramic view from Luxury Penthouse", true, epId8, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    imgId8_2, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "Master suite in Luxury Penthouse", false, epId8, false, seedDate, seedUser, seedDate, seedUser
                },
                // Images for epId9
                {
                    imgId9_1, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "Secluded Forest Cabin among trees", true, epId9, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    imgId9_2, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "Cozy interior of Forest Cabin", false, epId9, false, seedDate, seedUser, seedDate, seedUser
                },
                // Images for epId10
                {
                    imgId10_1, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "Exterior of Modern Desert Home", true, epId10, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    imgId10_2, "https://picsum.photos/seed/" + Guid.NewGuid().ToString() + "/800/600",
                    "Pool area of Modern Desert Home", false, epId10, false, seedDate, seedUser, seedDate, seedUser
                }
            });

        migrationBuilder.InsertData(
            table: "EstatePropertyDescriptions",
            columns: new[]
            {
                "Id", "LanguageCode", "Title", "Text", "EstatePropertyId", "IsDeleted", "Created", "CreatedBy",
                "LastModified", "LastModifiedBy"
            },
            values: new object[,]
            {
                // Descriptions for epId1
                {
                    descId1_EN, "en", "Charming Condo by the Beach",
                    "Experience the best of Miami in this beautiful 2-bedroom condo, just steps from the ocean. Features an open-plan living area and modern amenities.",
                    epId1, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    descId1_ES, "es", "Encantador Condominio Cerca de la Playa",
                    "Viva lo mejor de Miami en este hermoso condominio de 2 habitaciones, a pasos del océano. Cuenta con una sala de estar de planta abierta y comodidades modernas.",
                    epId1, false, seedDate, seedUser, seedDate, seedUser
                },
                // Descriptions for epId2
                {
                    descId2_EN, "en", "Relaxing Mountain Getaway",
                    "This 3-bedroom chalet offers stunning mountain views and a cozy fireplace. Perfect for family vacations or a quiet retreat.",
                    epId2, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    descId2_ES, "es", "Escapada Relajante en la Montaña",
                    "Este chalet de 3 habitaciones ofrece impresionantes vistas a la montaña y una acogedora chimenea. Perfecto para vacaciones familiares o un retiro tranquilo.",
                    epId2, false, seedDate, seedUser, seedDate, seedUser
                },
                // Descriptions for epId3
                {
                    descId3_EN, "en", "Sophisticated Urban Living",
                    "A stunning 1-bedroom loft in the heart of New York. High ceilings, large windows, and premium finishes throughout.",
                    epId3, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    descId3_ES, "es", "Vida Urbana Sofisticada",
                    "Un impresionante loft de 1 habitación en el corazón de Nueva York. Techos altos, grandes ventanales y acabados de primera calidad.",
                    epId3, false, seedDate, seedUser, seedDate, seedUser
                },
                // Descriptions for epId4
                {
                    descId4_EN, "en", "Ideal Family Home in Austin",
                    "A wonderful 4-bedroom house with a large backyard, perfect for families. Located in a friendly neighborhood with great schools.",
                    epId4, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    descId4_ES, "es", "Casa Familiar Ideal en Austin",
                    "Una maravillosa casa de 4 habitaciones con un gran patio trasero, perfecta para familias. Ubicada en un vecindario amigable con excelentes escuelas.",
                    epId4, false, seedDate, seedUser, seedDate, seedUser
                },
                // Descriptions for epId5
                {
                    descId5_EN, "en", "Peaceful Countryside Farmhouse",
                    "Escape to this beautiful 3-bedroom farmhouse offering tranquility and space. Features a wraparound porch and acres of land.",
                    epId5, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    descId5_ES, "es", "Tranquila Granja en el Campo",
                    "Escápese a esta hermosa granja de 3 habitaciones que ofrece tranquilidad y espacio. Cuenta con un porche envolvente y acres de terreno.",
                    epId5, false, seedDate, seedUser, seedDate, seedUser
                },
                // Descriptions for epId6
                {
                    descId6_EN, "en", "Stunning Lakefront Apartment",
                    "Modern 2-bedroom apartment with breathtaking views of the lake. Includes access to building amenities like a gym and pool.",
                    epId6, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    descId6_ES, "es", "Impresionante Apartamento Frente al Lago",
                    "Moderno apartamento de 2 habitaciones con impresionantes vistas al lago. Incluye acceso a servicios del edificio como gimnasio y piscina.",
                    epId6, false, seedDate, seedUser, seedDate, seedUser
                },
                // Descriptions for epId7
                {
                    descId7_EN, "en", "Elegant Historic Residence",
                    "Step back in time with this beautifully preserved 3-bedroom townhouse in Boston's historic district. Original details combined with modern comforts.",
                    epId7, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    descId7_ES, "es", "Elegante Residencia Histórica",
                    "Retroceda en el tiempo con esta casa adosada de 3 habitaciones bellamente conservada en el distrito histórico de Boston. Detalles originales combinados con comodidades modernas.",
                    epId7, false, seedDate, seedUser, seedDate, seedUser
                },
                // Descriptions for epId8
                {
                    descId8_EN, "en", "Ultimate Luxury Penthouse Experience",
                    "This expansive 4-bedroom penthouse offers unparalleled city and bay views, private elevator access, and bespoke interiors.",
                    epId8, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    descId8_ES, "es", "Máxima Experiencia de Lujo en Penthouse",
                    "Este amplio penthouse de 4 habitaciones ofrece vistas incomparables de la ciudad y la bahía, acceso privado por ascensor e interiores a medida.",
                    epId8, false, seedDate, seedUser, seedDate, seedUser
                },
                // Descriptions for epId9
                {
                    descId9_EN, "en", "Your Private Forest Retreat",
                    "A cozy 2-bedroom cabin nestled in the woods. Perfect for nature lovers seeking peace and quiet. Features a wood-burning stove.",
                    epId9, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    descId9_ES, "es", "Su Retiro Privado en el Bosque",
                    "Una acogedora cabaña de 2 habitaciones enclavada en el bosque. Perfecta para amantes de la naturaleza que buscan paz y tranquilidad. Cuenta con una estufa de leña.",
                    epId9, false, seedDate, seedUser, seedDate, seedUser
                },
                // Descriptions for epId10
                {
                    descId10_EN, "en", "Contemporary Desert Oasis",
                    "Sleek and modern 3-bedroom home in Phoenix, designed for desert living. Features an infinity pool and drought-tolerant landscaping.",
                    epId10, false, seedDate, seedUser, seedDate, seedUser
                },
                {
                    descId10_ES, "es", "Oasis Contemporáneo en el Desierto",
                    "Elegante y moderna casa de 3 habitaciones en Phoenix, diseñada para la vida en el desierto. Cuenta con una piscina infinita y paisajismo tolerante a la sequía.",
                    epId10, false, seedDate, seedUser, seedDate, seedUser
                }
            });
        
        migrationBuilder.InsertData(
    table: "PropertyMessageLogs", // Assuming table name is pluralized
    columns: new[] { "Id", "PropertyId", "SentOnUtc", "IsDeleted", "Created", "CreatedBy", "LastModified", "LastModifiedBy" },
    values: new object[,]
    {
        { pmlId1, epId1, seedDate.UtcDateTime.AddMinutes(5), false, seedDate, seedUser, seedDate, seedUser },
        { pmlId2, epId2, seedDate.UtcDateTime.AddHours(1), false, seedDate, seedUser, seedDate, seedUser },
        { pmlId3, epId3, seedDate.UtcDateTime.AddMinutes(10), false, seedDate, seedUser, seedDate, seedUser },
        { pmlId4, epId4, seedDate.UtcDateTime.AddDays(-1), false, seedDate, seedUser, seedDate, seedUser }, // Message from yesterday
        { pmlId5, epId5, seedDate.UtcDateTime.AddMinutes(30), false, seedDate, seedUser, seedDate, seedUser },
        { pmlId6, epId1, seedDate.UtcDateTime.AddHours(2), false, seedDate, seedUser, seedDate, seedUser }, // Second message for epId1
        { pmlId7, epId7, seedDate.UtcDateTime.AddMinutes(15), false, seedDate, seedUser, seedDate, seedUser },
        { pmlId8, epId8, seedDate.UtcDateTime.AddMinutes(45), false, seedDate, seedUser, seedDate, seedUser },
        { pmlId9, epId9, seedDate.UtcDateTime.AddHours(-3), false, seedDate, seedUser, seedDate, seedUser }, // Message from 3 hours ago
        { pmlId10, epId10, seedDate.UtcDateTime.AddMinutes(20), false, seedDate, seedUser, seedDate, seedUser }
    });

migrationBuilder.InsertData(
    table: "PropertyVisitLogs", // Assuming table name is pluralized
    columns: new[] { "Id", "PropertyId", "VisitedOnUtc", "Source", "IsDeleted", "Created", "CreatedBy", "LastModified", "LastModifiedBy" },
    values: new object[,]
    {
        { pvlId1, epId1, seedDate.UtcDateTime.AddMinutes(1), "Web", false, seedDate, seedUser, seedDate, seedUser },
        { pvlId2, epId2, seedDate.UtcDateTime.AddMinutes(5), "App", false, seedDate, seedUser, seedDate, seedUser },
        { pvlId3, epId3, seedDate.UtcDateTime.AddMinutes(8), "Referral", false, seedDate, seedUser, seedDate, seedUser },
        { pvlId4, epId4, seedDate.UtcDateTime.AddDays(-2).AddHours(3), "Web", false, seedDate, seedUser, seedDate, seedUser }, // Visit from 2 days ago
        { pvlId5, epId5, seedDate.UtcDateTime.AddHours(1), "App", false, seedDate, seedUser, seedDate, seedUser },
        { pvlId6, epId6, seedDate.UtcDateTime.AddMinutes(25), "Web", false, seedDate, seedUser, seedDate, seedUser },
        { pvlId7, epId7, seedDate.UtcDateTime.AddMinutes(50), "Ad Campaign", false, seedDate, seedUser, seedDate, seedUser }, // Custom source
        { pvlId8, epId1, seedDate.UtcDateTime.AddHours(4), "Web", false, seedDate, seedUser, seedDate, seedUser }, // Second visit for epId1
        { pvlId9, epId9, seedDate.UtcDateTime.AddDays(-1).AddHours(10), "App", false, seedDate, seedUser, seedDate, seedUser }, // Visit from yesterday
        { pvlId10, epId10, seedDate.UtcDateTime.AddMinutes(12), "Organic Search", false, seedDate, seedUser, seedDate, seedUser } // Custom source
    });
    }
}
