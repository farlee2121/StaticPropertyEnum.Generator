using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Generator.StaticPropertyEnum
{

    [AttributeUsage(AttributeTargets.Class|AttributeTargets.Struct)]
    public class StaticPropertyEnumAttribute : Attribute
    {
    }

    class StaticPropertyEnumSyntaxReciever : ISyntaxReceiver
    {
        private List<SyntaxNode> _possiblePropertyEnumDeclarations = new List<SyntaxNode>();
        public IReadOnlyCollection<SyntaxNode> PossiblePropertyEnumDeclarations => _possiblePropertyEnumDeclarations;
        public void OnVisitSyntaxNode(SyntaxNode syntaxNode)
        {
            var allowedTypes = new[] { SyntaxKind.ClassDeclaration, SyntaxKind.StructDeclaration, SyntaxKind.RecordDeclaration };
            
            if(allowedTypes.Contains(syntaxNode.Kind()))
            {
                // It appears I can't check for attributes here
                _possiblePropertyEnumDeclarations.Add(syntaxNode);
            }
            else
            {
                // no action needed: only applies to classes, records, and structs
            }
        }
    }

    [Generator]
    public class StaticPropertyEnumGenerator : ISourceGenerator
    {


        ITypeSymbol NodeToSymbol(GeneratorExecutionContext context, SyntaxNode syntaxNode)
        {
            var model = context.Compilation.GetSemanticModel(syntaxNode.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(syntaxNode, context.CancellationToken) as ITypeSymbol;
            return symbol;
        }
        AttributeData? TryGetEnumAttribute(INamedTypeSymbol attributeType, ITypeSymbol declarationSymbol)
        {
            var equatableAttributeData = declarationSymbol?
                .GetAttributes()
                .FirstOrDefault(attr => attr.AttributeClass?.Equals(attributeType, SymbolEqualityComparer.Default) ?? false);

            return equatableAttributeData;
        }

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
            var attributeSymbolType = context.Compilation.GetTypeByMetadataName(typeof(StaticPropertyEnumAttribute).FullName);

            //receiver.PossiblePropertyEnumDeclarations.Cast<ClassDeclarationSyntax>().First().

            var propertyEnumSymbols = receiver.PossiblePropertyEnumDeclarations
                .Select(node => NodeToSymbol(context, node))
                .Where(decoratedSymbol => TryGetEnumAttribute(attributeSymbolType, decoratedSymbol) != null);

            foreach (var enumSymbol in propertyEnumSymbols)
            {
                var fileName = $"{enumSymbol.ToDisplayString()}.generated.cs";
                var source = GetPropertyEnumAddonSource(enumSymbol);
                context.AddSource(fileName, source);
            }
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new StaticPropertyEnumSyntaxReciever());
        }
    }
}
