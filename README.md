# Sixpence.ORM - an ORM framework for .Net

`Sixpence.ORM`是一个基于`Dapper`的`ORM`框架，在`Dapper`的基础上，提供实体自动生成和`CRUD`封装，支持批量操作。我相信，本项目可以帮助你大大提升你的开发效率。

## 开始

1、appsettings.json

```json
{
  "DBSource": {
    "DriverType": "Postgresql",
    "ConnectionString": "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=123123;",
    "CommandTimeOut": 5
  }
}
```

2、startup.cs

```csharp
public class Startup
{
    public virtual void ConfigureServices(IServiceCollection services)
    {
        services.AddServiceContainer(options =>
        {
            options.Assembly.Add("Blog.dll"); // 添加你的项目
        });
    }

    public virtual void Configure(IApplicationBuilder app, IWebHostEnvironment env, IHttpContextAccessor accessor)
    {
        app.UseEntityGenerate(); // 实体自动生成
    }
}
```

## 基础使用

### 创建实体

`BaseEntity`内置字段有：updated_at、updated_by、updated_by_name、created_at、created_by、created_by_name、name

修改人和创建人会根据上下文自动获取，在你更新实体时自动赋值

```csharp
namespace Blog
{
    [Entity("test", "测试")]
    [KeyAttributes("code不能重复", "code")]
    public class Test : BaseEntity
    {
        [PrimaryColumn]
        public string id { get; set; }

        [Column("code", "编码", DataType.Varchar, 100)]
        public string code { get; set; }
    }
}
```

### 仓储模型

简单使用：

```csharp
public class TestService
{
    private Repository<Test> testRepository;

    public void CreateData(Test test) {
        testRepository.Create(test); // 创建
    }

    public void UpdateData(Test test) {
        testRepository.Update(test); // 更新
    }

    public Test Query(string id) {
        return testRepository.SingleQuery(id); // 查询单个记录
    }
}
```

事务：

```csharp
public void Transcation(Test data) {
    testRepository.Broker.ExecuteTransaction(() => {
        testRepository.Create(data);
        data.name = "123";
        testRepository.Update(data);
    });
}
```

