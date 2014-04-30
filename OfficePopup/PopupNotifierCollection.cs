using System.ComponentModel;
using DotNetControlsEx.OfficePopup;

namespace DotNetControlsEx.OfficePopup
{
    public class PopupNotifierCollection : System.Collections.CollectionBase
    {
        public PopupNotifierCollection()
        {
        }

        public PopupNotifier this[int index]
        {
            get { return (PopupNotifier)List[index]; }
            set { List[index] = value; }
        }

        public int Add(PopupNotifier item)
        {
            return this.List.Add(item);
        }

        public bool Contains(PopupNotifier item)
        {
            return this.List.Contains(item);
        }

        public void Remove(PopupNotifier item)
        {
            this.List.Remove(item);
        }

        public int IndexOf(PopupNotifier item)
        {
            return this.List.IndexOf(item);
        }
    }
}
