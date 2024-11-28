using EPYSLTEX.Core.DTOs;
using EPYSLTEXCore.Application.DTO;
using EPYSLTEXCore.Application.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EPYSLTEXCore.Application.Interfaces.YarnProductSetup
{
    public interface IYarnProductSetupService
    {

        

        Task<List<YarnProductSetupFinder>> GetAllFiberType(PaginationInfo paginationInfo);
    }
}
