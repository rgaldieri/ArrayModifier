# Array Modifier

A Blender-like array component for Unity. It enables the user to replicate a GameObject and all its components for any amount of times. Just like blender, it allows the user to specify an absolute/relative offset, based on the object's [Renderer.bounds](https://docs.unity3d.com/ScriptReference/Renderer-bounds.html). Objects can be merged at any time to create a single GameObject.


## Known limitations

* Collider2D are not supported
* Operations can't be undone
* Any update in the component inspector resets individual changes in children

## future improvements

* Support Collider2D
* Support Global-Space offset
* Adding a rebuild button
