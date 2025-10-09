import { CommonModule } from '@angular/common';
import { HttpClientModule } from '@angular/common/http';
import { Component, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CodeConverterService, ConversionOptions, ConversionPattern } from './services/converter.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, FormsModule, HttpClientModule],
  template: `
    <div class="app-container">
      <div class="header">
        <h1>🚀 C# to LINQ Converter</h1>
        <p>Transform traditional C# code to elegant LINQ expressions using AI-powered conversion</p>
        <div class="status-indicator" [class.healthy]="apiHealthy" [class.unhealthy]="!apiHealthy">
          {{ apiHealthy ? '✅ AI Service Online' : '❌ AI Service Unavailable' }}
        </div>
      </div>

      <div class="converter-wrapper">
        <!-- Input Section -->
        <div class="input-section">
          <div class="section-header">
            <label class="code-label">C# Code Input</label>
            <div class="conversion-options">
              <label class="option-label">
                <input type="checkbox" [(ngModel)]="options.preferMethodSyntax">
                Method Syntax
              </label>
              <label class="option-label">
                <input type="checkbox" [(ngModel)]="options.optimizePerformance">
                Optimize Performance
              </label>
            </div>
          </div>
          <textarea
            class="code-textarea"
            [(ngModel)]="csharpCode"
            placeholder="Enter your C# code here...

Example:
List<int> result = new List<int>();
foreach(var item in numbers)
{
    if(item > 10)
    {
        result.Add(item * 2);
    }
}">
          </textarea>
        </div>

        <!-- Button Section -->
        <div class="button-section">
          <button
            class="action-btn convert-btn"
            (click)="convertCode()"
            [disabled]="isLoading || !csharpCode.trim()">
            <span>
              {{ isLoading && activeOperation === 'convert' ? '🔄 Converting...' : '🔄 Convert to LINQ' }}
            </span>
          </button>
          <button
            class="action-btn explain-btn"
            (click)="explainCode()"
            [disabled]="isLoading || !csharpCode.trim()">
            <span>
              {{ isLoading && activeOperation === 'explain' ? '💡 Analyzing...' : '💡 Explain Code' }}
            </span>
          </button>
          <button
            class="action-btn patterns-btn"
            (click)="showPatterns = !showPatterns">
            <span>📚 {{ showPatterns ? 'Hide' : 'Show' }} Examples</span>
          </button>
        </div>

        <!-- Patterns Section -->
        <div class="patterns-section" *ngIf="showPatterns">
          <h3>🎯 Supported Conversion Patterns</h3>
          <div class="patterns-grid">
            <div class="pattern-card" *ngFor="let pattern of supportedPatterns">
              <h4>{{ pattern.name }}</h4>
              <p>{{ pattern.description }}</p>
              <div class="pattern-example">
                <div class="code-before">
                  <strong>C#:</strong>
                  <code>{{ pattern.example.csharp }}</code>
                </div>
                <div class="code-after">
                  <strong>LINQ:</strong>
                  <code>{{ pattern.example.linq }}</code>
                </div>
              </div>
            </div>
          </div>
        </div>

        <!-- Result Section -->
        <div class="result-section" *ngIf="linqCode || explanation">
          <!-- LINQ Conversion Result -->
          <div class="linq-result" *ngIf="linqCode">
            <div class="result-header">
              <h3>✨ LINQ Conversion Result</h3>
              <button class="copy-btn" (click)="copyToClipboard(linqCode)">📋 Copy</button>
            </div>
            <pre class="code-output">{{ linqCode }}</pre>
            
            <!-- Analysis Section -->
            <div class="analysis-section" *ngIf="codeAnalysis">
              <h4>📊 Code Analysis</h4>
              <div class="analysis-grid">
                <div class="analysis-item">
                  <strong>Complexity:</strong> {{ codeAnalysis.complexity }}
                </div>
                <div class="analysis-item">
                  <strong>Performance Gain:</strong> ~{{ codeAnalysis.estimatedPerformanceGain }}%
                </div>
                <div class="analysis-item">
                  <strong>Patterns:</strong> {{ codeAnalysis.detectedPatterns.join(', ') || 'None' }}
                </div>
              </div>
              
              <!-- Suggestions -->
              <div class="suggestions" *ngIf="suggestions && suggestions.length > 0">
                <h5>💡 Optimization Suggestions:</h5>
                <ul>
                  <li *ngFor="let suggestion of suggestions">{{ suggestion }}</li>
                </ul>
              </div>

              <!-- Required Namespaces -->
              <div class="namespaces" *ngIf="codeAnalysis.requiredNamespaces.length > 0">
                <h5>📦 Required Namespaces:</h5>
                <div class="namespace-tags">
                  <span class="namespace-tag" *ngFor="let ns of codeAnalysis.requiredNamespaces">
                    using {{ ns }};
                  </span>
                </div>
              </div>
            </div>
          </div>

          <!-- Explanation Result -->
          <div class="explanation-result" *ngIf="explanation">
            <div class="result-header">
              <h3>💡 Code Explanation</h3>
            </div>
            <div class="explanation-content" [innerHTML]="formatExplanation(explanation)"></div>
            
            <!-- LINQ Equivalent if available -->
            <div class="linq-equivalent" *ngIf="linqEquivalent">
              <h4>🔄 LINQ Equivalent:</h4>
              <pre class="code-output">{{ linqEquivalent }}</pre>
            </div>
          </div>
        </div>
      </div>
    </div>
  `,
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  csharpCode = '';
  linqCode = '';
  explanation = '';
  linqEquivalent = '';
  isLoading = false;
  activeOperation: 'convert' | 'explain' | null = null;
  apiHealthy = false;
  showPatterns = false;

  codeAnalysis: any = null;
  suggestions: string[] = [];
  supportedPatterns: ConversionPattern[] = [];

  options: ConversionOptions = {
    preferMethodSyntax: true,
    optimizePerformance: false
  };

  constructor(private codeConverterService: CodeConverterService) { }

  ngOnInit(): void {
    this.checkApiHealth();
    this.loadSupportedPatterns();
  }

  checkApiHealth(): void {
    this.codeConverterService.checkApiHealth().subscribe({
      next: (isHealthy) => {
        this.apiHealthy = isHealthy;
      },
      error: () => {
        this.apiHealthy = false;
      }
    });
  }

  loadSupportedPatterns(): void {
    this.codeConverterService.getSupportedPatterns().subscribe({
      next: (result) => {
        this.supportedPatterns = result.supportedPatterns;
      },
      error: (error) => {
        console.error('Failed to load patterns:', error);
      }
    });
  }

  convertCode(): void {
    if (!this.csharpCode.trim()) return;

    this.isLoading = true;
    this.activeOperation = 'convert';
    this.explanation = '';
    this.linqEquivalent = '';

    this.codeConverterService.convertCSharpToLinq(this.csharpCode, this.options).subscribe({
      next: (result) => {
        if (result.success) {
          this.linqCode = result.linqCode;
          this.codeAnalysis = result.analysis;
          this.suggestions = result.suggestions || [];
        } else {
          this.linqCode = `// Code converter error: ${result.message || 'Conversion failed'}`;
          this.codeAnalysis = null;
          this.suggestions = [];
        }
      },
      error: (error) => {
        console.error('Code converter failed:', error);
        this.linqCode = '// Error: Code converter is not available. Please check your Vertex AI configuration.';
        this.codeAnalysis = null;
        this.suggestions = [];
      },
      complete: () => {
        this.isLoading = false;
        this.activeOperation = null;
      }
    });
  }

  explainCode(): void {
    if (!this.csharpCode.trim()) return;

    this.isLoading = true;
    this.activeOperation = 'explain';
    this.linqCode = '';

    this.codeConverterService.explainCSharpCode(this.csharpCode, true).subscribe({
      next: (result) => {
        if (result.success) {
          this.explanation = result.explanation;
          this.linqEquivalent = result.linqEquivalent || '';
          this.codeAnalysis = result.analysis;
        } else {
          this.explanation = `Code explainer error: ${result.message || 'Explanation failed'}`;
          this.linqEquivalent = '';
          this.codeAnalysis = null;
        }
      },
      error: (error) => {
        console.error('Code explainer failed:', error);
        this.explanation = 'Error: Code explainer is not available. Please check your Vertex AI configuration.';
        this.linqEquivalent = '';
        this.codeAnalysis = null;
      },
      complete: () => {
        this.isLoading = false;
        this.activeOperation = null;
      }
    });
  }

  copyToClipboard(text: string): void {
    navigator.clipboard.writeText(text).then(() => {
      // Could show a toast notification here
      console.log('Code copied to clipboard');
    });
  }

  formatExplanation(text: string): string {
    // Convert markdown-style formatting to HTML
    return text
      .replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>')
      .replace(/\*(.*?)\*/g, '<em>$1</em>')
      .replace(/`(.*?)`/g, '<code>$1</code>')
      .replace(/\n/g, '<br>');
  }
}
