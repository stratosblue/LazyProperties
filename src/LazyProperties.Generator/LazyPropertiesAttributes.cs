﻿#pragma warning disable IDE0005
#pragma warning disable CS9113

using System;
using System.ComponentModel;

namespace LazyProperties;

/// <summary>
/// 标记属性或类型使用延迟属性
/// </summary>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
[EditorBrowsable(EditorBrowsableState.Never)]
internal sealed class LazyPropertyAttribute() : Attribute
{
}

/// <summary>
/// 标记类型的延迟属性初始化模板，支持变量 $Type$ 、$PropertyName$ 、$FieldName$
/// </summary>
/// <param name="template"></param>
[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
[EditorBrowsable(EditorBrowsableState.Never)]
internal sealed class LazyPropertyTemplateAttribute(string template) : Attribute
{
}