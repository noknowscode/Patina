using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Settings: Data", typeof(SettingsPage))]
public sealed partial class SettingsPageDataPreview : Preview
{
    public SettingsPageDataPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new SettingsViewModel(
            new PreviewAppSettings(),
            new PreviewLocalization(),
            PreviewServices.Store(PreviewState.Data),
            new PreviewTransfer(),
            PreviewServices.Navigator);
}
