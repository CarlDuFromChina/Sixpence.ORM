## 3.2.1

+ 移除`Sixpence.Common`引用
+ 添加`ILogger`接口
+ 添加`ILoggerFactory`接口

## 3.1.0

+ `IEntityManager`实现了`IDisposable`
+ `IDbClient`实现了`IDisposable`

# 3.0

### Features

+ 主框架升级到 .net 6.0
+ 移除了无用属性

## 2.2.0

### Features

+ 强化了`EntityManager`的批量操作，支持`DataTable`
+ 实体自动生成
+ `BaseEntity`的`api`优化
+ `DbDriver`的`api`加强
+ 实体字段特性`ColumnAttribute`优化传入参数

## 2.1.0

### Features

+ 添加了批量操作方法：`BulkCreate`、`BulkUpdate`、`BulkCreateOrUpdate`、`BulkDelete`
+ SQL查询参数类型从`Dictionary<string, object>`变更为`object`
+ 实体`ID`支持`Number`类型
+ 特性`EntityAttribute`的属性调整为选填

## 2.0.0

### Features

+ 添加`Repository`仓储模式
+ 添加`EntityManager`实体管理器
+ 添加`UseORM`方法，控制实体自动生成开关以及实体命名
