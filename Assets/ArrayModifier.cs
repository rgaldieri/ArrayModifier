using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.WSA;

//TODO Add list of components to add to the copied gameobjects
[RequireComponent(typeof(MeshFilter))]
[ExecuteInEditMode]
public class ArrayModifier : MonoBehaviour {
	#region ENUMS

	public enum OffsetType{
		Constant,
		Relative,
	}

	public enum ColliderOptions{
		NoCollider,
		KeepParentOnly,
		CopyOnParent,
		CreateMeshCollider
	}

	public enum MergeIndependentlyAction{
		NoMeshCollider,
		MeshColliderOnParentOnly,
		KeepThemAsChildren
	}

	#endregion

	#region EXPOSED VARIABLES
	// Current offset type
	public OffsetType offsetType;

	// how many copies should it make?
	public int CopiesCount = 2;
	
	// Sensible offset value
	public Vector3 Offset = Vector3.one; 
	
	// If true, use wolrd space, use local space otherwise
	public bool UseWorldSpace = false;

	[Tooltip("If all Meshes share the same single Material, this property should be set to true.")]
	public bool MergeSubMeshes = true;

	public ColliderOptions colliderOptions = ColliderOptions.NoCollider;
	public MergeIndependentlyAction mergeIndipendentlyAction = MergeIndependentlyAction.NoMeshCollider;

	#endregion

	private MeshRenderer renderer; // This gameObj renderer component
	
	void OnEnable(){
		StartBuild();
	}
	
	void OnDestroy(){
		ClearChildren();
	}

	public void StartBuild(){
		// renderer can be changed in the editor, it can't really be initialized elsewhere
		renderer = GetComponent<MeshRenderer>();
		if(!(Rebuild())){
			Debug.LogWarning("This component can't be changed outside Editor mode.");
		}
	}

