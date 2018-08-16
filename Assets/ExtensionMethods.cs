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

    public static void PasteComponent(this GameObject gameobject, Component c, bool DelayCall = false)
    {
        // Gameobject must be in the editor
        if(!(UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode))
		{
            UnityEditorInternal.ComponentUtility.CopyComponent(c);
            // Sometimes, call needs to be delayed
            if(!(DelayCall)){
                UnityEditorInternal.ComponentUtility.PasteComponentAsNew(gameobject);
            } else {
                UnityEditor.EditorApplication.delayCall += () =>
                {
                    UnityEditorInternal.ComponentUtility.PasteComponentAsNew(gameobject);
                };
            }
        }
	}
    
    public static void LocalReset(this Transform transform){
        transform.localPosition = Vector3.zero;
		transform.localScale = Vector3.one;
		transform.localRotation = Quaternion.identity;
    }
}