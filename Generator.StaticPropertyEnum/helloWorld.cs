using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Generator.StaticPropertyEnum
{
    [Generator]
    public class helloWorld : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {


            context.AddSource("HelloWorld-generated.cs", @"
            using System;
            namespace HelloWorld
            {
                public static class Hello
                {
                    public static string SayHello() {
                        return ""HelloWorld"";
                    }
                }
            }");
        }

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            //if(!Debugger.IsAttached) Debugger.Launch();
#endif
        }
    }
}
