using Godot;
using Godot.Collections;
using System.Linq;
using System.Reflection;
using System.Text;

/// <summary>
/// <see cref="ToolAttribute"/> is required to show our custom resources from GDScript (or CSharp)
/// 
/// <para>You can change the type this derives from, for this testing we will be deriving from <see cref="Node"/></para>
/// </summary>
[Tool]
public partial class ResourcesTypeExportWrapper : Node
{
    /// <summary>
    /// Make our keys to be <see cref="StringName"/> instead so we don't have to keep implicitly converting to <see cref="string"/> for comparing our key
    /// </summary>
    private System.Collections.Generic.Dictionary<StringName, (string hintString, PropertyHint hintType, Variant.Type propertyType)> PropertyFieldHintString = new();

    /// <summary>
    /// Get all the <see cref="GlobalClassAttribute"/> on the current project including the one from GDScript
    /// 
    /// <para>Make sure the class you referring to have <see cref="ToolAttribute"/> (@tool if GDScript) and <see cref="GlobalClassAttribute"/> (class_name if GDScript)</para>
    /// 
    /// <para>Then add it to our <see cref="PropertyFieldHintString"/> with underscores at the start to create a copy of the property that we will use to point to the correct type</para>
    /// </summary>
    private void InitAllProperty()
    {
        //Since this method will be called often, clear our cache in case we mistakenly cached a property/field that has been renamed or no longer exists
        PropertyFieldHintString.Clear();
        var globalClassList = ProjectSettings.Singleton.GetGlobalClassList().Select(x => x["class"].AsString());
        foreach (var field in GetType().GetFields(BindingFlags.DeclaredOnly | BindingFlags.GetField | BindingFlags.Instance |
                                                  BindingFlags.Public | BindingFlags.NonPublic))
        {
            var exportAttribute = field.GetCustomAttribute<ExportAttribute>();
            if (exportAttribute != null && exportAttribute.Hint == PropertyHint.ResourceType)
            {
                if (globalClassList.Any(x => x == exportAttribute.HintString))
                {
                    var hintStringBuilder = new StringBuilder();
                    var propertyHint = PropertyHint.ResourceType;
                    var nestedType = field.FieldType;
                    var propertyType = Variant.Type.Object;

                    if (GD.TypeToVariantType(nestedType) == Variant.Type.Array)
                    {
                        propertyHint = PropertyHint.ArrayType;
                        propertyType = Variant.Type.Array;

                        while (nestedType != null && GD.TypeToVariantType(nestedType) == Variant.Type.Array)
                        {
                            //Doesn't add the first item because otherwise 1d array will become 2d, 2d becomes 3d and so on..
                            if (nestedType != field.FieldType)
                                hintStringBuilder.Append($"{Variant.Type.Array:D}:");

                            nestedType = nestedType.GenericTypeArguments.Length > 0 ? nestedType.GenericTypeArguments[0] : null;
                        }

                        hintStringBuilder.Append($"{Variant.Type.Object:D}/{PropertyHint.ResourceType:D}:{exportAttribute.HintString}");
                    }

                    else if (GD.TypeToVariantType(nestedType) == Variant.Type.Dictionary)
                    {
                        GD.PrintErr("At the time of writing this, Dictionary is not supported, even if this script created a property that points to the correct type, it is ignored by godot.");
                        continue;
                    }

                    else
                    {
                        hintStringBuilder.Append(exportAttribute.HintString);
                    }
                    PropertyFieldHintString[$"_{field.Name}"] = (hintStringBuilder.ToString(), propertyHint, propertyType);
                }
                
                else
                    GD.PrintErr("Cannot find global class named ", exportAttribute.HintString,
                                " make sure the class you referring to have @tool ([Tool] if CSharp) and have assigned class_name ([GlobalClass] if CSharp)");
            }
        }

        foreach (var property in GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.Instance |
                                                         BindingFlags.Public | BindingFlags.NonPublic))
        {
            var exportAttribute = property.GetCustomAttribute<ExportAttribute>();
            if (exportAttribute != null && exportAttribute.Hint == PropertyHint.ResourceType)
            {
                if (globalClassList.Any(x => x == exportAttribute.HintString))
                {

                    var hintStringBuilder = new StringBuilder();
                    var propertyHint = PropertyHint.ResourceType;
                    var nestedType = property.PropertyType;
                    var propertyType = Variant.Type.Object;

                    if (GD.TypeToVariantType(nestedType) == Variant.Type.Array)
                    {
                        propertyHint = PropertyHint.ArrayType;
                        propertyType = Variant.Type.Array;

                        while (nestedType != null && GD.TypeToVariantType(nestedType) == Variant.Type.Array)
                        {
                            //Doesn't add the first item because otherwise 1d array will become 2d, 2d becomes 3d and so on..
                            if (nestedType != property.PropertyType)
                                hintStringBuilder.Append($"{Variant.Type.Array:D}:");

                            nestedType = nestedType.GenericTypeArguments.Length > 0 ? nestedType.GenericTypeArguments[0] : null;
                        }

                        hintStringBuilder.Append($"{Variant.Type.Object:D}/{PropertyHint.ResourceType:D}:{exportAttribute.HintString}");
                    }

                    else if (GD.TypeToVariantType(nestedType) == Variant.Type.Dictionary)
                    {
                        GD.PrintErr("At the time of writing this, Dictionary is not supported, even if this script created a property that points to the correct type, it is ignored by godot.");
                        continue;
                    }

                    else
                    {
                        hintStringBuilder.Append(exportAttribute.HintString);
                    }
                    PropertyFieldHintString[$"_{property.Name}"] = (hintStringBuilder.ToString(), propertyHint, propertyType);
                }

                else
                    GD.PrintErr("Cannot find global class named ", exportAttribute.HintString,
                                " make sure the class you referring to have @tool ([Tool] if CSharp) and have assigned class_name ([GlobalClass] if CSharp)");

            }
        }
    }

    #region Property Value Handler
    public override bool _PropertyCanRevert(StringName property)
    {
        if (PropertyFieldHintString.ContainsKey(property))
        {
            var result = property.ToString();
            result = result.Substring(1, result.Length - 1);
            return Get(result).VariantType != Variant.Type.Nil;
        }

        return base._PropertyCanRevert(property);
    }

    public override bool _Set(StringName property, Variant value)
    {
        if (PropertyFieldHintString.ContainsKey(property))
        {
            var result = property.ToString();
            result = result.Substring(1, result.Length - 1);
            Set(result, value);
            return true;
        }

        return base._Set(property, value);
    }

    public override Variant _Get(StringName property)
    {
        if (PropertyFieldHintString.ContainsKey(property))
        {
            var result = property.ToString();
            result = result.Substring(1, result.Length - 1);
            return Get(result);
        }
        return base._Get(property);
    }
    #endregion

    /// <summary>
    /// If you wonder why not just change the original property to get the correct type, for some reason it wont save the changes to our exported resources
    /// 
    /// <para>Instead of figuring out how to deal with this and get gud, we just create a "fake" property that points to the correct type of our resources</para>
    /// </summary>
    /// <param name="property"></param>
    public override void _ValidateProperty(Dictionary property)
    {
        var name = property["name"].AsString();

        //No longer assumes that the original field doesn't start with _
        //if (!name.StartsWith("_") && PropertyFieldHintString.ContainsKey($"_{name}"))

        if (PropertyFieldHintString.ContainsKey($"_{name}"))

            //Hide the original property as we already created a "fake" property that points to the correct type
            property["usage"] = (int)PropertyUsageFlags.NoEditor;

    }

    public override Array<Dictionary> _GetPropertyList()
    {
        //Get all property and field from the current type
        InitAllProperty();

        //Create a copy of the original property but with _ at the start of the name which will be stripped by the editor anyways
        var array = new Array<Dictionary>();
        foreach (var item in PropertyFieldHintString)
        {
            array.Add(new Dictionary
            {
                { "name", item.Key},
                { "type", (int)item.Value.propertyType },
                { "usage", (int)PropertyUsageFlags.Editor | (int)PropertyUsageFlags.EditorInstantiateObject },
                { "hint", (int)item.Value.hintType },
                { "hint_string", item.Value.hintString }
            });
        }
        return array;
    }
}
