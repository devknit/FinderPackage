
namespace Finder {

public class ElementSource
{
	public ElementSource( string path, int reference=-1)
	{
		this.path = path;
		this.reference = reference;
	}
	public string path;
	public int reference;
}
public sealed class ElementComponentSource : ElementSource
{
	public ElementComponentSource( string name, System.Type type, 
		string path, int reference) : base( path, reference)
	{
		this.name = name;
		this.type = type;
		this.findPath = string.Empty;
		this.localId = 0;
	}
	public ElementComponentSource( string name, System.Type type, 
		string findPath, long localId, string path, int reference) : base( path, reference)
	{
		this.name = name;
		this.type = type;
		this.findPath = findPath;
		this.localId = localId;
	}
	public string name;
	public string findPath;
	public long localId;
	public System.Type type;
}

} /* namespace Finder */
