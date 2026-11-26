using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Map: Empty", typeof(MapPage))]
public sealed partial class MapPageEmptyPreview : Preview
{
    public MapPageEmptyPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new MapViewModel(PreviewServices.Store(PreviewState.Empty));
}
