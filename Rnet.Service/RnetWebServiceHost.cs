using System;

namespace Rnet.Service
{

    class RnetWebServiceHost : System.ServiceModel.Web.WebServiceHost
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        /// <param name="singletonInstance"></param>
        /// <param name="baseAddresses"></param>
        public RnetWebServiceHost(object singletonInstance, params Uri[] baseAddresses)
            : base(singletonInstance, baseAddresses)
        {

        }

        protected override void OnOpening()
        {
            base.OnOpening();
        }

    }

}
