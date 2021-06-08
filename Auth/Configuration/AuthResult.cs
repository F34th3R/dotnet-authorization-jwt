using System.Collections.Generic;

namespace TodoApp.Auth.Configuration
{
    public class AuthResult
    {
        public string Token { get; set; }
        public string RefreshTokeRefrestn { get; set; }
        public bool Success { get; set; }
        public List<string> Errors { get; set; }
    }
}
