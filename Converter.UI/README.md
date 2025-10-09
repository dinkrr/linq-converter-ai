# C# to LINQ Converter UI

Transform traditional C# code into elegant LINQ expressions using AI-powered conversion with Google Vertex AI.

## Features

- **Smart Code Conversion**: Transform loops, conditionals, and complex logic into LINQ
- **Real-time Analysis**: Get instant feedback on conversion quality and performance
- **Code Explanation**: Understand the AI's reasoning and learning opportunities
- **Pattern Examples**: Explore common conversion patterns and best practices
- **Syntax Highlighting**: Color-coded C# and LINQ code display
- **Copy to Clipboard**: Quick copy functionality for converted code
- **Error Handling**: Graceful handling of conversion failures with helpful messages
- **Responsive Design**: Works seamlessly on desktop and mobile devices

## Project Structure

```
src/
├── app/
│   ├── app.component.ts          # Main application component
│   ├── app.component.html        # Main template
│   ├── app.component.scss        # Component styling
│   ├── app.component.spec.ts     # Component tests
│   └── services/
│       ├── converter.service.ts      # API communication service
│       └── converter.service.spec.ts # Service tests
├── assets/                       # Static assets
├── index.html                    # App entry point
├── main.ts                       # Application bootstrap
└── styles.scss                   # Global styles
```

## Technology Stack

- **Angular 17**: Modern Angular framework with standalone components
- **TypeScript 5.2**: Type-safe JavaScript development
- **SCSS**: Enhanced CSS with variables and mixins
- **Jasmine/Karma**: Testing framework for unit tests
- **RxJS**: Reactive programming for async operations
- **Vertex AI Integration**: Google Cloud AI services for intelligent code conversion

## Getting Started

### Prerequisites

- Node.js (v18 or higher) - Download from [nodejs.org](https://nodejs.org/)
- npm (comes with Node.js)
- Angular CLI (optional but recommended): `npm install -g @angular/cli`

### Installation

1. **Install Node.js**: Download and install Node.js from [nodejs.org](https://nodejs.org/)
2. **Clone or download this project**
3. **Open terminal/command prompt** in the project directory
4. **Install dependencies**:
   ```bash
   npm install
   ```
5. **Install Angular CLI globally** (recommended):
   ```bash
   npm install -g @angular/cli
   ```

### Development Server

Run the development server:
```bash
npm start
# or
ng serve
```

Navigate to `http://localhost:4200/`. The app will automatically reload if you change any source files.

### Building the Project

Build the project for production:
```bash
npm run build
# or
ng build
```

The build artifacts will be stored in the `dist/` directory.

### Running Tests

Execute unit tests:
```bash
npm test
# or
ng test
```

This will run tests in Chrome and provide coverage reports.

### Running Tests in CI/CD

For continuous integration:
```bash
ng test --watch=false --browsers=ChromeHeadless
```

## Usage

1. **Enter C# Code**: Type your traditional C# code in the left textarea
2. **Convert**: Click the "Convert to LINQ →" button to transform the code using AI
3. **Explain**: Click the "Explain Code →" button to get AI-powered explanations
4. **View Results**: The converted LINQ code appears in the right textarea, with explanations and analysis below
5. **Copy Results**: Use the copy button to quickly copy converted code to clipboard

### Example C# Input

```csharp
var result = new List<int>();
foreach (var item in numbers)
{
    if (item > 10)
        result.Add(item * 2);
}
```

### Example LINQ Output

```csharp
var result = numbers
    .Where(item => item > 10)
    .Select(item => item * 2)
    .ToList();
```

### Complex Example

```csharp
// Input C# Code
var grouped = new Dictionary<string, List<Customer>>();
foreach (var customer in customers)
{
    if (customer.IsActive)
    {
        if (!grouped.ContainsKey(customer.City))
            grouped[customer.City] = new List<Customer>();
        grouped[customer.City].Add(customer);
    }
}

// Generated LINQ
var grouped = customers
    .Where(customer => customer.IsActive)
    .GroupBy(customer => customer.City)
    .ToDictionary(g => g.Key, g => g.ToList());
```

## API Integration

The UI communicates with the backend Converter.API through the `ConverterService`. The backend uses Google Vertex AI with Gemini 2.0 Flash Lite models for intelligent code conversion using an Agent Framework architecture.

### Backend API Endpoints

- `POST /api/expression/convert` - Convert C# code to LINQ
- `POST /api/expression/explain` - Get AI explanation of code
- `GET /api/expression/health` - Check API health status

### Service Interface

```typescript
interface ConvertCodeRequest {
  csharpCode: string;
  options?: {
    preferMethodSyntax?: boolean;
    optimizePerformance?: boolean;
  }
}

interface ConvertCodeResponse {
  linqCode: string;
  success: boolean;
  message: string;
  analysis: CodeAnalysis;
  suggestions: string[];
  errorMessage?: string;
}

interface ExplainCodeRequest {
  csharpCode: string;
  includeLinqEquivalent?: boolean;
}

interface ExplainCodeResponse {
  explanation: string;
  success: boolean;
  message: string;
  analysis: CodeAnalysis;
  linqEquivalent?: string;
}

interface CodeAnalysis {
  complexity: string;
  detectedPatterns: string[];
  estimatedPerformanceGain: number;
  requiredNamespaces: string[];
  potentialIssues: string[];
}
```

### Configuration

Update the API base URL in `src/app/services/converter.service.ts`:
```typescript
private baseUrl = 'http://localhost:5037/api'; // Update for your environment
```

## Customization

### Styling

- Main colors and themes are defined in `src/app/app.component.scss`
- Global styles are in `src/styles.scss`
- The design uses CSS Grid and Flexbox for responsive layout
- Dark theme with syntax highlighting for better code readability

### Adding Features

- Component logic is in `src/app/app.component.ts`
- Service methods are in `src/app/services/converter.service.ts`
- Add new tests to the respective `.spec.ts` files
- Extend conversion options in the request models

### Environment Configuration

Create environment-specific configurations:
```typescript
// environment.ts
export const environment = {
  production: false,
  apiUrl: 'https://localhost:7001/api',
  enableDebugLogging: true
};
```

## Browser Support

- Chrome (latest)
- Firefox (latest)
- Safari (latest)
- Edge (latest)

## Contributing

1. Follow Angular style guide
2. Write unit tests for new features
3. Ensure responsive design works on all screen sizes
4. Test in multiple browsers