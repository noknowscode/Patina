using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Artwork: Empty", typeof(ArtworkPage))]
public sealed partial class ArtworkPageEmptyPreview : Preview
{
    public ArtworkPageEmptyPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new ArtworkViewModel(PreviewServices.Artwork, PreviewServices.Store(PreviewState.Empty), PreviewServices.Navigator);
}
