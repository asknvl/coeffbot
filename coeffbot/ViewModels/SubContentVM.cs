using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace coeffbot.ViewModels
{
    public class SubContentVM : ViewModelBase
    {
        public event Action OnCloseRequest;
        public void Close()
        {
            OnCloseRequest?.Invoke();
        }
    }
}
