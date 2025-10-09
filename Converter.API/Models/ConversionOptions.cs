namespace Converter.API.Models;

public class ConversionOptions
{
    public bool PreferMethodSyntax { get; set; } = true;

    public bool OptimizePerformance { get; set; } = false;
}