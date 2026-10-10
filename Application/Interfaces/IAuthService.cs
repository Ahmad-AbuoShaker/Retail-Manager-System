using Application.DTOs;
using Application.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Application.Interfaces
{
    public interface IAuthService
    {
        Task<BaseResponse<AuthenticationResponse>> LoginAsync(AuthenticationRequest request,CancellationToken ct);
        Task<BaseResponse<AuthenticationResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
      }
}
