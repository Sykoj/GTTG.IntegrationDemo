using System.ComponentModel;

namespace GTTG.IntegrationDemo.MouseInput {

    public interface IMouseInputSourceProvider : INotifyPropertyChanged {

        /// <summary>
        /// Current <see cref="IMouseInputSource"/>. If changed, notified.
        /// </summary>
        IMouseInputSource MouseInputSource { get; }
    }
}
