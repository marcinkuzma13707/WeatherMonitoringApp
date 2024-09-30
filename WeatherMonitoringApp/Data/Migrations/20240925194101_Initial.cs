using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WeatherMonitoringApp.Data.Migrations
{
    public partial class Initial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CountryId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cities_Countries_CountryId",
                        column: x => x.CountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WeatherData",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Temperature = table.Column<double>(type: "float", nullable: false),
                    WindSpeed = table.Column<double>(type: "float", nullable: false),
                    Clouds = table.Column<int>(type: "int", nullable: false),
                    LastUpdate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeatherData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WeatherData_Cities_CityId",
                        column: x => x.CityId,
                        principalTable: "Cities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Cities_CountryId",
                table: "Cities",
                column: "CountryId");

            migrationBuilder.CreateIndex(
                name: "IX_WeatherData_CityId",
                table: "WeatherData",
                column: "CityId");

         migrationBuilder.Sql(@"
        CREATE PROCEDURE sp_GetMaxWindSpeedByCountry
            @CountryName NVARCHAR(100) = NULL
        AS
        BEGIN
            IF @CountryName IS NOT NULL
            BEGIN
                SELECT TOP 1 wd.Id, c.Name AS City, co.Name AS Country, wd.WindSpeed, wd.Temperature, wd.LastUpdate, wd.Clouds
                FROM WeatherData wd
                INNER JOIN Cities c ON wd.CityId = c.Id
                INNER JOIN Countries co ON c.CountryId = co.Id
                WHERE co.Name = @CountryName
                ORDER BY wd.WindSpeed DESC
            END
            ELSE
            BEGIN
                SELECT TOP 1 wd.Id, c.Name AS City, co.Name AS Country, wd.WindSpeed, wd.Temperature, wd.LastUpdate, wd.Clouds
                FROM WeatherData wd
                INNER JOIN Cities c ON wd.CityId = c.Id
                INNER JOIN Countries co ON c.CountryId = co.Id
                ORDER BY wd.WindSpeed DESC
            END
        END");

            migrationBuilder.Sql(@"
        CREATE PROCEDURE sp_GetMinTemperatureAndMaxWindSpeed
                @TemperatureThreshold FLOAT
            AS
            BEGIN
                SELECT wd.Id, c.Name AS City, co.Name AS Country, MIN(wd.Temperature) AS Temperature, MAX(wd.WindSpeed) AS WindSpeed, wd.LastUpdate, wd.Clouds
                FROM WeatherData wd
                INNER JOIN Cities c ON wd.CityId = c.Id
                INNER JOIN Countries co ON c.CountryId = co.Id
                WHERE wd.Temperature <= @TemperatureThreshold
                GROUP BY wd.Id, c.Name, co.Name, wd.LastUpdate, wd.Clouds
                ORDER BY Temperature ASC, WindSpeed DESC
            END");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeatherData");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.Sql("DROP PROCEDURE sp_GetMaxWindSpeedByCountry");
            migrationBuilder.Sql("DROP PROCEDURE sp_GetMinTemperatureAndMaxWindSpeed");
        }
    }
}
