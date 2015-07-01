using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using Bazam.Wpf.UIHelpers;
using Bazam.Wpf.ViewModels;
using Nivix.Infrastructure;
using Nivix.Models;
using Nivix.Views.Dialogs;

namespace Nivix.ViewModels
{
    public class SetsViewModel : ViewModelBase<SetsViewModel>
    {
        #region Fields
        private bool _DataIsDirty;
        private string _NewSetCode;
        private string _NewSetName;
        private SetData _SelectedSet;
        private string _SelectedSetCfName;
        private string _SelectedSetCode;
        private string _SelectedSetGathererCode;
        private string _SelectedSetMtgImageCode;
        private string _SelectedSetName;
        private string _SelectedSetReplaceName;
        private string _SelectedSetTcgPlayerName;
        private IList<SetData> _Sets;
        #endregion

        #region Properties
        public ICommand AddCommand
        {
            get
            {
                return new RelayCommand(() => {
                    NewSetView dialog = new NewSetView();
                    dialog.DataContext = this;
                    bool? dialogResult = dialog.ShowDialog();

                    if (dialogResult != null && dialogResult.Value) {
                        Sets.Add(new SetData() {
                            Code = NewSetCode,
                            Name = NewSetName
                        });

                        Sets = Sets.OrderBy(s => s.Name).ToList();
                    }

                    NewSetCode = string.Empty;
                    NewSetName = string.Empty;
                });
            }
        }

        public bool DataIsDirty
        {
            get { return _DataIsDirty; }
            set { ChangeProperty(s => s.DataIsDirty, value); }
        }

        public ICommand DeleteCommand
        {
            get
            {
                return new RelayCommand(
                    () => {
                        DeleteSetView dialog = new DeleteSetView();
                        dialog.DataContext = this;
                        bool? dialogResult = dialog.ShowDialog();

                        if (dialogResult != null && dialogResult.Value) {
                            Sets.Remove(SelectedSet);
                            SelectedSet = null;
                        }
                    },
                    (vm) => { return SelectedSet != null; }
                );
            }
        }

        public string NewSetCode
        {
            get { return _NewSetCode; }
            set { ChangeProperty(s => s.NewSetCode, value); }
        }

        public string NewSetName
        {
            get { return _NewSetName; }
            set { ChangeProperty(s => s.NewSetName, value); }
        }

        public ICommand SaveCommand
        {
            get
            {
                return new RelayCommand(() => {
                    DataBeast.SaveSetData(Sets);
                    SetDataIsDirty();
                });
            }
        }

        public SetData SelectedSet
        {
            get { return _SelectedSet; }
            set 
            { 
                ChangeProperty(s => s.SelectedSet, value); 

                if(value != null) {
                    SelectedSetCfName = value.CfName;
                    SelectedSetCode = value.Code;
                    SelectedSetGathererCode = value.GathererCode;
                    SelectedSetMtgImageCode = value.MtgImageCode;
                    SelectedSetName = value.Name;
                    SelectedSetTcgPlayerName = value.TcgPlayerName;
                }
            }
        }

        public string SelectedSetCfName
        {
            get { return _SelectedSetCfName; }
            set 
            { 
                ChangeProperty(s => s.SelectedSetCfName, value);
                SelectedSet.CfName = value;
                SetDataIsDirty();
            }
        }

        public string SelectedSetCode
        {
            get { return _SelectedSetCode; }
            set 
            { 
                ChangeProperty(s => s.SelectedSetCode, value);
                SelectedSet.Code = value;
                SetDataIsDirty();
            }
        }

        public string SelectedSetGathererCode
        {
            get { return _SelectedSetGathererCode; }
            set 
            { 
                ChangeProperty(s => s.SelectedSetGathererCode, value);
                SelectedSet.GathererCode = value;
                SetDataIsDirty();
            }
        }

        public string SelectedSetMtgImageCode
        {
            get { return _SelectedSetMtgImageCode; }
            set 
            { 
                ChangeProperty(s => s.SelectedSetMtgImageCode, value);
                SelectedSet.MtgImageCode = value;
                SetDataIsDirty();
            }
        }

        public string SelectedSetName
        {
            get { return _SelectedSetName; }
            set { ChangeProperty(s => s.SelectedSetName, value); }
        }

        public string SelectedSetTcgPlayerName
        {
            get { return _SelectedSetTcgPlayerName; }
            set 
            { 
                ChangeProperty(s => s.SelectedSetTcgPlayerName, value);
                SelectedSet.TcgPlayerName = value;
                SetDataIsDirty();
            }
        }

        public IList<SetData> Sets
        {
            get { return _Sets; }
            set { ChangeProperty(s => s.Sets, value); }
        }
        #endregion

        public SetsViewModel()
        {
            DataIsDirty = false;
            LoadData();
        }

        private void LoadData()
        {
            Sets = DataBeast.GetSetData();
        }

        private void SetDataIsDirty()
        {
            SetData[] rawData = DataBeast.GetSetData().ToArray();
            SetData[] modifiedData = Sets.ToArray();

            DataIsDirty = !rawData.SequenceEqual(modifiedData);
        }
    }
}