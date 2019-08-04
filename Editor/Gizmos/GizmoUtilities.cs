// Cratesmith 2018
using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public static class GizmoUtilities 
{
	#if UNITY_EDITOR
	[InitializeOnLoadMethod]
	public static void Init()
	{
		Camera.onPreCull += OnPreCull;
	}

	private static void OnPreCull(Camera cam)
	{
		if (cam.cameraType != CameraType.SceneView) return;
		foreach (var sStaticMeshGizmo in s_staticMeshGizmos)
		{
			var matrix = sStaticMeshGizmo.Key;
			var mr = sStaticMeshGizmo.Value.Key;
			var mf = sStaticMeshGizmo.Value.Value;
			if (mf.sharedMesh==null)
			{
				continue;
			}
			for (int i = mr.subMeshStartIndex; i < Mathf.Min(mf.sharedMesh.subMeshCount, mr.sharedMaterials.Length); ++i)
			{
				Graphics.DrawMesh(mf.sharedMesh, matrix, mr.sharedMaterials[i], mr.gameObject.layer, cam, i);	
			}			
		}
		s_staticMeshGizmos.Clear();	

		foreach (var skinnedMeshgizmo in s_skinnedMeshGizmos)
		{
			var matrix = skinnedMeshgizmo.Key;
			var mr = skinnedMeshgizmo.Value;
			if (mr.sharedMesh == null)
			{
				continue;
			}
			for (int i = 0; i < Mathf.Min(mr.sharedMesh.subMeshCount, mr.sharedMaterials.Length); ++i)
			{
				Graphics.DrawMesh(mr.sharedMesh, matrix, mr.sharedMaterials[i], mr.gameObject.layer, cam, i);	
			}			
		}
		s_skinnedMeshGizmos.Clear();	
	}

	private static readonly HashSet<KeyValuePair<Matrix4x4, KeyValuePair<MeshRenderer, MeshFilter>>> s_staticMeshGizmos = new HashSet<KeyValuePair<Matrix4x4, KeyValuePair<MeshRenderer, MeshFilter>>>();
	private static readonly HashSet<KeyValuePair<Matrix4x4, SkinnedMeshRenderer>> s_skinnedMeshGizmos = new HashSet<KeyValuePair<Matrix4x4, SkinnedMeshRenderer>>();
	#endif

	/// <summary>
	/// Draw a model prefab as a selectable gizmo 
	/// </summary>
	public static void DrawModel(Animator animatorPrefab, Transform transform, bool drawIfPlaying = false)
	{
		if (animatorPrefab == null)
		{
			return;
		}
			
		DrawModel(animatorPrefab.gameObject, transform, drawIfPlaying);
	}

	/// <summary>
	/// Draw a model prefab as a selectable gizmo 
	/// </summary>
	public static void DrawModel(GameObject prefab, Transform transform, bool drawIfPlaying=false)
	{
#if UNITY_EDITOR
		if (prefab == null)
		{
			return;
		}

		if (Application.isPlaying && !drawIfPlaying)
		{
			return;
		}

		if (Event.current.type != EventType.Repaint && !Event.current.isMouse && !Event.current.isKey)
		{
			return;
		}

		var prevColor = Gizmos.color;
		Gizmos.color = Color.white;
		var prevMatrix = Gizmos.matrix;
		Gizmos.matrix = transform.localToWorldMatrix * prefab.transform.worldToLocalMatrix;
		foreach (var meshFilter in prefab.GetComponentsInChildren<MeshFilter>())
		{
			var mr = meshFilter.GetComponent<MeshRenderer>();
			if (mr == null || mr.enabled==false) continue;
			var localMatrix = Gizmos.matrix * Matrix4x4.TRS(meshFilter.transform.position, meshFilter.transform.rotation, meshFilter.transform.lossyScale);
			s_staticMeshGizmos.Add(new KeyValuePair<Matrix4x4, KeyValuePair<MeshRenderer, MeshFilter>>(localMatrix, new KeyValuePair<MeshRenderer, MeshFilter>(mr, meshFilter)));			

			if(Event.current.type == EventType.Repaint) continue;
			Gizmos.DrawMesh(meshFilter.sharedMesh, meshFilter.transform.position, meshFilter.transform.rotation, meshFilter.transform.lossyScale);
		}

		foreach (var smr in prefab.GetComponentsInChildren<SkinnedMeshRenderer>())
		{
			if (smr.enabled==false) continue;			
			var localMatrix = Gizmos.matrix * Matrix4x4.TRS(smr.transform.position, smr.transform.rotation, smr.transform.lossyScale);
			s_skinnedMeshGizmos.Add(new KeyValuePair<Matrix4x4, SkinnedMeshRenderer>(localMatrix, smr));			

			if(Event.current.type == EventType.Repaint) continue;
			Gizmos.DrawMesh(smr.sharedMesh, smr.transform.position, smr.transform.rotation, smr.transform.lossyScale);
		}
		Gizmos.matrix = prevMatrix;
		Gizmos.color = prevColor;
#endif
	}

}
