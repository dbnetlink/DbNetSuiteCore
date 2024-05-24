/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
type EventName = "onRowTransform" | "onNestedClick" | "onCellTransform" | "onPageLoaded" | "onRowSelected" | "onConfigureBinaryData" | "onViewRecordSelected" | "onInitialized" | "onOptionSelected" | "onOptionsLoaded" | "onFormElementCreated" | "onRecordUpdated" | "onRecordInserted" | "onRecordDeleted" | "onInsertInitalize" | "onRecordSelected" | "onFileSelected" | "onFormElementValidationFailed" | "onJsonUpdated";
type DataProvider = "SqlClient" | "SQLite" | "MySqlConnector" | "Npgsql" | "MySql" | null;
interface CellDataDownloadArgs {
    row: HTMLTableRowElement;
    cell: HTMLTableCellElement;
    fileName: string;
    columnName: string;
    image?: HTMLImageElement;
}
interface ViewRecordSelectedArgs {
    dialog: JQuery<HTMLElement> | undefined;
    record: Dictionary<object> | undefined;
    row: HTMLTableRowElement;
}
type InternalEventHandler = {
    context: DbNetSuite;
    method: Function;
};
type EmptyCallback = (sender: DbNetSuite, args?: object) => void;
declare class DbNetSuite {
    static DBNull: string;
    static datePickerOptions: JQueryUI.DatepickerOptions;
    protected element: JQuery<HTMLElement> | undefined;
    protected eventHandlers: Dictionary<Array<Function>>;
    protected internalEventHandlers: Dictionary<Array<InternalEventHandler>>;
    protected id: string;
    protected loadingPanel: JQuery<HTMLElement> | undefined;
    protected connectionString: string;
    protected culture: string;
    protected linkedControls: Array<DbNetSuite>;
    protected messageBox: MessageBox | undefined;
    protected parentControlType: string;
    parentChildRelationship: ParentChildRelationship;
    initialised: boolean;
    protected imageViewer: ImageViewer | undefined;
    protected parentControl: DbNetSuite | null;
    protected dataProvider: DataProvider | null;
    quickSearch: boolean;
    quickSearchDelay: number;
    quickSearchMinChars: number;
    quickSearchTimerId: number | undefined;
    quickSearchToken: string;
    constructor(id: string | null);
    bind(event: EventName, handler: Function): void;
    internalBind(event: EventName, callback: EmptyCallback): void;
    unbind(event: EventName, handler: Function): void;
    checkStyleSheetLoaded(): void;
    jQueryCheck(): void;
    addLinkedControl(control: DbNetSuite): void;
    fireEvent(event: EventName, params?: object | undefined): void;
    protected addPanel(panelId: string, parent?: JQuery<HTMLElement> | undefined): JQuery<HTMLElement>;
    protected addLoadingPanel(): void;
    protected showLoader(): void;
    protected hideLoader(): void;
    protected post<T>(action: string, request: any, blob?: boolean, page?: string | null): Promise<T>;
    controlElement(name: string): JQuery<HTMLElement>;
    protected controlElementId(name: string): string;
    disable(id: string, disabled: boolean): void;
    protected setInputElement(name: string, value: number): void;
    protected configureLinkedControls(id: object | null, pk?: string | null, fk?: string | null): void;
    protected linkedGridOrEdit(): boolean;
    protected parentGridOrEdit(): boolean;
    protected configureParentDeleteButton(disabled: boolean): void;
    protected info(text: string, element: JQuery<HTMLElement>): void;
    protected confirm(text: string, element: JQuery<HTMLElement>, callback: MessageBoxCallback): void;
    protected error(text: string, element?: JQuery<HTMLElement> | null): void;
    private showMessageBox;
    protected addDatePicker($input: JQuery<HTMLInputElement>, datePickerOptions: JQueryUI.DatepickerOptions): void;
    private pickerSelected;
    private _configureLinkedControl;
    protected _getRequest(): DbNetSuiteRequest;
    protected highlight(): void;
    protected viewImage(event: JQuery.ClickEvent<HTMLElement>): void;
    protected viewUrl(url: string, fileName: string, type?: string): void;
    private openImageViewer;
    protected quickSearchKeyPress(event: JQuery.TriggeredEvent): void;
    private runQuickSearch;
    protected copyTableToClipboard(table: HTMLTableElement): void;
    protected sleep(s: number): Promise<unknown>;
}
