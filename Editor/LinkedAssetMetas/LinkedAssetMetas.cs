#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.VersionControl;
using Object = UnityEngine.Object;
using ListType = System.Collections.Generic.List<LinkedAssetMetas.MetaGuid>;

[InitializeOnLoad]
public static class LinkedAssetMetas  
{
	[Serializable]
	public class UserData
	{
		public ListType fromLinks = new ListType();
		public ListType toLinks = new ListType();

		public static UserData Load(string assetPath)
		{
			UserData userData = null;
			try
			{
				AssetImporter importer = File.Exists(assetPath) 
                                        ? AssetImporter.GetAtPath(assetPath)
                                        : null;
				if (importer == null)
				{
					return new UserData();
				}

				userData = JsonUtility.FromJson<UserData>(importer.userData);
			}
			catch (ArgumentException e)
			{
				Debug.LogWarning($"LinkedAssetMetas: {assetPath} meta file json data corrupt.");
			}

			if (userData == null)
			{
				userData = new UserData();
			}

			return userData;
		}

		public void Save(string assetPath)
		{
			if (string.IsNullOrEmpty(assetPath))
			{
				return;
			}
			
            AssetImporter importer = File.Exists(assetPath) 
                ? AssetImporter.GetAtPath(assetPath)
                : null;
		    if (importer == null)
		    {
		        return;
		    }
		
			// check if a change has occured. Don't update if we just have a different order of the same values
			var prevData = Load(assetPath);
			if (prevData !=null 
				&& new HashSet<MetaGuid>(prevData.fromLinks).SetEquals(fromLinks)
			    && new HashSet<MetaGuid>(prevData.toLinks).SetEquals(toLinks))
			{
				return;
			}

			fromLinks = new ListType(new HashSet<MetaGuid>(fromLinks));
			toLinks = new ListType(new HashSet<MetaGuid>(toLinks));

			importer.userData = JsonUtility.ToJson(this);
			AssetDatabase.WriteImportSettingsIfDirty(assetPath);
		}
	}

	public static void AddLink(string fromAssetPath, string toAssetPath)
	{
		if (string.IsNullOrEmpty(fromAssetPath) || string.IsNullOrEmpty(toAssetPath))
		{
			return;
		}

		var fromGuid = AssetDatabase.AssetPathToGUID(fromAssetPath);
		var toGuid = AssetDatabase.AssetPathToGUID(toAssetPath);
		if (fromGuid == toGuid)
		{
			return; // we don't store cyclic links. There's no point
		}

		var wasAdded = false;

		var toUserData = UserData.Load(toAssetPath);
		if (!toUserData.fromLinks.Contains(fromGuid))
		{
			toUserData.fromLinks.Add(fromGuid);
			wasAdded = true;
		}
		toUserData.Save(toAssetPath);

		var fromUserData = UserData.Load(fromAssetPath);
		if (!fromUserData.toLinks.Contains(toGuid))
		{
			fromUserData.toLinks.Add(toGuid);
			wasAdded = true;
		}
		fromUserData.Save(fromAssetPath);

		if(wasAdded)
		{
			Debug.LogFormat("Linked Assets: (Added) {0} is now linked to {1}", 
				Path.GetFileName(fromAssetPath), 
				Path.GetFileName(toAssetPath));
		}
	}

	private static void CheckForBrokenLinksFrom(string fromAssetPath)
	{
		if (string.IsNullOrEmpty(fromAssetPath))
		{
			return;
		}

		var fromUserData = UserData.Load(fromAssetPath);
		if (fromUserData.toLinks.Count == 0)
		{
			return;
		}

		var unlinkedToGuids = new HashSet<MetaGuid>(fromUserData.toLinks);
		foreach (var fromAsset in AssetDatabase.LoadAllAssetsAtPath(fromAssetPath))
		{
			var so = new SerializedObject(fromAsset);
			var iter = so.GetIterator();
			while (iter.NextVisible(true))
			{
				if (iter.propertyType != SerializedPropertyType.ObjectReference || iter.objectReferenceValue == null)
				{
					continue;
				}

				var refGUID = string.Intern(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(iter.objectReferenceValue)));
				unlinkedToGuids.Remove(refGUID);
			}
		}
		
