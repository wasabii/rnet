namespace Rnet.Profiles.Russound
{

    [ProfileProvider]
    public class CAM66Provider : RussoundControllerProvider
    {

        protected override bool IsSupportedModel(string model)
        {
            return model == "CAM 6.6";
        }

        protected override int ZoneCount
        {
            get { return 6; }
        }

    }

}
