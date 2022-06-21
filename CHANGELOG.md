## 2.2.0

### Features

+ 强化了`EntityManager`的批量操作，支持`DataTable`

## 2.1.0

### Features

+ 添加了批量操作方法：`BulkCreate`、`BulkUpdate`、`BulkCreateOrUpdate`、`BulkDelete`
+ SQL查询参数类型从`Dictionary<string, object>`变更为`object`
+ 实体`ID`支持`Number`类型
+ 特性`EntityAttribute`的属性调整为选填

## 2.0.0

### Features

- 添加`Repository`仓储模式
- 添加`EntityManager`实体管理器
- 添加`UseORM`方法，控制实体自动生成开关以及实体命名