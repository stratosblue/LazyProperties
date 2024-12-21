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
    public async Task Should_Generate_Generic_Type_Success()
    {
        var code = """
                    using LazyProperties;

                    namespace LazyProperties.Test;

                    partial class SampleService<T1, T2>
                    {
                        [LazyProperty]
                        public partial IService Service { get; set; }

                        private T GetInstance<T>() => throw new System.NotImplementedException();
                    }

                    interface IService { }
                    """;

        var generatedCode = """
                            #nullable disable
                            using LazyProperties;

                            namespace LazyProperties.Test;

                            partial class SampleService<T1, T2>
                            {
                                private IService __Service;

                                public partial IService Service
                                {
                                    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                                    get => __Service ??= GetInstance<IService>();
                                    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                                    set => __Service = value;
                                }
                            }

                            """;

        await TestCodeGenerateAsync(code, generatedCode, "LazyProperties.LazyProperties.Test.SampleService_T1__T2_.g.cs");
    }

    [TestMethod]
    public async Task Should_Generate_Success()
    {
        var code = """
                    using LazyProperties;

                    namespace LazyProperties.Test;

                    partial class SampleService
                    {
                        [LazyProperty]
                        public partial IService Service { get; set; }

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
                                    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
                                    set => __Service = value;
                                }
                            }

                            """;
        await TestCodeGenerateAsync(code, generatedCode, "LazyProperties.LazyProperties.Test.SampleService.g.cs");
    }

    #endregion Public 方法

    #region Private 方法

    private string GetPredefineLazyPropertiesAttributesCode()
    {
        using var stream = typeof(LazyPropertiesGenerator).Assembly.GetManifestResourceStream("LazyProperties.LazyPropertiesAttributes.cs");
        using var reader = new StreamReader(stream!);
        return reader.ReadToEnd();
    }

    private async Task TestCodeGenerateAsync(string code, string generatedCode, string generatedCodehintName)
    {
        var test = new LangVersionPreviewCSharpSourceGeneratorTest<LazyPropertiesGenerator, DefaultVerifier>
        {
            TestState =
            {
                Sources = { code },
                GeneratedSources =
                {
                    (typeof(LazyPropertiesGenerator), "LazyProperties.LazyPropertiesAttributes.cs", SourceText.From(GetPredefineLazyPropertiesAttributesCode(), Encoding.UTF8)),
                    (typeof(LazyPropertiesGenerator), generatedCodehintName, SourceText.From(generatedCode, Encoding.UTF8)),
                },
            },
            ReferenceAssemblies = ReferenceAssemblies.Net.Net90
        };

        await test.RunAsync();
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
