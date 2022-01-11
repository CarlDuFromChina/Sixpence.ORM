# Sixpence.ORM - an ORM framework for .Net

## 准备

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
```

## 创建一个实体

```csharp
namespace Blog {
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

## 仓储模型

```csharp

    public class TestService
    {
        private Repository<Test> testRepository;
        
        public void CreateData(Test test) {
            testRepository.Create(test);
        }
        
        public void UpdateData(Test test) {
            testRepository.Update(test);
        }
        
        public Test Query(string id) {
            return testRepository.SingleQuery(id);
        }
    }
```
