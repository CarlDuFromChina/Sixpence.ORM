﻿using Sixpence.Common.IoC;
using Sixpence.ORM.Entity;
using Sixpence.ORM.EntityManager;
using System;
using System.Collections.Generic;
using System.Text;

namespace Sixpence.ORM.Extensions
{
    /// <summary>
    /// 创建实体前
    /// </summary>
    [ServiceRegister]
    public interface IPreCreateEntity
    {
        void Execute(IEntityManager manager, IEntity entity);
    }
}