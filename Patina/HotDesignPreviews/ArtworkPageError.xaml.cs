using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Artwork: Error", typeof(ArtworkPage))]
public sealed partial class ArtworkPageErrorPreview : Preview
{
    public ArtworkPageErrorPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new ArtworkViewModel(PreviewServices.Artwork, PreviewServices.Store(PreviewState.Error), PreviewServices.Navigator);
}
