import { ComponentFixture, TestBed } from '@angular/core/testing';
import { FormsModule } from '@angular/forms';
import { AppComponent } from './app.component';
import { ConverterService } from './services/converter.service';

describe('AppComponent', () => {
  let component: AppComponent;
  let fixture: ComponentFixture<AppComponent>;
  let converterService: jasmine.SpyObj<ConverterService>;

  beforeEach(async () => {
    const converterServiceSpy = jasmine.createSpyObj('ConverterService', ['convertExpression', 'explainExpression']);

    await TestBed.configureTestingModule({
      imports: [AppComponent, FormsModule],
      providers: [
        { provide: ConverterService, useValue: converterServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AppComponent);
    component = fixture.componentInstance;
    converterService = TestBed.inject(ConverterService) as jasmine.SpyObj<ConverterService>;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should have empty initial values', () => {
    expect(component.cceExpression).toBe('');
    expect(component.eceExpression).toBe('');
    expect(component.explanation).toBe('');
    expect(component.isLoading).toBe(false);
  });

  it('should render text areas and buttons', () => {
    const compiled = fixture.nativeElement as HTMLElement;
    const textareas = compiled.querySelectorAll('textarea');
    const buttons = compiled.querySelectorAll('button');

    expect(textareas.length).toBe(2);
    expect(buttons.length).toBe(2);
    expect(compiled.querySelector('.convert-btn')?.textContent?.trim()).toBe('Convert →');
    expect(compiled.querySelector('.explain-btn')?.textContent?.trim()).toBe('Explain →');
  });

  it('should call converter service on convert button click', async () => {
    const mockResult = { converted: 'ECE_CONVERTED: TEST', success: true };
    converterService.convertExpression.and.returnValue(Promise.resolve(mockResult));
    
    component.cceExpression = 'test expression';
    
    await component.convertExpression();
    
    expect(converterService.convertExpression).toHaveBeenCalledWith('test expression');
    expect(component.eceExpression).toBe('ECE_CONVERTED: TEST');
    expect(component.explanation).toBe('');
  });

  it('should call converter service on explain button click', async () => {
    const mockResult = { explanation: 'Test explanation', success: true };
    converterService.explainExpression.and.returnValue(Promise.resolve(mockResult));
    
    component.cceExpression = 'test expression';
    
    await component.explainExpression();
    
    expect(converterService.explainExpression).toHaveBeenCalledWith('test expression');
    expect(component.explanation).toBe('Test explanation');
  });

  it('should not call service with empty expression', async () => {
    component.cceExpression = '';
    
    await component.convertExpression();
    await component.explainExpression();
    
    expect(converterService.convertExpression).not.toHaveBeenCalled();
    expect(converterService.explainExpression).not.toHaveBeenCalled();
  });

  it('should handle conversion errors gracefully', async () => {
    converterService.convertExpression.and.returnValue(Promise.reject(new Error('API Error')));
    spyOn(console, 'error');
    
    component.cceExpression = 'test expression';
    
    await component.convertExpression();
    
    expect(component.eceExpression).toBe('Error: Conversion failed');
    expect(console.error).toHaveBeenCalledWith('Conversion failed:', jasmine.any(Error));
  });

  it('should handle explanation errors gracefully', async () => {
    converterService.explainExpression.and.returnValue(Promise.reject(new Error('API Error')));
    spyOn(console, 'error');
    
    component.cceExpression = 'test expression';
    
    await component.explainExpression();
    
    expect(component.explanation).toBe('Error: Explanation failed');
    expect(console.error).toHaveBeenCalledWith('Explanation failed:', jasmine.any(Error));
  });

  it('should show loading state during operations', async () => {
    let resolvePromise: (value: any) => void;
    const promise = new Promise(resolve => { resolvePromise = resolve; });
    converterService.convertExpression.and.returnValue(promise);
    
    component.cceExpression = 'test';
    const convertPromise = component.convertExpression();
    
    expect(component.isLoading).toBe(true);
    
    resolvePromise!({ converted: 'result', success: true });
    await convertPromise;
    
    expect(component.isLoading).toBe(false);
  });

  it('should disable buttons when loading', () => {
    component.isLoading = true;
    fixture.detectChanges();
    
    const buttons = fixture.nativeElement.querySelectorAll('button');
    buttons.forEach((button: HTMLButtonElement) => {
      expect(button.disabled).toBe(true);
    });
  });

  it('should show explanation section when explanation exists', () => {
    component.explanation = 'Test explanation';
    fixture.detectChanges();
    
    const explanationSection = fixture.nativeElement.querySelector('.explanation-section');
    expect(explanationSection).toBeTruthy();
    expect(explanationSection?.textContent).toContain('Test explanation');
  });

  it('should hide explanation section when explanation is empty', () => {
    component.explanation = '';
    fixture.detectChanges();
    
    const explanationSection = fixture.nativeElement.querySelector('.explanation-section');
    expect(explanationSection).toBeFalsy();
  });
});