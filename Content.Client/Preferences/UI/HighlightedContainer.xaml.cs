using Robust.Client.AutoGenerated;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.XAML;

namespace Content.Client.Preferences.UI;

[GenerateTypedNameReferences]
public sealed partial class HighlightedContainer : PanelContainer
{
    public HighlightedContainer()
    {
        RobustXamlLoader.Load(this);
    }
}
