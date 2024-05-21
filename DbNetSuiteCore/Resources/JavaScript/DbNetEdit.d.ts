/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
type DbNetEditResponseCallback = (response: DbNetEditResponse) => void;
declare enum EditMode {
    Insert = 0,
    Update = 1,
    Delete = 2
}
type ValidationMessageType = "Native" | "Application";
declare class DbNetEdit extends DbNetGridEdit {
    browseControl: DbNetGrid | undefined;
    browseDialog: BrowseDialog | undefined;
    browseDialogId: string;
    changes: Dictionary<object>;
    currentRow: number;
    delete: boolean;
    editMode: EditMode;
    editPanel: JQuery<HTMLElement> | undefined;
    formPanel: JQuery<HTMLElement> | undefined;
    formData: FormData;
    insert: boolean;
    layoutColumns: number;
    messagePanel: JQuery<HTMLElement> | undefined;
    search: boolean;
    totalRows: number;
    isEditDialog: boolean;
    uploadDialog: UploadDialog | undefined;
    validationMessageType: ValidationMessageType;
    constructor(id: string);
    initialize(primaryKey?: string | null): void;
    getRows(callback?: DbNetEditResponseCallback): void;
    columnValue(columnName: string): any;
    setColumnValue(columnName: string, value: string): void;
    private clearForm;
    disableForm(): void;
    private configureEdit;
    private configureForm;
    private imageLoaded;
    private updateForm;
    private updateOptions;
    private textTransform;
    private toTitleCase;
    private refreshOptions;
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
    private binaryElements;
    private primaryKeyCheck;
    deleteRecord(): void;
    deletionConfirmed(buttonPressed: MessageBoxButtonType): void;
    private recordDeleted;
    private applyChanges;
    private submitChanges;
    private cancelChanges;
    private validateChangesCallback;
    private applyChangesCallback;
    private updateBrowseControl;
    browseControlReloaded(): void;
    private openBrowseDialog;
    private browseDialogRowSelected;
    message(msg: string): void;
    private clearMessage;
    private formElement;
    private highlightField;
    private clearHighlightedFields;
    private selectDate;
    private editLookup;
    protected uploadFile(event: JQuery.TriggeredEvent): void;
    protected deleteFile(event: JQuery.TriggeredEvent): void;
    private uuid;
    private configureBinaryData;
    private getFileName;
    private downloadFile;
    saveFile($element: JQuery<HTMLElement>, file: File | null, fileMetaData?: FileMetaData | null): void;
    private applyLastModified;
    private hasFormData;
}
