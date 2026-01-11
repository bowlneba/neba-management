#nullable enable

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;

namespace Neba.Web.Server.Components;

/// <summary>
/// A searchable dropdown component with generic value and item types.
/// Supports keyboard navigation, filtering, and click-outside detection.
/// </summary>
/// <typeparam name="TValue">The type of the bound value (int, Guid, string, etc.)</typeparam>
/// <typeparam name="TItem">The type of items in the collection</typeparam>
public partial class NebaDropdown<TValue, TItem> : ComponentBase, IAsyncDisposable
{
#pragma warning disable CS8618
    private ElementReference _dropdownRef;
    private ElementReference _searchInputRef;
#pragma warning restore CS8618
    private IJSObjectReference? _jsModule;
    private DotNetObjectReference<NebaDropdown<TValue, TItem>>? _dotNetRef;
    private bool _isOpen;
    private string _filterText = string.Empty;
    private int _highlightedIndex;
    private List<TItem> _filteredItems = new();
    private string _componentId = string.Empty;

    [Inject] private IJSRuntime JSRuntime { get; set; } = default!;

    /// <summary>
    /// The currently selected value.
    /// </summary>
    [Parameter]
    public TValue? Value { get; set; }

    /// <summary>
    /// Event callback invoked when the value changes.
    /// </summary>
    [Parameter]
    public EventCallback<TValue> ValueChanged { get; set; }

    /// <summary>
    /// The collection of items to display in the dropdown.
    /// </summary>
    [Parameter, EditorRequired]
    public IEnumerable<TItem> Items { get; set; } = Enumerable.Empty<TItem>();

    /// <summary>
    /// Function to extract the display text from an item.
    /// Optional when TItem is string.
    /// </summary>
    [Parameter]
    public Func<TItem, string>? DisplayProperty { get; set; }

    /// <summary>
    /// Function to extract the value from an item.
    /// Optional when TItem is TValue.
    /// </summary>
    [Parameter]
    public Func<TItem, TValue>? ValueProperty { get; set; }

    /// <summary>
    /// Label text displayed above the dropdown.
    /// </summary>
    [Parameter]
    public string? Label { get; set; }

    /// <summary>
    /// Placeholder text shown in the search input.
    /// </summary>
    [Parameter]
    public string Placeholder { get; set; } = "Type to search...";

    /// <summary>
    /// Whether the dropdown is disabled.
    /// </summary>
    [Parameter]
    public bool Disabled { get; set; } = false;

    /// <summary>
    /// Whether the dropdown is required.
    /// </summary>
    [Parameter]
    public bool Required { get; set; } = false;

