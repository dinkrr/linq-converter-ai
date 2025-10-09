# Complete Step-by-Step Guide: Vertex AI Integration with Gemini Models

This guide will walk you through integrating Google Vertex AI with Gemini models into your C# to LINQ converter application from scratch.

## 📋 Prerequisites

- Windows machine with PowerShell
- .NET 9.0 SDK installed
- Visual Studio 2022 or VS Code
- Google account
- Credit card for Google Cloud billing setup

---

## 🚀 Step 1: Create Google Cloud Project

### 1.1 Access Google Cloud Console
1. Open your web browser and go to [Google Cloud Console](https://console.cloud.google.com/)
2. Sign in with your Google account
3. If prompted, accept the terms of service

### 1.2 Create a New Project
1. Click on the **project dropdown** at the top of the page (next to "Google Cloud")
2. Click **"New Project"**
3. Fill in the project details:
   - **Project name**: `csharp-linq-converter`
   - **Organization**: Leave as default or select your organization
   - **Location**: Leave as default
4. Click **"Create"**
5. Wait 1-2 minutes for the project to be created

### 1.3 Enable Billing
1. Go to **Navigation Menu** (☰) → **Billing**
2. Click **"Link a billing account"** or **"Create billing account"**
3. Follow the setup wizard:
   - Enter your billing information
   - Add a payment method (credit card required)
   - Accept terms and conditions
4. **Note**: New users get $300 in free credits

---

## 🔐 Step 2: Configure Authentication

### 2.1 Enable Required APIs
1. Go to **Navigation Menu** → **APIs & Services** → **Library**
2. Search for and enable these APIs (click Enable for each):
   - **Vertex AI API**
   - **Cloud Resource Manager API**
   - **Compute Engine API**

### 2.2 Create Service Account
1. Go to **Navigation Menu** → **IAM & Admin** → **Service Accounts**
2. Click **"Create Service Account"**
3. Fill in details:
   - **Service account name**: `vertex-ai-service-account`
   - **Description**: `Service account for C# to LINQ converter`
4. Click **"Create and Continue"**

### 2.3 Assign Roles
1. In the **"Grant this service account access to project"** section, add these roles:
   - **Vertex AI User** (`roles/aiplatform.user`)
   - **Storage Object Viewer** (`roles/storage.objectViewer`)
2. Click **"Continue"** → **"Done"**

### 2.4 Download Authentication Key
1. In the Service Accounts list, click on your newly created service account
2. Go to the **"Keys"** tab
3. Click **"Add Key"** → **"Create new key"**
4. Select **"JSON"** format
5. Click **"Create"**
6. The JSON key file will download automatically
7. **Important**: Move this file to a secure location

---

## 📦 Step 3: Install Dependencies

### 3.1 Navigate to Your Project
Open PowerShell and navigate to your API project:

```powershell
cd "C:\Users\SurajKentura\Desktop\Expression Converter\Converter.API\Converter.API"
```

### 3.2 Install NuGet Packages
Run these commands one by one:

```powershell
# Install Google Cloud AI Platform package
dotnet add package Google.Cloud.AIPlatform.V1 --version 3.22.0

# Install Google Authentication package
dotnet add package Google.Apis.Auth --version 1.68.0

# Install JSON handling package
dotnet add package Newtonsoft.Json --version 13.0.3
```

### 3.3 Verify Package Installation
Check that packages were installed:

```powershell
dotnet list package
```

You should see the three packages listed.

---

## ⚙️ Step 4: Configure Application Settings

### 4.1 Update appsettings.json
Open `appsettings.json` in your Converter.API project and add the Google Cloud configuration:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "GoogleCloud": {
    "ProjectId": "YOUR_PROJECT_ID_HERE",
    "Location": "us-central1",
    "Model": "gemini-1.5-flash-001"
  }
}
```

**Replace `YOUR_PROJECT_ID_HERE`** with your actual Google Cloud project ID (found in the Cloud Console).

### 4.2 Update appsettings.Development.json
Add the same configuration to your development settings:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "GoogleCloud": {
    "ProjectId": "YOUR_PROJECT_ID_HERE",
    "Location": "us-central1",
    "Model": "gemini-1.5-flash-001"
  }
}
```

---

## 🔑 Step 5: Setup Environment Variables

### 5.1 Create Secure Directory
Create a directory to store your authentication key:

```powershell
# Create a secure directory
New-Item -ItemType Directory -Path "C:\secrets" -Force
```

### 5.2 Move Your Key File
Move the downloaded JSON key file to the secure directory:

```powershell
# Move your downloaded key file (replace with actual filename)
Move-Item "C:\Users\SurajKentura\Downloads\csharp-linq-converter-*.json" "C:\secrets\google-cloud-key.json"
```

### 5.3 Set Environment Variable
Set the environment variable for authentication:

