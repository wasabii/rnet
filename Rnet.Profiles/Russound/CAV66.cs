namespace Rnet.Profiles.Russound
{

    [ProfileProvider]
    public class CAV66 : RussoundController
    {

        protected override bool IsSupportedModel(string model)
        {
            return model == "CAV 6.6";
        }


    }

}
