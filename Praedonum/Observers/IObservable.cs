using System.Collections.Generic;

namespace Praedonum.Observers
{
    public interface IObservable
    {
        #region Properties

        IList<IObserver> Observers { get; set; }

        #endregion

        #region Functions

        void Attach(IObserver observer);

        void Detach(IObserver observer);

        #endregion
    }
}
