#define WITH_SEARCHSTRING

using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEditorInternal;

namespace Finder {

public sealed class View : TreeView
{
	public enum Type
	{
		kTree,
		kList
	}
	public enum Column
	{
		kNone = 0x00,
		kName = 0x01,
		kExtension = 0x02,
		kPath = 0x04,
		kGuid = 0x08,
		kReference = 0x10,
		kDefault = kName | kExtension,
		kAll = kName | kExtension | kPath | kGuid | kReference
	}
	public static MultiColumnHeaderState CreateHeaderState( Column columnMask=Column.kNone)
	{
		var columns = new []
		{
			new MultiColumnHeaderState.Column
			{
				headerContent			= new GUIContent( "Name"),
				headerTextAlignment 	= TextAlignment.Center,
				canSort 				= false,
				width					= 200, 
				minWidth				= 50,
				autoResize				= true,
				allowToggleVisibility	= false
			},
			new MultiColumnHeaderState.Column
			{
				headerContent			= new GUIContent( "Extension"),
				headerTextAlignment 	= TextAlignment.Center,
				canSort 				= false,
				width					= 80, 
				minWidth				= 50,
				autoResize				= false,
				allowToggleVisibility	= true
			},
			new MultiColumnHeaderState.Column
			{
				headerContent			= new GUIContent( "Path"),
				headerTextAlignment 	= TextAlignment.Center,
				canSort 				= false,
				width					= 250, 
				minWidth				= 50,
				autoResize				= true,
				allowToggleVisibility	= true
			},
			new MultiColumnHeaderState.Column
			{
				headerContent			= new GUIContent( "Guid"),
				headerTextAlignment 	= TextAlignment.Center,
				canSort 				= false,
				width					= 200, 
				minWidth				= 50,
				autoResize				= false,
				allowToggleVisibility	= true
			},
			new MultiColumnHeaderState.Column
			{
				headerContent			= new GUIContent( "Reference"),
				headerTextAlignment 	= TextAlignment.Center,
				canSort 				= false,
				width					= 80, 
				minWidth				= 50,
				autoResize				= false,
				allowToggleVisibility	= true
			}
		};
		var headerState = new MultiColumnHeaderState( columns);
		
		if( columnMask != Column.kNone)
		{
			var visibleColumns = new List<int>();
			
			for( int i0 = 0, mask = 1; mask < (int)Column.kAll; ++i0, mask <<= 1)
			{
				if( ((int)columnMask & mask) != 0)
				{
					visibleColumns.Add( i0);
				}
			}
			headerState.visibleColumns = visibleColumns.ToArray();
		}
		return headerState;
	}
	public View( TreeViewState treeViewState, 
		MultiColumnHeader multiColumnHeader, 
		SearchFilter exploereSearchFilter,
		System.Action<Element> onClickCallback) :
		base( treeViewState, multiColumnHeader) 
	{
		searchFilter = (exploereSearchFilter != null)? 
			exploereSearchFilter : new SearchFilter();
		showAlternatingRowBackgrounds = true;
		getNewSelectionOverride = (clickedItem, keepMultiSelection, useShiftAsActionKey) => 
		{
			if( clickedItem is Element element)
			{
				/* フィルタリングされている場合のみ、親階層を展開する */
				if( searchFilter.valid != false)
				{
					Element extendElement = element.parentElement;
					
					while( extendElement != null)
					{
						SetExpanded( extendElement.id, true);
						extendElement = extendElement.parentElement;
					}
				}
				/* 選択されたアセットを Ping、または ActiveObject にする */
				/* LoadMainAssetAtPath でロードしたくないけど他に方法が... */
				onClickCallback?.Invoke( element);
			}
			var visibleRows = GetRows();
			var allIDs = visibleRows.Select( x => x.id).ToList();
			bool allowMultiselection = CanMultiSelect( clickedItem);
			
			return InternalEditorUtility.GetNewSelection(
				clickedItem.id, allIDs, state.selectedIDs, state.lastClickedID, 
				keepMultiSelection, useShiftAsActionKey, allowMultiselection);
		};
		multiColumnHeader.height = EditorGUIUtility.singleLineHeight + 4;
		multiColumnHeader.sortingChanged += OnSortingChanged;
	}
	public void Apply( List<Element> src, Type viewType)
	{
		switch( viewType)
		{
			case Type.kTree:
			{
				preBuildRows = PreBuildRows;
				buildRows = BuildTreeRows;
				preBuildFilterRows = PreBuildTreeFilterRows;
				buildFilterRows = BuildTreeFilterRows;
				break;
			}
			case Type.kList:
			{
				preBuildRows = PreBuildRows;
				buildRows = BuildListRows;
				preBuildFilterRows = PreBuildRows;
				buildFilterRows = BuildListFilterRows;
				break;
			}
			default:
			{
				preBuildRows = null;
				buildRows = null;
				preBuildFilterRows = null;
				buildFilterRows = null;
				break;
			}
		}
		if( buildRows != null)
		{
			elements = src;
			Reload();
			SetExpanded( "Assets".GetHashCode(), true);
		}
	}
	public void SetColumnHeaderEnable( View.Column column, bool bEnable)
	{
		var visibleColumns = multiColumnHeader.state.visibleColumns.ToList();
		
		for( int i0 = 0, mask = 1; mask < (int)Column.kAll; ++i0, mask <<= 1)
		{
			if( ((int)column & mask) != 0)
			{
				if( bEnable != false)
				{
					if( visibleColumns.Contains( i0) == false)
					{
						visibleColumns.Add( i0);
					}
				}
				else
				{
					if( visibleColumns.Contains( i0) != false)
					{
						visibleColumns.Remove( i0);
					}
				}
			}
		}
		multiColumnHeader.state.visibleColumns = visibleColumns.ToArray();
	}
	public Rect GetViewRect()
	{
		return treeViewRect;
	}
	public int GetSelectedCount()
	{
		return state.selectedIDs.Count;
	}
	public IEnumerable<TreeViewItem> FindRowElements( List<int> ids)
	{
		return GetRows().Where( x => ids.Contains( x.id));
	}
	public IEnumerable<TreeViewItem> FindRowElements( System.Func<Element, bool> onWhere)
	{
		return GetRows().Where( x => onWhere?.Invoke( x as Element) ?? false);
	}
	public bool ContainsSeelctedElements<T>( T value, System.Func<Element, T> onComparer)
	{
		if( state.selectedIDs.Count > 0)
		{
			var items = FindRowElements( state.selectedIDs);
			
			foreach( var item in items)
			{
				if( value.Equals( onComparer( item as Element)) != false)
				{
					return true;
				}
			}
		}
		return false;
	}
	public Element FirstSelectedElements( System.Func<Element, bool> onPredicate)
	{
		List<int> ids = state.selectedIDs;
		return GetRows().First( x => ids.Contains( x.id) && onPredicate( x as Element)) as Element;
	}
	public IEnumerable<T> SelectSelectedElements<T>( System.Func<Element, T> onSelector)
	{
		if( state.selectedIDs.Count > 0)
		{
			return FindRowElements( state.selectedIDs).Select( x => onSelector( x as Element));
		}
		return null;
	}
	public IEnumerable<T> SelectSelectedElements<T>( System.Func<Element, bool> onWhere, System.Func<Element, T> onSelector)
	{
		if( state.selectedIDs.Count > 0)
		{
			List<int> ids = state.selectedIDs;
			return FindRowElements( (element) =>
			{
				if( ids.Contains( element.id) != false)
				{
					return onWhere?.Invoke( element) ?? true;
				}
				return false;
				
			}).Select( x => onSelector( x as Element));
		}
		return null;
	}
	protected override void KeyEvent()
	{
		Event ev = Event.current;
		
		switch( ev.type)
		{
			case EventType.KeyDown:
			{
				switch( ev.keyCode)
				{
					case KeyCode.Return:
					case KeyCode.KeypadEnter:
					{
						if( GUIExpansion.HasKeyControl( ev.keyCode) == false)
						{
							string path = SelectSelectedElements( x => x.path)?.First();
							if( string.IsNullOrEmpty( path) == false)
							{
								AssetDatabase.OpenAsset( AssetDatabase.LoadMainAssetAtPath( path));
								GUIUtility.ExitGUI();
							}
							GUIExpansion.GrabKeyControl( ev.keyCode, treeViewControlID);
						}
						break;
					}
				}
				break;
			}
			case EventType.KeyUp:
			case EventType.Ignore:
			{
				GUIExpansion.ReleaseKeyControl( ev.keyCode);
				break;
			}
		}
	}
	protected override void DoubleClickedItem( int id)
	{
		if( FindItem( id, rootItem) is Element element)
		{
			if( element.directory != false)
			{
				SetExpanded( id, !IsExpanded( id));
			}
			else
			{
				element.OpenAsset();
				GUIUtility.ExitGUI();
			}
		}
	}
#if !WITH_SEARCHSTRING
	protected override void SearchChanged( string newSearch)
	{
		searchFilter.Change( newSearch);
	}
#endif
	protected override void RowGUI( RowGUIArgs args)
	{
		if( args.item is Element element)
		{
			int columnCount = args.GetNumVisibleColumns();
			
			for( int i0 = 0; i0 < columnCount; ++i0)
			{
				var cellRect = args.GetCellRect( i0);
				var columnIndex = args.GetColumn( i0);
				
			 	CenterRectUsingSingleLineHeight( ref cellRect);
				
				switch( (Column)(1 << columnIndex))
				{
					case Column.kName:
					{
						base.RowGUI( args);
						break;
					}
					case Column.kExtension:
					{
						DefaultGUI.Label( cellRect, element.extension, args.selected, args.focused);
						break;
					}
					case Column.kPath:
					{
						DefaultGUI.Label( cellRect, element.path, args.selected, args.focused);
						break;
					}
					case Column.kGuid:
					{
						DefaultGUI.Label( cellRect, element.guid, args.selected, args.focused);
						break;
					}
					case Column.kReference:
					{
						if( element.reference >= 0)
						{
							DefaultGUI.LabelRightAligned( cellRect, element.reference.ToString(), args.selected, args.focused);
						}
						break;
					}
					default:
					{
						base.RowGUI( args);
						break;
					}
				}
			}
		}
	}
	protected override TreeViewItem BuildRoot()
    {
        return new TreeViewItem{ id = 0, depth = -1, displayName = string.Empty };
    }
	protected override IList<TreeViewItem> BuildRows( TreeViewItem root)
    {
		var rows = GetRows() ?? new List<TreeViewItem>();
        rows.Clear();
		
		if( elements != null)
		{
			if( searchFilter.valid == false)
			{
				preBuildRows?.Invoke( root, elements);
				buildRows?.Invoke( elements, rows);
			}
			else
			{
				
				preBuildFilterRows?.Invoke( root, elements);
				buildFilterRows?.Invoke( elements, rows);
			}
		}
        return rows;
	}
	void PreBuildRows( TreeViewItem root, List<Element> elements)
	{
		foreach( var element in elements)
		{
			root.AddChild( element);
		}
	}
	void BuildTreeRows( List<Element> elements, IList<TreeViewItem> rows)
	{
		Element.TreeViewSort( elements);
		
		foreach( var element in elements)
		{
			var item = new Element( element);
			
			rows.Add( item);
			
			if( element.childElements.Count > 0)
			{
				if( IsExpanded( item.id) == false)
				{
					item.children = CreateChildListForCollapsedParent();
				}
				else
				{
					BuildTreeRows( element.childElements, rows);
				}
			}
		}
	}
	void BuildListRows( List<Element> elements, IList<TreeViewItem> rows)
	{
		Element.ListViewSort( elements);
		
		foreach( var element in elements)
		{
			if( element.directory == false)
			{
				var item = new Element( element);
				item.depth = 0;
				rows.Add( item);
			}
			if( element.childElements.Count > 0)
			{
				BuildListRows( element.childElements, rows);
			}
		}
	}
	void PreBuildTreeFilterRows( TreeViewItem root, List<Element> elements)
	{
		foreach( var element in elements)
		{
			element.CheckFilter( searchFilter);
			root.AddChild( element);
		}
	}
	void BuildTreeFilterRows( List<Element> elements, IList<TreeViewItem> rows)
	{
		Element.TreeViewSort( elements);
		
		foreach( var element in elements)
		{
			if( element.validCount > 0)
			{
				var item = new Element( element);
				
				rows.Add( item);
				
				if( element.childElements.Count > 0)
				{
					BuildTreeFilterRows( element.childElements, rows);
				}
			}
		}
	}
	void BuildListFilterRows( List<Element> elements, IList<TreeViewItem> rows)
	{
		Element.ListViewSort( elements);
		
		foreach( var element in elements)
		{
			if( element.directory == false)
			{
				if( searchFilter.Check( element) != false)
				{
					var item = new Element( element);
					item.depth = 0;
					rows.Add( item);
				}
			}
			if( element.childElements.Count > 0)
			{
				BuildListFilterRows( element.childElements, rows);
			}
		}
	}
	void OnSortingChanged( MultiColumnHeader multiColumnHeader)
	{
		if( multiColumnHeader.sortedColumnIndex >= 0)
		{
		}
	}
	Action<TreeViewItem, List<Element>> preBuildRows;
	Action<List<Element>, IList<TreeViewItem>> buildRows;
	Action<TreeViewItem, List<Element>> preBuildFilterRows;
	Action<List<Element>, IList<TreeViewItem>> buildFilterRows;
	SearchFilter searchFilter;
	List<Element> elements;
	Type displayType;
}

} /* namespace Finder */
