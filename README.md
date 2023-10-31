# Sixpence.ORM

> 一个轻量级的 .Net OMR 框架

## 介绍

`Sixpence.ORM`是一个基于`Dapper`的`ORM`框架，在`Dapper`的基础上，提供实体自动生成和`CRUD`封装，支持批量操作。我相信，本项目可以帮助你大大提升你的开发效率。

## 框架特点

+ **支持 .Net 6**
+ **配置简单，易上手**
+ **轻松定义通用实体类**
+ **支持批量创建、更新**
+ **灵活扩展**
+ **根据实体类自动迁移数据库**

### 链接

完整文档请访问 [Wiki](https://karl-du.gitbook.io/sixpence-orm/)

查看变更历史请访问 [CHANGELOG](https://github.com/CarlDuFromChina/Sixpence.ORM/blob/master/CHANGELOG.md)

## 安装

Sixpence.ORM 是一个[NuGet library](https://www.nuget.org/packages/Dapper)，你可以在通过`Nuget`安装

```shell
Install-Package Sixpence.ORM -Version 3.3.0
```

## 使用教程

`Sixpence.ORM`配置和使用都非常简单，你仅需要几分钟时间即可掌握该框架

### 入门

#### 1、安装

在启动类`startup.cs`里注册服务

```csharp
using Microsoft.AspNetCore.Builder;
using Sixpence.ORM;
using Sixpence.ORM.Postgres;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// 1、注册服务，配置数据库和实体
builder.Services.AddSorm(options =>
{
    options.UsePostgres("Host=localhost;Port=5432;Database=postgres;Username=postgres;Password=123123;", 20);
});

builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo() {  Title = "Postgres Demo 接口", Version = "v1"});
});

var app = builder.Build();

// 2、添加中间件，开启日志、自动合并实体类字段改动
app.UseSorm(options =>
{
    options.EnableLogging = true; // 启用日志记录 SQL
    options.MigrateDb = true; // 自动合并实体改动到数据库
});

app.UseAuthorization();

app.MapControllers();

app.MapSwagger();

app.UseSwagger();

app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("v1/swagger.json", "My API V1");
});

app.Run();
```

#### 2、实体类定义（Entity）

实体类**必须**要继承`BaseEntity`基类，此外还要注意特性声明

+ **EntityAttribute**：定义实体表名和逻辑名
+ **KeyAttributes**：定义唯一键
+ **PrimaryColumn**：定义主键
+ **Column**：定义列

以下是一个用户信息的实体类示例：

```csharp
using System;
using Sixpence.ORM;
using Sixpence.ORM.Entity;

namespace Postgres.Entity
{
    [Entity]
    public class UserInfo : BaseEntity
    {
        [PrimaryColumn]
        public string Id { get; set; }

        [Column]
        public string Code { get; set; }

        [Column]
        public bool IsAdmin { get; set; }
    }
}
```

对应生成的`pg sql`

```sql
create table user_info {
	id text primary key,
	code text,
	is_admin bool
}
```

#### 3、仓储模型（Repository）

我们定义好实体后，可以利用仓储模型（Repository）直接进行增删改查。示例代码如下：

```csharp
using System;
using Sixpence.ORM;
using Sixpence.ORM.Entity;

namespace Postgres.Entity
{
    [Entity]
    public class UserInfo : BaseEntity
    {
        [PrimaryColumn]
        public string Id { get; set; }

        [Column]
        public string Code { get; set; }

        [Column]
        public bool IsAdmin { get; set; }

        #region DAL
        public static List<UserInfo> FindAll()
        {
            return new Repository<UserInfo>().FindAll().ToList();
        }

        public static UserInfo FindById(string id)
        {
            return new Repository<UserInfo>().FindOne();
        }

        public static void InsertUserInfo(UserInfo userInfo)
        {
            new Repository<UserInfo>().Insert(userInfo);
        }

        public static void UpdateUserInfo(UserInfo userInfo)
        {
            new Repository<UserInfo>().Update(userInfo);
        }

        public static void DeleteUserInfo(string id)
        {
            new Repository<UserInfo>().Delete(id);
        }
        #endregion
    }
}
```

### 高级

#### 1、实体管理器（EntityManager）

有时候`Repository`无法满足我们复杂的使用场景，这个时候我们就需要`EntityManager`

**(1) 获取实例**

```csharp
var manager = EntityManagerFactory.GetManager();
```

**(2) 查询数据**

单条查询：

```csharp
var data = manager.QueryFirst<UserInfo>("123"); // 根据 id 查询数据
var data = manager.QueryFirst<UserInfo>("select * from test where id = @id", new { id = "123" }); // 原生 SQL 查询
```

多条查询：

```csharp
var dataList = manager.Query<UserInfo>(); // 查询 test 实体所有数据
var dataList = manager.Query<UserInfo>("select * from test where begin_time > @begin_time", new { begin_time = DateTime.Now });  // 原生 SQL 查询
```

**(3) 删除数据**

单条删除：

```csharp
var data = new UserInfo() { Id = "123" };
manager.Delete(data);
manager.Delete("user_info", "123");
```

批量删除：

```csharp
var dataList = new List<UserInfo>()
{
    new UserInfo() { Id = "B001"},
    new UserInfo() { Id = "B002"},
    new UserInfo() { Id = "B003"},
    // ...
};
manager.BulkDelete(dataList);
```

**(4) 更新数据**

单条更新：

```csharp
var data = new UserInfo() { Id = "123", Name = "王二" };
manager.Update(data);
```

批量更新：

```csharp
var dataList = manager.Query<Test>("select * from test where code in ('B001', 'B002', 'B003')").ToList();
dataList[0].Name = "test1";
dataList[1].Name = "test2";
dataList[2].Name = "test3";
manager.BulkUpdate(dataList);
```

**(5) 创建数据**

单条创建：

```csharp
var entity = new UserInfo() { Code = "A001", Name = "Test", Id = "123" };
var result = manager.Create(entity);
```

批量创建：

```csharp
var dataList = new List<UserInfo>()
{
    new UserInfo() { Id = Guid.NewGuid().ToString(), Code = "B001", Name = "测试1", CreatedAt = DateTime.Now, CreatedBy = "user", CreatedByName = "user", UpdatedAt = DateTime.Now, UpdatedBy = "user", UpdatedByName = "user" },
    new UserInfo() { Id = Guid.NewGuid().ToString(), Code = "B002", Name = "测试2" , CreatedAt = DateTime.Now, CreatedBy = "user", CreatedByName = "user", UpdatedAt = DateTime.Now, UpdatedBy = "user", UpdatedByName = "user" },
    new UserInfo() { Id = Guid.NewGuid().ToString(), Code = "B003", Name = "测试3", CreatedAt = DateTime.Now, CreatedBy = "user", CreatedByName = "user", UpdatedAt = DateTime.Now, UpdatedBy = "user", UpdatedByName = "user" },
};
var manager = EntityManagerFactory.GetManager();
manager.ExecuteTransaction(() => manager.BulkCreate(dataList));
```

#### 2、事务（Transcation）

`EntityManager`中使用事务：

```csharp
public void Transcation(UserInfo data)
{
    var manager = EntityManagerFactory.GetManager();
    manager.ExecuteTransaction(() => {
        manager.Create(data);
        data.Name = "123";
        manager.Update(data);
    });
}
```

`Repository`中使用事务：

```csharp
public void Transcation(Test test, UserInfo user)
{
    var manager = EntityManagerFactory.GetManager();
    var testRepository = new Repository<Test>(manager);
    var userRepository = new Repository<UserInfo>(manager);
    manager.ExecuteTransaction(() => {
        testRepository.Create(test);
        userRepository.Create(user);
    });
}
```

## 关于

本项目是作者利用空余时间开发，主要灵感来源于 [typeORM](https://typeorm.io/#/)。如果使用过程中遇到问题，欢迎报告 issue。
