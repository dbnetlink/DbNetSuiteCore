/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare class DbNetEdit extends DbNetGridEdit {
    changes: Dictionary<object>;
    columns: EditColumn[];
    currentRow: number;
    formPanel: JQuery<HTMLElement> | undefined;
    search: boolean;
    totalRows: number;
    primaryKey: string;
    constructor(id: string);
    initialize(): void;
    addLinkedControl(control: DbNetSuite): void;
    reload(): void;
    private configureEdit;
    private updateForm;
    private callServer;
    private getRequest;
    private configureToolbar;
    private updateColumns;
    private addEventListener;
    private handleClick;
    private openSearchDialog;
    private quickSearchKeyPress;
    private runQuickSearch;
    private runSearch;
    private getRecord;
    private applyChanges;
    private message;
    private clearMessage;
}