    /// <summary>
    /// HTML id attribute for label association.
    /// </summary>
    [Parameter]
    public string? Id { get; set; }

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        _componentId = Id ?? $"neba-dropdown-{Guid.NewGuid():N}";
        UpdateFilteredItems();
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        UpdateFilteredItems();
    }

    /// <inheritdoc />
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            _jsModule = await JSRuntime.InvokeAsync<IJSObjectReference>(
                "import", "./Components/NebaDropdown.razor.js");
        }

        if (_isOpen && _jsModule != null && _dotNetRef != null)
        {
            await _jsModule.InvokeVoidAsync("initializeDropdown", _dropdownRef, _dotNetRef);

            // Auto-focus the search input for better UX - use a small delay to ensure DOM is ready
            _ = Task.Run(async () =>
            {
                await Task.Delay(50);
                if (_searchInputRef.Context != null)
                {
                    try
                    {
                        await _searchInputRef.FocusAsync();
                    }
                    catch (InvalidOperationException)
                    {
                        // Silently ignore focus errors in test environments
                    }
                }
            });
        }
    }

    private void UpdateFilteredItems()
    {
        _filteredItems = GetFilteredItems().ToList();

        // Reset highlighted index when items change
        if (_highlightedIndex >= _filteredItems.Count)
        {
            _highlightedIndex = 0;
        }
    }

    private IEnumerable<TItem> GetFilteredItems()
    {
        if (string.IsNullOrWhiteSpace(_filterText))
            return Items;

        return Items.Where(item =>
            GetDisplayText(item).Contains(_filterText, StringComparison.OrdinalIgnoreCase));
    }

    private string GetDisplayText(TItem? item)
    {
        if (EqualityComparer<TItem>.Default.Equals(item, default))
            return string.Empty;

        if (DisplayProperty != null)
            return DisplayProperty(item!);

        return item?.ToString() ?? string.Empty;
    }

    private TValue? GetValue(TItem item)
    {
        if (ValueProperty != null)
            return ValueProperty(item);

        // If TItem is TValue, cast directly
        if (item is TValue value)
            return value;

        throw new InvalidOperationException(
            $"Cannot convert {typeof(TItem).Name} to {typeof(TValue).Name}. " +
            "Please provide a ValueProperty function.");
    }

    private string GetSelectedDisplayText()
    {
        if (EqualityComparer<TValue>.Default.Equals(Value, default))
            return string.Empty;

        var selectedItem = Items.FirstOrDefault(item =>
            EqualityComparer<TValue>.Default.Equals(GetValue(item), Value));

        return !EqualityComparer<TItem>.Default.Equals(selectedItem, default)
            ? GetDisplayText(selectedItem)
            : string.Empty;
    }

    private bool IsSelected(TItem item)
    {
        if (EqualityComparer<TValue>.Default.Equals(Value, default))
            return false;

        var itemValue = GetValue(item);
        return EqualityComparer<TValue>.Default.Equals(itemValue, Value);
    }

    private async Task ToggleDropdownAsync()
    {
        if (Disabled)
            return;

        _isOpen = !_isOpen;

        if (_isOpen)
        {
            _filterText = string.Empty;
            UpdateFilteredItems();
            _highlightedIndex = 0;

            // Highlight the currently selected item if it exists
            if (!EqualityComparer<TValue>.Default.Equals(Value, default))
            {
                var selectedIndex = _filteredItems.FindIndex(item => IsSelected(item));
                if (selectedIndex >= 0)
                {
                    _highlightedIndex = selectedIndex;
                }
            }
        }
        else
        {
            if (_jsModule != null)
            {
                await _jsModule.InvokeVoidAsync("cleanup");
            }
        }
    }

    private async Task OnInputClickAsync()
    {
        await ToggleDropdownAsync();
    }

    private async Task OnFilterTextChangedAsync(ChangeEventArgs e)
    {
        _filterText = e.Value?.ToString() ?? string.Empty;
        UpdateFilteredItems();
        _highlightedIndex = 0;
    }

    private async Task OnKeyDownAsync(KeyboardEventArgs e)
    {
        if (!_isOpen)
        {
            if (e.Key == "ArrowDown" || e.Key == "Enter" || e.Key == " ")
            {
                await ToggleDropdownAsync();
            }
            return;
        }

        switch (e.Key)
        {
            case "ArrowDown":
                _highlightedIndex = Math.Min(_highlightedIndex + 1, _filteredItems.Count - 1);
                StateHasChanged();
                break;

            case "ArrowUp":
                _highlightedIndex = Math.Max(_highlightedIndex - 1, 0);
                StateHasChanged();
                break;

            case "Enter":
                if (_filteredItems.Count > 0 && _highlightedIndex < _filteredItems.Count)
                {
                    await SelectItemAsync(_filteredItems[_highlightedIndex]);
                }
                break;

            case "Escape":
                await CloseDropdownAsync();
                break;

            case "Tab":
                await CloseDropdownAsync();
                break;
        }
    }

    private async Task SelectItemAsync(TItem item)
    {
        var itemValue = GetValue(item);
        Value = itemValue;
        await ValueChanged.InvokeAsync(itemValue);
        await CloseDropdownAsync();
    }

    private async Task CloseDropdownAsync()
    {
        _isOpen = false;
        _filterText = string.Empty;
        UpdateFilteredItems();

        if (_jsModule != null)
        {
            await _jsModule.InvokeVoidAsync("cleanup");
        }
    }

    private void OnMouseEnter(int index)
    {
        _highlightedIndex = index;
        StateHasChanged();
    }

    /// <summary>
    /// Called from JavaScript when clicking outside the dropdown.
    /// </summary>
    [JSInvokable]
    public async Task HandleClickOutsideAsync()
    {
        await CloseDropdownAsync();
        StateHasChanged();
    }

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        if (_jsModule != null)
        {
            await _jsModule.InvokeVoidAsync("cleanup");
            await _jsModule.DisposeAsync();
        }

        _dotNetRef?.Dispose();

        GC.SuppressFinalize(this);
    }
}
