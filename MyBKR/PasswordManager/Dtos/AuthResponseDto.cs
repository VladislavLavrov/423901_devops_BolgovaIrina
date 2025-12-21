namespace PasswordManager.Dtos
{
    public class AuthResponseDto
    {
        public int UserId { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
    }
}
