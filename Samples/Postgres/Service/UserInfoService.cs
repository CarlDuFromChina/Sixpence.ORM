using Postgres.Entity;
using Sixpence.ORM;
using Sixpence.ORM.Repository;

namespace Postgres.Service
{
    public class UserInfoService
    {
        private readonly IRepository<UserInfo> repository;
        public UserInfoService(IServiceProvider provider)
        {
            repository = provider.GetRequiredService<IRepository<UserInfo>>();
        }

        public List<UserInfo> FindAll()
        {
            return repository.FindAll().ToList();
        }

        public UserInfo FindById(string id)
        {
            return repository.FindOne();
        }

        public void InsertUserInfo(UserInfo userInfo)
        {
            repository.Create(userInfo);
        }

        public void UpdateUserInfo(UserInfo userInfo)
        {
            repository.Update(userInfo);
        }

        public void DeleteUserInfo(string id)
        {
            repository.Delete(id);
        }
    }
}
