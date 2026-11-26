using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Treatment: Data", typeof(TreatmentPage))]
public sealed partial class TreatmentPageDataPreview : Preview
{
    public TreatmentPageDataPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new TreatmentViewModel(PreviewServices.Treatment, PreviewServices.Store(PreviewState.Data), new PreviewAppSettings(), PreviewServices.Navigator);
}
