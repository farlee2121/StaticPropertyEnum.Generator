using System;
using Xunit;

namespace Generator.StaticPropertyEnum.Tests
{
    //[StaticPropertyEnum]
    public partial record Color
    {
        public string Id;

        public Color(string id) => (Id) = (id);
        public static Color Red = new Color("Red"); 
        public static Color Blue = new Color("Blue");
        public static Color Green = new Color("Green");
    } 

    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            //Color.KnownValues();
            Generated.HelloWorld.Hello.SayHello();
        }
    }
}
