using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneExtensions
{
	private static Scene s_dontDestroyScene;

	public static bool IsDontDestroy(this Scene @this)
	{
		if (!s_dontDestroyScene.IsValid())
		{
			var helperGO = new GameObject();
			Object.DontDestroyOnLoad(helperGO);
			s_dontDestroyScene = helperGO.scene;
			Object.DestroyImmediate(helperGO);
		}

		return @this == s_dontDestroyScene;		
	}

	public static GameObject CreateGameObject(this Scene @this, string name="GameObject")
	{
		var go = new GameObject(name);
		if (!@this.IsDontDestroy())
		{
			SceneManager.MoveGameObjectToScene(go, @this);
		}
		else
		{
			GameObject.DontDestroyOnLoad(go);
		}
		return go;
	}
}