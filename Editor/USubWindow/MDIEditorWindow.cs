using System;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;
using EditorWinEx;
using EditorWinEx.Internal;
using EditorWinEx.Internal.Serialization;

public class MDIEditorWindow : EditorWindow, IMessageDispatcher
{
    public static T CreateWindow<T>( System.Object handle=null) where T : MDIEditorWindow
    {
        T window = MDIEditorWindow.GetWindow<T>();
        if( handle != null)
        {
            window.m_Handle = EWSerializationObject.CreateInstance( handle);
        }
        else
        {
            window.m_Handle = null;
        }
        window.Clear();
        window.Init();
        window.m_IsInitialized = true;
        return window;
    }
    public static T CreateWindow<T>( System.Type type, params System.Object[] args) where T : MDIEditorWindow
    {
        System.Object obj = null;
        if( type != null)
        {
            obj = System.Activator.CreateInstance( type, args);
        }
        return CreateWindow<T>( obj);
    }
	public static T CreateNewWindow<T>(System.Object handle=null) where T : MDIEditorWindow
	{
	#if UNITY_2019_2_OR_NEWER
		T window = EditorWindow.CreateWindow<T>();
	#else
		T window = EditorWindow.CreateInstance<T>();
	#endif	
		if( handle != null)
		{
            window.m_Handle = EWSerializationObject.CreateInstance( handle);
        }
        else
        {
            window.m_Handle = null;
        }
        window.Clear();
        window.Init();
        window.m_IsInitialized = true;
        return window;
	}
    void OnGUI()
    {
        bool guienable = GUI.enabled;
        
        if( m_MsgBox != null && m_MsgBox.IsShowing)
        {
            GUI.enabled = false;
        }
        else
        {
            GUI.enabled = true;
        }
        GUI.BeginGroup( new Rect( 0, 0, position.width, position.height), GUIStyleCache.GetStyle( "LODBlackBox"));
        OnDrawGUI();
        GUI.EndGroup();

        GUI.enabled = guienable;
        DrawMsgBox( new Rect( 0, 0, position.width, position.height));
    }
    public void ShowMsgBox( int id, System.Object obj)
    {
        if( m_MsgBox != null)
        {
            m_MsgBox.ShowMsgBox( id, obj);
        }
    }
    public void HideMsgBox()
    {
        if( m_MsgBox != null)
        {
            m_MsgBox.HideMsgBox();
        }
    }
    public void AddDynamicSubWindow( string title, string icon, Action<Rect> action)
    {
        AddDynamicSubWindowInternal( title, icon, EWSubWindowToolbarType.None, SubWindowHelpBoxType.None, action);
    }
    public void AddDynamicSubWindow( string title, EWSubWindowIcon icon, Action<Rect> action)
    {
        AddDynamicSubWindowInternal( title, icon, EWSubWindowToolbarType.None, SubWindowHelpBoxType.None, action);
    }
    public void AddDynamicSubWindowWithToolBar( string title, string icon, EWSubWindowToolbarType toolbar, Action<Rect, Rect> action)
    {
        if( toolbar != EWSubWindowToolbarType.None)
        {
        	AddDynamicSubWindowInternal( title, icon, toolbar, SubWindowHelpBoxType.None, action);
        }
    }
    public void AddDynamicSubWindowWithToolBar( string title, EWSubWindowIcon icon, EWSubWindowToolbarType toolbar, Action<Rect, Rect> action)
    {
        if( toolbar != EWSubWindowToolbarType.None)
        {
	        AddDynamicSubWindowInternal( title, icon, toolbar, SubWindowHelpBoxType.None, action);
	    }
    }
    public void AddDynamicSubWindowWithHelpBox( string title, string icon, SubWindowHelpBoxType helpBoxType, Action<Rect, Rect> action)
    {
        if( helpBoxType != SubWindowHelpBoxType.None)
        {
	        AddDynamicSubWindowInternal( title, icon, EWSubWindowToolbarType.None, helpBoxType, action);
	    }
    }
    public void AddDynamicSubWindowWithHelpBox( string title, EWSubWindowIcon icon, SubWindowHelpBoxType helpBoxType, Action<Rect, Rect> action)
    {
        if (helpBoxType != SubWindowHelpBoxType.None)
        {
	        AddDynamicSubWindowInternal(title, icon, EWSubWindowToolbarType.None, helpBoxType, action);
	    }
    }
    public void AddDynamicFullSubWindow( string title, string icon, EWSubWindowToolbarType toolbar, SubWindowHelpBoxType helpBoxType, Action<Rect, Rect, Rect> action)
    {
        if( helpBoxType != SubWindowHelpBoxType.None && toolbar == EWSubWindowToolbarType.None)
        {
	        AddDynamicSubWindowInternal( title, icon, toolbar, helpBoxType, action);
	    }
    }
    public void AddDynamicFullSubWindow( string title, EWSubWindowIcon icon, EWSubWindowToolbarType toolbar, SubWindowHelpBoxType helpBoxType, Action<Rect, Rect, Rect> action)
    {
        if( helpBoxType != SubWindowHelpBoxType.None && toolbar == EWSubWindowToolbarType.None)
        {
	        AddDynamicSubWindowInternal( title, icon, toolbar, helpBoxType, action);
	    }
    }
    public void AddDynamicSubWindow<T>( T drawer) where T : SubWindowCustomDrawer
    {
        if( drawer != null && m_WindowTree != null)
        {
            m_WindowTree.AddDynamicWindow( drawer);
        }
    }
    public void RemoveDynamicSubWindow( Action<Rect> action)
    {
        RemoveDynamicSubWindowInternal( action);
    }
    public void RemoveDynamicSubWindow( Action<Rect, Rect> action)
    {
        RemoveDynamicSubWindowInternal( action);
    }
    public void RemoveDynamicSubWindow( Action<Rect, Rect, Rect> action)
    {
        RemoveDynamicSubWindowInternal( action);
    }
    public bool RemoveDynamicSubWindow<T>( T drawer)where T : SubWindowCustomDrawer
    {
        if( drawer != null && m_WindowTree != null)
        {
            return m_WindowTree.RemoveDynamicWindow( drawer);
        }
        return false;
    }
    public void RemoveAllDynamicSubWindow()
    {
        if( m_WindowTree != null)
        {
            m_WindowTree.RemoveAllDynamicWindow();
        }
    }
    protected virtual void OnEnable()
    {
        if( m_IsInitialized != false)
        {
            Init();
        }
    }
    protected virtual void OnProjectChange()
    {
        Init();
    }
    protected virtual void OnDisable()
    {
        if( m_WindowTree != null)
        {
            m_WindowTree.Disable();
        }
        if( m_ToolbarTree != null)
        {
            m_ToolbarTree.Disable();
        }
        if( m_MsgBox != null)
        {
            m_MsgBox.Disable();
        }
        SaveHandle();
    }
    protected virtual void OnDestroy()
    {
        if( m_WindowTree != null)
        {
            m_WindowTree.Destroy();
            m_WindowTree = null;
        }
        if( m_ToolbarTree != null)
        {
            m_ToolbarTree.Destroy();
            m_ToolbarTree = null;
        }
        if( m_MsgBox != null)
        {
            m_MsgBox.Destroy();
            m_MsgBox = null;
        }
        ClearHandle();
    }
    protected virtual void Clear()
    {
        if( m_WindowTree != null)
        {
            m_WindowTree.Destroy();
            m_WindowTree = null;
        }
        if( m_ToolbarTree != null)
        {
            m_ToolbarTree.Destroy();
            m_ToolbarTree = null;
        }
        if( m_MsgBox != null)
        {
            m_MsgBox.Destroy();
            m_MsgBox = null;
        }
    }
    protected virtual void Init()
    {
        LoadHandle();
        
        if( m_WindowTree == null)
        {
            if( Handle != null)
            {
                m_WindowTree = new SubWindowTree( Repaint, GetType().Name, Handle.GetType().Name);
            }
            else
            {
                m_WindowTree = new SubWindowTree( Repaint, GetType().Name, null);
            }
        }
        if( m_ToolbarTree == null)
        {
            m_ToolbarTree = new ToolBarTree();
        }
        if( m_MsgBox == null)
        {
            m_MsgBox = new EditorWindowMsgBox();
        }
        Type[] handleTypes = null;
        System.Object[] handles = null;
        if( Handle != null)
        {
            handleTypes = new Type[]{ Handle.GetType(), GetType() };
            handles = new object[]{ Handle, this };
        }
        else
        {
            handleTypes = new Type[]{ GetType() };
            handles = new object[]{ this };
        }
        EditorWindowComponentsInitializer.InitComponents( this, handleTypes, handles, m_WindowTree, m_ToolbarTree, m_MsgBox);
    }
    protected virtual void OnDrawGUI()
    {
        DrawWindowTree( new Rect( 0, 17, position.width, position.height - 17));
        DrawToolbar( new Rect( 0, 0, position.width, 17));
    }
    protected void DrawWindowTree( Rect rect)
    {
        if( m_WindowTree != null)
        {
            m_WindowTree.DrawWindowTree( rect);
        }
    }
    protected void DrawToolbar( Rect rect)
    {
        GUI.Box( rect, string.Empty, GUIStyleCache.GetStyle( "Toolbar"));
        if( m_WindowTree != null)
        {
            m_WindowTree.DrawLayoutButton( new Rect(rect.x + rect.width - 80, rect.y, 70, rect.height));
        }
        GUILayout.BeginArea( new Rect(rect.x + 6, rect.y, rect.width - 80, rect.height));
        GUILayout.BeginHorizontal();
        if( m_ToolbarTree != null)
        {
            m_ToolbarTree.DrawToolbar();
        }
        if( m_WindowTree != null)
        {
            m_WindowTree.DrawViewButton( new Rect( rect.x + rect.width - (80 + 76), rect.y, 70, rect.height));
        }
        OnDrawToolBar();
        GUILayout.EndHorizontal();
        GUILayout.EndArea();
    }
    protected void DrawMsgBox(Rect rect)
    {
        if( m_MsgBox != null)
        {
        	m_MsgBox.DrawMsgBox( rect);
        }
    }
    protected virtual void OnDrawToolBar()
    {
    }
    private void AddDynamicSubWindowInternal( string title, EWSubWindowIcon icon, EWSubWindowToolbarType toolbar, SubWindowHelpBoxType helpbox, Delegate action)
    {
        AddDynamicSubWindowInternal( title, GUIEx.GetIconPath( icon), toolbar, helpbox, action);
    }
    private void AddDynamicSubWindowInternal( string title, string icon, EWSubWindowToolbarType toolbar, SubWindowHelpBoxType helpbox, Delegate action)
    {
        if( m_WindowTree != null)
        {
            m_WindowTree.AddDynamicWindow( title, icon, toolbar, helpbox, action);
        }
    }
    private bool RemoveDynamicSubWindowInternal( Delegate action)
    {
        if( m_WindowTree != null)
        {
            return m_WindowTree.RemoveDynamicWindow( action);
        }
        return false;
    }
    public Type GetContainerType()
    {
        return GetType();
    }
    private void SaveHandle()
    {
        if( this.m_Handle != null)
        {
	        string id = GetType().FullName;
	        m_Handle.SaveObject( id);
	    }
    }
    private void LoadHandle()
    {
        if( this.m_Handle != null)
        {
	        string id = GetType().FullName;
	        this.m_Handle.LoadObject( id);
	    }
    }
    private void ClearHandle()
    {
        if( this.m_Handle != null)
        {
	        string id = GetType().FullName;
	        this.m_Handle.ClearObject(id);
	    }
    }
    protected System.Object Handle
    {
        get
        {
            if( m_Handle == null)
            {
                return null;
            }
            return m_Handle.Obj;
        }
    }
    
    [SerializeField]
    EWSerializationObject m_Handle;

    SubWindowTree m_WindowTree;
    ToolBarTree m_ToolbarTree;
    EditorWindowMsgBox m_MsgBox;
    bool m_IsInitialized;
}
