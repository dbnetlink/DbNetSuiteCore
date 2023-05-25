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
    record: Dictionary<String>;
}
declare class DbNetSuite {
    datePickerOptions: JQueryUI.DatepickerOptions;
    protected eventHandlers: Dictionary<Array<Function>>;
    constructor();
    bind(event: EventName, handler: Function): void;
    unbind(event: EventName, handler: Function): void;
    fireEvent(event: EventName, params?: Object | undefined): false | undefined;
}
