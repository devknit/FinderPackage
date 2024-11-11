
using System.Linq;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Finder
{
	public enum SearchType
	{
		kNone,
		kCheckMissing,
		kTracePrecedents,
		kTraceDependents
	}
	[System.Serializable]
	public sealed class Contents
	{
		public void OnEnable( System.Func<Contents> onCreateWindowContens)
		{
			m_ChangeProject = true;
			m_OnCreateWindowContens = onCreateWindowContens;
			
			if( m_Project == null)
			{
				m_Project = new Explorer( GetAllAssetElements(), View.Column.kName);
			}
			m_Project.OnEnable( m_ClickType);
			
			if( m_Target == null)
			{
				m_Target = new Explorer( new List<Element>(), View.Column.kName | View.Column.kReference);
			}
			m_Target.OnEnable( m_ClickType);
			
			if( m_Search == null)
			{
				m_Search = new Explorer( new List<Element>(), View.Column.kDefault);
			}
			m_Search.OnEnable( m_ClickType);
			
			if( m_Initialized == false)
			{
				m_Project.ColumnHeaderResizeToFit();
				m_Target.ColumnHeaderResizeToFit();
				m_Search.ColumnHeaderResizeToFit();
				m_Initialized = true;
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
			var newClickType = (ClickType)EditorGUILayout.Popup( (int)m_ClickType, kClickTypes, EditorStyles.toolbarPopup, GUILayout.Width( 100));
			if( m_ClickType != newClickType)
			{
				m_Project.SetClickType( newClickType);
				m_Target.SetClickType( newClickType);
				m_Search.SetClickType( newClickType);
				m_ClickType = newClickType;
			}
		}
		public void OnProjectChange()
		{
			m_ChangeProject = true;
		}
		public void OnProjectGUI()
		{
			if( m_ChangeProject != false)
			{
				m_Project.Apply( GetAllAssetElements());
				m_ChangeProject = false;
			}
			m_Project.OnGUI( this);
		}
		public void OnTargetGUI()
		{
			OnSearchToolbarGUI();
			m_Target.OnGUI( this);
		}
		public void OnSearchGUI()
		{
			OnSearchToolbarGUI();
			m_Search.OnGUI( this);
		}
		public void OnSearchToolbarGUI()
		{
			switch( m_SearchType)
			{
				case SearchType.kTracePrecedents:
				case SearchType.kTraceDependents:
				{
					using( new EditorGUILayout.HorizontalScope( EditorStyles.toolbar))
					{
						var toolbarButtonOn = new GUIStyle( EditorStyles.toolbarButton);
						toolbarButtonOn.normal = toolbarButtonOn.active;
						toolbarButtonOn.onNormal = toolbarButtonOn.onActive;
						
						if( GUILayout.Button( kSearchTypes[ 0], (
							m_SearchType == SearchType.kTracePrecedents)?
							toolbarButtonOn : EditorStyles.toolbarButton,
							GUILayout.ExpandWidth( false)) != false)
						{
							m_SearchType = SearchType.kTracePrecedents;
							ResearchAssets();
						}
						if( GUILayout.Button( kSearchTypes[ 1], (
							m_SearchType == SearchType.kTraceDependents)?
							toolbarButtonOn : EditorStyles.toolbarButton,
							GUILayout.ExpandWidth( false)) != false)
						{
							m_SearchType = SearchType.kTraceDependents;
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
					break;
				}
			}
		}
		List<Element> GetAllAssetElements()
		{
			var paths = AssetDatabase.GetAllAssetPaths();
			var builder = new ElementBuilder();
			
			if( paths != null)
			{
				for( int i0 = 0; i0 < paths.Length; ++i0)
				{
					builder.Append( paths[ i0]);
				}
			}
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
				case SearchType.kCheckMissing:
				{
					Search.CheckMissing( assetGuids, out targets, out searches);
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
			m_Search.Apply( builder.ToList());
			m_Search.ExpandAll();
			
			builder = new ElementBuilder();
			foreach( var trace in targets)
			{
				builder.Append( trace.Value);
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
			"Precedents", "Dependents"
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
		[SerializeField]
		bool m_Initialized;
		
		System.Func<Contents> m_OnCreateWindowContens;
	}
}
