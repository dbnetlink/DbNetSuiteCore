/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
type EventName = "onRowTransform" | "onNestedClick" | "onCellTransform" | "onPageLoaded" | "onRowSelected" | "onCellDataDownload" | "onViewRecordSelected" | "onInitialized";
interface CellDataDownloadArgs {
    row: HTMLTableRowElement;
    cell: HTMLTableCellElement;
    extension: string;
    fileName: string;
    columnName: string;
    image?: HTMLImageElement;
}
interface ViewRecordSelectedArgs {
    dialog: JQuery<HTMLElement>;
    record: Dictionary<string>;
}
type EventHandler = {
    sender: DbNetSuite;
    params: object | undefined;
};
declare class DbNetSuite {
    datePickerOptions: JQueryUI.DatepickerOptions;
    protected element: JQuery<HTMLElement> | undefined;
    protected eventHandlers: Dictionary<Array<EventHandler>>;
    protected id: string;
    protected loadingPanel: JQuery<HTMLElement> | undefined;
    bind(event: EventName, handler: EventHandler): void;
    unbind(event: EventName, handler: EventHandler): void;
    checkStyleSheetLoaded(): void;
    fireEvent(event: EventName, params?: object | undefined): false | undefined;
    protected addPanel(panelId: string, parent?: JQuery<HTMLElement> | undefined): JQuery<HTMLElement>;
    protected addLoadingPanel(): void;
    protected error(text: string): void;
    protected showLoader(): void;
    protected hideLoader(): void;
}
