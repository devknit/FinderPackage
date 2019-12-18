#define WITH_TREEVIEWITEM
#define WITH_SERIALIZE_LOCALFILEIDENTIFIER

using System.Linq;
using System.Reflection;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.IMGUI.Controls;
using UnityEditor.Experimental.SceneManagement;

namespace Finder {

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
			element.id = component.path.GetHashCode();
			element.name = component.name;
			element.extension = string.Empty;
			element.path = component.path;
			element.guid = string.Empty;
			element.directory = false;
			element.reference = component.reference;
			element.type = AssetType.kComponent;
			element.localId = component.localId;
			element.findPath = component.findPath;
			
			var content = EditorGUIUtility.ObjectContent( null, component.type);
			element.icon = content.image as Texture2D;
			
			return element;
		}
		return Create( source.path, source.reference);
	}
	public static Element Create( string path, int reference=-1)
	{
		if( string.IsNullOrEmpty( path) == false && path.IndexOf( ":") < 0)
		{
			string guid = AssetDatabase.AssetPathToGUID( path);
			if( string.IsNullOrEmpty( guid) == false)
			{
				var element = new Element();
				element.id = path.GetHashCode();
				element.name = System.IO.Path.GetFileNameWithoutExtension( path);
				element.extension = System.IO.Path.GetExtension( path);
				element.path = path;
				element.guid = guid;
				element.icon = AssetDatabase.GetCachedIcon( path) as Texture2D;
				element.directory = AssetDatabase.IsValidFolder( path);
				
				if( element.directory != false)
				{
					element.type = AssetType.kDirectory;
					element.reference = -1;
				}
				else
				{
					AssetType type;
					
					if( AssetTypes.kExtensions.TryGetValue( element.extension, out type) != false)
					{
						element.type = type;
					}
					else
					{
						element.type = AssetType.kUnknown;
					}
					element.reference = reference;
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
				return string.Compare( src1.path, src2.path);
			});
		}
	}
	public Element()
	{
		childElements = new List<Element>();
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
		extension = src.extension;
		path = src.path;
		guid = src.guid;
		type = src.type;
		icon = src.icon;
		findPath = src.findPath;
		directory = src.directory;
		reference = src.reference;
		localId = src.localId;
		parentElement = src.parentElement;
		childElements = src.childElements;
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
		extension = node.extension;
		path = node.path;
		guid = node.guid;
		type = node.type;
		icon = node.icon;
		findPath = node.findPath;
		directory = node.directory;
		reference = node.reference;
		localId = node.localId;
		childElements = srcChildElements;
		foreach( var child in childElements)
		{
			child.parent = this;
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
		if( element.parentElement != null)
		{
			element.parentElement.Remove( element);
		}
	#if WITH_TREEVIEWITEM
		children.Add( element);
		parent = this;
	#endif
		childElements.Add( element);
		element.parentElement = this;
		element.depth = depth + 1;
	}
	public void Remove( Element element)
	{
		if( childElements.Contains( element) != false)
		{
		#if WITH_TREEVIEWITEM
			children.Remove( element);
			element.parent = null;
		#endif
			childElements.Remove( element);
			element.parentElement = null;
			element.depth = 0;
		}
	}
	public bool CheckFilter( SearchFilter filter)
	{
		bool bValid = false;
		
		validCount = 0;
		
		foreach( var child in childElements)
		{
			if( child.CheckFilter( filter) != false)
			{
				++validCount;
			}
		}
		if( validCount > 0 || (directory == false && filter.Check( this) != false))
		{
			if( directory == false)
			{
				++validCount;
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
		if( localId != 0 && string.IsNullOrEmpty( findPath) == false)
		{
			GameObject gameObject = FindGameObject( findPath, localId);
			if( gameObject != null)
			{
				Selection.activeObject = gameObject;
			}
		}
		else
		{
			AssetDatabase.OpenAsset( AssetDatabase.LoadMainAssetAtPath( path));
		}
	}
	public bool CanPingObject()
	{
		return true;
	}
	public void PingObject( bool bDirectory)
	{
		if( bDirectory != false || directory == false)
		{
			if( localId != 0 && string.IsNullOrEmpty( findPath) == false)
			{
				GameObject gameObject = FindGameObject( findPath, localId);
				if( gameObject != null)
				{
					EditorGUIUtility.PingObject( gameObject);
				}
			}
			else
			{
				EditorGUIUtility.PingObject( AssetDatabase.LoadMainAssetAtPath( path));
			}
		}
	}
	public bool CanActiveObject()
	{
		return true;
	}
	public void ActiveObject( bool bDirectory)
	{
		if( bDirectory != false || directory == false)
		{
			if( localId != 0 && string.IsNullOrEmpty( findPath) == false)
			{
				GameObject gameObject = FindGameObject( findPath, localId);
				if( gameObject != null)
				{
					Selection.activeObject = gameObject;
				}
			}
			else
			{
				Selection.activeObject = AssetDatabase.LoadMainAssetAtPath( path);
			}
		}
	}
	internal string OnCompareString()
	{
		string compare = string.Empty;
		
		if( directory != false)
		{
			compare = kComparePrefix;
		}
		compare += name;
		
		return compare;
	}
	static GameObject FindGameObject( string findPath, long localId)
	{
		var prefabStage = PrefabStageUtility.GetCurrentPrefabStage();
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
			long localId;
			
			if( TryGetLocalFileIdentifier( instanceObject.transform, out localId) != false)
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
	static string kComparePrefix = 
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
	public string extension{ get; private set; }
	public string path{ get; private set; }
	public string guid{ get; private set; }
	public string findPath{ get; private set; }
	public AssetType type{ get; private set; }
#if !WITH_TREEVIEWITEM
	public Texture2D icon{ get; private set; }
#endif
	public bool directory{ get; private set; }
	public int reference{ get; private set; }
	public long localId{ get; private set; }
	public Element parentElement{ get; internal set; }
	public List<Element> childElements{ get; internal set; }
	
	public int validCount;
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
		
		foreach( var child in element.childElements)
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
		var childElements = new List<Element>();
		Element element;
		int childCount;
		int offset = 0;
		
		for( int i0 = 0; i0 < node.childCount; ++i0)
		{
			element = Deserialize( node.indexOfFirstChild + offset + i0, out childCount);
		#if WITH_TREEVIEWITEM
			children.Add( element);
		#endif
			childElements.Add( element);
			offset += childCount;
		}
		count = node.childCount + offset;
		
	#if WITH_TREEVIEWITEM
		return new Element( node, childElements, children);
	#else
		return new Element( node, childElements);
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
		extension = element.extension;
		path = element.path;
		guid = element.guid;
		type = element.type;
		icon = element.icon;
		findPath = element.findPath;
		directory = element.directory;
		reference = element.reference;
		localId = element.localId;
		childCount = element.childElements.Count;
		indexOfFirstChild = index;
	}
	[SerializeField]
	public int id;
	[SerializeField]
	public int depth;
	[SerializeField]
	public string name;
	[SerializeField]
	public string extension;
	[SerializeField]
	public string path;
	[SerializeField]
	public string guid;
	[SerializeField]
	public string findPath;
	[SerializeField]
	public AssetType type;
	[SerializeField]
	public Texture2D icon;
	[SerializeField]
	public bool directory;
	[SerializeField]
	public int reference;
	[SerializeField]
	public long localId;
	[SerializeField]
	public int childCount;
	[SerializeField]
	public int indexOfFirstChild;
}

} /* namespace Finder */
