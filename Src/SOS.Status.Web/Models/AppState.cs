namespace SOS.Status.Web.Models;

public class AppState
{
    public string? Username { get; set; }
    public event Action? OnChange;

    public void SetUsername(string? name)
    {
        Username = name;
        OnChange?.Invoke();
    }
}
