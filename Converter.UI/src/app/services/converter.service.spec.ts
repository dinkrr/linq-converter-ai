import { TestBed } from '@angular/core/testing';
import { ConverterService } from './converter.service';

describe('ConverterService', () => {
  let service: ConverterService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ConverterService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  describe('convertExpression', () => {
    it('should convert CCE expression to ECE format', async () => {
      const testExpression = 'CCE.${variable}.getValue()';
      const result = await service.convertExpression(testExpression);

      expect(result.success).toBe(true);
      expect(result.converted).toContain('ECE_CONVERTED:');
      expect(result.converted).toContain('ECE.#{VARIABLE}.VALUE');
      expect(result.message).toBe('Conversion completed successfully');
    });

    it('should return empty string for empty expression', async () => {
      const result = await service.convertExpression('');
      
      expect(result.success).toBe(true);
      expect(result.converted).toBe('');
    });

    it('should handle whitespace-only expressions', async () => {
      const result = await service.convertExpression('   ');
      
      expect(result.success).toBe(true);
      expect(result.converted).toBe('');
    });

    it('should transform CCE syntax elements correctly', async () => {
      const testExpression = 'CCE.field.getValue() + ${variable}';
      const result = await service.convertExpression(testExpression);

      expect(result.converted).toContain('ECE.FIELD.VALUE');
      expect(result.converted).toContain('#{VARIABLE}');
    });
  });

  describe('explainExpression', () => {
    it('should provide explanation for valid expression', async () => {
      const testExpression = 'CCE.field.getValue() + ${variable}';
      const result = await service.explainExpression(testExpression);

      expect(result.success).toBe(true);
      expect(result.explanation).toContain('CCE expression');
      expect(result.explanation).toContain('Components:');
      expect(result.explanation).toContain('Variables:');
      expect(result.explanation).toContain('Functions:');
      expect(result.explanation).toContain('Operators:');
      expect(result.message).toBe('Explanation generated successfully');
    });

    it('should return empty explanation for empty expression', async () => {
      const result = await service.explainExpression('');
      
      expect(result.success).toBe(true);
      expect(result.explanation).toBe('');
    });

    it('should detect variables in expression', async () => {
      const testExpression = 'field + ${var1} + ${var2}';
      const result = await service.explainExpression(testExpression);

      expect(result.explanation).toContain('var1, var2');
    });

    it('should detect functions in expression', async () => {
      const testExpression = 'getValue() + calculate()';
      const result = await service.explainExpression(testExpression);

      expect(result.explanation).toContain('getValue(), calculate()');
    });

    it('should detect operators in expression', async () => {
      const testExpression = 'a + b * c == d';
      const result = await service.explainExpression(testExpression);

      expect(result.explanation).toContain('+');
      expect(result.explanation).toContain('*');
      expect(result.explanation).toContain('==');
    });

    it('should handle expressions with no variables', async () => {
      const testExpression = 'simple expression';
      const result = await service.explainExpression(testExpression);

      expect(result.explanation).toContain('None detected');
    });
  });

  describe('private methods behavior', () => {
    it('should simulate API delay', async () => {
      const startTime = Date.now();
      await service.convertExpression('test');
      const endTime = Date.now();
      
      // Should take at least 1400ms (allowing for some variance)
      expect(endTime - startTime).toBeGreaterThan(1400);
    });

    it('should handle duplicate variables correctly', async () => {
      const testExpression = '${var1} + ${var1} + ${var2}';
      const result = await service.explainExpression(testExpression);

      // Should only list unique variables
      const variableMatches = result.explanation.match(/var1/g);
      expect(variableMatches?.length).toBeLessThanOrEqual(2); // Once in explanation, once in list
    });

    it('should handle complex expressions with multiple syntax elements', async () => {
      const complexExpression = 'CCE.${field1}.getValue() + ${field2} * calculate() == target';
      const conversionResult = await service.convertExpression(complexExpression);
      const explanationResult = await service.explainExpression(complexExpression);

      expect(conversionResult.converted).toContain('ECE_CONVERTED:');
      expect(conversionResult.converted).toContain('#{FIELD1}');
      expect(conversionResult.converted).toContain('#{FIELD2}');
      
      expect(explanationResult.explanation).toContain('field1, field2');
      expect(explanationResult.explanation).toContain('getValue(), calculate()');
      expect(explanationResult.explanation).toContain('+, *, ==');
    });
  });
});