```powershell
# Set environment variable for current session
$env:GOOGLE_APPLICATION_CREDENTIALS = "C:\secrets\google-cloud-key.json"

# Set permanently (requires restart of applications)
[Environment]::SetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS", "C:\secrets\google-cloud-key.json", "User")
```

### 5.4 Verify Environment Variable
Verify the environment variable is set:

```powershell
echo $env:GOOGLE_APPLICATION_CREDENTIALS
```

Should output: `C:\secrets\google-cloud-key.json`

---

## 🧪 Step 6: Test Integration

### 6.1 Build the Application
Build your project to ensure everything compiles:

```powershell
cd "C:\Users\SurajKentura\Desktop\Expression Converter\Converter.API\Converter.API"
dotnet build
```

### 6.2 Run the Application
Start your API:

```powershell
dotnet run
```

You should see output like:
```
info: Microsoft.Hosting.Lifetime[14]
      Now listening on: https://localhost:7001
      Now listening on: http://localhost:5000
```

### 6.3 Test Health Endpoint
Open a new PowerShell window and test the health endpoint:

```powershell
# Test the health endpoint
Invoke-RestMethod -Uri "https://localhost:7001/api/codeconversion/health" -SkipCertificateCheck
```

You should get a response indicating the service is healthy and Vertex AI is accessible.

### 6.4 Test Code Conversion
Test the actual conversion endpoint:

```powershell
# Create test request
$body = @{
    csharpCode = @"
var result = new List<int>();
foreach (var item in numbers)
{
    if (item > 10)
        result.Add(item * 2);
}
"@
    includeExplanation = $true
    optimizeForPerformance = $true
    includeComments = $true
} | ConvertTo-Json

# Send conversion request
Invoke-RestMethod -Uri "https://localhost:7001/api/codeconversion/convert" -Method POST -Body $body -ContentType "application/json" -SkipCertificateCheck
```

---

## 🎯 Step 7: End-to-End Testing

### 7.1 Start the Angular UI
Open a new PowerShell window and start the Angular application:

```powershell
cd "C:\Users\SurajKentura\Desktop\Expression Converter\Converter.UI"
npm install
npm start
```

### 7.2 Test in Browser
1. Open your browser and go to `http://localhost:4200`
2. You should see the C# to LINQ converter interface
3. Enter some C# code in the left panel
4. Click "Convert to LINQ" button
5. Verify you get LINQ code in the right panel

### 7.3 Sample Test Cases
Try these sample C# code snippets:

**Simple Loop:**
```csharp
var result = new List<int>();
foreach (var num in numbers)
{
    if (num % 2 == 0)
        result.Add(num);
}
```

**Nested Logic:**
```csharp
var dictionary = new Dictionary<string, List<Person>>();
foreach (var person in people)
{
    if (person.Age > 18)
    {
        if (!dictionary.ContainsKey(person.City))
            dictionary[person.City] = new List<Person>();
        dictionary[person.City].Add(person);
    }
}
```

---

## 🔧 Troubleshooting

### Common Issues and Solutions

#### Issue: "Authentication failed"
**Solution:**
1. Verify environment variable is set: `echo $env:GOOGLE_APPLICATION_CREDENTIALS`
2. Check the JSON key file exists: `Test-Path "C:\secrets\google-cloud-key.json"`
3. Restart your application after setting environment variables

#### Issue: "Project not found"
**Solution:**
1. Verify your project ID in `appsettings.json`
2. Ensure billing is enabled in Google Cloud Console
3. Check that Vertex AI API is enabled

#### Issue: "Permission denied"
**Solution:**
1. Verify service account has "Vertex AI User" role
2. Check that the service account key is valid
3. Ensure the project has billing enabled

#### Issue: "Model not found"
**Solution:**
1. Verify the model name in `appsettings.json` is correct: `gemini-1.5-flash-001`
2. Check that the region `us-central1` supports Gemini models
3. Try using a different region like `us-east1`

### Getting Help
- Check the Google Cloud Console for error messages
- Review the application logs in the console output
- Use the health endpoint to diagnose connectivity issues

---

## 🎉 Success Criteria

You've successfully integrated Vertex AI when:

✅ Google Cloud project is created with billing enabled  
✅ Vertex AI API is enabled  
✅ Service account is created with proper roles  
✅ NuGet packages are installed  
✅ Environment variables are set  
✅ Health endpoint returns success  
✅ Code conversion endpoint works  
✅ Angular UI can convert C# to LINQ  
✅ AI-generated explanations are provided  

---

## 🚀 Next Steps

After successful integration:

1. **Optimize Prompts**: Fine-tune the AI prompts for better LINQ conversion quality
2. **Add Caching**: Implement caching to reduce API calls for repeated conversions
3. **Monitor Usage**: Set up monitoring and alerts for API usage and costs
4. **Deploy to Production**: Deploy both API and UI to your preferred cloud platform
5. **Add Advanced Features**: Consider adding code quality analysis, performance benchmarking, etc.

Congratulations! Your C# to LINQ converter is now powered by Google's advanced Gemini AI models! 🎊