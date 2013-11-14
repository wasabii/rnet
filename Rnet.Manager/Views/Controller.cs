namespace Rnet.Manager.Views
{

    /// <summary>
    /// Base implementation for a controller.
    /// </summary>
    /// <typeparam name="TModel"></typeparam>
    /// <typeparam name="TViewModel"></typeparam>
    public abstract class Controller<TModel, TViewModel>
    {

        /// <summary>
        /// Initializes a new instance.
        /// </summary>
        public Controller()
        {

        }

        /// <summary>
        /// Creates the view model for the given model.
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        abstract TViewModel CreateViewModel(TModel model);

    }

}
