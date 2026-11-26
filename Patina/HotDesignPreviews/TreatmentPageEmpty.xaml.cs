using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Treatment: Empty", typeof(TreatmentPage))]
public sealed partial class TreatmentPageEmptyPreview : Preview
{
    public TreatmentPageEmptyPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new TreatmentViewModel(PreviewServices.Treatment, PreviewServices.Store(PreviewState.Empty), new PreviewAppSettings(), PreviewServices.Navigator);
}
