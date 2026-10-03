using Application.DTOs;
using Application.DTOs.Auht;
using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interface
{
    public interface IAuthService
    {
        Task<BaseResponse<AuthenticationResponse>> LoginAsync(AuthenticationRequest request);
        Task<BaseResponse<AuthenticationResponse>> RegisterAsync(RegisterRequest request);
      }
}
