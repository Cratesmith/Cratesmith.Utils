#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEngine;
using UnityEngine.Events;

public class ModalTextboxWindow : EditorWindow
{
    [SerializeField] private string                 value;
    [SerializeField] private UnityAction<string>    action;
    [SerializeField] private static GUIContent      titleText;
    [SerializeField] private bool                   initalized;
    [SerializeField] private int                    updateCount;
    [SerializeField] private Texture                icon;

    public static void Create(string title, UnityAction<string> action, string defaultValue = "", Texture icon=null)
    {
        var wnd = CreateInstance<ModalTextboxWindow>();
        titleText = new GUIContent(title);
        wnd.value = defaultValue;
        wnd.action = action;
        wnd.icon = icon;
        wnd.initalized = false;
        wnd.minSize = wnd.maxSize = new Vector2(250, 70);
        wnd.ShowAsDropDown(Rect.zero, wnd.minSize);
        wnd.updateCount = 0;
    }

    void OnGUI()
    {
        if (!initalized)
        {            
            if (Event.current!=null && Event.current.keyCode!=KeyCode.Return)
            {
                updateCount = 0;
                position = new Rect(GUIUtility.GUIToScreenPoint(Event.current.mousePosition), minSize);
                initalized = true;
                Repaint();
                Focus();
            }            
        }

        var cancelled = false;
        var confirmed = false;

        using (new EditorGUILayout.VerticalScope("box"))
        {
            using (new EditorGUILayout.HorizontalScope())
            {
                if (icon != null)
                {
                    GUILayout.Label(icon, GUILayout.Height(16), GUILayout.Width(16*(icon.width/icon.height)), GUILayout.ExpandWidth(false));
                }
                GUILayout.Label(titleText);                
            }

            GUI.SetNextControlName("input");
            value = EditorGUILayout.TextField("", value);

            EditorGUI.FocusTextInControl("input");

            using (new EditorGUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();

                if (GUILayout.Button("Cancel", GUILayout.Width(60))
                    || Event.current.keyCode == KeyCode.Escape)
                {
                    cancelled = true;
                }

                GUILayout.Space(5);

                if (GUILayout.Button("Ok", GUILayout.Width(60))
                    || Event.current.keyCode == KeyCode.Return)
                {
                    confirmed = true;
                }
            }
        }

        if(updateCount > 30)
        {
            if (cancelled)
            {
                Close();
            }
            else if (confirmed)
            {
                action.Invoke(value);
                Close();
            }
        }
    }

    void OnEnable()
    {
        EditorApplication.update += ForceFocus;
        EditorApplication.LockReloadAssemblies();        
    }

	private void Update()
	{
        updateCount++;
	}

	private void ForceFocus()
    {
        if (action == null)
        {
            Close();
            return;
        }

        if (focusedWindow != this)
        {
            Focus();
        }
    }

    void OnDisable()
    {
        EditorApplication.update -= ForceFocus;        
        UnityEditor.EditorApplication.UnlockReloadAssemblies();
    }
}
#endif