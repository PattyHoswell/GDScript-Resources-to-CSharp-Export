using Godot;
using Godot.Collections;

[Tool]
[GlobalClass]
public partial class TestCSharpResources : Resource
{
    
    [Export] public Vector2 myVector;

    [Export(PropertyHint.Range, "0,10")] public float myNumber;

    [Export] public string text;

    [Export] public PackedScene scene;

    [Export] public Array<int> arrayNumbers;
    
}
