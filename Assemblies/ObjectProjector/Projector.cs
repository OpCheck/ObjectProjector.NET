using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace ObjectProjector
{
	/// <summary>
	/// Creates a hash table containing a subset of the fields and properties of the source object before we serialize it and send it over a network.
	/// Creating a projection is the final step before pumping it to a serialization library that is smart enough to work with any object, such as Newtonsoft.Json.
	/// Using this strategy, we do not have to write any additional classes - so this prevents the proliferation of specialized view model classes.
	/// In addition, if we need to change the projection then we can do it where the projection is defined.
	/// We can exclude fields that we do not need and/or are restricted, reducing its size - which makes it faster to transfer over the network.
	/// We can also change the name of included fields.
	/// This class is highly configurable from top to bottom.
	/// </summary>
	public class Projector
	{
		/// <summary>
		/// Project the source object member names using the default behaviors of the projector.
		/// This is a convenience method.
		/// </summary>
		public static Hashtable CreateProjection (object SourceObject, string[] MemberNames)
		{
			Projector CreatedProjector = new Projector();
			CreatedProjector.Include(MemberNames);
			return CreatedProjector.CreateProjection(SourceObject);
		}


		/// <summary>
		/// Creates a new instance of this projector using the specified source object.
		/// </summary>
		public Projector (object SourceObject) : this()
		{
			_Source = SourceObject;
		}


		/// <summary>
		/// Creates a new instance of this projector.
		/// The source object must be set later.
		/// </summary>
		public Projector ()
		{
			_IncludedMemberNames = new Dictionary<string, bool>();
			_ExcludedMemberNames = new Dictionary<string, bool>();
			_MemberRenameMap = new Dictionary<string, string>();
			
			_IncludedPropertyNames = new Dictionary<string, bool>();
			_ExcludedPropertyNames = new Dictionary<string, bool>();
			_PropertyRenameMap = new Dictionary<string, string>();
			
			_IncludedFieldNames = new Dictionary<string, bool>();
			_ExcludedFieldNames = new Dictionary<string, bool>();
			_FieldRenameMap = new Dictionary<string, string>();
		}
		
		
		/// <summary>
		/// Include the field or property in the projection.
		/// This is called "explicit member inclusion."
		/// This is the same as calling the IncludeMember method.
		/// </summary>
		public void Include (string MemberName)
		{
			IncludeMember(MemberName);
		}


		public void IncludeAs (string MemberName, string OutputName)
		{
			IncludeMember(MemberName);
			RenameMemberAs(MemberName, OutputName);
		}
		
		
		public void RenameMemberAs (string MemberName, string OutputName)
		{
			_MemberRenameMap[MemberName] = OutputName;
		}


		public void Exclude (string MemberName)
		{
			ExcludeMember(MemberName);
		}


		/// <summary>
		/// Include the field or properties with these names in the resulting projection.
		/// This is called "explicit member inclusion."
		/// This is the same thing as calling the IncludeMembers method.
		/// </summary>
		public void Include (string[] MemberNames)
		{
			IncludeMembers(MemberNames);
		}


		public void Exclude (string[] MemberNames)
		{
			ExcludeMembers(MemberNames);
		}


		/// <summary>
		/// Include the field or property in the projection.
		/// This is called "explicit member inclusion."
		/// This is the same as calling the Include method.
		/// </summary>
		public void IncludeMember (string MemberName)
		{
			_IncludedMemberNames[MemberName] = true;
		}


		public void ExcludeMember (string MemberName)
		{
			_ExcludedMemberNames[MemberName] = true;
		}


		/// <summary>
		/// Include the field or properties with these names in the resulting projection.
		/// This is called "explicit member inclusion."
		/// This is the same thing as calling the array version of the Include method.
		/// </summary>
		public void IncludeMembers (string[] MemberNames)
		{
			foreach (string MemberName in MemberNames)
			{
				IncludeMember(MemberName);
			}
		}


		public void ExcludeMembers (string[] MemberNames)
		{
			foreach (string MemberName in MemberNames)
			{
				ExcludeMember(MemberName);
			}
		}


		/// <summary>
		/// Include the property in the projection.
		/// This is called "explicit property inclusion."
		/// </summary>
		public void IncludeProperty (string PropertyName)
		{
			_IncludedPropertyNames[PropertyName] = true;
		}


		/// <summary>
		/// Exclude the property with this name.
		/// It will not be in the resulting projection.
		/// </summary>
		public void ExcludeProperty (string PropertyName)
		{
			_ExcludedPropertyNames[PropertyName] = true;
		}


		/// <summary>
		/// Include only the properties with these names in the resulting projection.
		/// </summary>
		public void IncludeProperties (string[] PropertyNames)
		{
			foreach (string PropertyName in PropertyNames)
			{
				IncludeProperty(PropertyName);
			}
		}

		/// <summary>
		/// Exclude the properties with these names.
		/// They will not be in the resulting projection.
		/// </summary>
		public void ExcludeProperties (string[] PropertyNames)
		{
			foreach (string PropertyName in PropertyNames)
			{
				ExcludeProperty(PropertyName);
			}
		}


		/// <summary>
		/// Include the field in the projection.
		/// </summary>
		public void IncludeField (string FieldName)
		{
			_IncludedFieldNames[FieldName] = true;
		}


		public void ExcludeField (string FieldName)
		{
			_ExcludedFieldNames[FieldName] = true;
		}


		public void IncludeFields (string[] FieldNames)
		{
			foreach (string FieldName in FieldNames)
			{
				IncludeField(FieldName);
			}
		}


		public void ExcludeFields (string[] FieldNames)
		{
			foreach (string FieldName in FieldNames)
			{
				ExcludeField(FieldName);
			}
		}
	

		/// <summary>
		/// Creates an array of projection using the specified array of source objects.
		/// </summary>
		public Hashtable[] CreateProjections (object[] SourceObjects)
		{
			List<Hashtable> ProjectionList = new List<Hashtable>();
		
			foreach (object CurrentSourceObject in SourceObjects)
			{
				ProjectionList.Add(CreateProjection(CurrentSourceObject));
			}
			
			return ProjectionList.ToArray();
		}


		/// <summary>
		/// Creates a projection using the specified object.
		/// </summary>
		public Hashtable CreateProjection (object SourceObject)
		{
			//
			// RENDER THE PROPERTIES OF THE SOURCE OBJECT.
			//
			if (_IncludeBehavior == IncludeBehavior.IncludeNone)
				return CreateExclusiveProjection(SourceObject);
			else
				return CreateInclusiveProjection(SourceObject);
		}
		
		
		/// <summary>
		/// Creates a projection of the specified source object where by default, all members are included if not explicitly excluded.
		/// </summary>
		private Hashtable CreateInclusiveProjection (object SourceObject)
		{
			//
			// CREATE THE HASH TABLE.
			// ALL PROJECTIONS ARE "OLD SCHOOL" HASH TABLES.
			//
			Hashtable CreatedProjection = new Hashtable();

			//
			// GET THE TYPE OF THE ENTITY.
			// WE USE THIS TO REFLECT AGAINST THE SOURCE OBJECT SEVERAL TIMES.
			//
			Type SourceObjectType = SourceObject.GetType();

			//
			// BY DEFAULT, THIS METHOD INCLUDES ALL MEMBERS UNLESS THEY WERE EXPLICITLY EXCLUDED.
			//
			
			//
			// PROCESS PROPERTIES.
			//
			foreach (PropertyInfo CurrentPropertyInfo in SourceObjectType.GetProperties())
			{
				//
				// RENDER ALL PROPERTIES IF NOT OTHERWISE SPECIFIED.
				//
				if (!_ExcludedPropertyNames.ContainsKey(CurrentPropertyInfo.Name) && !_ExcludedMemberNames.ContainsKey(CurrentPropertyInfo.Name))
				{
					//
					// THIS PROPERTY IS NOT EXCLUDED - SO INCLUDE IT IN THE PROJECTION.
					//
					object CurrentValue = CurrentPropertyInfo.GetValue(SourceObject, null);
					
					//
					// IF THE VALUE IS NOT NULL, THEN INCLUDE IT.
					// IF THE VALUE IS NULL BUT WE ARE SUPPOSED TO INCLUDE NULL VALUES THEN ALSO INCLUDE IT.
					//
					if (CurrentValue != null || _NullValueBehavior == NullValueBehavior.IncludeNulls)
					{
						//
						// WE ARE INCLUDING THIS VALUE IN THE PROJECTION.
						//
						if (_MemberRenameMap.ContainsKey(CurrentPropertyInfo.Name))
							CreatedProjection[_MemberRenameMap[CurrentPropertyInfo.Name]] = CurrentValue;
						else
							CreatedProjection[CurrentPropertyInfo.Name] = CurrentValue;
					}
				}
			}

			//
			// PROCESS FIELDS.
			//
			foreach (FieldInfo CurrentFieldInfo in SourceObjectType.GetFields())
			{
				//
				// INCLUDE ALL FIELDS UNLESS EXPLICITLY EXCLUDED.
				//
				if (!_ExcludedFieldNames.ContainsKey(CurrentFieldInfo.Name) && !_ExcludedMemberNames.ContainsKey(CurrentFieldInfo.Name))
				{
					//
					// THIS FIELD IS NOT EXCLUDED - SO INCLUDE IT IN THE PROJECTION.
					//
					object CurrentValue = CurrentFieldInfo.GetValue(SourceObject);
					
					//
					// IF THE VALUE IS NOT NULL, THEN INCLUDE IT.
					// IF THE VALUE IS NULL BUT WE ARE SUPPOSED TO INCLUDE NULL VALUES THEN ALSO INCLUDE IT.
					//
					if (CurrentValue != null || _NullValueBehavior == NullValueBehavior.IncludeNulls)
					{
						//
						// WE ARE INCLUDING THIS VALUE IN THE PROJECTION.
						// RENAME AS SPECIFIED.
						//
						if (_MemberRenameMap.ContainsKey(CurrentFieldInfo.Name))
							CreatedProjection[_MemberRenameMap[CurrentFieldInfo.Name]] = CurrentValue;
						else
							CreatedProjection[CurrentFieldInfo.Name] = CurrentValue;
					}
				}
			}
			
			return CreatedProjection;
		}


		private Hashtable CreateExclusiveProjection (object SourceObject)
		{
			//
			// CREATE THE HASH TABLE.
			// ALL PROJECTIONS ARE "OLD SCHOOL" HASH TABLES.
			//
			Hashtable CreatedProjection = new Hashtable();

			//
			// GET THE TYPE OF THE ENTITY.
			// WE USE THIS TO REFLECT AGAINST THE SOURCE OBJECT SEVERAL TIMES.
			//
			Type SourceObjectType = SourceObject.GetType();

			//
			// BY DEFAULT, THIS METHOD EXCLUDES ALL MEMBERS UNLESS THEY WERE EXPLICITLY INCLUDED.
			//
			
			//
			// PROCESS THE MEMBERS.
			//
			foreach (string CurrentMemberName in _IncludedMemberNames.Keys)
			{
				//
				// THIS MEMBER NAME HAS BEEN EXPLICITLY INCLUDED.
				// GET THE VALUE OF EITHER THE PROPERTY OR FIELD.
				//
				object CurrentValue;
				
				PropertyInfo CurrentPropertyInfo = SourceObjectType.GetProperty(CurrentMemberName);
				
				if (CurrentPropertyInfo != null)
				{
					CurrentValue = CurrentPropertyInfo.GetValue(SourceObject, null);
				}
				else
				{
					//
					// A PROPERTY HAVING THE CURRENT MEMBER NAME DOES NOT EXIST ON THE SOURCE OBJECT.
					// WE ASSUME THAT THE SOURCE OBJECT HAS A FIELD WITH THE NAME SO WE GET THAT VALUE.
					//
					CurrentValue = SourceObjectType.GetField(CurrentMemberName).GetValue(SourceObject);
				}
				
				if (CurrentValue != null || _NullValueBehavior == NullValueBehavior.IncludeNulls)
				{
					//
					// WE ARE INCLUDING THIS VALUE IN THE PROJECTION.
					// RENAME AS SPECIFIED.
					//
					if (_MemberRenameMap.ContainsKey(CurrentMemberName))
						CreatedProjection[_MemberRenameMap[CurrentMemberName]] = CurrentValue;
					else
						CreatedProjection[CurrentMemberName] = CurrentValue;
				}
			}
			
			//
			// PROCESS THE PROPERTIES.
			//
			foreach (string CurrentName in _IncludedPropertyNames.Keys)
			{
				//
				// THIS MEMBER NAME HAS BEEN EXPLICITLY INCLUDED.
				// GET THE VALUE OF THE PROPERTY.
				//
				object CurrentValue = SourceObjectType.GetProperty(CurrentName).GetValue(SourceObject, null);

				if (CurrentValue != null || _NullValueBehavior == NullValueBehavior.IncludeNulls)
				{
					//
					// WE ARE INCLUDING THIS VALUE IN THE PROJECTION.
					// RENAME AS SPECIFIED.
					//
					if (_MemberRenameMap.ContainsKey(CurrentName))
						CreatedProjection[_MemberRenameMap[CurrentName]] = CurrentValue;
					else
						CreatedProjection[CurrentName] = CurrentValue;
				}
			}

			//
			// PROCESS THE FIELDS.
			//
			foreach (string CurrentName in _IncludedFieldNames.Keys)
			{
				//
				// THIS MEMBER NAME HAS BEEN EXPLICITLY INCLUDED.
				// GET THE VALUE OF THE FIELD.
				//
				object CurrentValue = SourceObjectType.GetField(CurrentName).GetValue(SourceObject);

				if (CurrentValue != null || _NullValueBehavior == NullValueBehavior.IncludeNulls)
				{
					//
					// WE ARE INCLUDING THIS VALUE IN THE PROJECTION.
					// RENAME AS SPECIFIED.
					//
					if (_MemberRenameMap.ContainsKey(CurrentName))
						CreatedProjection[_MemberRenameMap[CurrentName]] = CurrentValue;
					else
						CreatedProjection[CurrentName] = CurrentValue;
				}
			}
			
			return CreatedProjection;
		}
		
		
		/// <summary>
		/// Project the source object into an existing hash table.
		/// </summary>
		public void ProjectInto (Hashtable ProjectionHashB)
		{
			//
			// CREATE A PROJECTION USING THE SOURCE OBJECT.
			//
			Hashtable ProjectionHashA = CreateProjection(_Source);
			
			foreach (object Key in ProjectionHashA.Keys)
			{
				ProjectionHashB[Key] = ProjectionHashA[Key];
			}
		}


		/// <summary>
		/// Specifies the inclusion behavior of the projector.
		/// We either include members by default or we exclude them by default.
		/// </summary>
		public IncludeBehavior IncludeBehavior
		{
			set
			{
				_IncludeBehavior = value;
			}
		}


		/// <summary>
		/// Specifies the null value behavior of the projector.
		/// By default, null values are not included in projections.
		/// </summary>
		public NullValueBehavior NullValueBehavior
		{
			set
			{
				_NullValueBehavior = value;
			}
		}


		/// <summary>
		/// Sets the source object for this projector.
		/// The source object is the object that the projector reflects against to find properties and fields to render.
		/// </summary>
		public object Source
		{
			set
			{
				_Source = value;
			}
		}

		
		private object _Source;
		private IncludeBehavior _IncludeBehavior;
		private NullValueBehavior _NullValueBehavior;

		private Dictionary<string, bool> _IncludedMemberNames;
		private Dictionary<string, bool> _ExcludedMemberNames;
		private Dictionary<string, string> _MemberRenameMap;

		private Dictionary<string, bool> _IncludedPropertyNames;
		private Dictionary<string, bool> _ExcludedPropertyNames;
		private Dictionary<string, string> _PropertyRenameMap;

		private Dictionary<string, bool> _IncludedFieldNames;
		private Dictionary<string, bool> _ExcludedFieldNames;
		private Dictionary<string, string> _FieldRenameMap;
	}
}
