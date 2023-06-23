using System.Collections.Generic;
using System.Data;

namespace DbNetSuiteCore.ViewModels.DbNetCombo
{
    public class ComboViewModel : BaseViewModel
    {
        private int _size = 1;
        private bool _addEmptyOption;
        private int? _valueIndex = null;
        private int? _textIndex = null;
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
        public string ProcedureName { get; set; } = string.Empty;
        public string ValueColumn { get; set; } = string.Empty;

        public int ValueIndex => GetColumnIndex(ValueColumn, true, ref _valueIndex);
        public int TextIndex => GetColumnIndex(TextColumn, false, ref _textIndex);

        private int GetColumnIndex(string columnName, bool valueColumn, ref int? index)
        {
            if (index.HasValue) return index.Value;

            if (string.IsNullOrEmpty(ProcedureName))
            {
                index = valueColumn ? 0 : string.IsNullOrEmpty(TextColumn) ? 0 : 1;
            }
            else
            {
                index = 0;
                for (var i = 0; i < DataView.Table.Columns.Count; i++)
                {
                    if (DataView.Table.Columns[i].ColumnName.ToLower() == columnName.ToLower())
                    {
                        index = i;
                        break;
                    }
                }

            }
            return index.Value;
        }
    }
}