using BillLocket.Application.Common;
using BillLocket.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace BillLocket.Application.Interfaces
{
    public interface IUserService
    {
        Task<Result<UpdateUsernameResponse>> UpdateUsernameAsync(
            string userId,
            UpdateUsernameRequest request,
            CancellationToken cancellationToken = default);
    }
}
