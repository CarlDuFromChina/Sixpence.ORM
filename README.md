# Sixpence.ORM

> 一个轻量级的 .Net OMR 框架

## 介绍

`Sixpence.ORM`是一个基于`Dapper`的`ORM`框架，在`Dapper`的基础上，提供实体自动生成和`CRUD`封装，支持批量操作。我相信，本项目可以帮助你大大提升你的开发效率。

### 基础

核心类：

+ BaseEntity：实体基类，所有实体都应继承于该基类
+ EntityManager：通过此类，可以管理（CRUD）任何实体， `EntityManager` 就像放一个实体存储库的集合的地方
+ Repository：`Repository` 就像 `EntityManager` 一样，但其操作仅限于具体实体

实体特性：

+ ColumnAttribute：标注属性对应数据库中的列，必须指定列名、类型等
+ EntityAttribute：标注类是一个实体类，可以指定表名，逻辑名
+ KeyAttributesAttribute：标注实体类唯一键，可以是组合主键。插入或更新时会检查重复项
+ PrimaryColumnAttribute：标注属性是主键，可以指定列名、类型

### 链接

完整文档请访问 [Wiki](https://karl-du.gitbook.io/sixpence-orm/)

查看变更历史请访问 [CHANGELOG](https://github.com/CarlDuFromChina/Sixpence.ORM/blob/master/CHANGELOG.md)

## 安装

Sixpence.ORM 是一个[NuGet library](https://www.nuget.org/packages/Dapper)，你可以在通过`Nuget`安装

```shell
Install-Package Sixpence.ORM -Version 3.0.0
```

## 特性

+ **支持 DotNet Core 3.1 +**
+ **支持注入依赖系统**
+ **表自动生成**
+ **支持批量创建、更新**
+ **灵活扩展**
+ **使用简单直观**

## 使用教程

`Sixpence.ORM`配置和使用都非常简单，你仅需要几分钟时间即可掌握该框架

### 配置

1、我们需要在`appsettings.json`里配置数据库连接

```json
{
  "DBSource": {
    "DriverType": "Postgresql",
    "ConnectionString": "Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=123123;",
    "CommandTimeOut": 5
  }
}
```

2、我们在启动类`startup.cs`里注册服务

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
        app.UseORM(options => 
        {
            options.EntityClassNameCase = ClassNameCase.UnderScore; // 实体下划线命名
            options.AutoGenerate = true; // 自动生成实体
        });
    }
}
```

### 实体类定义（Entity）

实体类**必须**要继承`BaseEntity`基类，此外还要注意特性声明

+ EntityAttribute：定义实体表名和逻辑名
+ KeyAttributes：定义唯一键
+ PrimaryColumn：定义主键
+ Column：定义列

```csharp
namespace Blog
{
    [Entity]
    [KeyAttributes("code不能重复", "code")]
    public class test : BaseEntity
    {
        [PrimaryColumn]
        public string id { get; set; }

        [Column]
        public string code { get; set; }
      
      	[Column]
      	public DateTime? birthday { get; set; }
    }
}
```

### 仓储模型（Repository）

我们定义好实体后，可以利用仓储模型（Repository）直接进行增删改查。示例代码如下：

```csharp
public class TestService
{
    private Repository<Test> testRepository = new Repository<Test>(); // 实例化

    public void CreateData(Test test)
    {
        testRepository.Create(test); // 创建
    }

    public void UpdateData(Test test)
    {
        testRepository.Update(test); // 更新
    }

    public Test QueryById(string id)
    {
        return testRepository.FindOne(id); // 查询单个记录
    }
    
    public IList<Test> Query(string ids)
    {
        return testRepository.FindByIds(ids); // 根据多个id查询
    }
    
    public Test QueryByName(string name)
    {
        return testRepository.FindOne(new Dictionary<string, Object>() { { "name", name } }); // 条件查询
    }
}
```

### 实体管理器（EntityManager）

有时候`Repository`无法满足我们复杂的使用场景，这个时候我们就需要`EntityManager`

**(1) 获取实例**

```csharp
var manager = EntityManagerFactory.GetManager();
```

**(2) 查询数据**

单条查询：

```csharp
var data = manager.QueryFirst<Test>("123"); // 根据 id 查询数据
var data = manager.QueryFirst<Test>("select * from test where id = @id", new { id = "123" }); // 原生 SQL 查询
```

多条查询：

```csharp
var dataList = manager.Query<Test>(); // 查询 test 实体所有数据
var dataList = manager.Query<Test>("select * from test where begin_time > @begin_time", new { begin_time = DateTime.Now }); // 原生 SQL 查询
```

**(3) 删除数据**

单条删除：

```csharp
var test = new Test() { id = "123" };
manager.Delete(test);
manager.Delete("test", "123");
```

批量删除：

```csharp
var dataList = new List<Test>()
{
    new Test() { id = "B001"},
    new Test() { id = "B002"},
    new Test() { id = "B003"},
    // ...
};
manager.BulkDelete(dataList);
```

**(4) 更新数据**

单条更新：

```csharp
var test = new Test() { id = "123", name = "王二" };
manager.Update(data);
```

批量更新：

```csharp
var dataList = manager.Query<Test>("select * from test where code in ('B001', 'B002', 'B003')").ToList();
dataList[0].name = "test1";
dataList[1].name = "test2";
dataList[2].name = "test3";
manager.BulkUpdate(dataList);
```

**(5) 创建数据**

单条创建：

```csharp
var entity = new Test() { code = "A001", name = "Test", id = "123" };
var result = manager.Create(entity);
```

批量创建：

```csharp
var dataList = new List<Test>()
{
    new Test() { id = Guid.NewGuid().ToString(), code = "B001", name = "测试1", created_at = DateTime.Now, created_by = "user", created_by_name = "user", updated_at = DateTime.Now, updated_by = "user", updated_by_name = "user" },
    new Test() { id = Guid.NewGuid().ToString(), code = "B002", name = "测试2" , created_at = DateTime.Now, created_by = "user", created_by_name = "user", updated_at = DateTime.Now, updated_by = "user", updated_by_name = "user" },
    new Test() { id = Guid.NewGuid().ToString(), code = "B003", name = "测试3", created_at = DateTime.Now, created_by = "user", created_by_name = "user", updated_at = DateTime.Now, updated_by = "user", updated_by_name = "user" },
};
var manager = EntityManagerFactory.GetManager();
manager.ExecuteTransaction(() => manager.BulkCreate(dataList));
```

### 事务（Transcation）

`EntityManager`中使用事务：

```csharp
public void Transcation(Test data)
{
    var manager = EntityManagerFactory.GetManager();
    manager.ExecuteTransaction(() => {
        manager.Create(data);
        data.name = "123";
        manager.Update(data);
    });
}
```

`Repository`中使用事务：

```csharp
public void Transcation(Test test, User user)
{
    var manager = EntityManagerFactory.GetManager();
    var testRepository = new Repository<Test>(manager);
    var userRepository = new Repository<User>(manager);
    manager.ExecuteTransaction(() => {
        testRepository.Create(test);
        userRepository.Create(user);
    });
}
```

## 关于

本项目是作者利用空余时间开发，主要灵感来源于 [typeORM](https://typeorm.io/#/)。如果使用过程中遇到问题，欢迎报告 issue。
