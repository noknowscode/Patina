using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Collection: Data", typeof(CollectionPage))]
public sealed partial class CollectionPageDataPreview : Preview
{
    public CollectionPageDataPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new CollectionViewModel(PreviewServices.Store(PreviewState.Data));
}
