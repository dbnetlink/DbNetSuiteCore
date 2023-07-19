/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
type DbNetEditResponseCallback = (response: DbNetEditResponse) => void;
declare class DbNetEdit extends DbNetGridEdit {
    private applychanges;
    changes: Dictionary<object>;
    currentRow: number;
    delete: boolean;
    formPanel: JQuery<HTMLElement> | undefined;
    insert: boolean;
    layoutColumns: number;
    messagePanel: JQuery<HTMLElement> | undefined;
    primaryKey: string;
    search: boolean;
    totalRows: number;
    isEditDialog: boolean;
    constructor(id: string);
    initialize(): void;
    addLinkedControl(control: DbNetSuite): void;
    getRows(callback?: DbNetEditResponseCallback): void;
    private configureEdit;
    private configureForm;
    private updateForm;
    private callServer;
    getRequest(): DbNetEditRequest;
    private configureToolbar;
    private updateColumns;
    private handleClick;
    getRecord(primaryKey?: string | null): void;
    insertRecord(): void;
    deleteRecord(primaryKey?: string | null): void;
    private applyChanges;
    private editMode;
    private applyChangesCallback;
    private message;
    private clearMessage;
    private highlightField;
    private clearHighlightField;
    private selectDate;
    private editLookup;
    private selectTime;
}
