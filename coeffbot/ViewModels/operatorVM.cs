using coeffbot.Model.bot;
using coeffbot.Operators;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Text;
using System.Threading.Tasks;

namespace coeffbot.ViewModels
{
    public class operatorVM : SubContentVM
    {
        #region properties
        Operator op;
        Operator OP {
            get => op;
            set => this.RaiseAndSetIfChanged(ref op, value);
        }

        BotOperators bo;
        BotOperators BO
        {
            get => bo;
            set => this.RaiseAndSetIfChanged(ref bo, value);
        }

        public List<OperatorPermission> Permissions { get; } 

        public ObservableCollection<OperatorPermission> SelectedPermissions { get; } = new();
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> okCmd { get; }
        #endregion


        public operatorVM(BotOperators bo, Operator op)
        {
            OP = op;
            BO = bo;

            Permissions = new  List<OperatorPermission>() {
                new OperatorPermission() {
                    name = "Полный доступ", type = OperatorPermissionType.all
                },
                new OperatorPermission()
                {
                    name = "Установка статусов", type = OperatorPermissionType.set_user_status
                },
                new OperatorPermission()
                {
                    name = "Запрос статусов", type = OperatorPermissionType.get_user_status
                }
            };

            foreach (var p in op.permissions)
            {
                var found = Permissions.FirstOrDefault(o => o.type == p.type);
                if (found != null)
                    SelectedPermissions.Add(found);
            }

            #region commands
            okCmd = ReactiveCommand.Create(() => {

                OP.permissions.Clear();
                foreach (var p in SelectedPermissions)
                    OP.permissions.Add(p);

                OperatorUpdatedEvent?.Invoke(BO, OP);
            });
            #endregion
        }

        public event Action<BotOperators, Operator> OperatorUpdatedEvent;

    }
}
