namespace DevIO.Business.Models.Auth;

public class UserLogin
{
    public string AccessToken { get; set; }
    public double ExpiresIn { get; set; }
    public UserToken UserToken { get; set; }
}
