using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Artwork: Loading", typeof(ArtworkPage))]
public sealed partial class ArtworkPageLoadingPreview : Preview
{
    public ArtworkPageLoadingPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new ArtworkViewModel(PreviewServices.Artwork, PreviewServices.Store(PreviewState.Loading), PreviewServices.Navigator);
}
