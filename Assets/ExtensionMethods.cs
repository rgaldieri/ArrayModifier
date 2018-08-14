using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ExtensionMethods {
    // first param is the gameobject to merge with this, and the second is whether if the children with
    // colliders must be kept
    public static void DestroyColliders(this GameObject gameobject)
    {
        Collider[] colliders = gameobject.GetComponents<Collider>();
		foreach(Collider coll in colliders){
            Object.DestroyImmediate(coll);
		}
    }

    public static void DestroyMeshColliders(this GameObject gameobject)
    {
        Collider[] colliders = gameobject.GetComponents<Collider>();
		foreach(Collider coll in colliders){
            if(coll as MeshCollider){
              Object.DestroyImmediate(coll);
            }
		}
    }
}

