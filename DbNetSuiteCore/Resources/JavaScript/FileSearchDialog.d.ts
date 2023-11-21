declare class FileSearchDialog extends Dialog {
    parent: DbNetFile;
    timerId: number | undefined;
    inputBuffer: Dictionary<string>;
    constructor(id: string, parent: DbNetFile);
    private dialogOpened;
    private configureForOperator;
    private filterNumericKeyPress;
    private checkInputBuffer;
    private criteriaEntered;
    private selectDate;
    clear(): void;
    private apply;
    private getPageCallback;
}
