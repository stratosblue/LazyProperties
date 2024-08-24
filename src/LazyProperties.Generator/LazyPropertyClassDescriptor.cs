using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LazyProperties;

internal record struct LazyPropertyClassDescriptor(ClassDeclarationSyntax ClassDeclarationSyntax,
                                                   List<PropertyDeclarationSyntax> PropertyDeclarationSyntaxes,
                                                   string? GetterTemplate,
                                                   string? SetterTemplate);
