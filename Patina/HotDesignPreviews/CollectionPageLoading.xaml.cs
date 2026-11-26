using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Collection: Loading", typeof(CollectionPage))]
public sealed partial class CollectionPageLoadingPreview : Preview
{
    public CollectionPageLoadingPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new CollectionViewModel(PreviewServices.Store(PreviewState.Loading));
}
