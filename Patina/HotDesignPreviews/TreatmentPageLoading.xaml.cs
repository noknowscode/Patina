using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Treatment: Loading", typeof(TreatmentPage))]
public sealed partial class TreatmentPageLoadingPreview : Preview
{
    public TreatmentPageLoadingPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new TreatmentViewModel(PreviewServices.Treatment, PreviewServices.Store(PreviewState.Loading), new PreviewAppSettings(), PreviewServices.Navigator);
}
