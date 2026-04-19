public class UserState
{
    public int UserId { get; set; }
    public string UserName { get; set; } = "";
    public bool IsLoggedIn => UserId > 0;
}