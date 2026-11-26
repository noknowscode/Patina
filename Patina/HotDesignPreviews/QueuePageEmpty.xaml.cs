using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Queue: Empty", typeof(QueuePage))]
public sealed partial class QueuePageEmptyPreview : Preview
{
    public QueuePageEmptyPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new QueueViewModel(PreviewServices.Store(PreviewState.Empty));
}
