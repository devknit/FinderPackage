
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using UnityEditorInternal;

namespace Finder {

public enum SearchType
{
	kNone,
	kCheckMissing,
	kTracePrecedents,
	kTraceDependents
}
public enum ClickType
{
	kNone,
	kPing,
	kPingFileOnly,
	kActive,
	kActiveFileOnly
}

[System.Serializable]
public sealed class Contents
{
	public void OnEnable( System.Func<Contents> createWindowContens)
	{
		if( project == null)
		{
			project = new Explorer( GetAllAssetElements(), View.Column.kName);
		}
		project.OnEnable( OnClick);
		
		if( target == null)
		{
			target = new Explorer( new List<Element>(), View.Column.kName | View.Column.kReference);
		}
		target.OnEnable( OnClick);
		
		if( search == null)
		{
			search = new Explorer( new List<Element>(), View.Column.kDefault);
		}
		search.OnEnable( OnClick);
		
		onCreateWindowContens = createWindowContens;
		
		if( initialized == false)
		{
			project.ColumnHeaderResizeToFit();
			target.ColumnHeaderResizeToFit();
			search.ColumnHeaderResizeToFit();
			initialized = true;
		}
	}
	public void OnDisable()
	{
		if( project != null)
		{
			project.OnDisable();
		}
		if( target != null)
		{
			target.OnDisable();
		}
		if( search != null)
		{
			search.OnDisable();
		}
	}
	public void OnToolbarGUI()
	{
		clickType = (ClickType)EditorGUILayout.Popup( (int)clickType, kClickTypes, EditorStyles.toolbarPopup, GUILayout.Width( 100));
	}
	public void OnProjectChange()
	{
		project.Apply( GetAllAssetElements());
	}
	public void OnProjectGUI()
	{
		project.OnGUI( this);
	}
	public void OnTargetGUI()
	{
		OnSearchToolbarGUI();
		target.OnGUI( this);
	}
	public void OnSearchGUI()
	{
		OnSearchToolbarGUI();
		search.OnGUI( this);
	}
	public void OnSearchToolbarGUI()
	{
		switch( searchType)
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
						searchType == SearchType.kTracePrecedents)?
						toolbarButtonOn : EditorStyles.toolbarButton,
						GUILayout.ExpandWidth( false)) != false)
					{
						searchType = SearchType.kTracePrecedents;
						ResearchAssets();
					}
					if( GUILayout.Button( kSearchTypes[ 1], (
						searchType == SearchType.kTraceDependents)?
						toolbarButtonOn : EditorStyles.toolbarButton,
						GUILayout.ExpandWidth( false)) != false)
					{
						searchType = SearchType.kTraceDependents;
						ResearchAssets();
					}
					bool newRecursive = EditorGUILayout.ToggleLeft( "Recursive", recursive, GUILayout.Width( 80));
					if( recursive != newRecursive)
					{
						recursive = newRecursive;
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
	void OnClick( Element element)
	{
		switch( clickType)
		{
			case ClickType.kPing:
			{
				element.PingObject( true);
				break;
			}
			case ClickType.kPingFileOnly:
			{
				element.PingObject( false);
				break;
			}
			case ClickType.kActive:
			{
				element.ActiveObject( true);
				break;
			}
			case ClickType.kActiveFileOnly:
			{
				element.ActiveObject( false);
				break;
			}
		}
	}
	public void OpenSearchAssets( IEnumerable<string> assetGuids, SearchType newSearchType)
	{
		Contents contents = onCreateWindowContens?.Invoke();
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
				Search.TracePrecedents( assetGuids, recursive, out targets, out searches);
				break;
			}
			case SearchType.kTraceDependents:
			{
				Search.TraceDependents( assetGuids, recursive, out targets, out searches);
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
		search.Apply( builder.ToList());
		search.ExpandAll();
		
		builder = new ElementBuilder();
		foreach( var trace in targets)
		{
			builder.Append( trace.Value);
		}
		target.Apply( builder.ToList());
		target.ExpandAll();
		
		targetGuids = assetGuids.ToArray();
		searchType = newSearchType;
	}
	public bool ResearchAssets()
	{
		if( targetGuids != null && searchType != SearchType.kNone)
		{
			SearchAssets( targetGuids, searchType);
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
	Explorer project;
	[SerializeField]
	Explorer search;
	[SerializeField]
	Explorer target;
	
	[SerializeField]
	bool recursive;
	[SerializeField]
	SearchType searchType;
	[SerializeField]
	string[] targetGuids;
	[SerializeField]
	ClickType clickType = ClickType.kActiveFileOnly;
	[SerializeField]
	bool initialized;
	
	System.Func<Contents> onCreateWindowContens;
}

} /* namespace Finder */
