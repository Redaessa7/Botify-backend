using System;
using System.Collections.Generic;
using System.Text;

namespace Botify.Application.Features.Auths.Requests;
public sealed record VerifyOtpRequest(string Email, string Otp);
