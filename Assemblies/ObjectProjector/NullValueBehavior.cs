namespace ObjectProjector
{
	/// <summary>
	/// How the projector should treat members who values are null.
	/// </summary>
	public enum NullValueBehavior
	{
		/// <summary>
		/// Skip over members that are null.
		/// Do not include them in the resulting projection.
		/// </summary>
		ExcludeNulls,
		
		/// <summary>
		/// Include members that are null.
		/// This results in a key that maps to a null value.
		/// </summary>
		IncludeNulls
	}
}
