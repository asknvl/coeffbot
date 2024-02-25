using coeffbot.Models.messages;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace coeffbot.ViewModels
{
    public class messageControlVM : ViewModelBase
    {
        #region properties        
        bool isset;
        public bool IsSet
        {
            get => isset;
            set => this.RaiseAndSetIfChanged(ref isset, value); 

        }

        string code;
        public string Code
        {
            get => code;
            set => this.RaiseAndSetIfChanged(ref code, value); 

        }

        string description;
        public string Description
        {
            get => description;
            set => this.RaiseAndSetIfChanged(ref description, value);
        }
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> updateCmd { get; }
        public ReactiveCommand<Unit, Unit> showCmd { get; }
        #endregion        

        public messageControlVM(IMessageUpdater updater)
        {   
            updater.StateMessageUpdatedEvent += (code, isset) => {
                if (code.Equals(Code))
                    IsSet = isset;
            };

            #region commands
            updateCmd = ReactiveCommand.CreateFromTask(async () => {
                updater?.UpdateMessageRequest(Code);
            });
            showCmd = ReactiveCommand.CreateFromTask(async () => { 
                updater?.ShowMessageRequest(Code);
            });
            #endregion
        }

    }
}
