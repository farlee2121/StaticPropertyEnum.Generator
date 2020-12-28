using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Generator.StaticPropertyEnum
{

    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
    public class StaticPropertyEnumAttribute : Attribute
    {
    }

    class StaticPropertyEnumSyntaxReciever : ISyntaxReceiver
    {
        private List<SyntaxNode> _propertyEnumDeclarations = new List<SyntaxNode>();
        public IReadOnlyCollection<SyntaxNode> PropertyEnumDeclarations => _propertyEnumDeclarations;

        private AttributeSyntax? TryGetPropertyEnumAttribute(SyntaxList<AttributeListSyntax> attrLists)
        {
            return attrLists
                .SelectMany(l => l.Attributes)
                .FirstOrDefault(attr => attr.Name.ToFullString() == nameof(StaticPropertyEnumAttribute));
        }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            var allowedTypes = new[] { SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.RecordDeclaration };

            bool shouldAddNode = syntaxNode switch
            {
                // I can 
                ClassDeclarationSyntax dec => TryGetPropertyEnumAttribute(dec.AttributeLists) != null,
                StructDeclarationSyntax dec => TryGetPropertyEnumAttribute(dec.AttributeLists) != null,
                RecordDeclarationSyntax dec => TryGetPropertyEnumAttribute(dec.AttributeLists) != null,
                _ => false
            };

            if (shouldAddNode)
            {
                // It appears I can't check for attributes here
                _propertyEnumDeclarations.Add(syntaxNode);
            }
            else
            {
                // no action needed: not a candidate node
            }
        }
    }

    [Generator]
    public class StaticPropertyEnumGenerator : ISourceGenerator
    {

        string GetPropertyEnumAddonSource(ITypeSymbol typeSymbol)
        {
            // I don't think I need to split by record/class/struct
            // I want any static property or field of the same type as the containing type
            var enumFields = typeSymbol.GetMembers().Where(c =>
                c switch
                {
                    IFieldSymbol field => field.IsStatic && field.Type.Equals(field.ContainingType, SymbolEqualityComparer.Default),
                    IPropertySymbol prop => prop.IsStatic && prop.Type.Equals(prop.ContainingType, SymbolEqualityComparer.Default),
                    _ => false
                }
            );


            // consider moving this into a liquid file or similar
            var source = $@"
                namespace {typeSymbol.ContainingNamespace.ToDisplayString()}
                {{
                    public partial {nameof(typeSymbol.TypeKind)} {typeSymbol.Name}PropertyEnumExtensions
                    {{
                        public static IEnumerable<{typeSymbol.Name}> KnownValues(){{
                            return new []{{{string.Join(", ", enumFields.Select(field => field.Name))} }}
                        }}
                    }}
                }}                
            ";

            return source;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is StaticPropertyEnumSyntaxReciever receiver)) return;


            foreach (var node in receiver.PropertyEnumDeclarations)
            {
                // Note:  context.Compilation.GetSemanticModel().GetSymbolInfo() has an overload for getting symbols with a given attribute Syntax
                // another approach is that I could find every reference to the attribute, and use that method to get the class declarations
                var model = context.Compilation.GetSemanticModel(node.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(node, context.CancellationToken) as INamedTypeSymbol;
                var fileName = $"{symbol.ToDisplayString()}.generated.cs";
                var source = GetPropertyEnumAddonSource(symbol);
                context.AddSource(fileName, source);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            if (!Debugger.IsAttached) Debugger.Launch();
#endif
            context.RegisterForSyntaxNotifications(() => new StaticPropertyEnumSyntaxReciever());
        }
    }
}
