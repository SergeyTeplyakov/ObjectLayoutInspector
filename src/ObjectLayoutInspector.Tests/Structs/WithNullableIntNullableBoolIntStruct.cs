namespace ObjectLayoutInspector.Tests
{
    struct WithNullableIntIntStruct
    {
        int? theNullableInt;

        int theInt;
    } 

    struct WithNullableIntNullableBoolIntStruct
    {
        int? one;
        bool? two;
        int three;
    }    
}