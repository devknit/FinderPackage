
using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using System.Collections.Generic;
using UnityEditorInternal;

namespace Finder {

public sealed class ElementBuilder
{
	public ElementBuilder()
	{
		rootElements = new List<Element>();
		registered = new SortedDictionary<string, Element>();
	}
	public List<Element> ToList()
	{
		return rootElements.ToList();
	}
	public bool Append( ElementSource source)
	{
		if( source != null && string.IsNullOrEmpty( source.path) == false)
		{
			string[] elementNames = source.path.Split( '/');
			string elementName;
			string path = string.Empty;
			Element element = null;
			Element parent;
			
			for( int i0 = 0; i0 < elementNames.Length; ++i0)
			{
				elementName = elementNames[ i0];
				
				if( string.IsNullOrEmpty( path) == false)
				{
					path += "/";
				}
				path += elementName;
				parent = element;
				
				if( registered.TryGetValue( path, out element) == false)
				{
					if( i0 == elementNames.Length - 1)
					{
						element = Element.Create( source);
					}
					else
					{
						element = Element.Create( path);
					}
					if( element == null)
					{
						return false;
					}
					else
					{
						if( parent != null)
						{
							parent.Add( element);
						}
						if( i0 == 0)
						{
							rootElements.Add( element);
						}
						registered.Add( path, element);
					}
				}
			}
			return true;
		}
		return false;
	}
	public bool Append( string assetPath, int reference=-1)
	{
		if( string.IsNullOrEmpty( assetPath) == false)
		{
			string[] elementNames = assetPath.Split( '/');
			string elementName;
			string path = string.Empty;
			Element element = null;
			Element parent;
			
			for( int i0 = 0; i0 < elementNames.Length; ++i0)
			{
				elementName = elementNames[ i0];
				
				if( string.IsNullOrEmpty( path) == false)
				{
					path += "/";
				}
				path += elementName;
				parent = element;
				
				if( registered.TryGetValue( path, out element) == false)
				{
					element = Element.Create( path, reference);
					if( element == null)
					{
						return false;
					}
					else
					{
						if( parent != null)
						{
							parent.Add( element);
						}
						if( i0 == 0)
						{
							rootElements.Add( element);
						}
						registered.Add( path, element);
					}
				}
			}
			return true;
		}
		return false;
	}
	
	List<Element> rootElements;
	SortedDictionary<string, Element> registered;
}

} /* namespace Finder */
