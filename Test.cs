using Godot;

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
	}
}
