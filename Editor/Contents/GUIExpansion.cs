
using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace Finder {

public static class GUIExpansion
{
	public static int Toggle( bool value, GUIContent content, GUIStyle style, params GUILayoutOption[] options)
	{
		Rect rect = GUILayoutUtility.GetRect( content, style, options);
		return DoToggle( rect, GUIUtility.GetControlID( kToggleHash, FocusType.Passive, rect), value, content, style);
	}
	static int DoToggle( Rect rect, int id, bool value, GUIContent content, GUIStyle style)
	{
		return DoControl( rect, id, value, rect.Contains( Event.current.mousePosition), content, style);
	}
	static int DoControl( Rect rect, int id, bool on, bool hover, GUIContent content, GUIStyle style)
	{
		Event ev = Event.current;
		
		switch( ev.type)
		{
			case EventType.Repaint:
			{
				if( hover != false && HasMouseControl( id) < 0)
				{
					hover = false;
				}
				style.Draw( rect, content, hover, true, on, false);
				break;
			}
			case EventType.MouseDown:
			{
				if( rect.Contains( ev.mousePosition) != false)
				{
					GrabMouseControl( id, ev.button);
					ev.Use();
				}
				break;
			}
			case EventType.MouseUp:
			{
				int button = HasMouseControl( id);
				if( button >= 0)
                {
					ReleaseMouseControl();
					ev.Use();
					
					if( rect.Contains( ev.mousePosition) != false)
					{
						if( button == ev.button)
						{
							return button;
						}
					}
				}
				break;
			}
			case EventType.MouseDrag:
			{
				if( HasMouseControl( id) >= 0)
				{
					ev.Use();
				}
				break;
			}
		}
		return -1;
	}
	internal static void GrabMouseControl( int id, int button)
	{
		if( mouseControl.ContainsKey( id) == false)
		{
			mouseControl.Add( id, button);
		}
	}
	internal static int HasMouseControl( int id)
	{
		int button;
		
		if( mouseControl.TryGetValue( id, out button) == false)
		{
			button = -1;
		}
		return button;
	}
	internal static void ReleaseMouseControl()
	{
		mouseControl.Clear();
	}
	internal static void GrabKeyControl( KeyCode code, int id)
	{
		if( keyControl.ContainsKey( code) == false)
		{
			keyControl.Add( code, id);
		}
	}
	internal static bool HasKeyControl( KeyCode code)
	{
		return keyControl.ContainsKey( code);
	}
	internal static bool HasKeyControl( KeyCode code, int id)
	{
		int value;
		
		if( keyControl.TryGetValue( code, out value) != false)
		{
			return value == id;
		}
		return false;
	}
	internal static void ReleaseKeyControl( KeyCode code)
	{
		if( keyControl.ContainsKey( code) != false)
		{
			keyControl.Remove( code);
		}
	}
	static readonly int kToggleHash = "ExToggle".GetHashCode();
	
	static Dictionary<int, int> mouseControl = new Dictionary<int, int>();
	static Dictionary<KeyCode, int> keyControl = new Dictionary<KeyCode, int>();
}

} /* namespace Finder */
