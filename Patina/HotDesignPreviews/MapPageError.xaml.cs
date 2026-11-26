using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Map: Error", typeof(MapPage))]
public sealed partial class MapPageErrorPreview : Preview
{
    public MapPageErrorPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new MapViewModel(PreviewServices.Store(PreviewState.Error));
}
