# CodeFlow LLM Toolkit - Blazor WebAssembly Application

## Project Overview
A cross-platform web application for text/code file operations:
- **Code to Text**: Convert code files to .txt format
- **Merge Text**: Combine multiple text files into one
- **Split Text**: Split large files into smaller chunks

## Technology Stack
- **Framework**: Blazor WebAssembly (.NET 8)
- **UI Library**: MudBlazor (Material Design)
- **Hosting**: Static files (can be hosted anywhere)

## Development Guidelines
- Use MudBlazor components for all UI elements
- Follow Material Design principles
- Support dark/light theme toggle
- Use async/await for file operations
- Implement drag-and-drop file handling via JS interop

## Project Structure
```
/Components     - Reusable Blazor components
/Pages          - Main page components
/Services       - Business logic services
/wwwroot        - Static assets and index.html
```

## Commands
- `dotnet restore` - Restore packages
- `dotnet build` - Build project
- `dotnet run` - Run development server
- `dotnet publish -c Release` - Publish for production