		foreach (var unlinkedToGuid in unlinkedToGuids)
		{
			RemoveLinkGUID(AssetDatabase.AssetPathToGUID(fromAssetPath), unlinkedToGuid);
		}
	}

	public static void RemoveLink(string fromAssetPath, string toAssetPath)
	{
		if (string.IsNullOrEmpty(fromAssetPath) || string.IsNullOrEmpty(toAssetPath))
		{
			return;
		}

		RemoveLinkGUID(AssetDatabase.AssetPathToGUID(fromAssetPath), AssetDatabase.AssetPathToGUID(toAssetPath));
	}

	private static void RemoveLinkGUID(string fromGuid, string toGuid)
	{
		var wasRemoved = false;

		var fromAssetPath = AssetDatabase.GUIDToAssetPath(fromGuid);
		var toAssetPath = AssetDatabase.GUIDToAssetPath(toGuid);

		if (!string.IsNullOrEmpty(toAssetPath))
		{
			var toUserData = UserData.Load(toAssetPath);
			wasRemoved |= toUserData.fromLinks.Remove(fromGuid);
			toUserData.Save(toAssetPath);
		}

		if (!string.IsNullOrEmpty(fromAssetPath))
		{
			var fromUserData = UserData.Load(fromAssetPath);
			wasRemoved |= fromUserData.toLinks.Remove(toGuid);
			fromUserData.Save(fromAssetPath);			
		}

		if (wasRemoved)
		{
			Debug.LogFormat("Linked Assets: (Removed) {0} is no longer linked to {1}", 
				!string.IsNullOrEmpty(toAssetPath) ? Path.GetFileName(fromAssetPath):fromGuid, 
				!string.IsNullOrEmpty(toAssetPath) ? Path.GetFileName(toAssetPath):toGuid);
		}
	}

	public static void RemoveAllLinksTo(string toAssetPath)
	{
		if (string.IsNullOrEmpty(toAssetPath))
		{
			return;
		}

		var toGuid = AssetDatabase.AssetPathToGUID(toAssetPath);

		var toUserData = UserData.Load(toAssetPath);
		foreach (var fromGuid in toUserData.fromLinks)
		{
			var fromAssetPath = AssetDatabase.GUIDToAssetPath(fromGuid);
			var fromUserData = UserData.Load(fromAssetPath);
			var wasRemoved = fromUserData.toLinks.Remove(toGuid);
			fromUserData.Save(fromAssetPath);

			if (wasRemoved)
			{
				Debug.LogFormat("Linked Assets: (Removed) {0} is no longer linked to {1}", 
					Path.GetFileName(fromAssetPath), 
					Path.GetFileName(toAssetPath));
			}
		}
		toUserData.fromLinks.Clear();
		toUserData.Save(toAssetPath);
	}

	private static void MarkLinkedAssetsDirty(string toAssetPath)
	{
		if (string.IsNullOrEmpty(toAssetPath))
		{
			return;
		}		

		var toGuid = AssetDatabase.AssetPathToGUID(toAssetPath);
		var userData = UserData.Load(toAssetPath);
		var unlinkedGuids = new HashSet<MetaGuid>(userData.fromLinks);
		foreach (var _fromGuid in userData.fromLinks)
		{
			var fromGuid = string.Intern(_fromGuid); // intern to speed up checking
			if (fromGuid == toGuid) 
			{ 
				continue; 
			}

			var fromAssetPath = AssetDatabase.GUIDToAssetPath(fromGuid);

			// check that fromAsset still has an asset referencing a part of toGUID. 			
			// if no reference exists, we remove the links
			foreach (var fromAsset in AssetDatabase.LoadAllAssetsAtPath(fromAssetPath))
			{
				var fromSO = new SerializedObject(fromAsset);
				var fromIter = fromSO.GetIterator();
				while (fromIter.NextVisible(true))
				{
					if (fromIter.propertyType != SerializedPropertyType.ObjectReference || fromIter.objectReferenceValue == null)
					{
						continue;
					}

					var refToGUID = string.Intern(AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(fromIter.objectReferenceValue)));
					if (refToGUID == toGuid)
					{
						// reference exists
						unlinkedGuids.Remove(fromGuid);
						Debug.LogFormat("Linked Assets: (Reimported) {0} because it contains {1} which is linked to:{2}",
							Path.GetFileName(fromAssetPath), fromAsset.GetType().Name, 
							Path.GetFileName(toAssetPath));
						EditorUtility.SetDirty(fromAsset);
					}
				}
			}
		}

		foreach (var unlinkedFromGuid in unlinkedGuids)
		{
			RemoveLinkGUID(unlinkedFromGuid, toGuid);
		}

		AssetDatabase.SaveAssets();
	}

	public class Processor : UnityEditor.AssetPostprocessor
	{
		public override int GetPostprocessOrder()
		{
			return int.MaxValue;
		}

		static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets,
			string[] movedFromAssetPaths)
		{
			foreach (var movedAsset in movedAssets)
			{
				MarkLinkedAssetsDirty(movedAsset);
			}

			foreach (var importedAsset in importedAssets)
			{
				CheckForBrokenLinksFrom(importedAsset);
			}
			
			foreach (var deletedAsset in deletedAssets)
			{
				MarkLinkedAssetsDirty(deletedAsset);
				RemoveAllLinksTo(deletedAsset);
			}
		}	
	}
	
	[Serializable]
	public struct MetaGuid : IEquatable<MetaGuid>, IEquatable<string>
	{
		public string guid;
		public string name;

		public MetaGuid(string _guid) : this()
		{
			guid = _guid;
			name = Path.GetFileName(AssetDatabase.GUIDToAssetPath(_guid));
		}

		public static implicit operator MetaGuid(string guid)
		{
			return new MetaGuid(guid);
		}

		public static implicit operator string(MetaGuid meta)
		{
			return meta.guid;
		}

		public bool Equals(MetaGuid other)
		{
			return string.Equals(guid, other.guid);
		}

		public bool Equals(string other)
		{
			return guid.Equals(other);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			return obj is MetaGuid && Equals((MetaGuid) obj);
		}

		public override int GetHashCode()
		{
			return (guid != null ? guid.GetHashCode() : 0);
		}
	}
}
#endif