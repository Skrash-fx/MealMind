public class UserState
{
    private int _userId;
    private string _userName = "";

    public int UserId
    {
        get => _userId;
        set { _userId = value; NotifyStateChanged(); }
    }

    public string UserName
    {
        get => _userName;
        set { _userName = value; NotifyStateChanged(); }
    }

    public bool IsLoggedIn => UserId > 0;

    public event Action? OnChange;
    private void NotifyStateChanged() => OnChange?.Invoke();
}
