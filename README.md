# GiriMovies

**An offline, multi-device movie streaming application built with Blazor WebAssembly and .NET 8.**

GiriMovies is a personal software project designed for streaming videos across computers on a local network. It includes user authentication, device identification and watch-progress synchronisation, so a user can continue watching from another device.

## Features

- **Web client:** Blazor WebAssembly interface with an HTML5 video player.
- **Backend API:** ASP.NET Core Web API built with .NET 8.
- **Authentication:** User registration and login using JWT bearer tokens.
- **Device identification:** Device-aware session tracking.
- **Watch-progress synchronisation:** Save playback positions and retrieve them when opening a video on another device.
- **Video playback:** Resume playback using the saved position and browser video events.
- **Database:** PostgreSQL with Entity Framework Core.

## Technology Stack

| Area | Technologies |
|---|---|
| Frontend | Blazor WebAssembly, Razor, HTML, CSS, JavaScript |
| Backend | C#, .NET 8, ASP.NET Core Web API |
| Database | PostgreSQL, Entity Framework Core |
| Authentication | JWT bearer authentication |
| Device and security work | Device identification, X.509 client-certificate infrastructure |
| Development tools | Visual Studio, PowerShell |

## Architecture

The application uses a client-server structure:

```text
Computer / Browser
  Blazor WebAssembly client
          |
          | HTTP API requests
          v
ASP.NET Core Web API
          |
          v
      PostgreSQL
```

Multiple computers on the same local network can connect to the API server. The backend stores user, movie, session and watch-progress data.

## Current Implementation

The project includes:

- User registration and login.
- JWT-based authentication.
- Device identification and session-related services.
- API endpoints and data models for movies, users, sessions and watch progress.
- Blazor pages for login, the movie catalogue and video playback.
- HTML5 video playback with saved-position handling.
- Cross-device watch-progress retrieval and synchronisation.

### Work in progress

Client-certificate setup and deployment are still being developed. The repository includes certificate-generation scripts and related infrastructure, but certificate validation and deployment should be reviewed and completed before treating this as a production-ready security feature.

## Getting Started

### Prerequisites

- .NET 8 SDK
- Visual Studio 2022 or Visual Studio Code
- PostgreSQL
- PowerShell (for the certificate scripts)

### Configure the database

Create a PostgreSQL database and configure the application connection string in your local development settings.

For example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=GiriMoviesDb_Dev;Username=YOUR_USERNAME;Password=YOUR_PASSWORD"
  }
}
```

Use your own local credentials. Do not commit passwords, tokens, certificates or other secrets to the repository.

### Run the application

From the server project directory, apply the database migrations and start the API:

```bash
dotnet ef database update
dotnet run
```

Then run the client project as configured in your solution, if it is hosted separately. Use the local URL printed by the application when it starts.

## Example: Cross-Device Watch Progress

1. Sign in and start playing a video on one computer.
2. Let the application save the playback position.
3. Sign in on another computer.
4. Open the same video and retrieve the saved position.
5. Continue playback from that position.

## Repository Notes

This is a personal development project. Some deployment and security tasks are still in progress, so the repository should be treated as a learning and demonstration project rather than a production-ready streaming service.

