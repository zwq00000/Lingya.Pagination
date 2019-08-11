Lingya.Pagination
===================

[![Build status](https://ci.appveyor.com/api/projects/status/n0mgkxkxb87i9b1s?svg=true)](https://ci.appveyor.com/project/zwq000/lingya-pagination)

Data Pagination For EntityFrameworkCore 

＃安装
```ps
PS> Install-Package  Lingya.Pagination
```

# WebApi 使用方法

## 1. 基本用法
### 1.1 异步用法

```c#
[HttpGet()]
[ProducesResponseType(statusCode: 200, type: typeof(PageResult<Use>))]
public async Task<IActionResult> Index([FromQuery] PageParameter paramete = null) {
    var query = context.Users;
    return Ok(await query.ToPagingAsync(paramete));
}
```

### 1.2 searchKey 查询
```c#
[HttpGet()]
[ProducesResponseType(statusCode: 200, type: typeof(PageResult<Use>))]
public async Task<IActionResult> Index([FromQuery] PageParameter paramete = null) {
    if(paramete!=null || String.IsNullOrEmpty(paramete.SearchKey)){
      var query = context.Users.Where(u=>u.UserName.StartWith(parame.SearchKey));
      return Ok(await query.ToPagingAsync(paramete));
    }else{
      var query = context.Users;
      return Ok(await query.ToPagingAsync(paramete));
    }
}
```

### 1.3 Sort 排序
支持 多列 单顺序排序
```c#
var param = new PageParameter();
param.SortBy = "Col1,Col2";
param.Descending = true;
return await query.ToPagingAsync(param);

```

### 1.4 同步方法

同步方法支持普通集合,如 IEnumerable<T> 的分页


## 2. 返回格式
ToPagingAsync 扩展方法返回一个包含泛型集合的 分页结果，包括 page对象和values集合,

```json
{
  "page": {
    "total": 0, //总记录数量
    "pages": 0, //总页数
    "pageSize": 0, //页面大小
    "page": 0   //页号,从 1 开始计数
  },
  "values": [
    {
      "uid": "string",
      "userName": "string",
      "email": "string"
    }
  ]
}

```




## 4. 分页参数 PageParameter
- PageSize 页面大小，默认20
- Page 当前页码，默认为 1
- SearchKey 搜索字符串，默认 null
- SortBy 排序字段名
- Descending 逆序排序
