using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Treatment: Error", typeof(TreatmentPage))]
public sealed partial class TreatmentPageErrorPreview : Preview
{
    public TreatmentPageErrorPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new TreatmentViewModel(PreviewServices.Treatment, PreviewServices.Store(PreviewState.Error), new PreviewAppSettings(), PreviewServices.Navigator);
}
