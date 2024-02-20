# GDScript-Resources-to-CSharp-Export
 Export resources written in GDScript to your C# script with the built-in attribute, by default the ```[Export(PropertyHint.ResourceType, "...")]``` will still show all type that can be casted to ```Resources```. This is especially troublesome when you want to export a resources that are written on GDScript to your C# script, and what this script does. Is that it creates a clone property of the original that actually use this attribute and will refer to the correct type.

https://github.com/PattyHoswell/GDScript-Resources-to-CSharp-Export/assets/94728986/5489b873-1346-470f-8769-0cbaeb448512

# How To Use
Your script needs to have the ```[Tool]``` attribute that derives from ```ResourcesTypeExportWrapper```. This ```ResourcesTypeExportWrapper``` derives from ```Node``` for the purpose of this examples. But you can change it to any type you want. Then you can use the ```[Export(PropertyHint.ResourceType, "...")]``` to your field/property that you want to have the resources on.

# Additional Info
This example only provides you the means to export resources written in GDScript to C#. Please refer to the [Cross-language scripting](https://docs.godotengine.org/en/stable/tutorials/scripting/cross_language_scripting.html) documentation if you want to read more about cross scripting.
