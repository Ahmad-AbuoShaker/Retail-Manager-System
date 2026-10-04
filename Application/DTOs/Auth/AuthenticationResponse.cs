
namespace Application.DTOs.Auth
{ 
    public class AuthenticationResponse
    {
     
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}