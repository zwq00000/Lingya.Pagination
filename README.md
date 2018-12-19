# Lingya.Pagination
Data Pagination For WebApi 

＃安装
```ps
PS> Install-Package  Lingya.Pagination
```

# WebApi 使用方法

## 1. 基本用法

```c#
[HttpGet()]
[ProducesResponseType(statusCode: 200, type: typeof(PageResult<Use>))]
public async Task<IActionResult> Index([FromQuery] PageParamete paramete = null) {
    var query = context.Users;
    return Ok(await query.PagingAsync(paramete));
}
```

2. searchKey 查询
```c#
[HttpGet()]
[ProducesResponseType(statusCode: 200, type: typeof(PageResult<Use>))]
public async Task<IActionResult> Index([FromQuery] PageParamete paramete = null) {
    if(paramete!=null || String.IsNullOrEmpty(paramete.SearchKey)){
      var query = context.Users.Where(u=>u.UserName.StartWith(parame.SearchKey));
      return Ok(await query.PagingAsync(paramete));
    }else{
      var query = context.Users;
      return Ok(await query.PagingAsync(paramete));
    }
}
```

3. Sort 排序
...

## 2. 返回格式
PagingAsync 扩展方法返回一个包含泛型集合的 分页结果，包括 page对象和values集合,

```json
{
  "page": {
    "total": 0, #总记录数量
    "pages": 0, #总页数
    "pageSize": 0, #页面大小
    "page": 0   #页号,从 1 开始计数
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


## PageParamete
- PageSize 页面大小，默认20
- Page 当前页码，默认为 1
- SearchKey 搜索字符串，默认 null
- SortBy 排序字段名
- Descending 逆序排序
