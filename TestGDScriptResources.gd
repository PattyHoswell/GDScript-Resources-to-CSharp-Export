@tool
extends Resource
class_name TestGDScriptResources

@export var myVector : Vector2
@export_range(0, 10) var myNumber
@export var text : String
@export var scene : PackedScene
@export var arrayNumbers : Array[int]
