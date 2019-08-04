#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEngine;

public static class LinkedAssetMetasGUI
{
	public static void ObjectField(Rect rect, SerializedProperty objectProp, GUIContent label)
	{
		var fieldType = objectProp.GetSerializedPropertyType();
		ObjectField(rect, objectProp, fieldType, label);
	}

	public static void ObjectField(Rect rect, SerializedProperty objectProp, System.Type objectType, GUIContent label)
	{
		EditorGUI.BeginChangeCheck();
		EditorGUI.ObjectField(rect, objectProp, objectType, label);		
		if (EditorGUI.EndChangeCheck())
		{
			objectProp.serializedObject.ApplyModifiedProperties();
			AssetDatabase.SaveAssets();

			var toPath = AssetDatabase.GetAssetPath(objectProp.objectReferenceValue);
			foreach (var target in objectProp.serializedObject.targetObjects)
			{
				var fromPath = AssetDatabase.GetAssetPath(target);
				LinkedAssetMetas.AddLink(fromPath, toPath);
			}
		}
	}
}
#endif