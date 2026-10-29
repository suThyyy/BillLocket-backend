using System;
using System.Collections.Generic;
using System.Text;

namespace BillLocket.Application.DTOs
{
    public sealed record UpdateUsernameResponse(
       string Username,
       DateTimeOffset? NextChangeAllowedAt);
}
