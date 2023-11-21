using Microsoft.AspNetCore.Mvc;
using Postgres.Entity;
using Postgres.Service;

namespace UserInfoController;

[ApiController]
[Route("[controller]")]
public class UserInfoController : ControllerBase
{
    private readonly ILogger<UserInfoController> _logger;
    private readonly UserInfoService service;

    public UserInfoController(ILogger<UserInfoController> logger,
        UserInfoService service)
    {
        this.service = service;
        _logger = logger;
    }

    [HttpGet]
    public List<UserInfo> Get(string id = "")
    {
        if (!string.IsNullOrEmpty(id))
        {
            return new List<UserInfo>() { service.FindById(id) };
        }   
        return service.FindAll();
    }

    [HttpPost]
    public void Post(UserInfo userInfo)
    {
        service.InsertUserInfo(userInfo);
    }

    [HttpPut]
    public void Put(UserInfo userInfo)
    {
        service.UpdateUserInfo(userInfo);
    }

    [HttpDelete]
    public void Delete(string id)
    {
        service.DeleteUserInfo(id);
    }
}