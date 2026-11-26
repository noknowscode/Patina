using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Map: Data", typeof(MapPage))]
public sealed partial class MapPageDataPreview : Preview
{
    public MapPageDataPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new MapViewModel(PreviewServices.Store(PreviewState.Data));
}
