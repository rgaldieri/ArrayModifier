using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods {
    // first param is the gameobject to merge with this, and the second is whether if the children with
    // colliders must be kept
    public static void DestroyColliders(this GameObject gameobject)
    {
        Collider[] colliders = gameobject.GetComponents<Collider>();
		foreach(Collider col in colliders){
            Object.DestroyImmediate(col);
		}
    }
}
