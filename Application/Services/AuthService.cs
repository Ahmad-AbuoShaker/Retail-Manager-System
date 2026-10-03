using Application.Common.Interface;
using Application.DTOs;
using Application.DTOs.Auht;
using Application.Interface;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IAppDbContext _appDbContext;
        public AuthService(IAppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }
        public Task<BaseResponse<AuthenticationResponse>> LoginAsync(AuthenticationRequest request)
        {
            
        }

        public Task<BaseResponse<AuthenticationResponse>> RegisterAsync(RegisterRequest request)
        {
           
        }
    }
}
