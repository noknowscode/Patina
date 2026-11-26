using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Queue: Loading", typeof(QueuePage))]
public sealed partial class QueuePageLoadingPreview : Preview
{
    public QueuePageLoadingPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new QueueViewModel(PreviewServices.Store(PreviewState.Loading));
}
