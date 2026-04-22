#:project ../src/ObjectLayoutInspector/ObjectLayoutInspector.csproj
#:property PublishAot=false

using ObjectLayoutInspector;

Console.WriteLine("=== Nullable Type Layouts ===");
Console.WriteLine();

Console.WriteLine("--- int? ---");
TypeLayout.PrintLayout<int?>();
Console.WriteLine();

Console.WriteLine("--- DateTime? ---");
TypeLayout.PrintLayout<DateTime?>();
Console.WriteLine();

Console.WriteLine("--- bool? ---");
TypeLayout.PrintLayout<bool?>();
