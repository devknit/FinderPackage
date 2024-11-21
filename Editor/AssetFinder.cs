
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using MDI.Editor;

namespace Finder
{
	public sealed partial class AssetFinder : MDIEditorWindow, IHasCustomMenu
	{
	#if false
		[MenuItem( "Tools/Finder/Open", priority = 1)]
		static void Open()
		{
			CreateNewWindow<AssetFinder>( null).Show();
		}
		[MenuItem( "Assets/Finder/Open", priority = 21)]
		static void OpenFinder()
		{
			CreateNewWindow<AssetFinder>( null).Show();
		}
	#endif
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
		[MenuItem("Assets/Finder/Select To Dependencies &f", true, priority = 22)]
		static bool IsFindToDependencies()
		{
			return Selection.assetGUIDs?.Length > 0;
		}
		[MenuItem("Assets/Finder/Select To Dependencies &f", false, priority = 22)]
		static void FindToDependencies()
		{
			var window = CreateNewWindow<AssetFinder>( null);
			window.Show();
			window.m_Contents.FindAssets( Selection.assetGUIDs, FindReference.Mode.ToDependencies);
		}
		[MenuItem("Assets/Finder/Select From Dependencies", true, priority = 23)]
		static bool IsFindFromDependencies()
		{
			return Selection.assetGUIDs?.Length > 0;
		}
		[MenuItem("Assets/Finder/Select From Dependencies", false, priority = 23)]
		static void FindFromDependencies()
		{
			var window = CreateNewWindow<AssetFinder>( null);
			window.Show();
			window.m_Contents.FindAssets( Selection.assetGUIDs, FindReference.Mode.FromDependencies);
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
				s_ActiveWindows = new List<AssetFinder>();
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
				var window = CreateNewWindow<AssetFinder>( null);
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
						if( m_Contents.RefindAssets() != false)
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
		[SubWindow( "Select", SubWindowIcon.Project)]
		void OnSelectGUI( Rect rect)
		{
			GUILayout.BeginArea( rect);
			{
				m_Contents.OnSelectGUI();
			}
			GUILayout.EndArea();
		}
		[SubWindow( "Dependent", SubWindowIcon.Search)]
		void OnDependentGUI( Rect rect)
		{
			GUILayout.BeginArea( rect);
			{
				m_Contents.OnDependentGUI();
			}
			GUILayout.EndArea();
		}
		static List<AssetFinder> s_ActiveWindows = null;
		
		[SerializeField]
		Contents m_Contents;
	}
}
