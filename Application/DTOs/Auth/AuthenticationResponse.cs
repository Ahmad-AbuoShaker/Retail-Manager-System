
using System.Text.Json.Serialization;

namespace Application.DTOs.Auth
{ 
    public class AuthenticationResponse
    {
        public string? AccessToken { get; set; }
        
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public string? RefreshToken { get; set; }
    }
}