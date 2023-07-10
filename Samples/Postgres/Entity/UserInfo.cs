using System;
using Sixpence.ORM;
using Sixpence.ORM.Entity;

namespace Postgres.Entity
{
    [Entity]
    public class UserInfo : BaseEntity
    {
        [PrimaryColumn]
        public string id { get; set; }

        [Column]
        public string code { get; set; }

        [Column]
        public bool is_admin { get; set; }

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

