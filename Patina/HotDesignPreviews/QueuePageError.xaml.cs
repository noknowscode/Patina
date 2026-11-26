using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Queue: Error", typeof(QueuePage))]
public sealed partial class QueuePageErrorPreview : Preview
{
    public QueuePageErrorPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new QueueViewModel(PreviewServices.Store(PreviewState.Error));
}
