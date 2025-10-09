import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { catchError, retry } from 'rxjs/operators';

export interface ConversionOptions {
  preferMethodSyntax?: boolean;
  optimizePerformance?: boolean;
}

export interface CodeAnalysis {
  complexity: string;
  detectedPatterns: string[];
  potentialIssues: string[];
  requiredNamespaces: string[];
  estimatedPerformanceGain: number;
}

export interface ConversionResult {
  linqCode: string;
  success: boolean;
  message?: string;
  analysis?: CodeAnalysis;
  suggestions?: string[];
}

export interface ExplanationResult {
  explanation: string;
  success: boolean;
  message?: string;
  analysis?: CodeAnalysis;
  linqEquivalent?: string;
}

export interface ConversionPattern {
  name: string;
  description: string;
  example: {
    csharp: string;
    linq: string;
  };
}

@Injectable({
  providedIn: 'root'
})
export class CodeConverterService {
  private readonly apiUrl = 'http://localhost:5037/api/codeconversion';

  constructor(private http: HttpClient) { }

  /**
   * Converts C# code to LINQ equivalent using custom agent system
   */
  convertCSharpToLinq(csharpCode: string, options?: ConversionOptions): Observable<ConversionResult> {
    console.log('Converting C# code with options:', options);
    return this.http.post<ConversionResult>(`${this.apiUrl}/convert`, {
      cSharpCode: csharpCode,
      options: options
    }).pipe(
      retry({ count: 2, delay: 1000 }),
      catchError((error: HttpErrorResponse) => {
        console.error('Code converter agent error:', error);
        console.error('API URL:', this.apiUrl);
        console.error('Error details:', {
          status: error.status,
          statusText: error.statusText,
          url: error.url,
          message: error.message
        });
        return of({
          linqCode: '',
          success: false,
          message: 'Code converter agent is currently unavailable. Please check your connection and try again.'
        });
      })
    );
  }

  /**
   * Explains C# code using custom code analyzer agent
   */
  explainCSharpCode(csharpCode: string, includeLinqEquivalent: boolean = true): Observable<ExplanationResult> {
    console.log('Explaining C# code');
    return this.http.post<ExplanationResult>(`${this.apiUrl}/explain`, {
      cSharpCode: csharpCode,
      includeLinqEquivalent: includeLinqEquivalent
    }).pipe(
      retry({ count: 2, delay: 1000 }),
      catchError((error: HttpErrorResponse) => {
        console.error('Code explainer agent error:', error);
        console.error('API URL:', this.apiUrl);
        console.error('Error details:', {
          status: error.status,
          statusText: error.statusText,
          url: error.url,
          message: error.message
        });
        return of({
          explanation: 'Code explainer agent is currently unavailable. Please check your connection and try again.',
          success: false,
          message: 'AI service connection error'
        });
      })
    );
  }

  /**
   * Get supported conversion patterns and examples
   */
  getSupportedPatterns(): Observable<{ supportedPatterns: ConversionPattern[] }> {
    console.log('Getting supported patterns');
    return this.http.get<{ supportedPatterns: ConversionPattern[] }>(`${this.apiUrl}/patterns`).pipe(
      retry({ count: 2, delay: 1000 }),
      catchError((error: HttpErrorResponse) => {
        console.error('Patterns API error:', error);
        console.error('API URL:', this.apiUrl);
        console.error('Error details:', {
          status: error.status,
          statusText: error.statusText,
          url: error.url,
          message: error.message
        });
        return of({
          supportedPatterns: []
        });
      })
    );
  }

  /**
   * Check custom agent system health and capabilities
   */
  checkApiHealth(): Observable<boolean> {
    console.log('Checking API health at:', `${this.apiUrl}/health`);
    return this.http.get<any>(`${this.apiUrl}/health`).pipe(
      retry({ count: 3, delay: 2000 }),
      catchError((error: HttpErrorResponse) => {
        console.error('API health check failed:', error);
        console.error('API URL:', this.apiUrl);
        console.error('Error details:', {
          status: error.status,
          statusText: error.statusText,
          url: error.url,
          message: error.message
        });
        return of(false);
      })
    ).pipe(
      catchError(() => of(false))
    );
  }
}
