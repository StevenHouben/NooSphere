namespace ABC.Model.Primitives
{
	public interface INoo
	{
		string Id { get; set; }
		string Name { get; set; }
		string Description { get; set; }
		string Uri { get; set; }
		string BaseType { get; set; }
		void UpdateAllProperties( object newUser );
	}
}