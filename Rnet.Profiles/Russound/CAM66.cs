namespace Rnet.Profiles.Russound
{

    [ProfileProvider]
    public class CAM66 : RussoundController
    {

        protected override bool IsSupportedModel(string model)
        {
            return model == "CAM 6.6";
        }

    }

}
