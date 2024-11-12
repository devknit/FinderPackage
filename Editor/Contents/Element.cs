#define WITH_TREEVIEWITEM
#define WITH_SERIALIZE_LOCALFILEIDENTIFIER

using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;

namespace Finder
{
#if WITH_TREEVIEWITEM
	public sealed class Element : TreeViewItem
#else
	public sealed class Element
#endif
	{
		static string EnclosedString( string src, string begin, string end)
		{
			int beginIndex = src.LastIndexOf( begin);
			int endIndex = src.IndexOf( end);
			if( beginIndex >= 0 && endIndex >= 0 && beginIndex < endIndex)
			{
				return src.Substring( ++beginIndex, endIndex - beginIndex);
			}
			return string.Empty;
		}
		public static Element Create( ElementSource source)
		{
			if( source is ElementComponentSource component)
			{
				var element = new Element();
				element.id = component.Path.GetHashCode();
				element.name = component.Name;
				element.Extension = string.Empty;
				element.Path = component.Path;
				element.Guid = string.Empty;
				element.Directory = false;
				element.Reference = component.Reference;
				element.AssetType = AssetType.kComponent;
				element.LocalId = component.LocalId;
				element.FindPath = component.FindPath;
				
				var content = EditorGUIUtility.ObjectContent( null, component.Type);
				element.icon = content.image as Texture2D;
				
				return element;
			}
			return Create( source.Path, source.Reference);
		}
		public static Element Create( string path, int reference=-1)
		{
			if( string.IsNullOrEmpty( path) == false && path.IndexOf( ":") < 0)
			{
				string guid = path switch
				{
					"Library" => "Library",
					"Packages" => "Packages",
					"ProjectSettings" => "ProjectSettings",
					_ => AssetDatabase.AssetPathToGUID( path)
				};
				if( string.IsNullOrEmpty( guid) == false)
				{
                    var element = new Element
                    {
                        id = path.GetHashCode(),
                        Extension = System.IO.Path.GetExtension( path),
                        Path = path,
                        Guid = guid,
                        icon = AssetDatabase.GetCachedIcon( path) as Texture2D,
                        Directory = AssetDatabase.IsValidFolder( path)
                    };
                    if( element.Directory != false)
					{
						element.name = System.IO.Path.GetFileName( path);
						element.AssetType = AssetType.kDirectory;
						element.Reference = -1;
					}
					else
					{
						element.name = System.IO.Path.GetFileNameWithoutExtension( path);
						
						if( AssetTypes.kExtensions.TryGetValue( element.Extension, out AssetType assetType) != false)
						{
							element.AssetType = assetType;
						}
						else
						{
							element.AssetType = AssetType.kUnknown;
						}
						element.Reference = reference;
					}
					return element;
				}
			}
			return null;
		}
		public static void TreeViewSort( List<Element> elements)
		{
			if( elements.Count > 0)
			{
				elements.Sort( (src1, src2) =>
				{
					return string.Compare( 
						src1.OnCompareString(), 
						src2.OnCompareString());
				});
			}
		}
		public static void ListViewSort( List<Element> elements)
		{
			if( elements.Count > 0)
			{
				elements.Sort( (src1, src2) =>
				{
					return string.Compare( src1.Path, src2.Path);
				});
			}
		}
		public Element()
		{
			ChildElements = new List<Element>();
		#if WITH_TREEVIEWITEM
			children = new List<TreeViewItem>();
		#endif
		}
	#if WITH_TREEVIEWITEM
		public Element( Element src)
		{
			id = src.id;
			depth = src.depth;
			name = src.name;
			Extension = src.Extension;
			Path = src.Path;
			Guid = src.Guid;
			AssetType = src.AssetType;
			icon = src.icon;
			FindPath = src.FindPath;
			Directory = src.Directory;
			Reference = src.Reference;
			LocalId = src.LocalId;
			ParentElement = src.ParentElement;
			ChildElements = src.ChildElements;
			parent = src.parent;
			children = src.children;
		}
		public Element( SerializableElementNode node, List<Element> srcChildElements, List<TreeViewItem> srcChildren)
	#else
		public Element( SerializableElementNode node, List<Element> srcChildElements)
	#endif
		{
			id = node.id;
			depth = node.depth;
			name = node.name;
			Extension = node.Extension;
			Path = node.Path;
			Guid = node.Guid;
			AssetType = node.AssetType;
			icon = node.icon;
			FindPath = node.FindPath;
			Directory = node.Directory;
			Reference = node.Reference;
			LocalId = node.LocalId;
			ChildElements = srcChildElements;
			
			foreach( var child in ChildElements)
			{
				child.ParentElement = this;
			}
		#if WITH_TREEVIEWITEM
			children = srcChildren;
			foreach( var child in srcChildren)
			{
				child.parent = this;
			}
		#endif
		}
		public void Add( Element element)
		{
			if( element.ParentElement != null)
			{
				element.ParentElement.Remove( element);
			}
		#if WITH_TREEVIEWITEM
			children.Add( element);
			parent = this;
		#endif
			ChildElements.Add( element);
			element.ParentElement = this;
			element.depth = depth + 1;
		}
		public void Remove( Element element)
		{
			if( ChildElements.Contains( element) != false)
			{
			#if WITH_TREEVIEWITEM
				children.Remove( element);
				element.parent = null;
			#endif
				ChildElements.Remove( element);
				element.ParentElement = null;
				element.depth = 0;
			}
		}
		public bool IsFile()
		{
			return Directory == false && AssetType != AssetType.kComponent;
		}
		public bool CheckFilter( SearchFilter filter)
		{
			bool bValid = false;
			
			ValidCount = 0;
			
			foreach( var child in ChildElements)
			{
				if( child.CheckFilter( filter) != false)
				{
					++ValidCount;
				}
			}
			if( ValidCount > 0 || (Directory == false && filter.Check( this) != false))
			{
				if( Directory == false)
				{
					++ValidCount;
				}
				bValid = true;
			}
			return bValid;
		}
		public bool CanOpenAsset()
		{
			return true;
		}
		public void OpenAsset()
		{
			if( LocalId != 0 && string.IsNullOrEmpty( FindPath) == false)
			{
				GameObject gameObject = FindGameObject( FindPath, LocalId);
				if( gameObject != null)
				{
					Selection.activeObject = gameObject;
				}
			}
			else
			{
				AssetDatabase.OpenAsset( AssetDatabase.LoadMainAssetAtPath( Path));
			}
		}
		public bool CanPingObject()
		{
			return true;
		}
		public void PingObject( bool bDirectory)
		{
			if( bDirectory != false || Directory == false)
			{
				if( LocalId != 0 && string.IsNullOrEmpty( FindPath) == false)
				{
					GameObject gameObject = FindGameObject( FindPath, LocalId);
					if( gameObject != null)
					{
						EditorGUIUtility.PingObject( gameObject);
					}
				}
				else
				{
					EditorGUIUtility.PingObject( AssetDatabase.LoadMainAssetAtPath( Path));
				}
			}
		}
		public bool CanActiveObject()
		{
			return true;
		}
		public void ActiveObject( bool bDirectory)
		{
			if( bDirectory != false || Directory == false)
			{
				if( LocalId != 0 && string.IsNullOrEmpty( FindPath) == false)
				{
					GameObject gameObject = FindGameObject( FindPath, LocalId);
					if( gameObject != null)
					{
						Selection.activeObject = gameObject;
					}
				}
				else
				{
					Selection.activeObject = AssetDatabase.LoadMainAssetAtPath( Path);
				}
			}
		}
		internal string OnCompareString()
		{
			string compare = string.Empty;
			
			if( Directory != false)
			{
				compare = kComparePrefix;
			}
			compare += name;
			
			return compare;
		}
		static GameObject FindGameObject( string findPath, long localId)
		{
			var prefabStage = UnityEditor.SceneManagement.PrefabStageUtility.GetCurrentPrefabStage();
			if( prefabStage != null)
			{
				string[] directories = findPath.Split( '/');
				var selectObjects = new List<GameObject>();
				SelectGameObjects( selectObjects, prefabStage.prefabContentsRoot, directories, 0);
				if( selectObjects.Count > 0)
				{
					if( selectObjects.Count == 1)
					{
						return selectObjects[ 0];
					}
					else
					{
						Selection.instanceIDs = selectObjects.Select( x => x.GetInstanceID()).ToArray();
					}
				}
			}
			else
			{
				var gameObjects = Resources.FindObjectsOfTypeAll( typeof( GameObject)) as GameObject[];
				var objects = gameObjects.Where( c => (c.hideFlags & kNotHierarchy) == 0);
				return objects.FirstOrDefault( x => GetLocalIdFromGameObject( x) == localId);
			}
			return null;
		}
		static void SelectGameObjects( List<GameObject> selectObjects, GameObject gameObject, string[] directories, int depth)
		{
			if( gameObject.name == directories[ depth])
			{
				if( depth == directories.Length - 1)
				{
					selectObjects.Add( gameObject);
				}
				else
				{
					Transform transform = gameObject.transform;
					foreach( Transform child in transform)
					{
						SelectGameObjects( selectObjects, child.gameObject, directories, depth + 1);
					}
				}
			}
		}
		static bool TryGetLocalFileIdentifier( Object targetObject, out long localId)
		{
			if( targetObject != null)
			{
			#if WITH_SERIALIZE_LOCALFILEIDENTIFIER
				if( cachedInspectorModeInfo == null)
				{
					cachedInspectorModeInfo = typeof( SerializedObject).GetProperty( 
						"inspectorMode", BindingFlags.NonPublic | BindingFlags.Instance);
				}
				var serializedObject = new SerializedObject( targetObject);
				cachedInspectorModeInfo.SetValue( serializedObject, InspectorMode.Debug, null);
				SerializedProperty property = serializedObject.FindProperty( "m_LocalIdentfierInFile");
				if( property != null)
				{
					localId = property.longValue;
					return localId != 0;
				}
			#else
				string guid;
				return AssetDatabase.TryGetGUIDAndLocalFileIdentifier( targetObject, out guid, out localId);
			#endif
			}
			localId = 0;
			return false;
		}
		static long GetLocalIdFromGameObject( GameObject instanceObject)
		{
			long ret = 0;
			
			if( instanceObject != null)
			{
				if( TryGetLocalFileIdentifier( instanceObject.transform, out long localId) != false)
				{
					ret = localId;
				}
				else
				{
					var serializedObject = new SerializedObject( instanceObject);
					SerializedProperty property = 
						serializedObject.FindProperty( "m_CorrespondingSourceObject");
					
					if( property != null && property.objectReferenceValue != null)
					{
						var gameObject = property.objectReferenceValue as GameObject;
						if( TryGetLocalFileIdentifier( gameObject.transform, out localId) != false)
						{
							ret = localId;
						}
					}
				}
			}
			return ret;
		}
	#if WITH_SERIALIZE_LOCALFILEIDENTIFIER
		static PropertyInfo cachedInspectorModeInfo = null;
	#endif
		const HideFlags kNotHierarchy = HideFlags.NotEditable | HideFlags.HideAndDontSave;
		static readonly string kComparePrefix = 
			System.Text.Encoding.ASCII.GetString( 
				Enumerable.Repeat( (byte)0x20, 260).ToArray());
		
	#if WITH_TREEVIEWITEM
		public override string displayName{ get{ return name; } set{} }
	#endif
	#if !WITH_TREEVIEWITEM
		public int id{ get; private set; }
		public int depth{ get; private set; }
	#endif
		public string name{ get; private set; }
		public string Extension{ get; private set; }
		public string Path{ get; private set; }
		public string Guid{ get; private set; }
		public string FindPath{ get; private set; }
		public AssetType AssetType{ get; private set; }
	#if !WITH_TREEVIEWITEM
		public Texture2D icon{ get; private set; }
	#endif
		public bool Directory{ get; private set; }
		public int Reference{ get; private set; }
		public long LocalId{ get; private set; }
		public Element ParentElement{ get; internal set; }
		public List<Element> ChildElements{ get; internal set; }
		
		public int ValidCount{ get; internal set; }
	}
	[System.Serializable]
	public class SerializableElementRoot
	{
		public SerializableElementRoot()
		{
			root = new List<SerializableElementNode>();
		}
		public void OnBeforeSerialize( Element element)
		{
			root.Clear();
			Serialize( element);
		}
		public Element OnAfterDeserialize()
		{
			if( root.Count > 0)
			{
				int count;
				return Deserialize( 0, out count);
			}
			return new Element();
		}
		void Serialize( Element element)
		{
			root.Add( new SerializableElementNode( element, root.Count + 1));
			
			foreach( var child in element.ChildElements)
			{
				Serialize( child);
			}
		}
		Element Deserialize( int index, out int count)
		{
			SerializableElementNode node = root[ index];
		
		#if WITH_TREEVIEWITEM
			var children = new List<TreeViewItem>();
		#endif	
			var ChildElements = new List<Element>();
			Element element;
			int childCount;
			int offset = 0;
			
			for( int i0 = 0; i0 < node.ChildCount; ++i0)
			{
				element = Deserialize( node.IndexOfFirstChild + offset + i0, out childCount);
			#if WITH_TREEVIEWITEM
				children.Add( element);
			#endif
				ChildElements.Add( element);
				offset += childCount;
			}
			count = node.ChildCount + offset;
			
		#if WITH_TREEVIEWITEM
			return new Element( node, ChildElements, children);
		#else
			return new Element( node, ChildElements);
		#endif
		}
		[SerializeField]
		List<SerializableElementNode> root;
	}
	[System.Serializable]
	public class SerializableElementNode
	{
		public SerializableElementNode( Element element, int index)
		{
			id = element.id;
			depth = element.depth;
			name = element.name;
			Extension = element.Extension;
			Path = element.Path;
			Guid = element.Guid;
			AssetType = element.AssetType;
			icon = element.icon;
			FindPath = element.FindPath;
			Directory = element.Directory;
			Reference = element.Reference;
			LocalId = element.LocalId;
			ChildCount = element.ChildElements.Count;
			IndexOfFirstChild = index;
		}
		[SerializeField]
		public int id;
		[SerializeField]
		public int depth;
		[SerializeField]
		public string name;
		[SerializeField]
		public string Extension;
		[SerializeField]
		public string Path;
		[SerializeField]
		public string Guid;
		[SerializeField]
		public string FindPath;
		[SerializeField]
		public AssetType AssetType;
		[SerializeField]
		public Texture2D icon;
		[SerializeField]
		public bool Directory;
		[SerializeField]
		public int Reference;
		[SerializeField]
		public long LocalId;
		[SerializeField]
		public int ChildCount;
		[SerializeField]
		public int IndexOfFirstChild;
	}
} /* namespace Finder */
