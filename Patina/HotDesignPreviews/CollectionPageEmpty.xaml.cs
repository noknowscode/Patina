using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Collection: Empty", typeof(CollectionPage))]
public sealed partial class CollectionPageEmptyPreview : Preview
{
    public CollectionPageEmptyPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new CollectionViewModel(PreviewServices.Store(PreviewState.Empty));
}
