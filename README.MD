# ObjectProjector.NET

An object projection library for the .NET Framework.

## Motivation

This project started because we had a web service that had to send some of an object's data to the browser, but not all of it.
Consider the following data class that represents a user:

	public class User
	{
		//
		// WE WANT TO SEND THE FOLLOWING 4 FIELDS TO THE BROWSER.
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

2. Create an interface that exposes only the properties needed.
Make the data class implement that interface.

3. Mark each field or property of the data class with an attribute that indicates if the field or property should be included in the projection.
Write a custom projector class that discovers that metadata.

4. Serialize the entire data class to its target format - such as XML or JSON - and then apply a format-specific transformation to select only the data fields you need such as XPath or JSON Query Language, respectively.
For example, serialize the entire data class to XML, then use XSLT, XPath or manipulate the DOM directly to cut out anything you don't want to send.

5. 

## Getting Started
	
	using ObjectProjector;

	Projector RankProjector = new Projector(TargetRank);


## About


