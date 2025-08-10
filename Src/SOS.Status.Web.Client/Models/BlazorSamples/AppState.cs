namespace SOS.Status.Web.Client.Models.BlazorSamples;

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
