namespace Rnet.Drivers.Russound
{

    /// <summary>
    /// The driver for CAV 6.6 devices.
    /// </summary>
    public class CAV66 : ControllerDriver
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="controller"></param>
        public CAV66(RnetController controller)
            : base(controller)
        {

        }

        protected override int ZoneCount
        {
            get { return 6; }
        }

    }

}
