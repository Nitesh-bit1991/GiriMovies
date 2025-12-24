using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace GiriMovies.Server.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreatePostgreSQL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Movies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ThumbnailUrl = table.Column<string>(type: "text", nullable: false),
                    VideoUrl = table.Column<string>(type: "text", nullable: false),
                    DurationInSeconds = table.Column<int>(type: "integer", nullable: false),
                    Genre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ReleaseYear = table.Column<int>(type: "integer", nullable: false),
                    Rating = table.Column<double>(type: "double precision", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Movies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastLoginAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UserSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    DeviceType = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    DeviceId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    DeviceName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ComputerName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    MacAddress = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    ProcessorId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    LocalIP = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    Location = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CertificateThumbprint = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CertificateSubject = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CertificateValidFrom = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CertificateValidTo = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LoginTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastActivity = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LogoutTime = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    SessionToken = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserSessions_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WatchProgresses",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    MovieId = table.Column<int>(type: "integer", nullable: false),
                    CurrentPositionInSeconds = table.Column<int>(type: "integer", nullable: false),
                    LastWatchedDevice = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    LastWatchedDeviceId = table.Column<string>(type: "text", nullable: true),
                    LastWatchedDeviceName = table.Column<string>(type: "text", nullable: true),
                    LastWatchedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    ProgressPercentage = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WatchProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WatchProgresses_Movies_MovieId",
                        column: x => x.MovieId,
                        principalTable: "Movies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_WatchProgresses_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Movies",
                columns: new[] { "Id", "CreatedAt", "Description", "DurationInSeconds", "Genre", "Rating", "ReleaseYear", "ThumbnailUrl", "Title", "VideoUrl" },
                values: new object[,]
                {
                    { 1, new DateTime(2025, 11, 18, 13, 25, 48, 611, DateTimeKind.Utc).AddTicks(5672), "Two imprisoned men bond over a number of years, finding solace and eventual redemption through acts of common decency.", 8520, "Drama", 9.3000000000000007, 1994, "https://via.placeholder.com/300x450?text=Shawshank", "The Shawshank Redemption", "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/BigBuckBunny.mp4" },
                    { 2, new DateTime(2025, 11, 18, 13, 25, 48, 611, DateTimeKind.Utc).AddTicks(5675), "When the menace known as the Joker wreaks havoc and chaos on the people of Gotham, Batman must accept one of the greatest psychological and physical tests.", 9120, "Action", 9.0, 2008, "https://via.placeholder.com/300x450?text=Dark+Knight", "The Dark Knight", "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ElephantsDream.mp4" },
                    { 3, new DateTime(2025, 11, 18, 13, 25, 48, 611, DateTimeKind.Utc).AddTicks(5677), "A thief who steals corporate secrets through the use of dream-sharing technology is given the inverse task of planting an idea.", 8880, "Sci-Fi", 8.8000000000000007, 2010, "https://via.placeholder.com/300x450?text=Inception", "Inception", "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerBlazes.mp4" },
                    { 4, new DateTime(2025, 11, 18, 13, 25, 48, 611, DateTimeKind.Utc).AddTicks(5680), "The lives of two mob hitmen, a boxer, a gangster and his wife intertwine in four tales of violence and redemption.", 9240, "Crime", 8.9000000000000004, 1994, "https://via.placeholder.com/300x450?text=Pulp+Fiction", "Pulp Fiction", "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerEscapes.mp4" },
                    { 5, new DateTime(2025, 11, 18, 13, 25, 48, 611, DateTimeKind.Utc).AddTicks(5682), "A computer hacker learns from mysterious rebels about the true nature of his reality and his role in the war against its controllers.", 8160, "Sci-Fi", 8.6999999999999993, 1999, "https://via.placeholder.com/300x450?text=Matrix", "The Matrix", "https://commondatastorage.googleapis.com/gtv-videos-bucket/sample/ForBiggerFun.mp4" }
                });

            migrationBuilder.InsertData(
                table: "Users",
                columns: new[] { "Id", "CreatedAt", "Email", "LastLoginAt", "Name", "PasswordHash" },
                values: new object[] { 1, new DateTime(2025, 11, 18, 13, 25, 48, 611, DateTimeKind.Utc).AddTicks(5868), "test@netflix.com", new DateTime(2025, 11, 18, 13, 25, 48, 611, DateTimeKind.Utc).AddTicks(5869), "Test User", "$2a$11$zQjJ5VvZ7Z2xZ2xZ2xZ2xOe5Z2xZ2xZ2xZ2xZ2xZ2xZ2xZ2xZ2xZ2" });

            migrationBuilder.CreateIndex(
                name: "IX_Users_Email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_CertificateThumbprint",
                table: "UserSessions",
                column: "CertificateThumbprint");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_SessionToken",
                table: "UserSessions",
                column: "SessionToken");

            migrationBuilder.CreateIndex(
                name: "IX_UserSessions_UserId_IsActive",
                table: "UserSessions",
                columns: new[] { "UserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_WatchProgresses_MovieId",
                table: "WatchProgresses",
                column: "MovieId");

            migrationBuilder.CreateIndex(
                name: "IX_WatchProgresses_UserId_MovieId",
                table: "WatchProgresses",
                columns: new[] { "UserId", "MovieId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserSessions");

            migrationBuilder.DropTable(
                name: "WatchProgresses");

            migrationBuilder.DropTable(
                name: "Movies");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
