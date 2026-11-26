using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Map: Loading", typeof(MapPage))]
public sealed partial class MapPageLoadingPreview : Preview
{
    public MapPageLoadingPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new MapViewModel(PreviewServices.Store(PreviewState.Loading));
}
