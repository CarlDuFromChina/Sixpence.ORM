using Microsoft.AspNetCore.Mvc;
using Postgres.Entity;

namespace UserInfoController;

[ApiController]
[Route("[controller]")]
public class UserInfoController : ControllerBase
{
    private readonly ILogger<UserInfoController> _logger;

    public UserInfoController(ILogger<UserInfoController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public List<UserInfo> Get(string id = "")
    {
        if (!string.IsNullOrEmpty(id))
        {
            return new List<UserInfo>() { UserInfo.FindById(id) };
        }   
        return UserInfo.FindAll();
    }

    [HttpPost]
    public void Post(UserInfo userInfo)
    {
        UserInfo.InsertUserInfo(userInfo);
    }

    [HttpPut]
    public void Put(UserInfo userInfo)
    {
        UserInfo.UpdateUserInfo(userInfo);
    }

    [HttpDelete]
    public void Delete(string id)
    {
        UserInfo.DeleteUserInfo(id);
    }
}