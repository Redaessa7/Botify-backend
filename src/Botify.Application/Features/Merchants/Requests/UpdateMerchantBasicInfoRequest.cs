using System;
using System.Collections.Generic;
using System.Text;

namespace Botify.Application.Features.Merchants.Requests
{
    public sealed record UpdateMerchantBasicInfoRequest(string FirstName, string LastName, string? Bio, string? ProfileImageUrl);

}
