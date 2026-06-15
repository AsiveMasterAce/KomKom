# KomKom

KomKom is a Windows WPF task manager with a built-in timer, SQLite-backed task storage, toast notifications, and a packaged installer project.

## What it does

- Create and manage scheduled tasks with a title, priority, tags, and completion state.
- Mark tasks as important or completed, adjust priority, or delete them from the list.
- Run a simple timer with preset durations, pause/resume, reset, and a finished alarm.
- Show a toast notification and play a sound when the timer completes.
- Store data locally in SQLite under the user profile.

## Tech Stack

- .NET 8 WPF
- Entity Framework Core with SQLite
- Microsoft.Toolkit.Uwp.Notifications for toast notifications
- SharpVectors for SVG icon support

## Requirements

- Windows
- .NET 8 SDK
- Visual Studio 2022 or newer with WPF support

## Run the app

1. Open `KomKom.sln` in Visual Studio.
2. Restore NuGet packages.
3. Build and run the `KomKom` project.

The app creates its database automatically at:

`%LOCALAPPDATA%\KomKom\Data\tasks.db`

## Build the installer

The setup project lives in `KomKom.Setup` and produces an MSI through WiX.

To build the installer, open the solution and build the `KomKom.Setup` project. The release output is configured to publish a `KomKom.msi` package.

## Project layout

- `KomKom/` - Main WPF app
- `KomKom/Repository/` - SQLite data access
- `KomKom/ViewModels/` - UI logic
- `KomKom/Services/` - Timer and notification services
- `KomKom.Setup/` - WiX installer project

## Notes

- The app copies its timer sound and icon assets to the output directory.
- Tasks are persisted locally; no cloud sync is included.
