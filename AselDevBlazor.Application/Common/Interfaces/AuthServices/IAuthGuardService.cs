using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AselDevBlazor.Application.Common.Interfaces.AuthServices
{
    public interface IAuthGuardService
    {
        Task<bool> EnsureAuthorizedAsync(
        List<string>? roles = null,
        string? urlReturn = null);
    }
}