	// Transform the current sets of objects into a unique mesh. TODO: make this operation reversible
	public void Merge()
	{
		// Bringing it to the origin will make future calculations simpler
		Vector3 originalPos = this.transform.position;
		Quaternion originalRot = this.transform.rotation;
		this.transform.position = Vector3.zero;
		this.transform.rotation = Quaternion.identity;
		
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
		transform.GetComponent<MeshFilter>().sharedMesh.CombineMeshes(combine, MergeSubMeshes);
		transform.gameObject.SetActive(true);

		// Manage what happens to colliders afterwards
		HandleColliders();
		// Destroy all children objects that must be destroyed
		ClearChildren();
		// Destroy this component
		UnityEditor.EditorApplication.delayCall += () =>
		{
			DestroyImmediate(this);
		};
		// Scale must be reset
		transform.localScale = Vector3.one;
		// Setting the position back to the origin
		this.transform.position = originalPos;
		this.transform.rotation = originalRot;
	}
	#region COLLIDERS FUNCTIONS
	private void HandleColliders(){
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
			case ColliderOptions.CopyOnParent:
				switch(mergeIndipendentlyAction){
					case MergeIndependentlyAction.NoMeshCollider:
						DestroyMeshColliders();
						break;
					case MergeIndependentlyAction.KeepThemAsChildren:
					case MergeIndependentlyAction.MeshColliderOnParentOnly:
					default:
						break;
				}
				CopyCollidersToThis();
				break;
			case ColliderOptions.KeepParentOnly:
			default:
				break;
		}
	}

	private void CopyCollidersToThis(){
		foreach(Transform tr in transform){
			Collider[] childColliders = tr.GetComponents<Collider>();
			foreach(Collider coll in childColliders){
				// If the collider is a mesh collider
				if(coll as MeshCollider){
					// If users wanted it to be treated differently
					if( mergeIndipendentlyAction == MergeIndependentlyAction.KeepThemAsChildren){
						CreateMeshColliderChild(tr.gameObject, coll);
					}
				} else {
					CopyStandardCollider(coll);
				}
			}
		}
	}
	
	// Copy a non-mesh collider to the gameObject containing this Component
	private void CopyStandardCollider(Collider coll){
		if(coll as BoxCollider){
			BoxCollider box = coll as BoxCollider;
			box.center = coll.gameObject.transform.position + box.center;
			PasteComponentOnGameObject(box);
		}
		if(coll as SphereCollider){
			SphereCollider sphere = coll as SphereCollider;
			sphere.center = coll.gameObject.transform.position + sphere.center;
			PasteComponentOnGameObject(sphere);
		}
		if(coll as CapsuleCollider){
			CapsuleCollider capsule = coll as CapsuleCollider;
			capsule.center = coll.gameObject.transform.position + capsule.center;
			PasteComponentOnGameObject(capsule);
		}
		if(coll as WheelCollider){
			WheelCollider wheel = coll as WheelCollider;
			wheel.center = coll.gameObject.transform.position + wheel.center;
			PasteComponentOnGameObject(wheel);
		}
	}

	private void CreateMeshColliderChild(GameObject go, Collider coll){
		GameObject newGo = new GameObject("MeshCollider_placeholder");
		newGo.transform.parent = this.transform;
		newGo.transform.localPosition = go.transform.localPosition;
		// Copy Component to object
		PasteComponentOnGameObject(coll, true);
	}
	#endregion

	#region EXTENSION METHODS WRAPPER
	// Destroy all colliders on gameObject
	private void DestroyColliders(){
		this.gameObject.DestroyColliders();
	}

	// Destroy mesh colliders only on gameObject
	private void DestroyMeshColliders(){
		this.gameObject.DestroyMeshColliders();
	}
	
	private void PasteComponentOnGameObject(Component c, bool DelayCall = false){
		gameObject.PasteComponent(c, DelayCall);
	}
	#endregion

	private bool Rebuild()
	{
		// If not in the editor, do not rebuild
		if(UnityEditor.EditorApplication.isPlayingOrWillChangePlaymode)
		{
			return false;
		}
		// Destroy existing children
		ClearChildren();
		// Make CopiesCount copies of this object
		MakeCopies();
		return true;
	}
	
	private void ClearChildren()
	{
		foreach (Transform t in transform)
		{
			// TODO Checkign whether if it has a MeshFilter is not a strong contorl
			// Improve with array of children
			if(t.gameObject.GetComponent<MeshFilter>() != null){
				UnityEditor.EditorApplication.delayCall += () =>
				{
					if(t != null){
						DestroyImmediate(t.gameObject);
					}
				};
			}
		}
	}

	private void MakeCopies()
	{
		// Getting the offset for each new object
		Vector3 currentOffset = (offsetType == OffsetType.Constant) ? Offset : GetRelativeOffset();
		Space space = (UseWorldSpace) ? Space.World : Space.Self;
		// make CopiesCount - 1 copies
		for(int i = 1; i < CopiesCount ; i++)
		{
			var obj = MakeSingleCopy();
			// Offsetting the copy
			obj.transform.Translate(currentOffset.x * i, currentOffset.y * i, currentOffset.z * i, space);
			obj.transform.name = "Copy " + i;
		}
	}


	private Vector3 GetRelativeOffset()
	{
		return new Vector3(Offset.x * renderer.bounds.size.x, Offset.y * renderer.bounds.size.y,
			Offset.z * renderer.bounds.size.z);
	}
	
	private GameObject MakeSingleCopy()
	{
		// Mae a copy
		var copy = new GameObject();
		// reparent it
		copy.transform.parent = gameObject.transform;
		// Copy all components (but this)
		CopyComponents(copy);
		return copy;
	}

	private void CopyComponents(GameObject go)
	{
		// Transform doesn't get copied. Doing it manually
		go.transform.LocalReset();
		var components = GetComponents<Component>();
		// Copy all component but ArrayModifier
		foreach (var comp in components)
		{
			// Not copying ArrayModifier to avoid recursion
			if(!(comp as ArrayModifier))
			{
				UnityEditorInternal.ComponentUtility.CopyComponent(comp);
				UnityEditorInternal.ComponentUtility.PasteComponentAsNew(go);
			}
			
		}
	}

}