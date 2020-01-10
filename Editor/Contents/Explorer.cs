#define WITH_SEARCHSTRING

using System.Text;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using UnityEditorInternal;

namespace Finder {

[System.Serializable]
public sealed class Explorer : ISerializationCallbackReceiver
{
	public Explorer( List<Element> src, View.Column columnMask=View.Column.kDefault) 
	{
		headerState = View.CreateHeaderState( columnMask);
		viewState = new TreeViewState();
		serializableElement = new SerializableElementRoot();
		elements = src;
	}
	public void OnEnable( System.Action<Element> onClickCallback)
	{
		if( EditorGUIUtility.isProSkin == false)
		{
			listTexture = AssetDatabase.GUIDToAssetPath( "ecd3c38ceb9b5f2469b3da8ee9386ad1");
			treeTexture = "UnityEditor.HierarchyWindow";
			typeTexture = "FilterByType";
		}
		else
		{
			listTexture = AssetDatabase.GUIDToAssetPath( "b85dfab077e810344919c003de51ea9a");
			treeTexture = "d_UnityEditor.HierarchyWindow";
			typeTexture = "d_FilterByType";
		}
		
		MultiColumnHeaderState _headerState = View.CreateHeaderState();
		
		if( MultiColumnHeaderState.CanOverwriteSerializedFields( headerState, _headerState) != false)
        {
            MultiColumnHeaderState.OverwriteSerializedFields( headerState, _headerState);
        }
        headerState = _headerState;
		
		searchFilter = new SearchFilter( OnFilterChange);
		
		view = new View( viewState, new MultiColumnHeader( headerState), searchFilter, onClickCallback);
		
		searchField = new SearchField();
		searchField.downOrUpArrowKeyPressed += view.SetFocusAndEnsureSelectedItem;
		
		objectTypes = new PopupList.InputData();
		objectTypes.onSelectCallback = OnPopupSelect;
		for( int i0 = 0; i0 < AssetTypes.kTypeNames.Length; ++i0)
		{
			objectTypes.NewOrMatchingElement( AssetTypes.kTypeNames[ i0]);
		}		
		
		searchFilter.Change( view.searchString);
		Apply( elements);
	}
	public void OnDisable()
	{
		searchField.downOrUpArrowKeyPressed -= view.SetFocusAndEnsureSelectedItem;
	}
	public void Apply( List<Element> src)
	{
		elements = src;
		view.Apply( elements, viewType);
	}
	public void Apply()
	{
		if( elements != null)
		{
			view.Apply( elements, viewType);
		}
	}
	public void ExpandAll()
	{
		view.ExpandAll();
	}
	public void CollapseAll()
	{
		view.CollapseAll();
	}
	public void ColumnHeaderResizeToFit()
	{
		view.multiColumnHeader.ResizeToFit();
	}
	public void SetColumnHeaderEnable( View.Column column, bool bEnable)
	{
		view.SetColumnHeaderEnable( column, bEnable);
	}
	public void OnGUI( Contents contents)
	{
		Event ev = Event.current;
		
		if( ev.type == EventType.KeyDown)
		{
			switch( ev.keyCode)
			{
				case KeyCode.F:
				{
					FilterPath();
					ev.Use();
					break;
				}
				case KeyCode.L:
				{
					if( ev.control != false)
					{
						searchField.SetFocus();
						ev.Use();
					}
					break;
				}
			}
		}
		using( new EditorGUILayout.VerticalScope())
		{
			using( new EditorGUILayout.HorizontalScope( EditorStyles.toolbar))
			{
				int mouseButton;
				
				mouseButton = GUIExpansion.Toggle( viewType == View.Type.kTree, 
					new GUIContent( EditorGUIUtility.Load( treeTexture) as Texture2D),
					EditorStyles.toolbarButton, GUILayout.ExpandWidth( false));
				switch( mouseButton)
				{
					case 0:
					{
						if( viewType != View.Type.kTree)
						{
							viewType = View.Type.kTree;
							Apply();
						}
						break;
					}
					case 1:
					{
						if( viewType == View.Type.kTree)
						{
							var contextMenu = new GenericMenu();
							
							contextMenu.AddItem( new GUIContent( "ExpandAll"), false, () =>
							{
								view.ExpandAll();
							});
							contextMenu.AddItem( new GUIContent( "CollapseAll"), false, () =>
							{
								view.CollapseAll();
							});
							contextMenu.ShowAsContext();
						}
						break;
					}
				}
				mouseButton = GUIExpansion.Toggle( viewType == View.Type.kList, 
					new GUIContent( AssetDatabase.LoadMainAssetAtPath( listTexture) as Texture2D),
					EditorStyles.toolbarButton, GUILayout.ExpandWidth( false));
				switch( mouseButton)
				{
					case 0:
					{
						if( viewType != View.Type.kList)
						{
							viewType = View.Type.kList;
							Apply();
						}
						break;
					}
					case 1:
					{
						if( viewType == View.Type.kList)
						{
						}
						break;
					}
				}
				GUILayout.Space( 6);
				
			#if false
				if( viewType == View.Type.kList)
				{
					EditorGUI.BeginDisabledGroup( true);
					EditorGUILayout.TextField( "Asssets/", EditorStyles.toolbarTextField);
					EditorGUI.EndDisabledGroup();
				}
			#endif
			#if WITH_SEARCHSTRING
				string newSearchString = searchField.OnToolbarGUI( searchString, GUILayout.ExpandWidth( true));
				if( searchString != newSearchString)
				{
					searchString = newSearchString;
					searchFilter.Change( searchString);
					view.Reload();
				}
			#else
				view.searchString = searchField.OnToolbarGUI( view.searchString, GUILayout.ExpandWidth( true));
			#endif
				GUILayout.Space( 4);
				
				var content = EditorGUIUtility.TrIconContent( typeTexture, "Search by Type");
				Rect rect = GUILayoutUtility.GetRect( content, EditorStyles.toolbarButton, GUILayout.ExpandWidth( false));
				
				if( EditorGUI.DropdownButton( rect, content,
					FocusType.Passive, EditorStyles.toolbarButton) != false)
				{
					PopupWindow.Show( rect, new PopupList( objectTypes));
				}
			}
			using( var scope = new EditorGUILayout.VerticalScope( GUILayout.ExpandHeight( true)))
			{
				OnContextMenuEvent( contents, Event.current);
				view.OnGUI( scope.rect);
			}
		}
	}
	void OnContextMenuEvent( Contents contents, Event ev)
	{
		if( ev.type == EventType.MouseUp && ev.button == 1)
		{
			if( view.GetViewRect().Contains( ev.mousePosition) != false)
			{
				int selectedCount = view.GetSelectedCount();
				if( selectedCount > 0)
				{
					var contextMenu = new GenericMenu();
					
					if( selectedCount == 1)
					{
						contextMenu.AddItem( new GUIContent( "Open"), false, () =>
						{
							Element element = view.FirstSelectedElements( ( x => x.CanOpenAsset()));
							if( element.directory != false)
							{
								view.SetExpanded( element.id, !view.IsExpanded( element.id));
							}
							else
							{
								AssetDatabase.OpenAsset( AssetDatabase.LoadMainAssetAtPath( element.path));
							}
						});
						contextMenu.AddItem( new GUIContent( "Show in Explorer"), false, () =>
						{
							Element element = view.FirstSelectedElements( ( x => true));
							EditorUtility.RevealInFinder( element.path);
						});
					}
					if( selectedCount == 1 && view.ContainsSeelctedElements( true, x => x.directory) != false)
					{
						contextMenu.AddItem( new GUIContent( "Filter Path"), false, () =>
						{
							FilterPath();
						});
					}
					contextMenu.AddItem( new GUIContent( "Copy Path"), false, () =>
					{
						var elements = view.SelectSelectedElements( x => x.path);
						var builder = new System.Text.StringBuilder();
						foreach( var element in elements)
						{
							builder.AppendLine( element);
						}
						EditorGUIUtility.systemCopyBuffer = builder.ToString();
					});
					contextMenu.AddItem( new GUIContent( "Copy Guid"), false, () =>
					{
						var elements = view.SelectSelectedElements( x => x.guid);
						var builder = new System.Text.StringBuilder();
						foreach( var element in elements)
						{
							builder.AppendLine( element);
						}
						EditorGUIUtility.systemCopyBuffer = builder.ToString();
					});
					contextMenu.AddItem( new GUIContent( "Export Package/Select Only"), false, () =>
					{
						var assetPaths = view.SelectSelectedElements( x => x.IsFile(), x => x.path).ToArray();
						if( assetPaths.Length > 0)
						{
							string directory = System.IO.Path.GetFullPath( "Assets/../");
							string fileName = System.DateTime.Now.ToString( "yyyy-MM-dd_HH-mm-ss");
							string savePath = EditorUtility.SaveFilePanel( "Export Package", directory, fileName, "unitypackage");
							if( string.IsNullOrEmpty( savePath) == false)
							{
								AssetDatabase.ExportPackage( assetPaths, savePath, ExportPackageOptions.Default | ExportPackageOptions.Interactive);
							}
						}
					});
					contextMenu.AddItem( new GUIContent( "Export Package/Include Dependencies"), false, () =>
					{
						var assetPaths = view.SelectSelectedElements( x => x.IsFile(), x => x.path).ToArray();
						if( assetPaths.Length > 0)
						{
							string directory = System.IO.Path.GetFullPath( "Assets/../");
							string fileName = System.DateTime.Now.ToString( "yyyy-MM-dd_HH-mm-ss");
							string savePath = EditorUtility.SaveFilePanel( "Export Package", directory, fileName, "unitypackage");
							if( string.IsNullOrEmpty( savePath) == false)
							{
								AssetDatabase.ExportPackage( assetPaths, savePath, ExportPackageOptions.IncludeDependencies | ExportPackageOptions.Interactive);
							}
						}
					});
					if( selectedCount == 1)
					{
						contextMenu.AddItem( new GUIContent( "Ping"), false,
						() =>
						{
							Element element = view.FirstSelectedElements( x => x.CanPingObject());
							element?.PingObject( true);
						});
						contextMenu.AddItem( new GUIContent( "Active"), false,
						() =>
						{
							Element element = view.FirstSelectedElements( x => x.CanActiveObject());
							element?.ActiveObject( true);
						});
					}
					contextMenu.AddItem( new GUIContent( "Search/Check Missing"), false, () =>
					{
						var elements = view.SelectSelectedElements( x => x.guid);
						contents?.SearchAssets( elements, SearchType.kCheckMissing);
					});
					contextMenu.AddItem( new GUIContent( "Search/Trace Precedents"), false, () =>
					{
						var elements = view.SelectSelectedElements( x => x.guid);
						contents?.SearchAssets( elements, SearchType.kTracePrecedents);
					});
					contextMenu.AddItem( new GUIContent( "Search/Trace Dependents"), false, () =>
					{
						var elements = view.SelectSelectedElements( x => x.guid);
						contents?.SearchAssets( elements, SearchType.kTraceDependents);
					});
					contextMenu.AddItem( new GUIContent( "Search/Trace Precedents new tab"), false, () =>
					{
						var elements = view.SelectSelectedElements( x => x.guid);
						contents?.OpenSearchAssets( elements, SearchType.kTracePrecedents);
					});
					contextMenu.AddItem( new GUIContent( "Search/Trace Dependents new tab"), false, () =>
					{
						var elements = view.SelectSelectedElements( x => x.guid);
						contents?.OpenSearchAssets( elements, SearchType.kTraceDependents);
					});
					if( view.ContainsSeelctedElements( AssetType.kMaterial, x => x.type) != false)
					{
						contextMenu.AddItem( new GUIContent( "Material Cleaner"), false, () =>
						{
							var elements = view.SelectSelectedElements( x => x.guid);
							MaterialCleaner.Clean( elements);
						});
					}
					contextMenu.ShowAsContext();
					ev.Use();
				}
			}
		}
	}
	void FilterPath()
	{
		if( view.GetSelectedCount() == 1)
		{
			Element element = view.FirstSelectedElements( ( x => true));
			var builder = new StringBuilder();
			
			builder.AppendFormat( "p:~/{0} ", element.path);
			searchFilter.ToBuildStringTypes( builder);
			searchFilter.ToBuildStringNames( builder);
			
			string newSearchString = builder.ToString();
			if( searchString != newSearchString)
			{
				searchString = newSearchString;
				searchFilter.Change( searchString);
				view.Reload();
			}
		}
	}
	void OnFilterChange( bool filterValid)
	{
		var types = searchFilter.types;
		
		foreach( var element in objectTypes.elements)
		{
			element.selected = types.ContainsValue( element.label);
		}
	}
	void OnPopupSelect( PopupList.Element selectElement)
	{
		if( Event.current.control == false)
		{
			foreach( var element in objectTypes.elements)
			{
				if( element != selectElement)
				{
					element.selected = false;
				}
			}
		}
		selectElement.selected = !selectElement.selected;
		
		IEnumerable<string> selectedDisplayNames = (from item in objectTypes.elements where item.selected select item.label);
		
		var builder = new StringBuilder();
		
		foreach( var typeName in selectedDisplayNames)
		{
			if( builder.Length > 0)
			{
				builder.Append( " ");
			}
			builder.Append( "t:");
			builder.Append( typeName);
		}
		searchFilter.ToBuildStringPaths( builder);
		searchFilter.ToBuildStringNames( builder);
		
	#if WITH_SEARCHSTRING
		string newSearchString = builder.ToString();
		if( searchString != newSearchString)
		{
			searchString = newSearchString;
			searchFilter.Change( searchString);
			view.Reload();
		}
	#else
		view.searchString = builder.ToString();
	#endif
	}
	public void OnBeforeSerialize()
	{
		serializableElement.OnBeforeSerialize
		(
			new Element
			{
				childElements = elements,
			}
		);
	}
	public void OnAfterDeserialize()
	{
		Element rootElement = serializableElement.OnAfterDeserialize();
		elements = rootElement.childElements;
	}
	
	[SerializeField]
	TreeViewState viewState;
	[SerializeField]
	MultiColumnHeaderState headerState;
	[SerializeField]
	SerializableElementRoot serializableElement;
	[SerializeField]
	View.Type viewType;
	[SerializeField]
	string searchString;
	
	[System.NonSerialized]
	List<Element> elements;
	[System.NonSerialized]
	SearchField searchField;
	[System.NonSerialized]
	SearchFilter searchFilter;
	[System.NonSerialized]
	View view;
	[System.NonSerialized]
	PopupList.InputData objectTypes;
	
	[System.NonSerialized]
	string listTexture;
	[System.NonSerialized]
	string treeTexture;
	[System.NonSerialized]
	string typeTexture;
}

} /* namespace Finder */
