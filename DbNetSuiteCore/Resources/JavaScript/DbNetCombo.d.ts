declare class DbNetCombo extends DbNetSuite {
    addEmptyOption: boolean;
    addFilter: boolean;
    currentValue: string;
    emptyOptionText: string;
    linkedControls: Array<DbNetSuite>;
    sql: string;
    params: Dictionary<object>;
    filterDelay: number;
    filterMinChars: number;
    filterTimerId: number | undefined;
    filterToken: string;
    constructor(id: string);
    initialize(): void;
    addLinkedControl(control: DbNetGrid): void;
    reload(): void;
    private configureCombo;
    private filterKeyPress;
    private applyFilter;
    callServer(action: string): void;
    private getRequest;
    private post;
}
