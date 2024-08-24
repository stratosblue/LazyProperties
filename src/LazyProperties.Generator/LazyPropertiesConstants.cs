namespace LazyProperties;

public static class LazyPropertiesConstants
{
    #region Public 字段

    /// <summary>
    /// GetInstance&lt;$Type$&gt;()
    /// <br/>
    /// $Type$ 、$PropertyName$ 、$FieldName$
    /// </summary>
    public const string DefaultLazyPropertyGetterTemplate = "$FieldName$ ??= GetInstance<$Type$>()";

    /// <summary>
    /// $FieldName$ = value
    /// <br/>
    /// $Type$ 、$PropertyName$ 、$FieldName$
    /// </summary>
    public const string DefaultLazyPropertySetterTemplate = "$FieldName$ = value";

    #endregion Public 字段
}
