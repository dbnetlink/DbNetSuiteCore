/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
declare class DbNetEdit extends DbNetGridEdit {
    changes: Dictionary<object>;
    currentRow: number;
    formPanel: JQuery<HTMLElement> | undefined;
    layoutColumns: number;
    messagePanel: JQuery<HTMLElement> | undefined;
    primaryKey: string;
    search: boolean;
    totalRows: number;
    isEditDialog: boolean;
    constructor(id: string);
    initialize(_callback?: Function | undefined): void;
    addLinkedControl(control: DbNetSuite): void;
    getRows(callback?: Function): void;
    private configureEdit;
    private configureForm;
    private updateForm;
    private callServer;
    getRequest(): DbNetEditRequest;
    private configureToolbar;
    private updateColumns;
    private addEventListener;
    private handleClick;
    getRecord(primaryKey?: string | null): void;
    private applyChanges;
    private message;
    private clearMessage;
    private selectDate;
    private editLookup;
    private selectTime;
}
