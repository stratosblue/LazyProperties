using System.Text;
using LazyProperties.Generator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Testing;
using Microsoft.CodeAnalysis.Testing;
using Microsoft.CodeAnalysis.Text;

namespace LazyProperties.Test;

[TestClass]
public class GeneralTest
{
    #region Public 方法

    [TestMethod]
    public async Task Should_Generate_Success()
    {
        var code = """
                    using LazyProperties;
                    
                    namespace LazyProperties.Test;

                    partial class SampleService
                    {
                        [LazyProperty]
                        public partial IService Service { get; }

                        private T GetInstance<T>() => throw new System.NotImplementedException();
                    }

                    interface IService { }
                    """;

        var generatedCode = """
                            #nullable disable
                            using LazyProperties;
                            
                            namespace LazyProperties.Test;

                            partial class SampleService
                            {
                                private IService __Service;

                                public partial IService Service
                                {
                                    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                                    get => __Service ??= GetInstance<IService>();
                                }
                            }

                            """;

        var test = new LangVersionPreviewCSharpSourceGeneratorTest<LazyPropertiesGenerator, DefaultVerifier>()
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(LazyPropertiesGenerator), "LazyProperties.Generator.LazyPropertiesAttributes.cs", SourceText.From(GetPredefineLazyPropertiesAttributesCode(), Encoding.UTF8)),
                    (typeof(LazyPropertiesGenerator), "LazyProperties.SampleService.g.cs", SourceText.From(generatedCode, Encoding.UTF8)),
                },
            },
        };

        test.ReferenceAssemblies = ReferenceAssemblies.Net.Net90;

        await test.RunAsync();
    }

    #endregion Public 方法

    #region Private 方法

    private string GetPredefineLazyPropertiesAttributesCode()
    {
        using var stream = typeof(LazyPropertiesGenerator).Assembly.GetManifestResourceStream("LazyProperties.Generator.LazyPropertiesAttributes.cs");
        using var reader = new StreamReader(stream!);
        return reader.ReadToEnd();
    }

    #endregion Private 方法
}

internal class LangVersionPreviewCSharpSourceGeneratorTest<TSourceGenerator, TVerifier> : CSharpSourceGeneratorTest<TSourceGenerator, TVerifier>
    where TSourceGenerator : new()
    where TVerifier : IVerifier, new()
{
    #region Protected 方法

    protected override ParseOptions CreateParseOptions()
    {
        return new CSharpParseOptions(LanguageVersion.CSharp13, DocumentationMode.Diagnose);
    }

    #endregion Protected 方法
}
