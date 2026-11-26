using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Artwork: Data", typeof(ArtworkPage))]
public sealed partial class ArtworkPageDataPreview : Preview
{
    public ArtworkPageDataPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new ArtworkViewModel(PreviewServices.Artwork, PreviewServices.Store(PreviewState.Data), PreviewServices.Navigator);
}
