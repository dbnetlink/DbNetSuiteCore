declare class SearchDialog extends Dialog {
    parent: DbNetGridEdit;
    timerId: number | undefined;
    inputBuffer: Dictionary<string>;
    constructor(id: string, parent: DbNetGridEdit);
    private dialogOpened;
    private configureForOperator;
    private filterNumericKeyPress;
    private checkInputBuffer;
    private criteriaEntered;
    private selectDate;
    private lookup;
    private selectTime;
    clear(): void;
    private apply;
    private getPageCallback;
}
