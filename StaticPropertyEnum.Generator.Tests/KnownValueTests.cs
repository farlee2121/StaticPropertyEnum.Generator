using StaticMemberEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Generator.StaticPropertyEnum.Tests
{
    [StaticMemberEnum]
    public partial class ColorClass
    {
        public string Id;

        public ColorClass(string id) => (Id) = (id);
        public static ColorClass Red = new ColorClass("Red"); 
        public static ColorClass Blue = new ColorClass("Blue");
        public static ColorClass Green = new ColorClass("Green");
    } 

    [StaticMemberEnum]
    public partial record ColorRecord
    {
        public string Id;

        public ColorRecord(string id) => (Id) = (id);
        public static ColorRecord Red = new ColorRecord("Red");
        public static ColorRecord Blue = new ColorRecord("Blue");
        public static ColorRecord Green = new ColorRecord("Green");
    }

    [StaticMemberEnum]
    public partial struct ColorStruct
    {
        public string Id;

        public ColorStruct(string id) => (Id) = (id);
        public static ColorStruct Red = new ColorStruct("Red");
        public static ColorStruct Blue = new ColorStruct("Blue");
        public static ColorStruct Green = new ColorStruct("Green");
    }

    [StaticMemberEnumAttribute]
    public partial record ColorFullName
    {
        public string Id;

        public ColorFullName(string id) => (Id) = (id);
        public static ColorFullName Red = new ColorFullName("Red");
    }

    [StaticMemberEnum]
    public partial record ColorPropsAndFields
    {
        public string Id;

        public ColorPropsAndFields(string id) => (Id) = (id);
        public static ColorPropsAndFields Red = new ("Red");
        public static ColorPropsAndFields Blue { get; } = new ("Blue");
    }

    [StaticMemberEnum]
    public partial record ColorIgnoreNonColors
    {
        public string Id;

        public ColorIgnoreNonColors(string id) => (Id) = (id);
        public static ColorIgnoreNonColors Red = new ("Red");
        public static ColorIgnoreNonColors Blue { get; } = new ("Blue");
        public static int Five = 5;
        public static string Hello { get; } = "Hello";
    }

    [StaticMemberEnum]
    public partial record ColorIgnorePrivate
    {
        public string Id;

        public ColorIgnorePrivate(string id) => (Id) = (id);
        public static ColorIgnorePrivate Red = new("Red");
        private static ColorIgnorePrivate Blue { get; } = new("Blue");
        protected static ColorIgnorePrivate Green { get; } = new("Green");
        internal static ColorIgnorePrivate Black { get; } = new("Black");
    }

    [StaticMemberEnum]
    public partial record ColorSplitDefinition
    {
        public string Id;

        public ColorSplitDefinition(string id) => (Id) = (id);
        public static ColorSplitDefinition Red = new("Red");
    }

    public partial record ColorSplitDefinition
    {
        public static ColorSplitDefinition Blue { get; } = new("Blue");
    }

    public class KnownValueTests
    {
        [Fact]
        public void FetchKnownValues_OnClass()
        {
            var expected = new[] { ColorClass.Red, ColorClass.Blue, ColorClass.Green }.ToHashSet();
            var actual =  ColorClass.KnownValues().ToHashSet();

            Assert.True(expected.SetEquals(actual));
        }

        [Fact]
        public void FetchKnownValues_OnRecord()
        {
            var expected = new[] { ColorRecord.Red, ColorRecord.Blue, ColorRecord.Green }.ToHashSet();
            var actual = ColorRecord.KnownValues().ToHashSet();

            Assert.True(expected.SetEquals(actual));
        }

        [Fact]
        public void FetchKnownValues_OnStruct()
        {
            var expected = new[] { ColorStruct.Red, ColorStruct.Blue, ColorStruct.Green }.ToHashSet();
            var actual = ColorStruct.KnownValues().ToHashSet();

            Assert.True(expected.SetEquals(actual));
        }

        [Fact]
        public void FetchKnownValues_InDifferentNamespace()
        {
            var expected = new[] { DifferentNamespace.Color.Red, DifferentNamespace.Color.Blue, DifferentNamespace.Color.Green }.ToHashSet();
            var actual = DifferentNamespace.Color.KnownValues().ToHashSet();

            Assert.True(expected.SetEquals(actual));
        }

        //class NestedClass
        //{
        //    [StaticPropertyEnum]
        //    public partial record ColorNested
        //    {
        //        public string Id;

        //        public ColorNested(string id) => (Id) = (id);
        //        public static ColorNested Red = new ColorNested("Red");
        //        public static ColorNested Blue = new ColorNested("Blue");
        //        public static ColorNested Green = new ColorNested("Green");
        //    }
        //}

        //[Fact]
        //public void FetchKnownValues_OnSubclass()
        //{
        //    var expected = new[] { NestedClass.ColorNested.Red, NestedClass.ColorNested.Blue, NestedClass.ColorNested.Green }.ToHashSet();
        //    var actual = NestedClass.ColorNested.KnownValues().ToHashSet();

        //    Assert.True(expected.SetEquals(actual));
        //}

        [Fact]
        public void FetchKnownValues_FullAttributeName()
        {
            var expected = new[] { ColorFullName.Red }.ToHashSet();
            var actual = ColorFullName.KnownValues().ToHashSet();

            Assert.True(expected.SetEquals(actual));
        }

        [Fact]
        public void IgnoresNonStaticMembers()
        {
            // Non-static members cause a stackoverflow (infinite construction loop)
            // Should probably be warned against with an analyzer
        }

        [Fact]
        public void GetsPropertiesAndFields()
        {
            var expected = new[] { ColorPropsAndFields.Red, ColorPropsAndFields.Blue }.ToHashSet();
            var actual = ColorPropsAndFields.KnownValues().ToHashSet();

            Assert.True(expected.SetEquals(actual));
        }

        [Fact]
        public void IgnoresMembersOfWrongType() // Should be the same type as the containing definition
        {
            var expected = new[] { ColorIgnoreNonColors.Red, ColorIgnoreNonColors.Blue }.ToHashSet();
            var actual = ColorIgnoreNonColors.KnownValues().ToHashSet();

            Assert.True(expected.SetEquals(actual));
        }

        [Fact]
        public void IgnoresPrivate() // Should be the same type as the containing definition
        {
            var expected = new[] { ColorIgnorePrivate.Red }.ToHashSet();
            var actual = ColorIgnorePrivate.KnownValues().ToHashSet();

            Assert.True(expected.SetEquals(actual));
        }

        [Fact]
        public void SplitDefinition()
        {
            var expected = new[] { ColorSplitDefinition.Red, ColorSplitDefinition.Blue }.ToHashSet();
            var actual = ColorSplitDefinition.KnownValues().ToHashSet();

            Assert.True(expected.SetEquals(actual));

        }
    }
}

namespace DifferentNamespace
{
    [StaticMemberEnum]
    public partial class Color{
        public string Id;

        public Color(string id) => (Id) = (id);
        public static Color Red = new Color("Red");
        public static Color Blue = new Color("Blue");
        public static Color Green = new Color("Green");
    }
}