using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Survey: Loading", typeof(SurveyPage))]
public sealed partial class SurveyPageLoadingPreview : Preview
{
    public SurveyPageLoadingPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new SurveyViewModel(new SurveyStart(PreviewServices.Artwork.Id), PreviewServices.Store(PreviewState.Loading), new PreviewPhotoService(), new PreviewAppSettings(), PreviewServices.Navigator);
}
