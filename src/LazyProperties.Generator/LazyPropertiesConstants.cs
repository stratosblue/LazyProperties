namespace LazyProperties;

public static class LazyPropertiesConstants
{
    #region Public 字段

    /// <summary>
    /// GetInstance&lt;$Type$&gt;()
    /// <br/>
    /// $Type$ 、$PropertyName$ 、$FieldName$
    /// </summary>
    public const string DefaultLazyPropertyTemplate = "$FieldName$ ??= GetInstance<$Type$>()";

    #endregion Public 字段
}
