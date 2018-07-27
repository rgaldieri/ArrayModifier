﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA;

//TODO Randomize params, like rotation, texture or position
//TODO Add list of components to add to the copied gameobjects
[RequireComponent(typeof(MeshFilter))]
public class ArrayModifier : MonoBehaviour {

	public enum OffsetType{
		Constant,
		Relative
	}

	public enum ColliderOptions{
		NoCollider,
		KeepParentOnly,
		MergeIndependently,
		CreateMeshCollider
	}

	#region EXPOSED VARIABLES
	public OffsetType offsetType;

	// how many copies should this have?
	public int CopiesCount = 2;
	
	// Sensible offset value
	public Vector3 Offset = Vector3.one; 
	
	// Randomization variables
	public bool IsRandomized = false;
	[Range(1,10000)]
	public int RandomSeed = 1;
	// Randomization bounds
	public Vector3 RandomizationUpperBound = Vector3.zero;
	public Vector3 RandomizationLowerBound= Vector3.zero;
	
	[Tooltip("Leave copies' colliders as children of this GameObject. If disabled, colliders will be removed")]
	public bool LeaveCollidersAsChildren = false;
	[Tooltip("If all Meshes share the same single Material, this property should be set to true.")]
	public bool MergeSubMeshes = true;

	public ColliderOptions colliderOptions = ColliderOptions.NoCollider;
	
	#endregion

	private MeshRenderer renderer; // This gameObj renderer component
	
	// Transform the current sets of objects into a unique mesh. TODO: Can this operation be reversed?
	public void Merge()
	{
		// Bringing it to the origin before to do anything
		Vector3 originalPos = this.transform.position;
		this.transform.position = Vector3.zero;
		
		// TODO Reparent non-copies items
		// Create new mesh object
		MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();
		CombineInstance[] combine = new CombineInstance[meshFilters.Length];
		int i = 0;
		while (i < meshFilters.Length) {
			combine[i].mesh = meshFilters[i].sharedMesh;
			combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
			meshFilters[i].gameObject.SetActive(false);
			i++;
		}
		// Actually Combining the meshes
		transform.GetComponent<MeshFilter>().sharedMesh = new Mesh();
		transform.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine);
		transform.gameObject.SetActive(true);
		// Scale must be reset
		transform.localScale = Vector3.one;
		// Setting the position back to the origin
		this.transform.position = originalPos;
		// Manage what happens to colliders afterwards
		HandleColliders();
		// Destroy all children objects
		ClearChildren();
		// Destroy this component
		UnityEditor.EditorApplication.delayCall += () =>
		{
			DestroyImmediate(this);
		};
	}
	private void OnValidate()
	{
		// renderer can be changed in the editor, it can't really be initialized elsewhere
		renderer = GetComponent<MeshRenderer>();
		
		Rebuild();
	}
	
	private void HandleColliders(){
		Collider[] colliders = GetComponents<Collider>();
		switch(colliderOptions){
			case ColliderOptions.NoCollider:
				// Destroying previous colliders
				gameObject.DestroyColliders();
				break;
			case ColliderOptions.CreateMeshCollider:
				// Destroying previous colliders
				gameObject.DestroyColliders();
				// Setting up the new collider
				MeshCollider mc = gameObject.AddComponent<MeshCollider>();
				mc.sharedMesh = gameObject.GetComponent<MeshFilter>().sharedMesh;
				mc.convex = true;
				break;
			case ColliderOptions.MergeIndependently:
				foreach(Transform tr in transform){
					Collider[] childColliders = tr.GetComponents<Collider>();
					foreach(Collider coll in childColliders){
						//TODO center is wrong, how to handle?
						UnityEditorInternal.ComponentUtility.CopyComponent(coll);
						UnityEditorInternal.ComponentUtility.PasteComponentAsNew(gameObject);
						
					}
				}
				break;
			case ColliderOptions.KeepParentOnly:
			default:
				break;
		}
	}
	
	private void DestroyColliders(){

	}

	private bool Rebuild()
	{
		if(UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
		{
			return false;
		}
		// Destroy existing objects
		ClearChildren();
		// Make copies for every object
		MakeCopies();
		return true;
	}
	
	private void ClearChildren()
	{
		foreach (Transform t in transform)
		{
			UnityEditor.EditorApplication.delayCall += () =>
			{
				DestroyImmediate(t.gameObject);
			};
		}
	}

	private void MakeCopies()
	{
		// Getting the offset for each new object
		Vector3 currentOffset = (offsetType == OffsetType.Constant) ? Offset : GetRelativeOffset();

		for(int i = 1; i < CopiesCount ; i++)
		{
			var obj = GetACopy();
			obj.transform.Translate(currentOffset.x * i, currentOffset.y * i, currentOffset.z * i);
			obj.transform.name = "Copy " + i;
		}
	}

	private Vector3 GetRelativeOffset()
	{
		return new Vector3(Offset.x * renderer.bounds.size.x, Offset.y * renderer.bounds.size.y,
			Offset.z * renderer.bounds.size.z);
	}
	
	private GameObject GetACopy()
	{
		var copy = new GameObject();
		copy.transform.parent = gameObject.transform;
		copy.transform.localPosition = Vector3.zero;
		CopyComponents(copy);
		return copy;
	}

	private void CopyComponents(GameObject go)
	{
		// Transform doesn't get copied. Doing it manually
		go.transform.localPosition = Vector3.zero; // position is the same of parent
		go.transform.localScale = Vector3.one; // Scale is the same of parent
		go.transform.localRotation = Quaternion.identity;
		var components = GetComponents<Component>();
		foreach (var comp in components)
		{
			if(!(comp as ArrayModifier))
			{
				UnityEditorInternal.ComponentUtility.CopyComponent(comp);
				UnityEditorInternal.ComponentUtility.PasteComponentAsNew(go);
			}
		}
	}
}