namespace Rnet.Drivers.Russound
{

    /// <summary>
    /// The driver for CAM 6.6 devices.
    /// </summary>
    public class CAM66 : ControllerDriver
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="controller"></param>
        public CAM66(RnetController controller)
            : base(controller)
        {

        }

        protected override int ZoneCount
        {
            get { return 6; }
        }

    }

}
