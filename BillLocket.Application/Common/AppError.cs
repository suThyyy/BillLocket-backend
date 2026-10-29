using System;
using System.Collections.Generic;
using System.Text;

namespace BillLocket.Application.Common
{
    public sealed record AppError(
     string Code,
     string Message,
     IReadOnlyDictionary<string, object?>? Details = null);
}
