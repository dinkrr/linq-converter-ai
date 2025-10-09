# Google Cloud Project Setup for Vertex AI

This guide will walk you through setting up a Google Cloud project and enabling Vertex AI for your C# to LINQ converter application.

## Prerequisites

- A Google account
- A valid credit card (for billing setup, even if using free tier)
- Administrative access to create cloud resources

## Step 1: Create a Google Cloud Project

### 1.1 Access Google Cloud Console
1. Go to [Google Cloud Console](https://console.cloud.google.com/)
2. Sign in with your Google account

### 1.2 Create New Project
1. Click on the project dropdown (next to "Google Cloud" logo)
2. Click **"New Project"**
3. Fill in project details:
   - **Project name**: `csharp-linq-converter` (or your preferred name)
   - **Organization**: Select your organization (if applicable)
   - **Location**: Choose your preferred location
4. Click **"Create"**
5. Wait for the project to be created (usually takes 1-2 minutes)

### 1.3 Select Your Project
1. Once created, make sure your new project is selected
2. You can verify this by checking the project name in the top navigation bar

## Step 2: Enable Billing

### 2.1 Set Up Billing Account
1. Go to **Navigation Menu** (☰) → **Billing**
2. Click **"Link a billing account"** or **"Create billing account"**
3. Follow the prompts to:
   - Enter billing information
   - Add a payment method (credit card)
   - Accept terms and conditions
4. **Important**: Google Cloud offers $300 in free credits for new users

### 2.2 Verify Billing is Enabled
1. Go to **Navigation Menu** → **Billing**
2. Ensure your project is linked to an active billing account
3. You should see "Billing enabled" status

## Step 3: Enable Vertex AI API

### 3.1 Enable the API
1. Go to **Navigation Menu** (☰) → **APIs & Services** → **Library**
2. Search for "Vertex AI API"
3. Click on **"Vertex AI API"** from the results
4. Click **"Enable"** button
5. Wait for the API to be enabled (usually takes 30-60 seconds)

### 3.2 Enable Additional Required APIs
Enable these additional APIs that Vertex AI depends on:
1. **Cloud Resource Manager API**
2. **Compute Engine API**
3. **Cloud Storage API**

For each API:
1. Search in the API Library
2. Click on the API
3. Click **"Enable"**

### 3.3 Verify APIs are Enabled
1. Go to **APIs & Services** → **Enabled APIs**
2. Verify you see:
   - Vertex AI API
   - Cloud Resource Manager API
   - Compute Engine API
   - Cloud Storage API

## Step 4: Set Up Authentication

### 4.1 Create a Service Account
1. Go to **Navigation Menu** → **IAM & Admin** → **Service Accounts**
2. Click **"Create Service Account"**
3. Fill in service account details:
   - **Service account name**: `vertex-ai-service-account`
   - **Service account ID**: Will be auto-generated
   - **Description**: `Service account for C# to LINQ converter Vertex AI integration`
4. Click **"Create and Continue"**

### 4.2 Grant Required Roles
In the "Grant this service account access to project" section, add these roles:
1. **Vertex AI User** (`roles/aiplatform.user`)
2. **Storage Object Viewer** (`roles/storage.objectViewer`)
3. Click **"Continue"**
4. Skip "Grant users access to this service account" section
5. Click **"Done"**

### 4.3 Create and Download Service Account Key
1. In the Service Accounts list, click on your newly created service account
2. Go to the **"Keys"** tab
3. Click **"Add Key"** → **"Create new key"**
4. Select **"JSON"** format
5. Click **"Create"**
6. The key file will be downloaded automatically
7. **Important**: Store this file securely and never commit it to version control

## Step 5: Configure Your Application

### 5.1 Set Up Environment Variables
1. Place your downloaded JSON key file in a secure location (e.g., `C:\secrets\`)
2. Set the environment variable:

**PowerShell:**
```powershell
$env:GOOGLE_APPLICATION_CREDENTIALS = "C:\secrets\your-service-account-key.json"
```

**Command Prompt:**
```cmd
set GOOGLE_APPLICATION_CREDENTIALS=C:\secrets\your-service-account-key.json
```

**Permanent Setup (Windows):**
1. Open System Properties → Advanced → Environment Variables
2. Add new system variable:
   - Name: `GOOGLE_APPLICATION_CREDENTIALS`
   - Value: `C:\secrets\your-service-account-key.json`

### 5.2 Update Application Configuration
Update your `appsettings.json` in the Converter.API project:

```json
{
  "GoogleCloud": {
    "ProjectId": "your-project-id",
    "Location": "us-central1",
    "Model": "gemini-1.5-flash-001"
  }
}
```

Replace `your-project-id` with your actual Google Cloud project ID.

## Step 6: Test the Setup

### 6.1 Install Google Cloud CLI (Optional but Recommended)
1. Download from [Google Cloud CLI](https://cloud.google.com/sdk/docs/install)
2. Install following the installer instructions
3. Open a new terminal and run:
   ```bash
   gcloud auth application-default login
   ```
4. Follow the authentication flow

### 6.2 Test API Access
Run this simple test in your terminal:
```bash
gcloud ai models list --region=us-central1
```

You should see a list of available models including Gemini models.

### 6.3 Test Your Application
1. Start your Converter.API project
2. Use the health endpoint to verify Vertex AI connectivity:
   ```
   GET https://localhost:7001/api/codeconversion/health
   ```
3. You should receive a successful response indicating Vertex AI is accessible

## Step 7: Security Best Practices

### 7.1 Secure Your Service Account Key
- Never commit the JSON key file to version control
- Store it in a secure location with restricted access
- Consider using environment-specific keys for different deployments

### 7.2 Set Up IAM Policies
1. Go to **IAM & Admin** → **IAM**
2. Review and ensure principle of least privilege
3. Regularly audit service account permissions

### 7.3 Enable Audit Logging
1. Go to **IAM & Admin** → **Audit Logs**
2. Enable audit logs for Vertex AI API
3. Configure log retention policies

## Step 8: Cost Management

### 8.1 Set Up Budget Alerts
1. Go to **Navigation Menu** → **Billing** → **Budgets & Alerts**
2. Click **"Create Budget"**
3. Set up alerts for your expected usage
4. Configure email notifications

### 8.2 Monitor Usage
1. Go to **Navigation Menu** → **Vertex AI** → **Dashboard**
2. Monitor your API usage and costs
3. Set up usage quotas if needed

### 8.3 Understand Pricing
- Vertex AI charges per prediction request
- Gemini 1.5 Flash pricing: ~$0.075 per 1K input tokens, ~$0.30 per 1K output tokens
- Monitor your usage in the billing console

## Troubleshooting

### Common Issues and Solutions

**Authentication Errors:**
- Verify `GOOGLE_APPLICATION_CREDENTIALS` environment variable is set correctly
- Ensure the service account has proper roles assigned
- Check that the JSON key file is valid and not corrupted

**API Not Enabled:**
- Verify Vertex AI API is enabled in your project
- Check that billing is properly set up
- Ensure you're using the correct project ID

**Permission Denied:**
- Check service account roles in IAM console
- Verify the service account has `Vertex AI User` role
- Ensure the project has billing enabled

**Model Not Found:**
- Verify the model name in your configuration
- Check that the specified region supports your model
- Use `gcloud ai models list` to see available models

**Network Issues:**
- Check firewall settings if running on corporate network
- Verify outbound HTTPS (443) traffic is allowed
- Consider using a VPN if behind restrictive firewalls

### Getting Help

1. **Google Cloud Support**: Available through the Cloud Console
2. **Stack Overflow**: Tag questions with `google-cloud-vertex-ai`
3. **Google Cloud Documentation**: [Vertex AI Documentation](https://cloud.google.com/vertex-ai/docs)
4. **Community Forums**: [Google Cloud Community](https://www.googlecloudcommunity.com/)

## Next Steps

After completing this setup:

1. **Test your C# to LINQ converter** with various code samples
2. **Monitor usage and costs** in the first few weeks
3. **Optimize prompts** for better conversion quality
4. **Implement caching** to reduce API calls for repeated conversions
5. **Set up monitoring and alerting** for production deployments

## Production Deployment Considerations

### For Production Environments:
1. **Use Workload Identity** instead of service account keys when deploying to GKE
2. **Implement proper secret management** (Azure Key Vault, AWS Secrets Manager, etc.)
3. **Set up monitoring and logging** with Cloud Monitoring
4. **Configure auto-scaling** based on API usage
5. **Implement rate limiting** to control costs
6. **Set up backup authentication methods** for high availability

Your Google Cloud project is now ready to power your C# to LINQ converter with advanced AI capabilities!