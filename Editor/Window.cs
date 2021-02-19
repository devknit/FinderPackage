
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;

namespace Finder {

public sealed partial class Window : MDIEditorWindow, IHasCustomMenu
{
	[MenuItem( "Tools/Finder/Open")]
	static void Open()
	{
		CreateNewWindow<Window>().Show();
	}
	[MenuItem( "Tools/Finder/Closes")]
	static void Closes()
	{
		if( activeWindows != null)
		{
			var windows = activeWindows.ToArray();
			
			for( int i0 = 0; i0 < windows.Length; ++i0)
			{
				windows[ i0].Close();
			}
			activeWindows = null;
		}
	}
	[MenuItem( "Assets/Assets/Finder/Open &f")]
	static void OpenFinder()
	{
		CreateNewWindow<Window>().Show();
	}
	[MenuItem("Assets/Assets/Finder/Check Missing", false)]
	static void CheckMissing()
	{
		var window = CreateNewWindow<Window>();
		window.Show();
		window.contents.SearchAssets( Selection.assetGUIDs, SearchType.kCheckMissing);
	}
	[MenuItem("Assets/Assets/Finder/Trace Precedents", false)]
	static void TracePrecedents()
	{
		var window = CreateNewWindow<Window>();
		window.Show();
		window.contents.SearchAssets( Selection.assetGUIDs, SearchType.kTracePrecedents);
	}
	[MenuItem("Assets/Assets/Finder/Trace Dependents", false)]
	static void TraceDependents()
	{
		var window = CreateNewWindow<Window>();
		window.Show();
		window.contents.SearchAssets( Selection.assetGUIDs, SearchType.kTraceDependents);
	}
	[MenuItem("Assets/Assets/Finder/Trace Precedents", true)]
	static bool IsTracePrecedents()
	{
		return Selection.assetGUIDs?.Length > 0;
	}
	[MenuItem("Assets/Assets/Finder/Trace Dependents", true)]
	static bool IsTraceDependents()
	{
		return Selection.assetGUIDs?.Length > 0;
	}
	
	public void AddItemsToMenu( GenericMenu menu)
	{
		menu.AddItem
		(
			new GUIContent( "Close Tabs"),
			false,
			() => { Closes(); }
		);
	}
	protected override void OnProjectChange()
	{
		base.OnProjectChange();
		contents.OnProjectChange();
	}
	protected override void OnEnable()
	{
		if( activeWindows == null)
		{
			activeWindows = new List<Window>();
		}
		if( activeWindows.Contains( this) == false)
		{
			activeWindows.Add( this);
		}
		base.OnEnable();
		
		if( contents == null)
		{
			contents = new Contents();
		}
		contents.OnEnable( () =>
		{
			var window = CreateNewWindow<Window>();
			var windowPosition = position;
			windowPosition.x += 32;
			windowPosition.y += 32;
			window.position = windowPosition;
			window.Show();
			return window.contents;
		});
	}
	protected override void OnDisable()
	{
		if( activeWindows != null)
		{
			if( activeWindows.Contains( this) != false)
			{
				activeWindows.Remove( this);
			}
		}
		base.OnDisable();
		
		if( contents != null)
		{
			contents.OnDisable();
		}
	}
	protected override void OnDrawGUI()
	{
		base.OnDrawGUI();
		
		Event ev = Event.current;
		
		if( ev.type == EventType.KeyDown)
		{
			switch( ev.keyCode)
			{
				case KeyCode.Escape:
				{
					Close();
					ev.Use();
					break;
				}
				case KeyCode.F5:
				{
					if( contents.ResearchAssets() != false)
					{
						ev.Use();
						Repaint();
					}
					break;
				}
				case KeyCode.F:
				{
					ev.Use();
					break;
				}
			}
		}
	}
	protected override void OnDrawToolBar()
	{
		contents.OnToolbarGUI();
	}
	[EWSubWindow( "Project", EWSubWindowIcon.Project)]
	void OnProjectGUI( Rect rect)
	{
		GUILayout.BeginArea( rect);
		{
			contents.OnProjectGUI();
		}
		GUILayout.EndArea();
	}
	[EWSubWindow( "Target", EWSubWindowIcon.Project)]
	void OnTargetGUI( Rect rect)
	{
		GUILayout.BeginArea( rect);
		{
			contents.OnTargetGUI();
		}
		GUILayout.EndArea();
	}
	[EWSubWindow( "Search", EWSubWindowIcon.Search)]
	void OnSearchGUI( Rect rect)
	{
		GUILayout.BeginArea( rect);
		{
			contents.OnSearchGUI();
		}
		GUILayout.EndArea();
	}
	
	static List<Window> activeWindows = null;
	
	[SerializeField]
	Contents contents;
}

} /* namespace Finder */
