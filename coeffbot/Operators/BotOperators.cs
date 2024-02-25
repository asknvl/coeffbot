using coeffbot.ViewModels;
using Newtonsoft.Json;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace coeffbot.Operators
{
    public class BotOperators : ViewModelBase
    {
        #region properties        
        string? _geotag;
        [JsonProperty]
        public string? geotag
        {
            get => _geotag;
            set => this.RaiseAndSetIfChanged(ref _geotag, value);   
        }

        Operator selectedOperator;
        [JsonIgnore]
        public Operator SelectedOperator
        {
            get => selectedOperator;
            set
            {                
                this.RaiseAndSetIfChanged(ref selectedOperator, value);
                if (value != null)
                    OperatorSelectedEvent?.Invoke(this, selectedOperator);
            }
        }

        [JsonProperty]
        public ObservableCollection<Operator> Operators { get; } = new();
        #endregion

        #region commands
        public ReactiveCommand<Unit, Unit> addOperatorCmd { get; }
        public ReactiveCommand<Unit, Unit> deleteOperatorCmd { get; }
        #endregion

        public BotOperators()
        {
            addOperatorCmd = ReactiveCommand.Create(() => {
                var op = new Operator();
                //Operators.Add(op);
                OperatosParameterRequest?.Invoke(this, op);
            });

            deleteOperatorCmd = ReactiveCommand.Create(() => {
                if (SelectedOperator != null)
                {
                    OperatorRemoved?.Invoke(this, SelectedOperator);
                    Operators.Remove(SelectedOperator);
                }
            });
        }

        public event Action<BotOperators, Operator> OperatosParameterRequest;
        public event Action<BotOperators, Operator> OperatorRemoved;
        public event Action<BotOperators, Operator> OperatorSelectedEvent;
    }
}
