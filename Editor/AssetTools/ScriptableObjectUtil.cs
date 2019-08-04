using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;

public static class ScriptableObjectUtil
{
	class CreateAssetPopupLookup<TBaseType> where TBaseType:ScriptableObject
	{
		public System.Type[] types;
		public string[] names;
		private static readonly Dictionary<System.Type, CreateAssetPopupLookup<TBaseType>> s_cloneTypes = new Dictionary<System.Type, CreateAssetPopupLookup<TBaseType>>();

		public static CreateAssetPopupLookup<TBaseType> Get()
		{
			return Get(typeof(TBaseType));
		}

		public static CreateAssetPopupLookup<TBaseType> Get(System.Type type)
		{
			CreateAssetPopupLookup<TBaseType> output = null;
			if (!s_cloneTypes.TryGetValue(type, out output))
			{
				var _types = new System.Type[] {null,null}.Concat(System.AppDomain.CurrentDomain.GetAssemblies()				
						.SelectMany(x => x.GetTypes())
						.Where(t => !t.IsAbstract && type.IsAssignableFrom(t)))
					.ToArray();

				var _names = new[] { "New "+type.Name, "{NULL}" }
					.Concat(_types.Skip(2).Select(x => x.Name))
					.ToArray();

				output = s_cloneTypes[type] = new CreateAssetPopupLookup<TBaseType>()
				{
					types = _types,
					names = _names,
				};
			}
			return output;
		}
	}

	public static void CreateAssetPopup<TBaseType>(string baseDir, System.Action<TBaseType> onCreate)  where TBaseType:ScriptableObject
	{
		var types = CreateAssetPopupLookup<TBaseType>.Get();

		var result = EditorGUILayout.Popup(0, types.names);
		if (result == 0)
		{
			return;
		}

		var cloneType = types.types[result];
		if (cloneType!=null)
		{
			ModalTextboxWindow.Create("New "+cloneType.Name, saveName=>
				{					
					var assetObj = ScriptableObject.CreateInstance(cloneType);
					var assetPath = AssetDatabase.GenerateUniqueAssetPath(Path.Combine(baseDir, saveName + ".asset"));
					Directory.CreateDirectory(baseDir);
					AssetDatabase.CreateAsset(assetObj,assetPath);
					var asset = (TBaseType)AssetDatabase.LoadAssetAtPath(assetPath, cloneType);
					onCreate?.Invoke(asset);
				},typeof(TBaseType).Name+"_" + cloneType.Name, ScriptAssetUtil.GetIconForType(cloneType));
		}
		else
		{
			onCreate?.Invoke(null);
		}
	}
}
#endif