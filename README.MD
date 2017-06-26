# ObjectProjector.NET

An object projection library for the .NET Framework.

## What It Does

Dynamically creates an object 

## Terminology

A **source object** is any object that contains data we want.

A **projection** of a **source object** is another object - generated at runtime - that contains a subset of the data fields and properties of the source object.

A **projector** is the class that creates the projection.

## Motivation

This project started because we had a web service that had to send some of an object's data to the browser, but not all of it.
Consider the following data class that represents a user:

	public class User
	{
		//
		// WE ONLY WANT TO SEND THE FOLLOWING 4 FIELDS TO THE BROWSER.
		//
		public int UserId;
		public string FirstName;
		public string LastName;
		public string EmailAddress;
		
		//
		// WE DO NOT WANT TO SEND THE FOLLOWING 2 FIELDS TO THE BROWSER BECAUSE THIS CREATES A SECURITY RISK AND THE BROWSER DOESN'T NEED THIS DATA.
		//
		public byte[] Salt;
		public byte[] SaltedHashedPassword;
		
		//
		// WE DO NOT WANT TO SEND THE FOLLOWING 2 FIELDS TO THE BROWSER BECAUSE AND THE BROWSER DOESN'T NEED THIS DATA.
		//
		public bool? Active;
		public int? AccountsDatabaseId;
	}

How do we expose only what the browser needs to work?
How do we not expose sensitive information?  We do not want to expose any sensitive information such as the salt, or hashed password.
There are probably dozens solutions to this problem - each with its own trade-offs.
Here are the some of the solutions we considered:

1. Create a new data class that contains only the fields needed.
Before serializing, copy the required fields from the original data class to the new data class.

1. Create an interface that exposes only the properties needed.
Make the data class implement that interface.

1. Apply an attribute to each member of the data class that indicates if the member should be included in the projection.
Write a custom projector class that reflects against the data class instance and selects the appropriate members to create the projection.

1. Serialize the entire data class to its target representation (such as XML or JSON) and then apply a representation-specific transformation to select only the data fields you need.
For example, we could serialize the entire data class to XML, then use XSLT, XPath or DOM manipulation to cut out anything you don't want.

1. Create a hash table, manually add the required values from the source object, then serialize the hash table.

1. **Write a custom projector class that can be configured to select the required values from the source object to create the projection.**

## Getting Started

Let's create a projection of a user that contains only the first 4 fields:

	using System.Collections;
	using ObjectProjector;

	Projector UserProjector = new Projector(SourceUser);
	UserProjector.Include(new string[] {"UserId", "FirstName", "LastName", "EmailAddress"});
	Hashtable UserProjection = UserProjector.CreateProjection();

What the above code does:

1. Creates a projector and specifies the source object as a constructor parameter.

1. Specifies the 4 members of the user instance that will be included in the projection.

1. Creates the projection.



## About


