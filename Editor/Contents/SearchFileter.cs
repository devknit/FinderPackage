
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Finder {

public sealed class SearchFilter
{
	public bool valid
	{
		get;
		private set;
	}
	public void Change( string value)
	{
		valid = false;
		
		names.Clear();
		paths.Clear();
		types.Clear();
		
		if( value != null)
		{
			value = value.Trim();
			
			if( value.Length > 0)
			{
				string[] argv = value.Split( ' ');
				AssetType type;
				string arg;
				
				for( int i0 = 0; i0 < argv.Length; ++i0)
				{
					arg = argv[ i0];
					
					if( AssetTypes.kFilters.TryGetValue( arg, out type) != false)
					{
						if( types.ContainsKey( type) == false)
						{
							types.Add( type, arg.Substring( 2, arg.Length - 2));
						}
					}
					else
					{
						if( arg.IndexOf( "p:~/", StringComparison.OrdinalIgnoreCase) >= 0)
						{
							arg = arg.Substring( 4, arg.Length - 4);
							
							if( arg.Length > 0 && paths.ContainsKey( arg) == false)
							{
								paths.Add( arg, 0);
							}
						}
						else if( arg.IndexOf( "p:", StringComparison.OrdinalIgnoreCase) >= 0)
						{
							arg = arg.Substring( 2, arg.Length - 2);
							
							if( arg.Length > 0 && paths.ContainsKey( arg) == false)
							{
								paths.Add( arg, 1);
							}
						}
						else if( names.Contains( arg) == false)
						{
							names.Add( arg);
						}
					}
				}
				if( names.Count > 0 || paths.Count > 0 || types.Count > 0)
				{
					valid = true;
				}
			}
		}
		onChangeCallback?.Invoke( valid);
	}
	public bool Check( Element element)
	{
		if( valid != false)
		{
			if( types.Count > 0)
			{
				if( types.ContainsKey( element.type) == false)
				{
					return false;
				}
			}
			if( paths.Count > 0)
			{
				bool check = false;
				
				foreach( var path in paths)
				{
					if( path.Value == 0)
					{
						if( element.path.IndexOf( path.Key, StringComparison.OrdinalIgnoreCase) == 0)
						{
							check = true;
							break;
						}
					}
					else if( path.Value == 1)
					{
						if( element.path.IndexOf( path.Key, StringComparison.OrdinalIgnoreCase) >= 0)
						{
							check = true;
							break;
						}
					}
				}
				if( check == false)
				{
					return false;
				}
			}
			foreach( var name in names)
			{
				if( element.name.IndexOf( name, StringComparison.OrdinalIgnoreCase) < 0)
				{
					return false;
				}
			}
		}
		return true;
	}
	public void ToBuildStringNames( System.Text.StringBuilder builder)
	{
		foreach( var name in names)
		{
			if( builder.Length > 0)
			{
				builder.Append( " ");
			}
			builder.Append( name);
		}
	}
	public void ToBuildStringPaths( System.Text.StringBuilder builder)
	{
		foreach( var path in paths)
		{
			if( builder.Length > 0)
			{
				builder.Append( " ");
			}
			if( path.Value == 0)
			{
				builder.Append( "p:~/");
			}
			else if( path.Value == 1)
			{
				builder.Append( "p:");
			}
			builder.Append( path.Key);
		}
	}
	public void ToBuildStringTypes( System.Text.StringBuilder builder)
	{
		foreach( var type in types.Values)
		{
			if( builder.Length > 0)
			{
				builder.Append( " ");
			}
			builder.Append( "t:");
			builder.Append( type);
		}
	}
	public SearchFilter( Action<bool> onFilterChange=null)
	{
		names = new List<string>();
		paths = new Dictionary<string, int>();
		types = new Dictionary<AssetType, string>();
		onChangeCallback = onFilterChange;
	}
	
	public List<string> names;
	public Dictionary<string, int> paths;
	public Dictionary<AssetType, string> types;
	Action<bool> onChangeCallback;
}

} /* namespace Finder */
