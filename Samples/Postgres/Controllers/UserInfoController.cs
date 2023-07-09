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
  public List<UserInfo> Get()
  {
    return UserInfo.FindAll();
  }
}