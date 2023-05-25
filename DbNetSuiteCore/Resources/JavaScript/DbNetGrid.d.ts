/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
type DbConnectionType = "Sqlite" | "SqlServer";
type ColumnPropertyType = "format" | "lookup" | "style" | "foreignKey" | "filter" | "filterMode" | "download" | "image";
type ToolbarPosition = "Top" | "Bottom" | "Hidden";
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
interface Dictionary<T> {
    [Key: string]: T;
}
declare class DbNetGrid extends DbNetSuite {
    autoRowSelect: boolean;
    booleanDisplayMode: BooleanDisplayMode;
    cellIndexCache: Dictionary<number>;
    columnName: string | undefined;
    columns: GridColumn[];
    columnFilters: Dictionary<string>;
    connectionType: DbConnectionType;
    connectionString: string;
    copy: boolean;
    culture: string;
    currentPage: number;
    defaultColumn: GridColumn | undefined;
    dragAndDrop: boolean;
    dropIcon: JQuery<HTMLElement> | undefined;
    dropTarget: JQuery<HTMLElement> | undefined;
    element: JQuery<HTMLElement>;
    export_: boolean;
    fixedFilterParams: Dictionary<object>;
    fixedFilterSql: string;
    fromPart: string;
    frozenHeader: boolean;
    googleChartOptions: GoogleChartOptions | undefined;
    gridPanel: JQuery<HTMLElement> | undefined;
    groupBy: boolean;
    id: string;
    initialised: boolean;
    linkedGrids: Array<DbNetGrid>;
    loadingPanel: JQuery<HTMLElement> | undefined;
    lookupDialog: LookupDialog | undefined;
    multiRowSelect: boolean;
    multiRowSelectLocation: MultiRowSelectLocation;
    navigation: boolean;
    nestedGrid: boolean;
    optimizeForLargeDataset: boolean;
    orderBy: string;
    orderByDirection: string;
    pageSize: number;
    primaryKey: string | undefined;
    procedureName: string;
    procedureParams: Dictionary<object>;
    quickSearch: boolean;
    quickSearchDelay: number;
    quickSearchMinChars: number;
    quickSearchTimerId: number | undefined;
    quickSearchToken: string;
    rowSelect: boolean;
    search: boolean;
    searchDialog: SearchDialog | undefined;
    searchFilterJoin: string;
    searchParams: Array<SearchParam>;
    toolbarButtonStyle: ToolbarButtonStyle;
    toolbarPanel: JQuery<HTMLElement> | undefined;
    toolbarPosition: ToolbarPosition;
    totalPages: number;
    view: boolean;
    viewDialog: ViewDialog | undefined;
    constructor(id: string);
    initialize(): void;
    setColumnExpressions(...columnExpressions: string[]): void;
    setColumnKeys(...columnKeys: string[]): void;
    setColumnLabels(...labels: string[]): void;
    setColumnProperty(columnName: string | Array<string>, property: ColumnPropertyType, propertyValue: any): void;
    setColumnProperties(columnName: string, properties: GridColumnProperties): void;
    addNestedGrid(handler: Function): void;
    addLinkedGrid(grid: DbNetGrid): void;
    columnIndex(columnName: string): number;
    columnCell(columnName: string, row: HTMLTableRowElement | undefined): HTMLTableCellElement | null;
    selectedRow(): HTMLTableRowElement;
    selectedRows(): JQuery<HTMLElement> | undefined;
    columnValue(columnName: string, row: HTMLTableRowElement): any;
    reload(): void;
    getDataArray(callback: Function): void;
    private matchingColumn;
    private addPanel;
    private gridElement;
    private gridElementId;
    private setInputElement;
    private configureToolbar;
    private configureGrid;
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
    private quickSearchKeyPress;
    private columnFilterKeyPress;
    private runQuickSearch;
    private runColumnFilterSearch;
    clearColumnFilters(): void;
    getPage(callback?: Function): void;
    lookup($input: JQuery<HTMLInputElement>): void;
    private activeElementId;
    private focusActiveElement;
    private download;
    private htmlExport;
    private downloadSpreadsheet;
    private getViewContent;
    private openSearchDialog;
    private copyGrid;
    private message;
    private getRequest;
    private addEventListener;
    private disable;
    private showLoader;
    private hideLoader;
    private post;
    downloadCellData(element: HTMLElement, image: boolean): void;
    private openNestedGrid;
    private configureNestedGrid;
    private assignForeignKey;
    private error;
}
