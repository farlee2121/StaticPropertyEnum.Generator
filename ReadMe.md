# Static Property Enum Generator

## Why
Enums are great, but often data has a fixed set of known values AND should allows for unknown values.

This can be accomplished with static members on a type. For example [System.Drawing.Color](https://referencesource.microsoft.com/#System.Drawing/commonui/System/Drawing/Color.cs,9103fd761ca562ae).
More examples and exploration is availabe [here](https://spencerfarley.com/2020/08/07/structs-vs-enums/).

This comes with some disadvantages though, like not being able to list the known values without writing boilerplate for every type.

That's what this library solves. It uses the new C# 9 [Source Generators](https://devblogs.microsoft.com/dotnet/introducing-c-source-generators/) to automattically add a `.KnownValues()`
including all static members that have the same type as the containing declaration.

Example. Given
```cs
namespace Some.Namespace{
	[StaticMemberEnum]
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
IEnumerable<GolfClubTypes> clubs = GolfClubTypes.KnownValues();
```

## A few potential gotchas
- The member enum definition has to be partial because source generators behave about the same as adding new sources files.  
- This generator currently doesn't work on nested classes. It would require the whole hierarchy of classes to be partial. That seems smelly to me, so I decided not to support it. Let me know in the github issues if you have reasons I should change my mind.
- Classes won't have value-based equality behavior by default, and structs don't define `==` by default. I recommend using records.
	- You can use a generator like [Generator.Equals](https://github.com/diegofrata/Generator.Equals) if you still want a class or struct
	- Structs should only be used with very small values (under 16 bytes). See [the offical explanation](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/choosing-between-class-and-struct)



