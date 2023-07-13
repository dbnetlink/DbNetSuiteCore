declare class SearchDialog extends Dialog {
    parent: DbNetGridEdit;
    constructor(id: string, parent: DbNetGridEdit);
    private dialogOpened;
    private configureForOperator;
    private criteriaEntered;
    private selectDate;
    private lookup;
    private selectTime;
    clear(): void;
    private apply;
    private getPageCallback;
}
