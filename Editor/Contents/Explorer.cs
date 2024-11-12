#define WITH_SEARCHSTRING

using System.Text;
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;

namespace Finder
{
	[System.Serializable]
	public sealed class Explorer : ISerializationCallbackReceiver
	{
		public Explorer( List<Element> elements, View.Column columnMask=View.Column.kDefault) 
		{
			m_HeaderState = View.CreateHeaderState( columnMask);
			m_ViewState = new TreeViewState();
			m_SerializableElement = new SerializableElementRoot();
			m_Elements = elements;
		}
		public void OnEnable( ClickType clickType)
		{
			if( EditorGUIUtility.isProSkin == false)
			{
			//	m_ListTexture = AssetDatabase.GUIDToAssetPath( "ecd3c38ceb9b5f2469b3da8ee9386ad1");
			//	m_TreeTexture = "UnityEditor.HierarchyWindow";
				m_TypeTexture = "FilterByType";
			}
			else
			{
			//	m_ListTexture = AssetDatabase.GUIDToAssetPath( "b85dfab077e810344919c003de51ea9a");
			//	m_TreeTexture = "d_UnityEditor.HierarchyWindow";
				m_TypeTexture = "d_FilterByType";
			}
			
			MultiColumnHeaderState headerState = View.CreateHeaderState();
			
			if( MultiColumnHeaderState.CanOverwriteSerializedFields( m_HeaderState, headerState) != false)
			{
				MultiColumnHeaderState.OverwriteSerializedFields( m_HeaderState, headerState);
			}
			m_HeaderState = headerState;
			m_SearchFilter = new SearchFilter( OnFilterChange);
			
			m_View = new View( m_ViewState, new MultiColumnHeader( m_HeaderState), m_SearchFilter);
			m_View.SetClickType( clickType);
			
			m_SearchField = new SearchField();
			m_SearchField.downOrUpArrowKeyPressed += m_View.SetFocusAndEnsureSelectedItem;
			
			m_ObjectTypes = new PopupList.InputData();
			m_ObjectTypes.onSelectCallback = OnPopupSelect;
			
			for( int i0 = 0; i0 < AssetTypes.kTypeNames.Length; ++i0)
			{
				m_ObjectTypes.NewOrMatchingElement( AssetTypes.kTypeNames[ i0]);
			}		
			m_SearchFilter.Change( m_View.searchString);
			Apply( m_Elements);
		}
		public void OnDisable()
		{
			m_SearchField.downOrUpArrowKeyPressed -= m_View.SetFocusAndEnsureSelectedItem;
		}
		public void SetClickType( ClickType type)
		{
			m_View.SetClickType( type);
		}
		public void Apply( List<Element> src)
		{
			m_Elements = src;
			m_View.Apply( m_Elements, View.Type.kTree);
		}
		public void Apply()
		{
			if( m_Elements != null)
			{
				m_View.Apply( m_Elements, View.Type.kTree);
			}
		}
		public void ExpandAll()
		{
			m_View.ExpandAll();
		}
		public void CollapseAll()
		{
			m_View.CollapseAll();
		}
		public void ColumnHeaderResizeToFit()
		{
			m_View.multiColumnHeader.ResizeToFit();
		}
		public void SetColumnHeaderEnable( View.Column column, bool bEnable)
		{
			m_View.SetColumnHeaderEnable( column, bEnable);
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
							m_SearchField.SetFocus();
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
				/* リスト表示廃止に伴い無効化 */
				#if false
					int mouseButton;
					
					mouseButton = GUIExpansion.Toggle( m_ViewType == View.Type.kTree, 
						new GUIContent( EditorGUIUtility.Load( m_TreeTexture) as Texture2D),
						EditorStyles.toolbarButton, GUILayout.ExpandWidth( false));
					switch( mouseButton)
					{
						case 0:
						{
							if( m_ViewType != View.Type.kTree)
							{
								m_ViewType = View.Type.kTree;
								Apply();
							}
							break;
						}
						case 1:
						{
							if( m_ViewType == View.Type.kTree)
							{
								var contextMenu = new GenericMenu();
								
								contextMenu.AddItem( new GUIContent( "ExpandAll"), false, () =>
								{
									m_View.ExpandAll();
								});
								contextMenu.AddItem( new GUIContent( "CollapseAll"), false, () =>
								{
									m_View.CollapseAll();
								});
								contextMenu.ShowAsContext();
							}
							break;
						}
					}
					mouseButton = GUIExpansion.Toggle( m_ViewType == View.Type.kList, 
						new GUIContent( AssetDatabase.LoadMainAssetAtPath( m_ListTexture) as Texture2D),
						EditorStyles.toolbarButton, GUILayout.ExpandWidth( false));
					switch( mouseButton)
					{
						case 0:
						{
							if( m_ViewType != View.Type.kList)
							{
								m_ViewType = View.Type.kList;
								Apply();
							}
							break;
						}
						case 1:
						{
							if( m_ViewType == View.Type.kList)
							{
							}
							break;
						}
					}
					GUILayout.Space( 6);
				#endif
				#if false
					if( m_ViewType == View.Type.kList)
					{
						EditorGUI.BeginDisabledGroup( true);
						EditorGUILayout.TextField( "Asssets/", EditorStyles.toolbarTextField);
						EditorGUI.EndDisabledGroup();
					}
				#endif
				#if WITH_SEARCHSTRING
					string newSearchString = m_SearchField.OnToolbarGUI( m_SearchString, GUILayout.ExpandWidth( true));
					if( m_SearchString != newSearchString)
					{
						m_SearchString = newSearchString;
						m_SearchFilter.Change( m_SearchString);
						m_View.Reload();
					}
				#else
					m_View.searchString = searchField.OnToolbarGUI( m_View.searchString, GUILayout.ExpandWidth( true));
				#endif
					GUILayout.Space( 4);
					
					var content = EditorGUIUtility.TrIconContent( m_TypeTexture, "Search by Type");
					Rect rect = GUILayoutUtility.GetRect( content, EditorStyles.toolbarButton, GUILayout.ExpandWidth( false));
					
					if( EditorGUI.DropdownButton( rect, content,
						FocusType.Passive, EditorStyles.toolbarButton) != false)
					{
						PopupWindow.Show( rect, new PopupList( m_ObjectTypes));
					}
				}
				using( var scope = new EditorGUILayout.VerticalScope( GUILayout.ExpandHeight( true)))
				{
					OnContextMenuEvent( contents, Event.current);
					m_View.OnGUI( scope.rect);
				}
			}
		}
		void OnContextMenuEvent( Contents contents, Event ev)
		{
			if( ev.type == EventType.MouseUp && ev.button == 1)
			{
				if( m_View.GetViewRect().Contains( ev.mousePosition) != false)
				{
					int selectedCount = m_View.GetSelectedCount();
					if( selectedCount > 0)
					{
						var contextMenu = new GenericMenu();
						
						if( selectedCount == 1)
						{
							contextMenu.AddItem( new GUIContent( "Open"), false, () =>
							{
								Element element = m_View.FirstSelectedElements( ( x => x.CanOpenAsset()));
								if( element.Directory != false)
								{
									m_View.SetExpanded( element.id, !m_View.IsExpanded( element.id));
								}
								else
								{
									AssetDatabase.OpenAsset( AssetDatabase.LoadMainAssetAtPath( element.Path));
								}
							});
							contextMenu.AddItem( new GUIContent( "Show in Explorer"), false, () =>
							{
								Element element = m_View.FirstSelectedElements( x => true);
								EditorUtility.RevealInFinder( element.Path);
							});
						}
						if( selectedCount == 1 && m_View.ContainsSeelctedElements( true, x => x.Directory) != false)
						{
							contextMenu.AddItem( new GUIContent( "Filter Path"), false, () =>
							{
								FilterPath();
							});
						}
						contextMenu.AddItem( new GUIContent( "Copy Path"), false, () =>
						{
							var elements = m_View.SelectSelectedElements( x => x.Path);
							var builder = new System.Text.StringBuilder();
							foreach( var element in elements)
							{
								builder.AppendLine( element);
							}
							EditorGUIUtility.systemCopyBuffer = builder.ToString();
						});
						contextMenu.AddItem( new GUIContent( "Copy Guid"), false, () =>
						{
							var elements = m_View.SelectSelectedElements( x => x.Guid);
							var builder = new System.Text.StringBuilder();
							foreach( var element in elements)
							{
								builder.AppendLine( element);
							}
							EditorGUIUtility.systemCopyBuffer = builder.ToString();
						});
						contextMenu.AddItem( new GUIContent( "Export Package/Select Only"), false, () =>
						{
							var assetPaths = m_View.SelectSelectedElements( x => x.IsFile(), x => x.Path).ToArray();
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
							var assetPaths = m_View.SelectSelectedElements( x => x.IsFile(), x => x.Path).ToArray();
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
								Element element = m_View.FirstSelectedElements( x => x.CanPingObject());
								element?.PingObject( true);
							});
							contextMenu.AddItem( new GUIContent( "Active"), false,
							() =>
							{
								Element element = m_View.FirstSelectedElements( x => x.CanActiveObject());
								element?.ActiveObject( true);
							});
						}
						contextMenu.AddItem( new GUIContent( "Select To Dependencies/New Window"), false, () =>
						{
							var elements = m_View.SelectSelectedElements( x => x.Guid);
							contents?.OpenSearchAssets( elements, SearchType.kTraceDependents);
						});
						contextMenu.AddItem( new GUIContent( "Select To Dependencies/Current Window"), false, () =>
						{
							var elements = m_View.SelectSelectedElements( x => x.Guid);
							contents?.SearchAssets( elements, SearchType.kTraceDependents);
						});
						contextMenu.AddItem( new GUIContent( "Select From Dependencies/New Window"), false, () =>
						{
							var elements = m_View.SelectSelectedElements( x => x.Guid);
							contents?.OpenSearchAssets( elements, SearchType.kTracePrecedents);
						});
						contextMenu.AddItem( new GUIContent( "Select From Dependencies/Current Window"), false, () =>
						{
							var elements = m_View.SelectSelectedElements( x => x.Guid);
							contents?.SearchAssets( elements, SearchType.kTracePrecedents);
						});
					#if false
						if( m_View.ContainsSeelctedElements( AssetType.kMaterial, x => x.AssetType) != false)
						{
							contextMenu.AddItem( new GUIContent( "Material Cleaner"), false, () =>
							{
								var elements = m_View.SelectSelectedElements( x => x.Guid);
								MaterialCleaner.Clean( elements);
							});
						}
					#endif
						contextMenu.ShowAsContext();
						ev.Use();
					}
				}
			}
		}
		void FilterPath()
		{
			if( m_View.GetSelectedCount() == 1)
			{
				Element element = m_View.FirstSelectedElements( ( x => true));
				var builder = new StringBuilder();
				
				builder.AppendFormat( "p:~/{0} ", element.Path);
				m_SearchFilter.ToBuildStringTypes( builder);
				m_SearchFilter.ToBuildStringNames( builder);
				
				string newSearchString = builder.ToString();
				if( m_SearchString != newSearchString)
				{
					m_SearchString = newSearchString;
					m_SearchFilter.Change( m_SearchString);
					m_View.Reload();
				}
			}
		}
		void OnFilterChange( bool filterValid)
		{
			foreach( var element in m_ObjectTypes.elements)
			{
				element.Selected = m_SearchFilter.ContainsTypeValue( element.Label);
			}
		}
		void OnPopupSelect( PopupList.Element selectElement)
		{
			if( Event.current.control == false)
			{
				foreach( var element in m_ObjectTypes.elements)
				{
					if( element != selectElement)
					{
						element.Selected = false;
					}
				}
			}
			selectElement.Selected = !selectElement.Selected;
			
			IEnumerable<string> selectedDisplayNames = (from item in m_ObjectTypes.elements where item.Selected select item.Label);
			
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
			m_SearchFilter.ToBuildStringPaths( builder);
			m_SearchFilter.ToBuildStringNames( builder);
			
		#if WITH_SEARCHSTRING
			string newSearchString = builder.ToString();
			if( m_SearchString != newSearchString)
			{
				m_SearchString = newSearchString;
				m_SearchFilter.Change( m_SearchString);
				m_View.Reload();
			}
		#else
			m_View.searchString = builder.ToString();
		#endif
		}
		public void OnBeforeSerialize()
		{
			m_SerializableElement.OnBeforeSerialize
			(
				new Element
				{
					ChildElements = m_Elements,
				}
			);
		}
		public void OnAfterDeserialize()
		{
			Element rootElement = m_SerializableElement.OnAfterDeserialize();
			m_Elements = rootElement.ChildElements;
		}
		[SerializeField]
		TreeViewState m_ViewState;
		[SerializeField]
		MultiColumnHeaderState m_HeaderState;
		[SerializeField]
		SerializableElementRoot m_SerializableElement;
		// [SerializeField]
		// View.Type m_ViewType;
		[SerializeField]
		string m_SearchString;
		[System.NonSerialized]
		List<Element> m_Elements;
		[System.NonSerialized]
		SearchField m_SearchField;
		[System.NonSerialized]
		SearchFilter m_SearchFilter;
		[System.NonSerialized]
		View m_View;
		[System.NonSerialized]
		PopupList.InputData m_ObjectTypes;
		
		// [System.NonSerialized]
		// string m_ListTexture;
		// [System.NonSerialized]
		// string m_TreeTexture;
		[System.NonSerialized]
		string m_TypeTexture;
	}
}
