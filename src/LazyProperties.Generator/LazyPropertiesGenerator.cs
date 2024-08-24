using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LazyProperties.Generator;

[Generator(LanguageNames.CSharp)]
public class LazyPropertiesGenerator : IIncrementalGenerator
{
    #region Public 方法

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var declarationsProvider = context.SyntaxProvider.CreateSyntaxProvider(FilterSyntaxNode, TransformSyntaxNode);

        var compilationPropertiesProvider = context.AnalyzerConfigOptionsProvider.Select((configOptions, token) =>
        {
            configOptions.GlobalOptions.TryGetValue("build_property.LazyPropertyGlobalTemplate", out var lazyPropertyGlobalTemplateValue);

            return new CompilationProperties()
            {
                GlobalTemplate = lazyPropertyGlobalTemplateValue
            };
        });

        context.RegisterImplementationSourceOutput(declarationsProvider.Combine(compilationPropertiesProvider), (context, input) =>
        {
            var (descriptor, compilationProperties) = input;
            var (classDeclarationSyntax, lazyPropertyDeclarations, classTemplate) = descriptor;

            var template = compilationProperties.GlobalTemplate;

            if (!string.IsNullOrWhiteSpace(classTemplate))
            {
                template = classTemplate;
            }
            if (string.IsNullOrWhiteSpace(template))
            {
                template = LazyPropertiesConstants.DefaultLazyPropertyTemplate;
            }

            var @namespace = "";
            IEnumerable<UsingDirectiveSyntax> usings = [];
            {
                var parenSyntax = classDeclarationSyntax.Parent;
                while (parenSyntax is not null)
                {
                    if (parenSyntax is BaseNamespaceDeclarationSyntax namespaceDeclarationSyntax)
                    {
                        usings = namespaceDeclarationSyntax.Usings.Concat((namespaceDeclarationSyntax.Parent as CompilationUnitSyntax)?.Usings ?? []);
                        @namespace = (namespaceDeclarationSyntax.Name as IdentifierNameSyntax)?.Identifier.ValueText
                                     ?? namespaceDeclarationSyntax.Name.ToString();
                        break;
                    }
                    parenSyntax = classDeclarationSyntax.Parent;
                }
            }

            var className = classDeclarationSyntax.Identifier.ValueText;

            var generateField = template.Contains("$FieldName$");

            template = template.Replace("$Type$", "{0}")
                               .Replace("$PropertyName$", "{1}")
                               .Replace("$FieldName$", "{2}");

            var builder = new StringBuilder();

            builder.AppendLine($$"""
                               #nullable disable
                               """);

            foreach (var item in usings)
            {
                builder.AppendLine(item.ToString());
            }

            builder.AppendLine($$"""

                               namespace {{@namespace}};

                               partial class {{className}}
                               {
                               """);

            foreach (var propertyDeclarationSyntax in lazyPropertyDeclarations)
            {
                var propertyType = propertyDeclarationSyntax.Type.ToString();
                var propertyName = propertyDeclarationSyntax.Identifier.ValueText;
                var fieldName = $"__{propertyName}";

                var modifiers = propertyDeclarationSyntax.Modifiers.ToString();

                if (generateField)
                {
                    builder.AppendLine($"    private {propertyType} {fieldName};");
                    builder.AppendLine();
                }

                builder.AppendLine($$"""
                                       {{modifiers}} {{propertyType}} {{propertyName}}
                                       {
                                   """);

                var hasGet = propertyDeclarationSyntax.AccessorList!.Accessors.Any(m => m.ExpressionBody is null && m.Keyword.ValueText == "get");
                var hasSet = propertyDeclarationSyntax.AccessorList!.Accessors.Any(m => m.ExpressionBody is null && m.Keyword.ValueText == "set");

                if (hasGet)
                {
                    builder.AppendLine($"""
                                               [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                                               get => {string.Format(template, propertyType, propertyName, fieldName)};
                                       """);
                }

                if (hasSet)
                {
                    builder.AppendLine($"""
                                               [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                                               set => {fieldName} = value;
                                       """);
                }

                builder.AppendLine("    }");
            }

            builder.AppendLine("}");

            context.AddSource($"LazyProperties.{className}.g.cs", builder.ToString());
        });

        context.RegisterPostInitializationOutput(context =>
        {
            using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("LazyProperties.Generator.LazyPropertiesAttributes.cs");
            using var reader = new StreamReader(stream);
            context.AddSource("LazyProperties.Generator.LazyPropertiesAttributes.cs", reader.ReadToEnd());
        });
    }

    #endregion Public 方法

    #region Private 方法

    private static IEnumerable<PropertyDeclarationSyntax> GetLazyPropertyDeclarationSyntaxes(ClassDeclarationSyntax classDeclarationSyntax)
    {
        var isClassMarkedLazyProperty = classDeclarationSyntax.AttributeLists.Any(m => m.Attributes.Any(m => (m.Name as IdentifierNameSyntax)?.Identifier.Text == "LazyProperty"));
        var lazyPropertyDeclarationSelector = classDeclarationSyntax.Members.OfType<PropertyDeclarationSyntax>()
                                                                            .Where(m => m.Modifiers.Any(m => m.Text == "partial"));

        if (!isClassMarkedLazyProperty)
        {
            lazyPropertyDeclarationSelector = lazyPropertyDeclarationSelector.Where(m => m.AttributeLists.Any(m => m.Attributes.Any(m => (m.Name as IdentifierNameSyntax)?.Identifier.Text == "LazyProperty")));
        }

        return lazyPropertyDeclarationSelector;
    }

    private bool FilterSyntaxNode(SyntaxNode node, CancellationToken token)
    {
        if (node is ClassDeclarationSyntax classDeclarationSyntax
            && classDeclarationSyntax.Modifiers.Any(m => m.Text == "partial"))
        {
            var lazyPropertyDeclarations = GetLazyPropertyDeclarationSyntaxes(classDeclarationSyntax);

            return lazyPropertyDeclarations.Any();
        }
        return false;
    }

    private LazyPropertyClassDescriptor TransformSyntaxNode(GeneratorSyntaxContext context, CancellationToken token)
    {
        var classDeclarationSyntax = (ClassDeclarationSyntax)context.Node;
        var lazyPropertyDeclarations = GetLazyPropertyDeclarationSyntaxes(classDeclarationSyntax).ToList();

        var templateAttribute = classDeclarationSyntax.AttributeLists.SelectMany(m => m.Attributes).FirstOrDefault(m => (m.Name as IdentifierNameSyntax)?.Identifier.Text == "LazyPropertyTemplate");

        var template = (templateAttribute?.ArgumentList?.Arguments[0].Expression as LiteralExpressionSyntax)?.Token.ValueText;

        return new(classDeclarationSyntax, lazyPropertyDeclarations, template);
    }

    #endregion Private 方法
}
