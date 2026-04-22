#:project ../src/ObjectLayoutInspector/ObjectLayoutInspector.csproj
#:property PublishAot=false

using ObjectLayoutInspector;

Console.WriteLine("--- Primitive array: int[5] ---");
ArrayLayout.PrintLayout<int>(5);

Console.WriteLine("--- Struct array with inline fields: Point[3] ---");
ArrayLayout.PrintLayout<Point>(3);

Console.WriteLine("--- Struct array with values: Point[3] ---");
var points = new Point[] { new(1, 2), new(3, 4), new(5, 6) };
ArrayLayout.PrintLayout(points);

Console.WriteLine("--- Nullable array with values: int?[4] ---");
var nullables = new int?[] { 42, null, 7, null };
ArrayLayout.PrintLayout((Array)nullables);

Console.WriteLine("--- Reference type array: string[4] ---");
ArrayLayout.PrintLayout<string>(4);

record struct Point(int X, int Y);