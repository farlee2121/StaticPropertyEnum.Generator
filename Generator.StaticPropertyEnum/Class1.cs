using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;

namespace Generator.StaticPropertyEnum
{

    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Struct)]
    public class StaticPropertyEnumAttribute : Attribute
    {
    }

    class StaticPropertyEnumSyntaxReciever : ISyntaxReceiver
    {
        public List<SyntaxNode> _staticEnumDeclarations;
        public IReadOnlyCollection<SyntaxNode> StaticEnumDeclarations => _staticEnumDeclarations;
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            if(syntaxNode.IsKind(SyntaxKind.ClassDeclaration) || syntaxNode.IsKind(SyntaxKind.RecordDeclaration) || syntaxNode.IsKind(SyntaxKind.StructDeclaration))
            {
                _staticEnumDeclarations.Add(syntaxNode);
            }
            else
            {
                // no action needed: only applies to classes, records, and structs
            }
        }
    }

    [Generator]
    public class Class1 : ISourceGenerator
    {
        public void Execute(GeneratorExecutionContext context)
        {
            //context.Compilation.
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new StaticPropertyEnumSyntaxReciever());
        }
    }
}
