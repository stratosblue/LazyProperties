using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LazyProperties.Generator;

internal record struct LazyPropertyClassDescriptor(ClassDeclarationSyntax ClassDeclarationSyntax, List<PropertyDeclarationSyntax> PropertyDeclarationSyntaxes, string? ClassTemplate);
