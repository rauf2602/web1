namespace BlazorApp4.Services;

public class SearchState
{
    private string _text = "";
    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value;
                NotifyChanged();
            }
        }
    }

    public event Action? OnChange;

    public void NotifyChanged()
    {
        OnChange?.Invoke();
    }

    public void Clear()
    {
        _text = "";
        NotifyChanged();
    }
}