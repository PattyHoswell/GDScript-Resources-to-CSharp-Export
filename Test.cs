using Godot;
using Godot.Collections;
/// <summary>
/// <see cref="ToolAttribute"/> is required to show our custom resources from GDScript (or CSharp)
/// </summary>
[Tool]
public partial class Test : ResourcesTypeExportWrapper
{
    /// <summary>
    /// You can also use CSharp resources but this is redundant, I can't think of situation where you want to use this.
    /// 
    /// <para>Just use <see cref="TestCSharpResources"/> as the base since you already have the type/para>
    /// </summary>
	[Export(PropertyHint.ResourceType, "TestCSharpResources")]
    public Resource CSharpResources;

    [Export(PropertyHint.ResourceType, "TestGDScriptResources")] 
    public Resource GDScriptResources;

    //Supports array with no arguments, this will show as Array[TestGDScriptResources] in the editor
    [Export(PropertyHint.ResourceType, "TestGDScriptResources")]
    public Array GDScriptResourcesArray;

    //Supports typed array
    [Export(PropertyHint.ResourceType, "TestGDScriptResources")]
    public Resource[] GDScriptResourcesTypedArray;

    //Supports nested array, there is no limit to how much nesting you can do
    [Export(PropertyHint.ResourceType, "TestGDScriptResources")]
    public Array<Array<Resource>> GDScriptResourcesNestedTypedArray;

    //You can even do this if you're crazy enough
    //[Export(PropertyHint.ResourceType, "TestGDScriptResources")]
    //public Array<Array<Array<Array<Array<Array<Array<Array<Array<Array<Array<Array<Array<Array<Array<Array<Array<Array<Array<Array<Array<Array<Array<Resource>>>>>>>>>>>>>>>>>>>>>>> GDScriptResourcesCrazyArray;

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
	{
        //If we are on the editor then skip, we only want our script to run in game
        if (Engine.IsEditorHint())
            return;

        //A side by side comparison on gdscript resources and csharp resources
		if (CSharpResources != null)
        {
            GD.Print("<--CSharp Resources-->");
            var testCSharpResources = (TestCSharpResources)CSharpResources;
            GD.Print(testCSharpResources.myVector);
            GD.Print(testCSharpResources.myNumber);
            GD.Print(testCSharpResources.text);
            GD.Print(testCSharpResources.scene);
            foreach (var number in testCSharpResources.arrayNumbers)
                GD.Print(number);
        }
        else
            GD.Print(nameof(CSharpResources), " is null");

        if (GDScriptResources != null)
        {
            GD.Print("<--GDScript Resources-->");
            GD.Print(GDScriptResources.Get("myVector"));
            GD.Print(GDScriptResources.Get("myNumber"));
            GD.Print(GDScriptResources.Get("text"));
            GD.Print(GDScriptResources.Get("scene"));
            foreach (var number in GDScriptResources.Get("arrayNumbers").AsGodotArray())
                GD.Print(number);
        }
        else
            GD.Print(nameof(GDScriptResources), " is null");


        #region Array

        if (GDScriptResourcesArray != null)
            Recursion(GDScriptResourcesArray, "GDScript Resources Array");
        else
            GD.Print(nameof(GDScriptResourcesArray), " is null");

        if (GDScriptResourcesTypedArray != null)
            Recursion(Get(nameof(GDScriptResourcesTypedArray)).AsGodotArray(), "GDScript Resources Typed Array");
        else
            GD.Print(nameof(GDScriptResourcesTypedArray), " is null");

        if (GDScriptResourcesNestedTypedArray != null)
            Recursion(Get(nameof(GDScriptResourcesNestedTypedArray)).AsGodotArray(), "GDScript Resources Nested Typed Array");
        else
            GD.Print(nameof(GDScriptResourcesNestedTypedArray), " is null");

        #endregion
    }

    void Recursion(Array parentArray, string name)
    {
        foreach (var item in parentArray)
        {
            var gdscriptResource = item.As<Resource>();
            if (gdscriptResource != null)
                PrintGDScriptResources(gdscriptResource, name);

            else if (item.VariantType == Variant.Type.Array)
                Recursion(item.AsGodotArray(), name);
        }
    }

    void PrintGDScriptResources(Resource gdscriptResource, string name)
    {
        GD.Print("<--", name, "-->");
        GD.Print(gdscriptResource.Get("myVector"));
        GD.Print(gdscriptResource.Get("myNumber"));
        GD.Print(gdscriptResource.Get("text"));
        GD.Print(gdscriptResource.Get("scene"));
        foreach (var number in gdscriptResource.Get("arrayNumbers").AsGodotArray())
            GD.Print(number);
    }
}
