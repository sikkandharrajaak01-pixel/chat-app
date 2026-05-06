# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

This is an ASP.NET Core 8.0 MVC application with real-time chat functionality using SignalR. It uses Entity Framework Core with SQL Server for data persistence and session-based authentication.

## Common Commands

```bash
# Build the project
dotnet build

# Run the application
dotnet run

# Add a new database migration
dotnet ef migrations add <MigrationName>

# Update database to latest migration
dotnet ef database update

# Remove last migration
dotnet ef migrations remove
```

## Architecture

- **ChatHub** (`ChatHub.cs`): SignalR hub handling real-time message delivery. Maintains a static `ConcurrentDictionary` mapping user IDs to connection IDs.
- **ApplicationDBContext** (`ApplicationDBContext.cs`): Entity Framework Core context with `UsersList` and `Message` DbSets.
- **Controllers**:
  - `AccountController`: Handles login with session-based authentication (plain text password comparison).
  - `ChatController`: Manages chat views and user lists.
- **Models**:
  - `UsersList`: User entity with Id, username, and password.
  - `Message`: Message entity with SenderId, ReceiverId, Text, and SentAt.

## Database

SQL Server connection string in `appsettings.json`:
```
Server=ms-mdu-024;Database=exercise;Trusted_Connection=true;Encrypt=false
```

## Key Patterns

- SignalR connection mapping uses static `ConcurrentDictionary<int, string>` in ChatHub
- Session stores `UserId` as `SetInt32("UserId")` after login
- Messages are persisted to DB and delivered in real-time via SignalR
