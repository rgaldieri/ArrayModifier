using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArrayModifier))]
[CanEditMultipleObjects]
public class EditorArrayModifier : Editor {
	// storing the selected offset type in the editor script
	private int _selectedOffsetType;
	private ArrayModifier _target;

	// SERIALIZED PROPERTIES
	private SerializedProperty _offsetType;

	private SerializedProperty _copiesCount;
	private SerializedProperty _offset;

	private SerializedProperty _mergeSubMeshes;
	private SerializedProperty _colliderOptions;
	private SerializedProperty _mergeIndipendentlyAction;

	// List of all the possible offset types in OffsetType
	private readonly string[] _offsetTypes = System.Enum.GetNames(typeof(ArrayModifier.OffsetType));
	
	// Show the Merge folder in the editor?
	private bool _showMerge = false;
	
	private void OnEnable(){
		// Initializing target
		_target = (ArrayModifier)target;
		// Initializing all the serialized properties
		_offsetType = serializedObject.FindProperty("offsetType");
		_copiesCount = serializedObject.FindProperty("CopiesCount");
		_offset = serializedObject.FindProperty("Offset");
		_mergeSubMeshes = serializedObject.FindProperty("MergeSubMeshes");
		_colliderOptions = serializedObject.FindProperty("colliderOptions");
		_mergeIndipendentlyAction = serializedObject.FindProperty("mergeIndipendentlyAction");
		
		// Initializing the selected offset type local value
		_selectedOffsetType = _offsetType.enumValueIndex;
	}

	public override void OnInspectorGUI(){
		// Allowing mixed values to be shown
		EditorGUI.showMixedValue = true;
		// Update object before creating the inspector
		serializedObject.Update();
		
		// Offset type 
		EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			// Drawing the Selector element
			_selectedOffsetType = GUILayout.SelectionGrid(_selectedOffsetType, _offsetTypes, _offsetTypes.Length);
			// Assign the value to offsetType
			_offsetType.enumValueIndex = _selectedOffsetType;
			GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		// Number of copies
		EditorGUILayout.PropertyField(_copiesCount);
		// offset for each axis
		EditorGUILayout.PropertyField(_offset);
		
		// Merging options
		EditorGUI.indentLevel = 1;
		_showMerge = EditorGUILayout.Foldout(_showMerge, "Merge");
		if (_showMerge)
		{
			EditorGUILayout.PropertyField(_mergeSubMeshes);
			EditorGUILayout.PropertyField(_colliderOptions);
			// If the collider option is set to CopyOnParent, display the warning message and further options
			if(_colliderOptions.enumValueIndex == 2){
				EditorGUILayout.HelpBox("MeshCollider can't be repositioned in a parent object. You can ignore mesh colliders, keep the parent mesh collider only, or keep them in children GameObjects.", MessageType.Warning);
				EditorGUILayout.PropertyField(_mergeIndipendentlyAction);
			}
			EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			if(GUILayout.Button("Merge", GUILayout.Width(90))){
				_target.Merge();
			}
			GUILayout.FlexibleSpace();
			EditorGUILayout.EndHorizontal();
		}
		EditorGUI.indentLevel = 0;

		// If anything has been changed, invoke OnValidate
		if(GUI.changed){
			// Clamp the number of _copiesCount
			_copiesCount.intValue = (int)Mathf.Clamp(_copiesCount.intValue, 2.0f, 100.0f);
			// apply the changes first. This triggers the OnValidate function 
			serializedObject.ApplyModifiedProperties();
			SceneView.RepaintAll();
		}
	}
}
