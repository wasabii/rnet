namespace Rnet.Service.Objects
{

    public class RequestTarget
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public RequestTarget(string[] path, object target)
            : base()
        {
            Path = path;
            Object = target;
        }

        public string[] Path { get; private set; }

        public object Object { get; private set; }

    }

}
