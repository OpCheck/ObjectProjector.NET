namespace ObjectProjector
{
	/// <summary>
	/// How the object projector selects whether a member should be in a projection hash.
	/// </summary>
	public enum IncludeBehavior
	{
		/// <summary>
		/// Do not include any members into the resulting projection unless explicitly included.
		/// </summary>
		IncludeNone,
		
		/// <summary>
		/// Include all members into the resulting projection unless explicitly excluded.
		/// </summary>
		IncludeAll
	}
}
