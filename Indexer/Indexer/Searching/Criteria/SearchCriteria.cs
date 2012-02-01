namespace Sando.Indexer.Searching.Criteria
{
	public abstract class SearchCriteria
	{
		public abstract string ToQueryString();
		
		public override bool Equals(object obj)
		{
			return this.ToQueryString().Equals(obj);
		}

		public override int GetHashCode()
		{
			return this.ToQueryString().GetHashCode();
		}
	}
}
