namespace Rnet.Profiles.Russound
{

    [ProfileProvider]
    public class CAV66Provider : RussoundControllerProvider
    {

        protected override bool IsSupportedModel(string model)
        {
            return model == "CAV 6.6";
        }

        protected override int ZoneCount
        {
            get { return 6; }
        }

    }

}
