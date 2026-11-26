using Uno.UI.HotDesign;

namespace Patina.HotDesignPreviews;

[Preview("Survey: Error", typeof(SurveyPage))]
public sealed partial class SurveyPageErrorPreview : Preview
{
    public SurveyPageErrorPreview() => this.InitializeComponent();

    protected override object? LoadDataContext() =>
        new SurveyViewModel(new SurveyStart(PreviewServices.Artwork.Id), PreviewServices.Store(PreviewState.Error), new PreviewPhotoService(), new PreviewAppSettings(), PreviewServices.Navigator);
}
