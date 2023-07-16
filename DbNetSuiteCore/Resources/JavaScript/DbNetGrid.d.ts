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
    ImageAndText = 2
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
declare class DbNetGrid extends DbNetGridEdit {
    autoRowSelect: boolean;
    booleanDisplayMode: BooleanDisplayMode;
    cellIndexCache: Dictionary<number>;
    columnName: string | undefined;
    columnFilters: Dictionary<string>;
    copy: boolean;
    currentPage: number;
    defaultColumn: GridColumn | undefined;
    delete: boolean;
    dragAndDrop: boolean;
    dropIcon: JQuery<HTMLElement> | undefined;
    dropTarget: JQuery<HTMLElement> | undefined;
    editDialog: EditDialog | undefined;
    editDialogControl: DbNetEdit | undefined;
    editDialogId: string;
    export_: boolean;
    fixedFilterParams: Dictionary<object>;
    fixedFilterSql: string;
    frozenHeader: boolean;
    googleChartOptions: GoogleChartOptions | undefined;
    gridGenerationMode: GridGenerationMode;
    gridPanel: JQuery<HTMLElement> | undefined;
    groupBy: boolean;
    insert: boolean;
    multiRowSelect: boolean;
    multiRowSelectLocation: MultiRowSelectLocation;
    nestedGrid: boolean;
    optimizeForLargeDataset: boolean;
    orderBy: string;
    orderByDirection: string;
    pageSize: number;
    primaryKey: string | undefined;
    procedureName: string;
    procedureParams: Dictionary<object>;
    rowSelect: boolean;
    totalPages: number;
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
    private handleRowClick;
    private handleHeaderClick;
    private handleRowSelectClick;
    private handleClick;
    private columnFilterKeyPress;
    private runColumnFilterSearch;
    clearColumnFilters(): void;
    getPage(callback?: Function): void;
    private activeElementId;
    private focusActiveElement;
    private download;
    private htmlExport;
    private downloadSpreadsheet;
    private getViewContent;
    private initEditDialog;
    private openEditDialog;
    private insertRow;
    private deleteRow;
    private copyGrid;
    private message;
    getRequest(): DbNetGridRequest;
    private addEventListener;
    downloadCellData(element: HTMLElement, image: boolean): void;
    private openNestedGrid;
    private configureNestedGrid;
    configureLinkedGrid(control: DbNetSuite, fk: object | null): void;
    private assignForeignKey;
}
