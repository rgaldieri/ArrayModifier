using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(ArrayModifier))]
[CanEditMultipleObjects]
public class EditorArrayModifier : Editor {
	private int _selectedOffsetType;
	private ArrayModifier _target;

	private SerializedProperty _offsetType;

	private SerializedProperty _copiesCount;
	private SerializedProperty _offset;

	private SerializedProperty _isRandomized;
	private SerializedProperty _randomSeed;
	private SerializedProperty _randomizationUpperBound;
	private SerializedProperty _randomizationLowerBound;
	private SerializedProperty _leaveCollidersAsChildren;
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
		_isRandomized = serializedObject.FindProperty("IsRandomized");
		_randomSeed = serializedObject.FindProperty("RandomSeed");
		_randomizationUpperBound = serializedObject.FindProperty("RandomizationUpperBound");
		_randomizationLowerBound = serializedObject.FindProperty("RandomizationLowerBound");
		_leaveCollidersAsChildren = serializedObject.FindProperty("LeaveCollidersAsChildren");
		_mergeSubMeshes = serializedObject.FindProperty("MergeSubMeshes");
		_colliderOptions = serializedObject.FindProperty("colliderOptions");
		_mergeIndipendentlyAction = serializedObject.FindProperty("mergeIndipendentlyAction");
		
		// Initializing the selected offset type local value
		_selectedOffsetType = _offsetType.enumValueIndex;
	}

	public override void OnInspectorGUI(){
		// Allowing mixed values to be shown
		EditorGUI.showMixedValue = true;
		
		serializedObject.Update();
		
		// Offset type 
		EditorGUILayout.BeginHorizontal();
			GUILayout.FlexibleSpace();
			// Drawing the UI element
			_selectedOffsetType = GUILayout.SelectionGrid(_selectedOffsetType, _offsetTypes, _offsetTypes.Length);
			// Assign the value to fitType
			_offsetType.enumValueIndex = _selectedOffsetType;
			GUILayout.FlexibleSpace();
		EditorGUILayout.EndHorizontal();
		
		// Number of copies
		EditorGUILayout.PropertyField(_copiesCount);
		// offset for each axis
		EditorGUILayout.PropertyField(_offset);

		#region RANDOMIZATION
			// Randomization options panel
			EditorGUILayout.PropertyField(_isRandomized);
			EditorGUI.showMixedValue = false; // Do not want to show randomization options if mixed
			if(_isRandomized.boolValue){
				EditorGUI.indentLevel = 1;
				EditorGUILayout.PropertyField(_randomSeed);
				EditorGUILayout.PropertyField(_randomizationUpperBound);
				EditorGUILayout.PropertyField(_randomizationLowerBound);
				EditorGUI.indentLevel = 0;
			}
			EditorGUI.showMixedValue = true;
		#endregion
		
		#region MERGING
			EditorGUI.indentLevel = 1;
			_showMerge = EditorGUILayout.Foldout(_showMerge, "Merge");
			if (_showMerge)
			{
				EditorGUILayout.PropertyField(_leaveCollidersAsChildren);
				EditorGUILayout.PropertyField(_mergeSubMeshes);
				EditorGUILayout.PropertyField(_colliderOptions);
				if(_colliderOptions.enumValueIndex == 2){
					EditorGUILayout.HelpBox("MeshCollider can't be repositioned in a parent object. You can either ignore mesh colliders only, or keep them in a children object.", MessageType.Warning);
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
		#endregion

		// TODO do I want this check BEFORE the button click?
		if(GUI.changed){
			// Clamp the number of _copiesCount
			_copiesCount.intValue = (int)Mathf.Clamp(_copiesCount.intValue, 2.0f, 100.0f);
			// apply the changes first. This triggers the OnValidate function 
			serializedObject.ApplyModifiedProperties();
			SceneView.RepaintAll();
		}
		


	}
}
