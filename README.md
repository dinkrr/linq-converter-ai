# LINQ Converter AI

A full-stack AI-powered application for converting C# code expressions to LINQ using Google Cloud Vertex AI.

## 🏗️ Architecture

This project consists of:
- **Backend API**: .NET 9 Web API (`Converter.API`) with Agent-based AI architecture
- **Frontend**: Angular application (`Converter.UI`)
- **AI Integration**: Google Cloud Vertex AI with Gemini 2.0 Flash Lite models

## 🚀 Quick Start

### Prerequisites
- .NET 9 SDK
- Node.js and npm
- Google Cloud account with Vertex AI enabled

### Running the Application

1. **Backend Setup**:
   ```bash
   cd Converter.API
   dotnet restore
   dotnet run
   ```

2. **Frontend Setup**:
   ```bash
   cd Converter.UI
   npm install
   ng serve
   ```

3. **Full Stack** (using VS Code tasks):
   - Press `Ctrl+Shift+P` → "Tasks: Run Task" → "build-all"

## 📚 Documentation

### 🛠️ Setup & Configuration
- [Google Cloud Setup](docs/setup/GOOGLE_CLOUD_SETUP.md) - Configure Google Cloud and Vertex AI
- [Vertex AI Integration](docs/setup/VERTEX_AI_INTEGRATION_GUIDE.md) - AI service integration guide

### 🏛️ Architecture & Design
- [Architecture Overview](docs/architecture/ARCHITECTURE.md) - High-level system design

### 📖 Development Guides
- [Debug Guide](docs/guides/DEBUG_GUIDE.md) - Troubleshooting and debugging
- [Agent Mode Guide](docs/guides/AGENT_MODE_GUIDE.md) - AI agent development guidelines

### 🔧 Component Documentation
- [API Documentation](Converter.API/README.md) - Backend API details
- [UI Documentation](Converter.UI/README.md) - Frontend application guide

## 🛠️ Development

### Available VS Code Tasks
- `build` - Build the .NET API
- `watch` - Run API in watch mode
- `ng serve` - Start Angular dev server
- `build-all` - Build both backend and frontend
- `start-full-stack` - Start complete application

### API Endpoints
- `POST /api/expression/convert` - Convert code expressions
- `POST /api/expression/explain` - Explain code with AI insights
- See [API Tests](api-tests.http) for examples

## 🏢 Project Structure

```
linq-converter-ai/
├── docs/                     # 📚 All documentation
│   ├── setup/               # Setup & configuration guides
│   ├── architecture/        # System design documentation
│   └── guides/              # Development guides
├── Converter.API/           # 🔧 .NET Backend API
├── Converter.UI/            # 🎨 Angular Frontend
├── api-tests.http           # 🧪 API test requests
└── linq-converter-ai.sln    # 📦 Solution file
```

## 🤝 Contributing

1. Follow the architecture guidelines in [`docs/architecture/`](docs/architecture/)
2. Use the debug guide for troubleshooting: [`docs/guides/DEBUG_GUIDE.md`](docs/guides/DEBUG_GUIDE.md)
3. Test your changes using the provided API tests

---

🚀 **Ready to convert some expressions?** Start with the [Google Cloud Setup](docs/setup/GOOGLE_CLOUD_SETUP.md) guide!