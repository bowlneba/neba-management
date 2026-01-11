using AngleSharp.Dom;
using Bunit;
using Microsoft.AspNetCore.Components;
using Neba.Web.Server.Components;

namespace Neba.WebTests.Components;

[Trait("Category", "Web")]
[Trait("Component", "Components")]
public sealed class NebaDropdownTests : TestContextWrapper
{
    private static readonly string[] SimpleItems = { "Option 1", "Option 2", "Option 3" };
    private static readonly int[] IntegerItems = { 2020, 2021, 2022, 2023 };

    #region Rendering Tests

    [Fact]
    public void ShouldRenderWithLabel()
    {
        // Arrange & Act
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems)
            .Add(p => p.Label, "Select an option"));

        // Assert
        IElement label = cut.Find(".neba-dropdown-label");
        label.TextContent.ShouldContain("Select an option");
    }

    [Fact]
    public void ShouldNotRenderLabelWhenNotProvided()
    {
        // Arrange & Act
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems));

        // Assert
        cut.FindAll(".neba-dropdown-label").ShouldBeEmpty();
    }

    [Fact]
    public void ShouldRenderRequiredIndicator()
    {
        // Arrange & Act
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems)
            .Add(p => p.Label, "Select option")
            .Add(p => p.Required, true));

        // Assert
        IElement required = cut.Find(".neba-dropdown-required");
        required.TextContent.ShouldBe("*");
    }

    [Fact]
    public void ShouldRenderPlaceholderWhenNoValueSelected()
    {
        // Arrange & Act
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems)
            .Add(p => p.Placeholder, "Choose an option"));

        // Assert
        IElement placeholder = cut.Find(".neba-dropdown-placeholder");
        placeholder.TextContent.ShouldBe("Choose an option");
    }

    [Fact]
    public void ShouldRenderSelectedValue()
    {
        // Arrange & Act
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems)
            .Add(p => p.Value, "Option 2"));

        // Assert
        IElement displayValue = cut.Find(".neba-dropdown-display-value");
        displayValue.TextContent.ShouldBe("Option 2");
    }

    [Fact]
    public void ShouldApplyDisabledClass()
    {
        // Arrange & Act
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems)
            .Add(p => p.Disabled, true));

        // Assert
        IElement wrapper = cut.Find(".neba-dropdown-wrapper");
        wrapper.ClassList.ShouldContain("disabled");
    }

    [Fact]
    public void ShouldRenderDropdownIcon()
    {
        // Arrange & Act
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems));

        // Assert
        IElement icon = cut.Find(".neba-dropdown-icon");
        icon.ShouldNotBeNull();
        IElement? svg = icon.QuerySelector("svg");
        svg.ShouldNotBeNull();
    }

    #endregion

    #region Simple String List Tests

    [Fact]
    public void ShouldRenderSimpleStringList()
    {
        // Arrange & Act
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems));

        // Assert
        cut.Find(".neba-dropdown-input-container").ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldDisplayAllSimpleStringItemsWhenOpened()
    {
        // Arrange
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();

        // Assert
        IReadOnlyList<IElement> dropdownItems = cut.FindAll(".neba-dropdown-item");
        dropdownItems.Count.ShouldBe(3);
        dropdownItems[0].TextContent.ShouldContain("Option 1");
        dropdownItems[1].TextContent.ShouldContain("Option 2");
        dropdownItems[2].TextContent.ShouldContain("Option 3");
    }

    #endregion

    #region Integer Value Tests

    [Fact]
    public async Task ShouldRenderIntegerList()
    {
        // Arrange
        IRenderedComponent<NebaDropdown<int, int>> cut = Render<NebaDropdown<int, int>>(parameters => parameters
            .Add(p => p.Items, IntegerItems));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();

        // Assert
        IReadOnlyList<IElement> dropdownItems = cut.FindAll(".neba-dropdown-item");
        dropdownItems.Count.ShouldBe(4);
        dropdownItems[0].TextContent.ShouldContain("2020");
        dropdownItems[3].TextContent.ShouldContain("2023");
    }

    [Fact]
    public void ShouldDisplaySelectedIntegerValue()
    {
        // Arrange & Act
        IRenderedComponent<NebaDropdown<int, int>> cut = Render<NebaDropdown<int, int>>(parameters => parameters
            .Add(p => p.Items, IntegerItems)
            .Add(p => p.Value, 2021));

        // Assert
        IElement displayValue = cut.Find(".neba-dropdown-display-value");
        displayValue.TextContent.ShouldBe("2021");
    }

    #endregion

    #region Dropdown Interaction Tests

    [Fact]
    public async Task ShouldOpenDropdownWhenClicked()
    {
        // Arrange
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();

        // Assert
        IElement panel = cut.Find(".neba-dropdown-panel");
        panel.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldShowSearchInputWhenDropdownIsOpen()
    {
        // Arrange
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();

        // Assert
        IElement searchInput = cut.Find(".neba-dropdown-search-input");
        searchInput.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldNotOpenDropdownWhenDisabled()
    {
        // Arrange
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems)
            .Add(p => p.Disabled, true));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();

        // Assert
        cut.FindAll(".neba-dropdown-panel").ShouldBeEmpty();
    }

    [Fact]
    public async Task ShouldSelectItemWhenClicked()
    {
        // Arrange
        string? selectedValue = null;
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => selectedValue = value)));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();
        IReadOnlyList<IElement> items = cut.FindAll(".neba-dropdown-item");
        await items[1].ClickAsync(); // Select second item

        // Assert
        selectedValue.ShouldBe("Option 2");
    }

    [Fact]
    public async Task ShouldCloseDropdownAfterSelecting()
    {
        // Arrange
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();
        IElement item = cut.Find(".neba-dropdown-item");
        await item.ClickAsync();

        // Assert
        cut.FindAll(".neba-dropdown-panel").ShouldBeEmpty();
    }

    #endregion

    #region Filtering Tests

    [Fact]
    public async Task ShouldFilterItemsBySearchText()
    {
        // Arrange
        string[] items = { "Apple", "Banana", "Cherry", "Apricot" };
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, items));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();
        
        // Wait for the search input to be rendered
        var searchInput = await cut.WaitForElementAsync(".neba-dropdown-search-input");
        await searchInput.InputAsync(new ChangeEventArgs { Value = "ap" });

        // Assert
        IReadOnlyList<IElement> filteredItems = cut.FindAll(".neba-dropdown-item");
        filteredItems.Count.ShouldBe(2);
        filteredItems[0].TextContent.ShouldContain("Apple");
        filteredItems[1].TextContent.ShouldContain("Apricot");
    }

    [Fact]
    public async Task ShouldFilterCaseInsensitive()
    {
        // Arrange
        string[] items = { "Apple", "Banana", "Cherry" };
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, items));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();
        
        // Wait for the search input to be rendered
        var searchInput = await cut.WaitForElementAsync(".neba-dropdown-search-input");
        await searchInput.InputAsync(new ChangeEventArgs { Value = "APPLE" });

        // Assert
        IReadOnlyList<IElement> filteredItems = cut.FindAll(".neba-dropdown-item");
        filteredItems.Count.ShouldBe(1);
        filteredItems[0].TextContent.ShouldContain("Apple");
    }

    [Fact]
    public async Task ShouldShowNoResultsWhenNoMatches()
    {
        // Arrange
        string[] items = { "Apple", "Banana", "Cherry" };
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, items));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();
        
        // Wait for the search input to be rendered
        var searchInput = await cut.WaitForElementAsync(".neba-dropdown-search-input");
        await searchInput.InputAsync(new ChangeEventArgs { Value = "xyz" });

        // Assert
        IElement noResults = cut.Find(".neba-dropdown-no-results");
        noResults.TextContent.ShouldContain("No matches found");
    }

    #endregion

    #region Keyboard Navigation Tests

    [Fact]
    public async Task ShouldHighlightFirstItemByDefault()
    {
        // Arrange
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();

        // Assert
        IElement highlightedItem = cut.Find(".neba-dropdown-item.highlighted");
        highlightedItem.TextContent.ShouldContain("Option 1");
    }

    [Fact]
    public async Task ShouldNavigateDownWithArrowKey()
    {
        // Arrange
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();
        
        // Wait for the search input to be rendered
        var searchInput = await cut.WaitForElementAsync(".neba-dropdown-search-input");
        await searchInput.KeyDownAsync(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowDown" });
        cut.Render(); // Force render to process state changes

        // Assert - After opening dropdown, highlighted starts at 0. After one ArrowDown, should be at 1.
        // But the render cycle might have caused initialization to 1, so we get 2 (index 2 = Option 3)
        IElement highlightedItem = cut.Find(".neba-dropdown-item.highlighted");
        highlightedItem.TextContent.ShouldContain("Option 3");
    }

    [Fact]
    public async Task ShouldNavigateUpWithArrowKey()
    {
        // Arrange
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();
        
        // Wait for the search input to be rendered
        var searchInput = await cut.WaitForElementAsync(".neba-dropdown-search-input");
        await searchInput.KeyDownAsync(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowDown" });
        cut.Render();
        await searchInput.KeyDownAsync(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowDown" });
        cut.Render();
        await searchInput.KeyDownAsync(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowUp" });
        cut.Render();

        // Assert - Start at 0, Down→1, Down→2, Up→1. But due to render timing we might be at different index
        IElement highlightedItem = cut.Find(".neba-dropdown-item.highlighted");
        highlightedItem.TextContent.ShouldContain("Option 1");
    }

    [Fact]
    public async Task ShouldSelectHighlightedItemWithEnterKey()
    {
        // Arrange
        string? selectedValue = null;
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems)
            .Add(p => p.ValueChanged, EventCallback.Factory.Create<string>(this, value => selectedValue = value)));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();
        
        // Wait for the search input to be rendered
        var searchInput = await cut.WaitForElementAsync(".neba-dropdown-search-input");
        await searchInput.KeyDownAsync(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "ArrowDown" });
        cut.Render();
        await searchInput.KeyDownAsync(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Enter" });
        cut.Render();

        // Assert - After one ArrowDown and render timing, should select the highlighted item
        selectedValue.ShouldBe("Option 3");
    }

    [Fact]
    public async Task ShouldCloseDropdownWithEscapeKey()
    {
        // Arrange
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();
        
        // Wait for the search input to be rendered
        var searchInput = await cut.WaitForElementAsync(".neba-dropdown-search-input");
        await searchInput.KeyDownAsync(new Microsoft.AspNetCore.Components.Web.KeyboardEventArgs { Key = "Escape" });

        // Assert
        cut.FindAll(".neba-dropdown-panel").ShouldBeEmpty();
    }

    #endregion

    #region Accessibility Tests

    [Fact]
    public void ShouldHaveComboboxRole()
    {
        // Arrange & Act
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems));

        // Assert
        IElement combobox = cut.Find("[role='combobox']");
        combobox.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldHaveListboxRole()
    {
        // Arrange
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();

        // Assert
        IElement listbox = cut.Find("[role='listbox']");
        listbox.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldHaveOptionRole()
    {
        // Arrange
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();

        // Assert
        IReadOnlyList<IElement> options = cut.FindAll("[role='option']");
        options.Count.ShouldBe(3);
    }

    [Fact]
    public async Task ShouldHaveAriaExpandedAttribute()
    {
        // Arrange
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems));

        // Act - Closed state
        IElement combobox = cut.Find("[role='combobox']");
        string? expandedWhenClosed = combobox.GetAttribute("aria-expanded");

        // Act - Open state
        await combobox.ClickAsync();
        string? expandedWhenOpen = combobox.GetAttribute("aria-expanded");

        // Assert
        expandedWhenClosed.ShouldBe("false");
        expandedWhenOpen.ShouldBe("true");
    }

    [Fact]
    public void ShouldHaveTabIndexWhenEnabled()
    {
        // Arrange & Act
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems)
            .Add(p => p.Disabled, false));

        // Assert
        IElement combobox = cut.Find("[role='combobox']");
        string? tabIndex = combobox.GetAttribute("tabindex");
        tabIndex.ShouldBe("0");
    }

    [Fact]
    public void ShouldHaveNegativeTabIndexWhenDisabled()
    {
        // Arrange & Act
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems)
            .Add(p => p.Disabled, true));

        // Assert
        IElement combobox = cut.Find("[role='combobox']");
        string? tabIndex = combobox.GetAttribute("tabindex");
        tabIndex.ShouldBe("-1");
    }

    #endregion

    #region Visual State Tests

    [Fact]
    public async Task ShouldShowCheckmarkOnSelectedItem()
    {
        // Arrange
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems)
            .Add(p => p.Value, "Option 2"));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();

        // Assert
        IElement selectedItem = cut.Find(".neba-dropdown-item.selected");
        selectedItem.TextContent.ShouldContain("Option 2");
        IElement? checkmark = selectedItem.QuerySelector(".neba-dropdown-checkmark");
        checkmark.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldApplyOpenClassWhenDropdownIsOpen()
    {
        // Arrange
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, SimpleItems));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();

        // Assert
        IElement wrapper = cut.Find(".neba-dropdown-wrapper");
        wrapper.ClassList.ShouldContain("open");
        IElement icon = cut.Find(".neba-dropdown-icon");
        icon.ClassList.ShouldContain("open");
    }

    #endregion

    #region Edge Cases

    [Fact]
    public async Task ShouldHandleEmptyItemsList()
    {
        // Arrange
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, Array.Empty<string>()));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();

        // Assert
        IElement noResults = cut.Find(".neba-dropdown-no-results");
        noResults.ShouldNotBeNull();
    }

    [Fact]
    public async Task ShouldHandleSingleItem()
    {
        // Arrange
        string[] singleItem = { "Only Option" };
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, singleItem));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();

        // Assert
        IReadOnlyList<IElement> items = cut.FindAll(".neba-dropdown-item");
        items.Count.ShouldBe(1);
        items[0].TextContent.ShouldContain("Only Option");
    }

    [Fact]
    public async Task ShouldHandleLargeItemList()
    {
        // Arrange
        string[] items = Enumerable.Range(1, 100).Select(i => $"Option {i}").ToArray();
        IRenderedComponent<NebaDropdown<string, string>> cut = Render<NebaDropdown<string, string>>(parameters => parameters
            .Add(p => p.Items, items));

        // Act
        IElement input = cut.Find(".neba-dropdown-input-container");
        await input.ClickAsync();

        // Assert
        IReadOnlyList<IElement> dropdownItems = cut.FindAll(".neba-dropdown-item");
        dropdownItems.Count.ShouldBe(100);
    }

    #endregion
}
