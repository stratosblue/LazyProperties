using LazyProperties;

namespace LazyPropertiesSample;

internal partial class SampleService(IServiceProvider serviceProvider)
{
    #region Public 属性

    [LazyProperty]
    public partial IEchoService Service { get; }

    #endregion Public 属性

    #region Private 方法

    private T GetInstance<T>() => (T)(serviceProvider.GetService(typeof(T)) ?? throw new InvalidOperationException());

    #endregion Private 方法
}
