/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
type EventName = "onRowTransform" | "onNestedClick" | "onCellTransform" | "onPageLoaded" | "onRowSelected" | "onBinaryDataDownload" | "onViewRecordSelected" | "onInitialized" | "onOptionSelected" | "onOptionsLoaded" | "onFormElementCreated" | "onRecordUpdated" | "onRecordInserted" | "onRecordDeleted" | "onInsertInitalize" | "onRecordSelected" | "onFileSelected";
interface CellDataDownloadArgs {
    row: HTMLTableRowElement;
    cell: HTMLTableCellElement;
    extension: string;
    fileName: string;
    columnName: string;
    image?: HTMLImageElement;
}
interface ViewRecordSelectedArgs {
    dialog: JQuery<HTMLElement> | undefined;
    record: Dictionary<object> | undefined;
}
type EventHandler = {
    sender: DbNetSuite;
    params: object | undefined;
};
type InternalEventHandler = {
    context: DbNetSuite;
    method: EventHandler;
};
type EmptyCallback = (sender: DbNetSuite, args?: object) => void;
declare class DbNetSuite {
    static DBNull: string;
    datePickerOptions: JQueryUI.DatepickerOptions;
    protected element: JQuery<HTMLElement> | undefined;
    protected eventHandlers: Dictionary<Array<EventHandler>>;
    protected internalEventHandlers: Dictionary<Array<InternalEventHandler>>;
    protected id: string;
    protected loadingPanel: JQuery<HTMLElement> | undefined;
    protected connectionString: string;
    protected connectionType: DbConnectionType;
    protected culture: string;
    protected linkedControls: Array<DbNetSuite>;
    protected messageBox: MessageBox | undefined;
    protected parentControlType: string;
    parentChildRelationship: ParentChildRelationship;
    initialised: boolean;
    protected imageViewer: ImageViewer | undefined;
    constructor(id: string | null);
    bind(event: EventName, handler: EventHandler): void;
    internalBind(event: EventName, callback: EmptyCallback): void;
    unbind(event: EventName, handler: EventHandler): void;
    checkStyleSheetLoaded(): void;
    addLinkedControl(control: DbNetSuite): void;
    fireEvent(event: EventName, params?: object | undefined): void;
    protected addPanel(panelId: string, parent?: JQuery<HTMLElement> | undefined): JQuery<HTMLElement>;
    protected addLoadingPanel(): void;
    protected showLoader(): void;
    protected hideLoader(): void;
    protected post<T>(action: string, request: any, blob?: boolean, page?: string | null): Promise<T>;
    controlElement(name: string): JQuery<HTMLElement>;
    protected controlElementId(name: string): string;
    protected disable(id: string, disabled: boolean): void;
    protected setInputElement(name: string, value: number): void;
    protected configureLinkedControls(id: object | null, pk?: string | null, fk?: string | null): void;
    protected info(text: string, element: JQuery<HTMLElement>): void;
    protected confirm(text: string, element: JQuery<HTMLElement>, callback: MessageBoxCallback): void;
    protected error(text: string, element?: JQuery<HTMLElement> | null): void;
    private showMessageBox;
    protected addDatePicker($input: JQuery<HTMLInputElement>, datePickerOptions: JQueryUI.DatepickerOptions): void;
    private pickerSelected;
    protected addTimePicker($input: JQuery<HTMLInputElement>): void;
    private _configureLinkedControl;
    private _getRequest;
    protected highlight(): void;
    protected viewImage(event: JQuery.ClickEvent<HTMLElement>): void;
}
