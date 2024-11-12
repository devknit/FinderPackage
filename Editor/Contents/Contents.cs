
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Finder
{
	public enum SearchType
	{
		kNone,
		kTraceDependents,
		kTracePrecedents,
	}
	[System.Serializable]
	public sealed class Contents
	{
		public void OnEnable( System.Func<Contents> onCreateWindowContens)
		{
			m_ChangeProject = true;
			m_OnCreateWindowContens = onCreateWindowContens;
			
			if( m_Project != null)
			{
				m_Project.OnEnable( m_ClickType);
				m_Project.ColumnHeaderResizeToFit();
			}
			if( m_Target != null)
			{
				m_Target.OnEnable( m_ClickType);
				m_Target.ColumnHeaderResizeToFit();
			}
			if( m_Search != null)
			{
				m_Search.OnEnable( m_ClickType);
				m_Search.ColumnHeaderResizeToFit();
			}
		}
		public void OnDisable()
		{
			if( m_Project != null)
			{
				m_Project.OnDisable();
			}
			if( m_Target != null)
			{
				m_Target.OnDisable();
			}
			if( m_Search != null)
			{
				m_Search.OnDisable();
			}
		}
		public void OnToolbarGUI()
		{
			var newClickType = (ClickType)EditorGUILayout.Popup( (int)m_ClickType, kClickTypes, EditorStyles.toolbarPopup, GUILayout.Width( 120));
			if( m_ClickType != newClickType)
			{
				if( m_Project != null)
				{
					m_Project.SetClickType( newClickType);
				}
				if( m_Target != null)
				{
					m_Target.SetClickType( newClickType);
				}
				if( m_Search != null)
				{
					m_Search.SetClickType( newClickType);
				}
				m_ClickType = newClickType;
			}
			if( m_SearchType != SearchType.kNone)
			{
				using( new EditorGUILayout.HorizontalScope( EditorStyles.toolbar))
				{
					var newSearchType = (SearchType)EditorGUILayout.Popup( (int)m_SearchType, kSearchTypes, EditorStyles.toolbarPopup, GUILayout.Width( 140));
					if( m_SearchType != newSearchType)
					{
						m_SearchType = newSearchType;
						ResearchAssets();
					}
					bool newRecursive = EditorGUILayout.ToggleLeft( "Recursive", m_Recursive, GUILayout.Width( 80));
					
					if( m_Recursive != newRecursive)
					{
						m_Recursive = newRecursive;
						ResearchAssets();
					}
					GUILayout.FlexibleSpace();
				}
			}
		}
		public void OnProjectChange()
		{
			m_ChangeProject = true;
		}
		public void OnProjectGUI()
		{
			if( m_Project == null)
			{
				m_Project = new Explorer( GetAllAssetElements(), View.Column.kName);
				m_Project.OnEnable( m_ClickType);
				m_Project.ColumnHeaderResizeToFit();
			}
			else if( m_ChangeProject != false)
			{
				m_Project.Apply( GetAllAssetElements());
			}
			m_ChangeProject = false;
			m_Project.OnGUI( this);
		}
		public void OnTargetGUI()
		{
			OnSearchToolbarGUI();
			
			if( m_Target == null)
			{
				m_Target = new Explorer( new List<Element>(), View.Column.kName | View.Column.kReference | View.Column.kMissing);
				m_Target.OnEnable( m_ClickType);
				m_Target.ColumnHeaderResizeToFit();
			}
			m_Target.OnGUI( this);
		}
		public void OnSearchGUI()
		{
			OnSearchToolbarGUI();
			
			if( m_Search == null)
			{
				m_Search = new Explorer( new List<Element>(), View.Column.kDefault);
				m_Search.OnEnable( m_ClickType);
				m_Search.ColumnHeaderResizeToFit();
			}
			m_Search.OnGUI( this);
		}
		public void OnSearchToolbarGUI()
		{
		}
		List<Element> GetAllAssetElements()
		{
			EditorUtility.DisplayProgressBar( "Enumerating Assets", "", 0);
			var paths = AssetDatabase.GetAllAssetPaths();
			var builder = new ElementBuilder();
			
			if( paths != null)
			{
				for( int i0 = 0; i0 < paths.Length; ++i0)
				{
					EditorUtility.DisplayProgressBar( "Enumerating Assets", paths[ i0], i0 / (float)paths.Length);
					builder.Append( paths[ i0]);
				}
			}
			EditorUtility.DisplayProgressBar( "Enumerating Assets", "Done", 1);
			EditorUtility.ClearProgressBar();
			return builder.ToList();
		}
		public void OpenSearchAssets( IEnumerable<string> assetGuids, SearchType newSearchType)
		{
			Contents contents = m_OnCreateWindowContens?.Invoke();
			if( contents != null)
			{
				contents.SearchAssets( assetGuids, newSearchType);
			}
			
		}
		public void SearchAssets( IEnumerable<string> assetGuids, SearchType newSearchType)
		{
			Dictionary<string, ElementSource> targets;
			Dictionary<string, ElementSource> searches;
			ElementBuilder builder;
			
			switch( newSearchType)
			{
				case SearchType.kTracePrecedents:
				{
					Search.TracePrecedents( assetGuids, m_Recursive, out targets, out searches);
					break;
				}
				case SearchType.kTraceDependents:
				{
					Search.TraceDependents( assetGuids, m_Recursive, out targets, out searches);
					break;
				}
				default:
				{
					return;
				}
			}
			builder = new ElementBuilder();
			foreach( var found in searches)
			{
				builder.Append( found.Value);
			}
			if( m_Search == null)
			{
				m_Search = new Explorer( new List<Element>(), View.Column.kDefault);
				m_Search.OnEnable( m_ClickType);
				m_Search.ColumnHeaderResizeToFit();
			}
			m_Search.Apply( builder.ToList());
			m_Search.ExpandAll();
			
			builder = new ElementBuilder();
			foreach( var trace in targets)
			{
				builder.Append( trace.Value);
			}
			if( m_Target == null)
			{
				m_Target = new Explorer( new List<Element>(), View.Column.kName | View.Column.kReference | View.Column.kMissing);
				m_Target.OnEnable( m_ClickType);
				m_Target.ColumnHeaderResizeToFit();
			}
			m_Target.Apply( builder.ToList());
			m_Target.ExpandAll();
			
			m_TargetGuids = assetGuids.ToArray();
			m_SearchType = newSearchType;
		}
		public bool ResearchAssets()
		{
			if( m_TargetGuids != null && m_SearchType != SearchType.kNone)
			{
				SearchAssets( m_TargetGuids, m_SearchType);
				return true;
			}
			return false;
		}
		static readonly string[] kClickTypes = new []
		{
			"None", 
			"Ping", "Ping - file only", 
			"Active", "Active - file only"
		};
		static readonly string[] kSearchTypes = new []
		{
			"", "To Dependencies", "From Dependencies"
		};
		[SerializeField]
		Explorer m_Project;
		[SerializeField]
		Explorer m_Search;
		[SerializeField]
		Explorer m_Target;
		[SerializeField]
		bool m_ChangeProject;
		[SerializeField]
		bool m_Recursive;
		[SerializeField]
		SearchType m_SearchType;
		[SerializeField]
		string[] m_TargetGuids;
		[SerializeField]
		ClickType m_ClickType = ClickType.kActiveFileOnly;
		
		System.Func<Contents> m_OnCreateWindowContens;
	}
}
