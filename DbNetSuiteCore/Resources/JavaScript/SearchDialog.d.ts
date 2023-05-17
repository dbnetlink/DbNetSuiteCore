declare class SearchDialog extends Dialog {
    parent: DbNetGrid;
    constructor(id: string, parent: DbNetGrid);
    private dialogOpened;
    private configureForOperator;
    private pickerSelected;
    private criteriaEntered;
    private selectDate;
    private lookup;
    private selectTime;
    clear(): void;
    private apply;
    private getPageCallback;
    private addDatePicker;
    private addTimePicker;
}
