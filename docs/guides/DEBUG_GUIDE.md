# 🔧 VS Code Debugging Guide

## 🚀 **One-Click Full Stack Debugging**

### **Quick Start (Recommended)**
1. **Press F5** or **Ctrl+Shift+D** to open Run and Debug
2. **Select "🚀🌐 API + UI (Full Stack Debug)"**
3. **Click the green play button** or press **F5**

This will:
- ✅ Build the .NET API
- ✅ Install npm dependencies 
- ✅ Start API with debugging on `http://localhost:5037`
- ✅ Start Angular dev server on `http://localhost:4200`
- ✅ Open both in your browser automatically
- ✅ Enable breakpoints in both API and UI code

## 🎯 **Available Debug Configurations**

### **1. 🚀🌐 API + UI (Full Stack Debug)** - *Recommended*
- **What it does**: Starts both API and Angular with full debugging
- **Best for**: Full-stack development and testing
- **Ports**: API on 5037, UI on 4200
- **Auto-opens**: Both Swagger UI and Angular app

### **2. 🚀 API Only (Debug)**
- **What it does**: Starts only the .NET API with debugging
- **Best for**: Backend development and API testing
- **Port**: 5037
- **Auto-opens**: Swagger UI

### **3. 🌐 Angular Only (Debug)**
- **What it does**: Starts only Angular dev server
- **Best for**: Frontend development
- **Port**: 4200
- **Auto-opens**: Angular app

## 🔍 **How to Set Breakpoints**

### **API Breakpoints (C#)**
1. **Open any C# file** (e.g., `VertexAIService.cs`)
2. **Click in the left margin** next to line numbers
3. **Red dot appears** = breakpoint set
4. **Start debugging** and trigger the API call

**Best places for breakpoints:**
```csharp
// VertexAIService.cs
public async Task<VertexAIResponse> GenerateTextAsync(VertexAIRequest request)
{
    // Breakpoint here to inspect incoming requests
    _logger.LogDebug("Starting Vertex AI request...");
    
    try
    {
        var accessToken = await GetAccessTokenAsync(); // Breakpoint: Check auth
        var response = await _httpClient.SendAsync(httpRequest); // Breakpoint: Check API response
    }
    catch (Exception ex)
    {
        // Breakpoint: Catch errors
    }
}
```

### **UI Breakpoints (TypeScript)**
1. **Open browser Developer Tools** (F12)
2. **Go to Sources tab**
3. **Find your TypeScript files**
4. **Click line numbers** to set breakpoints
5. **Use Angular app** to trigger breakpoints

## 🧪 **Testing During Debug**

### **Method 1: Use HTTP Test File**
1. **Open `api-tests.http`** in VS Code
2. **Click "Send Request"** above any HTTP call
3. **Breakpoints will trigger** if set

### **Method 2: Use Angular UI**
1. **Go to `http://localhost:4200`**
2. **Enter C# code** in the text area
3. **Click "Convert to LINQ"**
4. **Breakpoints will trigger** in API

### **Method 3: Use Swagger UI**
1. **Go to `http://localhost:5037`**
2. **Try any API endpoint**
3. **Breakpoints will trigger** in API

## 🛠️ **Debug Tools & Tips**

### **VS Code Debug Features**
- **Variables panel**: See current variable values
- **Call Stack**: See execution path
- **Debug Console**: Execute code while paused
- **Watch**: Monitor specific variables

### **Keyboard Shortcuts**
- **F5**: Start debugging
- **F10**: Step over (next line)
- **F11**: Step into (enter method)
- **Shift+F11**: Step out (exit method)
- **Ctrl+Shift+F5**: Restart debugging
- **Shift+F5**: Stop debugging

### **Advanced Debugging**
```json
// Add to launch.json for specific debugging
{
    "justMyCode": false,  // Debug into framework code
    "enableStepFiltering": false,  // Don't skip property/auto-generated code
    "suppressJITOptimizations": true  // Better variable inspection
}
```

## 🔧 **Troubleshooting**

### **Common Issues & Solutions**

#### **Issue**: "Cannot connect to runtime process"
**Solution**: 
1. Stop debugging (Shift+F5)
2. Build project (`Ctrl+Shift+P` → "Tasks: Run Task" → "build")
3. Try again

#### **Issue**: "Port already in use"
**Solution**:
```cmd
# Kill processes using the ports
netstat -ano | findstr :5037
taskkill /PID <PID> /F

netstat -ano | findstr :4200
taskkill /PID <PID> /F
```

#### **Issue**: "Google credentials not found"
**Solution**:
1. Check `GOOGLE_APPLICATION_CREDENTIALS` environment variable
2. Verify JSON key file exists at specified path
3. Restart VS Code after setting environment variables

#### **Issue**: Angular not starting
**Solution**:
```cmd
cd Converter.UI
npm install
ng serve
```

## 📊 **Debug Workflow Examples**

### **Debugging C# to LINQ Conversion**
1. **Set breakpoint** in `VertexAIService.GenerateTextAsync()`
2. **Start full-stack debugging** (F5)
3. **Open Angular app** (`http://localhost:4200`)
4. **Enter C# code** and click convert
5. **Step through** the conversion process
6. **Inspect** request/response data
7. **Check** generated LINQ output

### **Debugging Authentication Issues**
1. **Set breakpoint** in `GetAccessTokenAsync()`
2. **Start API debugging**
3. **Use HTTP test file** to trigger API call
4. **Check** credential loading
5. **Verify** token generation

### **Debugging UI Issues**
1. **Start Angular debugging**
2. **Open browser DevTools** (F12)
3. **Set breakpoints** in TypeScript code
4. **Test UI interactions**
5. **Monitor** API calls in Network tab

## 🎯 **Quick Debug Checklist**

- [ ] **Environment variables set** (`GOOGLE_APPLICATION_CREDENTIALS`)
- [ ] **Google Cloud credentials** file exists
- [ ] **Ports 5037 and 4200** are available
- [ ] **Dependencies installed** (npm and NuGet)
- [ ] **Project builds** without errors
- [ ] **Breakpoints set** in appropriate locations
- [ ] **Full-stack debug config** selected

**Happy Debugging! 🚀**