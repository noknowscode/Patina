using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Survey: Data", typeof(SurveyPage))]
public sealed partial class SurveyPageDataPreview : Preview
{
    public SurveyPageDataPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new SurveyViewModel(new SurveyStart(PreviewServices.Artwork.Id), PreviewServices.Store(PreviewState.Data), new PreviewPhotoService(), new PreviewAppSettings(), PreviewServices.Navigator);
}
