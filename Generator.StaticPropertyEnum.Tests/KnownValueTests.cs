using Generator.StaticPropertyEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Generator.StaticPropertyEnum.Tests
{
    [StaticPropertyEnum]
    public partial class ColorClass
    {
        public string Id;

        public ColorClass(string id) => (Id) = (id);
        public static ColorClass Red = new ColorClass("Red"); 
        public static ColorClass Blue = new ColorClass("Blue");
        public static ColorClass Green = new ColorClass("Green");
    } 

    [StaticPropertyEnum]
    public partial record ColorRecord
    {
        public string Id;

        public ColorRecord(string id) => (Id) = (id);
        public static ColorRecord Red = new ColorRecord("Red");
        public static ColorRecord Blue = new ColorRecord("Blue");
        public static ColorRecord Green = new ColorRecord("Green");
    }

    [StaticPropertyEnum]
    public partial struct ColorStruct
    {
        public string Id;

        public ColorStruct(string id) => (Id) = (id);
        public static ColorStruct Red = new ColorStruct("Red");
        public static ColorStruct Blue = new ColorStruct("Blue");
        public static ColorStruct Green = new ColorStruct("Green");
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

        //class SubClass
        //{
        //    [StaticPropertyEnum]
        //    public partial record ColorSubClass
        //    {
        //        public string Id;

        //        public ColorSubClass(string id) => (Id) = (id);
        //        public static ColorSubClass Red = new ColorSubClass("Red");
        //        public static ColorSubClass Blue = new ColorSubClass("Blue");
        //        public static ColorSubClass Green = new ColorSubClass("Green");
        //    }
        //}

        //[Fact]
        //public void FetchKnownValues_OnSubclass()
        //{
        //    var expected = new[] { SubClass.ColorSubClass.Red, SubClass.ColorSubClass.Blue, SubClass.ColorSubClass.Green }.ToHashSet();
        //    var actual = SubClass.ColorSubClass.KnownValues().ToHashSet();

        //    Assert.True(expected.SetEquals(actual));
        //}

        //[StaticPropertyEnumAttribute]
        //public partial record ColorFullName
        //{
        //    public string Id;

        //    public ColorFullName(string id) => (Id) = (id);
        //    public static ColorFullName Red = new ColorFullName("Red");
        //}

        //[Fact]
        //public void FetchKnownValues_FullAttributeName()
        //{
        //    var expected = new[] { ColorFullName.Red }.ToHashSet();
        //    var actual = ColorFullName.KnownValues().ToHashSet();

        //    Assert.True(expected.SetEquals(actual));
        //}

        [Fact]
        public void IgnoresNonStaticMembers()
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void GetsPropertiesAndFields()
        {
            throw new NotImplementedException();

        }

        [Fact]
        public void IgnoresMembersOfWrongType() // Should be the same type as the containing definition
        {
            throw new NotImplementedException();
        }

        [Fact]
        public void DefinedPartiallyInMultipleNamesapces()
        {
            throw new NotImplementedException();

        }
    }
}

namespace DifferentNamespace
{
    [StaticPropertyEnum]
    public partial class Color{
        public string Id;

        public Color(string id) => (Id) = (id);
        public static Color Red = new Color("Red");
        public static Color Blue = new Color("Blue");
        public static Color Green = new Color("Green");
    }
}