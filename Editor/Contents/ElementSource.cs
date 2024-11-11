﻿
namespace Finder
{
	public class ElementSource
	{
		public ElementSource( string path, int reference=-1)
		{
			m_Path = path;
			m_Reference = reference;
		}
		public string Path
		{
			get{ return m_Path; }
		}
		public int Reference
		{
			get{ return m_Reference; }
			internal set{ m_Reference = value; }
		}
		readonly string m_Path;
		int m_Reference;
	}
	public sealed class ElementComponentSource : ElementSource
	{
		public ElementComponentSource( string name, System.Type type, 
			string path, int reference) : base( path, reference)
		{
			m_Name = name;
			m_Type = type;
			m_FindPath = string.Empty;
			m_LocalId = 0;
		}
		public ElementComponentSource( string name, System.Type type, 
			string findPath, long localId, string path, int reference) : base( path, reference)
		{
			m_Name = name;
			m_Type = type;
			m_FindPath = findPath;
			m_LocalId = localId;
		}
		public string Name
		{
			get{ return m_Name; }
		}
		public string FindPath
		{
			get{ return m_FindPath; }
		}
		public long LocalId
		{
			get{ return m_LocalId; }
		}
		public System.Type Type
		{
			get{ return m_Type; }
		}
		readonly string m_Name;
		readonly string m_FindPath;
		readonly long m_LocalId;
		readonly System.Type m_Type;
	}
}
