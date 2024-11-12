
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using MDI.Editor;

namespace Finder
{
	public sealed partial class Finder : MDIEditorWindow, IHasCustomMenu
	{
		[MenuItem( "Tools/Finder/Open", priority = 1)]
		static void Open()
		{
			CreateNewWindow<Finder>( null).Show();
		}
		[MenuItem( "Tools/Finder/Closes", priority = 2)]
		static void Closes()
		{
			if( s_ActiveWindows != null)
			{
				var windows = s_ActiveWindows.ToArray();
				
				for( int i0 = 0; i0 < windows.Length; ++i0)
				{
					windows[ i0].Close();
				}
				s_ActiveWindows = null;
			}
		}
		[MenuItem( "Assets/Finder/Open &f", priority = 21)]
		static void OpenFinder()
		{
			CreateNewWindow<Finder>( null).Show();
		}
		[MenuItem("Assets/Finder/Trace Dependents", true, priority = 22)]
		static bool IsTraceDependents()
		{
			return Selection.assetGUIDs?.Length > 0;
		}
		[MenuItem("Assets/Finder/Trace Dependents", false, priority = 22)]
		static void TraceDependents()
		{
			var window = CreateNewWindow<Finder>( null);
			window.Show();
			window.m_Contents.SearchAssets( Selection.assetGUIDs, SearchType.kTraceDependents);
		}
		[MenuItem("Assets/Finder/Trace Precedents", true, priority = 23)]
		static bool IsTracePrecedents()
		{
			return Selection.assetGUIDs?.Length > 0;
		}
		[MenuItem("Assets/Finder/Trace Precedents", false, priority = 23)]
		static void TracePrecedents()
		{
			var window = CreateNewWindow<Finder>( null);
			window.Show();
			window.m_Contents.SearchAssets( Selection.assetGUIDs, SearchType.kTracePrecedents);
		}
		[MenuItem("Assets/Finder/Check Missing", false, priority = 24)]
		static void CheckMissing()
		{
			var window = CreateNewWindow<Finder>( null);
			window.Show();
			window.m_Contents.SearchAssets( Selection.assetGUIDs, SearchType.kCheckMissing);
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
			m_Contents.OnProjectChange();
		}
		protected override void OnEnable()
		{
			if( s_ActiveWindows == null)
			{
				s_ActiveWindows = new List<Finder>();
			}
			if( s_ActiveWindows.Contains( this) == false)
			{
				s_ActiveWindows.Add( this);
			}
			base.OnEnable();
			
			if( m_Contents == null)
			{
				m_Contents = new Contents();
			}
			m_Contents.OnEnable( () =>
			{
				var window = CreateNewWindow<Finder>( null);
				var windowPosition = position;
				windowPosition.x += 32;
				windowPosition.y += 32;
				window.position = windowPosition;
				window.Show();
				return window.m_Contents;
			});
		}
		protected override void OnDisable()
		{
			if( s_ActiveWindows != null)
			{
				if( s_ActiveWindows.Contains( this) != false)
				{
					s_ActiveWindows.Remove( this);
				}
			}
			base.OnDisable();
			
			if( m_Contents != null)
			{
				m_Contents.OnDisable();
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
						if( m_Contents.ResearchAssets() != false)
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
			m_Contents.OnToolbarGUI();
		}
		[SubWindow( "Project", SubWindowIcon.Project, false)]
		void OnProjectGUI( Rect rect)
		{
			GUILayout.BeginArea( rect);
			{
				m_Contents.OnProjectGUI();
			}
			GUILayout.EndArea();
		}
		[SubWindow( "Target", SubWindowIcon.Project)]
		void OnTargetGUI( Rect rect)
		{
			GUILayout.BeginArea( rect);
			{
				m_Contents.OnTargetGUI();
			}
			GUILayout.EndArea();
		}
		[SubWindow( "Search", SubWindowIcon.Search)]
		void OnSearchGUI( Rect rect)
		{
			GUILayout.BeginArea( rect);
			{
				m_Contents.OnSearchGUI();
			}
			GUILayout.EndArea();
		}
		static List<Finder> s_ActiveWindows = null;
		
		[SerializeField]
		Contents m_Contents;
	}
}
