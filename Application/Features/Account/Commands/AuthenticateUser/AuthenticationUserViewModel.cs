using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Application.Features.Account.Commands.AuthenticateUser
{
    public class AuthenticationUserViewModel
    {
        public string JWToken { get; set; }
        [JsonIgnore]
        public string RefreshToken { get; set; }
    }
}