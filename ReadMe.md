# Static Property Enum

## Why
Enums are great, but often data has a fixed set of known values AND should allows for unknown values.

This can be accomplished with static properties on a type. For example [System.Drawing.Color](https://referencesource.microsoft.com/#System.Drawing/commonui/System/Drawing/Color.cs,9103fd761ca562ae).
More examples and exploration is availabe [here](https://spencerfarley.com/2020/08/07/structs-vs-enums/).

This comes with some disadvantages though, like not being able to list the known values without writing boilerplate for every type.

That's what this library solves. It uses the new C# 9 [Source Generators](https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/) to automattically add a `.KnownValues()`
including all static members that have the same type as the containing declaration.

Example. Given
```cs
namespace Some.Namespace{
	[StaticPropertyEnum]
	partial class GolfClubTypes{
		private readonly string _id;
		public GolfClubTypes(string id) => _id = id;
		public static GolfClubTypes Driver = new GolfClubTypes("Driver");
		public static GolfClubTypes Iron = new GolfClubTypes("Iron");
		public static GolfClubTypes Wedge = new GolfClubTypes("Wedge");
		public static GolfClubTypes Hybrid = new GolfClubTypes("Hybrid");
		public static GolfClubTypes Wood = new GolfClubTypes("Wood");
	}
}
```

The library generates
```cs
namespace Some.Namespace{
	partial class GolfClubTypes{
		public static IEnumerable<GolfClubTypes> KnownValues(){
			return new []{Driver, Iron, Wedge, Hybrid, Woood};
		}
	}
}
```

So that you can call
```cs
var clubs = GolfClubTypes.KnownValues();
```

<!-- Hmm, this got me thinking that Union-types could be approximated in C# using these with different constructors, but pattern matching would be hard
	Then I realized a similar effect could be accomplished by sub-typing records. And it would pattern match well.
 -->

