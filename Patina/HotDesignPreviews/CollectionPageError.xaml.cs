using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Collection: Error", typeof(CollectionPage))]
public sealed partial class CollectionPageErrorPreview : Preview
{
    public CollectionPageErrorPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new CollectionViewModel(PreviewServices.Store(PreviewState.Error));
}
