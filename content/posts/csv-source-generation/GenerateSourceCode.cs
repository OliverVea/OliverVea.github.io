
private static string GenerateSourceCode(string staticClassName, string fileLocation, string csvText)
{
    var reader = new CsvReader();
    var csvData = reader.ReadCsv(csvText);
    
    var parser = new CsvParser();
    var csvDocument = parser.ParseCsv(csvData);
    
    var csvTemplate = new CsvTemplate
    {
        StaticClassName = staticClassName,
        CsvDocument = csvDocument,
        FileLocation = fileLocation
    };

    return csvTemplate.TransformText();
}