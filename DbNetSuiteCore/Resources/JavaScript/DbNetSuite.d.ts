/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
type EventName = "onRowTransform" | "onNestedClick" | "onCellTransform" | "onPageLoaded" | "onRowSelected" | "onCellDataDownload" | "onViewRecordSelected" | "onInitialized" | "onOptionSelected" | "onOptionsLoaded" | "onFormElementCreated" | "onRecordUpdated" | "onRecordInserted";
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
    method: EmptyCallback;
};
declare enum MessageBoxType {
    Error = 0,
    Warning = 1,
    Info = 2,
    Question = 3
}
type EmptyCallback = () => any;
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
    initialised: boolean;
    constructor(id: string | null);
    bind(event: EventName, handler: EventHandler): void;
    internalBind(event: EventName, callback: EmptyCallback): void;
    unbind(event: EventName, handler: EventHandler): void;
    checkStyleSheetLoaded(): void;
    addLinkedControl(control: DbNetSuite): void;
    fireEvent(event: EventName, params?: object | undefined): void;
    protected addPanel(panelId: string, parent?: JQuery<HTMLElement> | undefined): JQuery<HTMLElement>;
    protected addLoadingPanel(): void;
    protected error(text: string): void;
    protected showLoader(): void;
    protected hideLoader(): void;
    protected post<T>(action: string, request: any, blob?: boolean, page?: string | null): Promise<T>;
    controlElement(name: string): JQuery<HTMLElement>;
    protected controlElementId(name: string): string;
    protected disable(id: string, disabled: boolean): void;
    protected setInputElement(name: string, value: number): void;
    protected configureLinkedControls(fk: object | null): void;
    protected info(text: string): void;
    protected addDatePicker($input: JQuery<HTMLInputElement>, datePickerOptions: JQueryUI.DatepickerOptions): void;
    private pickerSelected;
    protected addTimePicker($input: JQuery<HTMLInputElement>): void;
    private configureLinkedControl;
}
