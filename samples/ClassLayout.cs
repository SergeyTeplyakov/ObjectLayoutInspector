#:project ../src/ObjectLayoutInspector/ObjectLayoutInspector.csproj
#:property PublishAot=false

using ObjectLayoutInspector;

Console.WriteLine("=== Class Layouts ===");
Console.WriteLine();

Console.WriteLine("--- String ---");
TypeLayout.PrintLayout<string>();
Console.WriteLine();

Console.WriteLine("--- Exception ---");
TypeLayout.PrintLayout<Exception>();
