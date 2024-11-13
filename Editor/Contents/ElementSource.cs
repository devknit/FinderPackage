
namespace Finder
{
	public class ElementSource
	{
		public ElementSource( string path, int reference, int missing)
		{
			m_Path = path;
			m_Reference = reference;
			m_Missing = missing;
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
		public int Missing
		{
			get{ return m_Missing; }
			internal set{ m_Missing = value; }
		}
		readonly string m_Path;
		int m_Reference;
		int m_Missing;
	}
	public sealed class ElementComponentSource : ElementSource
	{
		public ElementComponentSource( string name, System.Type type, 
			string findPath, long localId, string path, int reference, int missing) : base( path, reference, missing)
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
