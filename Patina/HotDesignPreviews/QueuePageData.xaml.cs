using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Queue: Data", typeof(QueuePage))]
public sealed partial class QueuePageDataPreview : Preview
{
    public QueuePageDataPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new QueueViewModel(PreviewServices.Store(PreviewState.Data));
}
