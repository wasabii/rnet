namespace Rnet.Profiles.Types
{

    public sealed class NumericValue
    {

        decimal value;
        //decimal min;
        //decimal max;
        //decimal step;
        //decimal multiple;

        /// <summary>
        /// Gets or sets the current value.
        /// </summary>
        public decimal Value
        {
            get { return value; }
            set { this.value = value; }
        }

    }

}
