# Code Text Toolkit

A modern, cross-platform web application for text and code file operations. Built with Blazor WebAssembly and MudBlazor for a beautiful Material Design UI.

## 🚀 Try It Now

**[Launch Code Text Toolkit](https://JacYosAkiCra.github.io/CodeTextToolkit/)** - No installation required!

---

![.NET 8](https://img.shields.io/badge/.NET-8.0-purple)
![Blazor WASM](https://img.shields.io/badge/Blazor-WebAssembly-blue)
![MudBlazor](https://img.shields.io/badge/UI-MudBlazor-green)
[![Deploy to GitHub Pages](https://github.com/JacYosAkiCra/CodeTextToolkit/actions/workflows/deploy.yml/badge.svg)](https://github.com/JacYosAkiCra/CodeTextToolkit/actions/workflows/deploy.yml)

## Features

### 🔄 Code to Text
Convert code files to plain text format (.txt). Supports multiple programming languages:
- C, C++, C#
- Python
- JavaScript, TypeScript
- Java, Go, Rust
- PHP, Ruby, Swift, Kotlin
- HTML, CSS, SCSS
- JSON, XML, YAML
- SQL, Shell scripts, PowerShell
- And many more...

**Features:**
- Select individual files or entire folders
- Download all converted files as a single ZIP archive
- Customizable ZIP file name
- Option to preserve original extension in filename

### 📎 Merge Files
Combine multiple text files into a single document with options to:
- Select individual files or entire folders
- Include or exclude subfolders when selecting a folder
- Add file headers between sections
- Reorder files via drag-and-drop
- Preview merged output before downloading

### ✂️ Split File
Split large text files into smaller chunks with:
- Intelligent splitting that respects code structure
- Configurable chunk size (512 bytes to 10 MB)
- Quick presets (4KB, 8KB, 12KB, 32KB, 64KB, 128KB)
- Preview individual chunks before downloading
- Download all chunks as a single ZIP archive

## Key Benefits

- **🔒 Privacy First**: All processing happens in your browser. Files never leave your device.
- **🌐 Cross-Platform**: Works on Windows, Mac, Linux, and mobile devices.
- **⚡ Fast**: Powered by WebAssembly for near-native performance.
- **📦 No Installation**: Just open the URL and start working.
- **🌙 Dark Mode**: Automatic theme detection with manual toggle.

## Getting Started

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Development

1. Clone the repository:
```bash
git clone https://github.com/yourusername/CodeTextToolkit.git
cd CodeTextToolkit
```

2. Restore packages:
```bash
dotnet restore
```

3. Run the development server:
```bash
dotnet run
```

4. Open your browser to `http://localhost:5062`

### Building for Production

```bash
dotnet publish -c Release
```

The output will be in `bin/Release/net8.0/publish/wwwroot/`. This can be deployed to any static web host.

## Deployment Options

Since this is a Blazor WebAssembly app, you can deploy it to:

- **GitHub Pages**: Free static hosting
- **Azure Static Web Apps**: Great for CI/CD integration
- **Netlify**: Simple drag-and-drop deployment
- **Vercel**: Fast global CDN
- **Any static web host**: Just upload the `wwwroot` folder

## Technology Stack

- **Framework**: Blazor WebAssembly (.NET 8)
- **UI Library**: [MudBlazor](https://mudblazor.com/) (Material Design)
- **Icons**: Material Design Icons
- **Fonts**: Google Roboto

## Project Structure

```
CodeTextToolkit/
├── Components/          # Reusable Blazor components
│   └── FileDropZone.razor
├── Layout/              # Application layout
│   └── MainLayout.razor
├── Pages/               # Page components
│   ├── Home.razor
│   ├── CodeToText.razor
│   ├── Merge.razor
│   └── Split.razor
├── Services/            # Business logic
│   └── FileProcessingService.cs
├── wwwroot/             # Static assets
│   ├── css/
│   ├── js/
│   └── index.html
├── Program.cs           # Application entry point
└── _Imports.razor       # Global imports
```

## License

MIT License - feel free to use this project for personal or commercial purposes.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
