
using System;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;
using System.Linq;

namespace Finder {

public class PopupList : PopupWindowContent
{
	public class Element
	{
		public Element( string text)
		{
			content = new GUIContent( text);
		}
		public GUIContent content;
		public bool selected;
		public string label{ get{ return content.text; } }
	}
	public class InputData
	{
		public InputData()
		{
			elements = new List<Element>();
		}
		public Element NewOrMatchingElement( string label)
		{
			foreach( var element in elements)
			{
				if( element.label.Equals( label, StringComparison.OrdinalIgnoreCase) != false)
				{
					return element;
				}
			}
			var newElement = new Element( label);
			elements.Add( newElement);
			
			return newElement;
		}
		public List<Element> elements;
		public Action<Element> onSelectCallback;
	}
	public PopupList( InputData inputData)
	{
		data = inputData;
	}
	public override Vector2 GetWindowSize()
	{
		return new Vector2( kWindowWidth, data.elements.Count * kLineHeight + 2 * kMargin);
	}
	public override void OnGUI( Rect windowRect)
	{
		Event ev = Event.current;
		
		if( ev.type == EventType.Layout)
		{
			return;
		}
		if( styles == null)
		{
			styles = new Styles();
		}
		if( ev.type == EventType.KeyDown && ev.keyCode == KeyCode.Escape)
		{
			editorWindow.Close();
			GUIUtility.ExitGUI();
		}
		int i0 = 0;
		
		foreach( Element element in data.elements)
		{
			Rect rect = new Rect(
				windowRect.x, 
				windowRect.y + kMargin + i0 * kLineHeight, 
				windowRect.width, 
				kLineHeight);
			
			switch( ev.type)
			{
				case EventType.Repaint:
				{
					bool selected = element.selected;
					bool hover = i0 == selectedIndex;
					styles.menuItem.Draw( rect, element.content, hover, selected, selected, false);
					break;
				}
				case EventType.MouseDown:
				{
					if( ev.button == 0 && rect.Contains( ev.mousePosition) != false)
					{
						data.onSelectCallback?.Invoke( element);
						ev.Use();
					}
					break;
				}
				case EventType.MouseMove:
				{
					if( rect.Contains( ev.mousePosition) != false)
					{
						selectedIndex = i0;
						ev.Use();
					}
					break;
				}
			}
			++i0;
		}
		if( ev.type == EventType.Repaint)
		{
			styles.background.Draw( windowRect, false, false, false, false);
		}
	}
	const float kWindowWidth = 150;
	const float kLineHeight = 16;
	const float kMargin = 10;
	
	class Styles
	{
		public GUIStyle menuItem = "MenuItem";
		public GUIStyle background = "grey_border";
	}
	static Styles styles;
	
	InputData data;
	int selectedIndex;
}

} /* namespace Finder */
