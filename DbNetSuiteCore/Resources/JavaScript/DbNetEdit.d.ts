/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
type DbNetEditResponseCallback = (response: DbNetEditResponse) => void;
type EditMode = "update" | "insert";
declare class DbNetEdit extends DbNetGridEdit {
    browseDialog: BrowseDialog | undefined;
    browseDialogControl: DbNetGrid | undefined;
    browseDialogId: string;
    changes: Dictionary<object>;
    currentRow: number;
    delete: boolean;
    editMode: EditMode;
    editPanel: JQuery<HTMLElement> | undefined;
    formPanel: JQuery<HTMLElement> | undefined;
    insert: boolean;
    layoutColumns: number;
    messagePanel: JQuery<HTMLElement> | undefined;
    primaryKey: string;
    search: boolean;
    totalRows: number;
    isEditDialog: boolean;
    constructor(id: string);
    initialize(primaryKey?: string | null): void;
    addLinkedControl(control: DbNetSuite): void;
    getRows(callback?: DbNetEditResponseCallback): void;
    private clearForm;
    disableForm(disable: boolean): void;
    private configureEdit;
    private configureForm;
    private updateForm;
    private callServer;
    getRequest(): DbNetEditRequest;
    private configureToolbar;
    private updateColumns;
    private handleClick;
    configureLinkedControl(control: DbNetSuite, pk: string | null): void;
    getRecord(primaryKey?: string | null): void;
    insertRecord(): void;
    updateForeignKeyValue(fk: object | string): void;
    private configureToolbarButtons;
    private formElements;
    deleteRecord(): void;
    deletionConfirmed(buttonPressed: MessageBoxButtonType): void;
    private recordDeleted;
    private applyChanges;
    private cancelChanges;
    private applyChangesCallback;
    private initBrowseDialog;
    private openBrowseDialog;
    private browseDialogRowSelected;
    private message;
    private clearMessage;
    private highlightField;
    private clearHighlightedFields;
    private selectDate;
    private editLookup;
    private selectTime;
    private uuid;
}
