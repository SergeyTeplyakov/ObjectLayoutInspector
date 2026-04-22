#:project ../src/ObjectLayoutInspector/ObjectLayoutInspector.csproj
#:property PublishAot=false

using ObjectLayoutInspector;

Console.WriteLine("=== Struct Layouts ===");
Console.WriteLine();

Console.WriteLine("--- DateTime ---");
TypeLayout.PrintLayout<DateTime>();
Console.WriteLine();

Console.WriteLine("--- Guid ---");
TypeLayout.PrintLayout<Guid>();
