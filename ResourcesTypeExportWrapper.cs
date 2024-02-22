using Godot;
using Godot.Collections;
using System.Linq;
using System.Reflection;

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
    private Dictionary<StringName, string> PropertyFieldHintString = new();

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
                    PropertyFieldHintString[$"_{field.Name}"] = exportAttribute.HintString;

                else
                    GD.PrintErr("Cannot find global class named ", exportAttribute.HintString,
                                " make sure the class you referring to have @tool ([Tool] if CSharp]) and have assigned class_name ([GlobalClass] if CSharp)");
            }
        }

        foreach (var property in GetType().GetProperties(BindingFlags.DeclaredOnly | BindingFlags.GetProperty | BindingFlags.Instance |
                                                         BindingFlags.Public | BindingFlags.NonPublic))
        {
            var exportAttribute = property.GetCustomAttribute<ExportAttribute>();
            if (exportAttribute != null && exportAttribute.Hint == PropertyHint.ResourceType)
            {
                if (globalClassList.Any(x => x == exportAttribute.HintString))
                    PropertyFieldHintString[$"_{property.Name}"] = exportAttribute.HintString;

                else
                    GD.PrintErr("Cannot find global class named ", exportAttribute.HintString,
                                " make sure the class you referring to have @tool ([Tool] if CSharp]) and have assigned class_name ([GlobalClass] if CSharp)");

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
            return Get(result).As<Resource>() != null;
        }

        return base._PropertyCanRevert(property);
    }

    public override bool _Set(StringName property, Variant value)
    {
        if (PropertyFieldHintString.ContainsKey(property))
        {
            var result = property.ToString();
            result = result.Substring(1, result.Length - 1);
            Set(result, value.As<Resource>());
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
            return Get(result).As<Resource>();
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
                { "type", (int)Variant.Type.Object },
                { "usage", (int)PropertyUsageFlags.Editor | (int)PropertyUsageFlags.EditorInstantiateObject },
                { "hint", (int)PropertyHint.ResourceType },
                { "hint_string", item.Value }
            });
        }
        return array;
    }
}
