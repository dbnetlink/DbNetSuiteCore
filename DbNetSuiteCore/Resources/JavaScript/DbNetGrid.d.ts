/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
type DbConnectionType = "Sqlite" | "SqlServer";
type ColumnPropertyType = "format" | "lookup" | "style" | "foreignKey" | "filter" | "filterMode" | "download" | "image";
type ToolbarPosition = "Top" | "Bottom" | "Hidden" | undefined;
declare enum ToolbarButtonStyle {
    Image = 0,
    Text = 1,
    ImageAndText = 2,
    TextAndImage = 3
}
declare enum BooleanDisplayMode {
    TrueFalse = 0,
    YesNo = 1,
    Checkbox = 2
}
declare enum MultiRowSelectLocation {
    Left = 0,
    Right = 1
}
declare enum GridGenerationMode {
    Display = 0,
    DataTable = 1
}
interface Dictionary<T> {
    [Key: string]: T;
}
type DbNetGridResponseCallback = (response: DbNetGridResponse) => void;
declare class DbNetGrid extends DbNetGridEdit {
    autoRowSelect: boolean;
    booleanDisplayMode: BooleanDisplayMode;
    cellIndexCache: Dictionary<number>;
    columnFilters: Dictionary<string>;
    copy: boolean;
    currentPage: number;
    defaultColumn: GridColumn | undefined;
    dragAndDrop: boolean;
    dropIcon: JQuery<HTMLElement> | undefined;
    dropTarget: JQuery<HTMLElement> | undefined;
    editDialog: EditDialog | undefined;
    editControl: DbNetEdit | undefined;
    editDialogId: string;
    export_: boolean;
    frozenHeader: boolean;
    googleChartOptions: GoogleChartOptions | undefined;
    gridGenerationMode: GridGenerationMode;
    gridPanel: JQuery<HTMLElement> | undefined;
    groupBy: boolean;
    height: number;
    isBrowseDialog: boolean;
    multiRowSelect: boolean;
    multiRowSelectLocation: MultiRowSelectLocation;
    nestedGrid: boolean;
    orderBy: string;
    orderByDirection: string;
    pageSize: number;
    procedureName: string;
    procedureParams: Dictionary<object>;
    rowSelect: boolean;
    totalPages: number;
    totalRows: number;
    update: boolean;
    view: boolean;
    viewDialog: ViewDialog | undefined;
    viewLayoutColumns: number;
    constructor(id: string);
    initialize(): void;
    addNestedGrid(handler: EventHandler): void;
    addLinkedGrid(grid: DbNetGrid): void;
    columnIndex(columnName: string): number;
    columnCell(columnName: string, row: HTMLTableRowElement | undefined): HTMLTableCellElement | null;
    selectedRow(): HTMLTableRowElement;
    selectedRows(): JQuery<HTMLElement> | undefined;
    columnValue(columnName: string, row: HTMLTableRowElement): any;
    reload(): void;
    getDataArray(callback: Function): void;
    private configureToolbar;
    private configureGrid;
    private configureDataTable;
    private renderChart;
    private loadChart;
    private drawChart;
    private updateColumns;
    private columnDragStarted;
    private columnDragStopped;
    private columnDrag;
    private dragDropOver;
    private dragDropOut;
    private dragDropped;
    private addRowEventHandlers;
    selectRow(tr: JQuery<HTMLElement>): void;
    private handleRowClick;
    private handleHeaderClick;
    private handleRowSelectClick;
    private handleClick;
    private columnFilterKeyPress;
    private runColumnFilterSearch;
    clearColumnFilters(): void;
    getPage(callback?: DbNetGridResponseCallback): void;
    private activeElementId;
    private focusActiveElement;
    private download;
    private htmlExport;
    private downloadSpreadsheet;
    private getViewContent;
    private configureViewDialog;
    private assignPrimaryKey;
    private refreshRow;
    private openEditDialog;
    private updateRow;
    private primaryKeyCheck;
    private insertRow;
    deleteRow(): void;
    deletionConfirmed(buttonPressed: MessageBoxButtonType): void;
    private recordDeleted;
    table(): HTMLTableElement;
    private copyGrid;
    getRequest(): DbNetGridRequest;
    private addEventListener;
    private openNestedGrid;
    private configureNestedGrid;
    configureLinkedControl(control: DbNetSuite, id: object | null, pk: string | null, fk: object | null): void;
    private initialiseEdit;
    downloadBinaryData(element: HTMLElement, image: boolean): void;
    configureEditButtons(edit: DbNetEdit): void;
    configureEdit(sender: DbNetEdit): void;
    viewElement(columnName: string): HTMLElement | undefined;
    private nextRecord;
    private previousRecord;
}
