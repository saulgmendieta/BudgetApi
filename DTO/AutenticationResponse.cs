namespace BasicApi.DTO
{
    public class AutenticationResponse
    {
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
        public int Expire { get; set; }

    }
}
