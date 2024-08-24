# LazyProperties

Template property code generator based on `partial` property syntax. 基于 `partial` 属性语法的属性模板化代码生成器。

-------
#### 例：
为属性生成延迟获取的代码
```C#
internal partial class SampleClass(IServiceProvider serviceProvider)
{
    [LazyProperty]
    partial IService Service { get; }

    public T GetInstance<T>() => (T)(serviceProvider.GetService(typeof(T)) ?? throw new InvalidOperationException());
}
```
生成代码为
```C#
partial class SampleClass
{
    private IService __Service;

    partial IService Service
    {
        [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
        get => __Service ??= GetInstance<IService>();
    }
}
```

## 使用方法

### 引用包
```xml
<ItemGroup>
  <PackageReference Include="LazyProperties" Version="1.0.0" />
</ItemGroup>
```
### 声明类型

- 需要 `C#13` (partial 属性) 及以上
- 类型必须标记为 `partial`
- 需要自动生成的属性必须标记为 `partial`
- 可使用 `LazyPropertyTemplateAttribute` 为类型声明 `get`、`set` 访问器的模板代码
  - 默认模板:
    - `get`: `$FieldName$ ??= GetInstance<$Type$>()`
    - `set`: `$FieldName$ = value`
  - 模板支持以下变量:
    - `$Type$`: 类型名称
    - `$PropertyName$`: 属性名称
    - `$FieldName$`: 生成的字段名称 (仅当模板包含此变量时才会生成字段)
- 使用 `LazyPropertyAttribute` 标记属性为自动生成
  - 当使用 `LazyPropertyAttribute` 标记类型时，类型的所有 `partial` 属性都会进行生成

```C#
partial class SampleService
{
    [LazyProperty]
    public partial IService Service { get; }

    private T GetInstance<T>() => throw new NotImplementedException());
}
```

### 设置全局模板

针对单个类型设置 `LazyPropertyTemplateAttribute` 在一些时候过于麻烦，可以使用 `LazyPropertyGlobalGetterTemplate` 和 `LazyPropertyGlobalSetterTemplate` 进行针对项目的全局设置，示例：

```C#
<PropertyGroup>
  <LazyPropertyGlobalGetterTemplate>$FieldName$ ??= default</LazyPropertyGlobalGetterTemplate>
  <LazyPropertyGlobalGetterTemplate>$FieldName$ = value ?? throw new InvalidOperationException()</LazyPropertyGlobalGetterTemplate>
</PropertyGroup>
```

- `LazyPropertyTemplateAttribute` 的优先级高于 `LazyPropertyGlobalGetterTemplate` 和 `LazyPropertyGlobalSetterTemplate`
- xml中某些特殊字符需要转义，如: `<` -> `&lt;`、`>` -> `&gt;`
