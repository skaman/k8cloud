using Microsoft.AspNetCore.Components.Forms;
using System.Text.Json;

namespace K8Cloud.Web.Components.Contexts;

public sealed class ModifiedEventArgs : EventArgs
{
    public ModifiedEventArgs(bool isModified)
    {
        IsModified = isModified;
    }
    public bool IsModified { get; }
}

public sealed class ModifiedContext
{
    private readonly EditContext _editContext;
    private string _oldState;
    private bool _isModified;

    public event EventHandler<ModifiedEventArgs>? OnModifiedChanged;

    public ModifiedContext(EditContext editContext)
    {
        _editContext = editContext;
        _editContext.OnFieldChanged += HandleFieldChanged;
        _oldState = JsonSerializer.Serialize(_editContext.Model);
    }

    private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        Update(e.FieldIdentifier.Model);
    }

    public bool IsModified() => _isModified;

    public void MarkAsUnmodified()
    {
        _editContext.MarkAsUnmodified();
        _oldState = JsonSerializer.Serialize(_editContext.Model);
        _isModified = false;
        OnModifiedChanged?.Invoke(this, new ModifiedEventArgs(false));
    }

    public void Update()
    {
        Update(_editContext.Model);
    }

    public void Update(object model)
    {
        var newState = JsonSerializer.Serialize(model);
        var isModified = _oldState != newState;

        if (isModified && _isModified != isModified)
        {
            _isModified = true;
            _oldState = newState;
            OnModifiedChanged?.Invoke(this, new ModifiedEventArgs(true));
        }
    }
}
