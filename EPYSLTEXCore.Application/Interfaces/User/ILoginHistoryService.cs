using EPYSLTEX.Core.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEX.Core.Services
{
    public interface ILoginHistoryService
    {
        Task SaveAsync(LoginHistory entity);
        Task<LoginHistory> GetAsync(LoginHistory entity);
    }
}
