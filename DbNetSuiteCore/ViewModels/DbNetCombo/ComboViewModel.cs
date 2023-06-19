using DbNetSuiteCore.Helpers;
using System.Collections.Generic;
using System.Data;

namespace DbNetSuiteCore.ViewModels.DbNetCombo
{
    public class ComboViewModel : BaseViewModel
    {
        private int _size = 1;
        private bool _addEmptyOption;

        public DataView DataView { get; set; }
        public bool AddFilter { get; set; }
        public int Size
        {
            get => MultipleSelect ? (_size < 4 ? 4 : _size) : _size;
            set => _size = value;
        }
        public bool MultipleSelect { get; set; } = false;
        public string EmptyOptionText { get; set; } = string.Empty;
        public bool AddEmptyOption
        {
            get => (_addEmptyOption || string.IsNullOrEmpty(EmptyOptionText) == false) && Size == 1;
            set => _addEmptyOption = value;
        }

        public List<string> DataOnlyColumns { get; set; }
        public string TextColumn { get; set; } = string.Empty;
    }
}