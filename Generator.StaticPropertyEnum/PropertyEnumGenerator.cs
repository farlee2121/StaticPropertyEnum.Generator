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

    enum DeclarationType
    {
        Class,
        Record,
        Struct
    }

    record PropertyEnumCandidate(SyntaxNode SyntaxNode, DeclarationType DeclarationType, AttributeSyntax? AttributeNode);

    class StaticPropertyEnumSyntaxReciever : ISyntaxReceiver
    {
        // IMPORTANT: Can't get type or fully-qualified names until we have the compilation
        static readonly string[] AttributeNames = new[] { "StaticPropertyEnum", "StaticPropertyEnumAttribute" };
        private List<PropertyEnumCandidate> _propertyEnumDeclarations = new List<PropertyEnumCandidate>();
        public IReadOnlyCollection<PropertyEnumCandidate> PropertyEnumDeclarations => _propertyEnumDeclarations;

        private AttributeSyntax? TryGetPropertyEnumAttribute(SyntaxList<AttributeListSyntax> attrLists)
        {
            return attrLists
                .SelectMany(l => l.Attributes)
                .FirstOrDefault(attr => AttributeNames.Contains(attr.Name.ToFullString()));
        }

        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            var candidate = syntaxNode switch
            {
                ClassDeclarationSyntax dec => new PropertyEnumCandidate(syntaxNode, DeclarationType.Class, TryGetPropertyEnumAttribute(dec.AttributeLists)),
                StructDeclarationSyntax dec => new PropertyEnumCandidate(syntaxNode, DeclarationType.Struct, TryGetPropertyEnumAttribute(dec.AttributeLists)),
                RecordDeclarationSyntax dec => new PropertyEnumCandidate(syntaxNode, DeclarationType.Record, TryGetPropertyEnumAttribute(dec.AttributeLists)),
                _ => null
            };

            if (candidate?.AttributeNode != null)
            {
                _propertyEnumDeclarations.Add(candidate);
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


        string GetGeneratedTypeKeyword(DeclarationType type)
        {
            //SOURCE: no type kind for records https://stackoverflow.com/questions/64077005/determine-whether-class-is-a-record-using-roslyn
            return type switch
            {
                DeclarationType.Record  => "record",
                DeclarationType.Class => "class",
                DeclarationType.Struct => "struct",
                _ => "impossible",
            };
        }

        string GetPropertyEnumAddonSource(DeclarationType declarationType, ITypeSymbol typeSymbol)
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
                using System.Collections.Generic;
                
                namespace {typeSymbol.ContainingNamespace.ToDisplayString()}
                {{
                    public partial {GetGeneratedTypeKeyword(declarationType)} {typeSymbol.Name}
                    {{
                        public static IEnumerable<{typeSymbol.Name}> KnownValues(){{
                            return new []{{{string.Join(", ", enumFields.Select(field => field.Name))} }};
                        }}
                    }}
                }}                
            ";

            return source;
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (!(context.SyntaxReceiver is StaticPropertyEnumSyntaxReciever receiver)) return;


            foreach (var candidate in receiver.PropertyEnumDeclarations)
            {
                // Note:  context.Compilation.GetSemanticModel().GetSymbolInfo() has an overload for getting symbols with a given attribute Syntax
                // another approach is that I could find every reference to the attribute, and use that method to get the class declarations
                var model = context.Compilation.GetSemanticModel(candidate.SyntaxNode.SyntaxTree);
                var symbol = model.GetDeclaredSymbol(candidate.SyntaxNode, context.CancellationToken) as INamedTypeSymbol;
                var fileName = $"{symbol.ToDisplayString()}.generated.cs";
                var source = GetPropertyEnumAddonSource(candidate.DeclarationType, symbol);
                context.AddSource(fileName, source);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
#if DEBUG
            //if (!Debugger.IsAttached) Debugger.Launch();
#endif
            context.RegisterForSyntaxNotifications(() => new StaticPropertyEnumSyntaxReciever());
        }
    }
}
