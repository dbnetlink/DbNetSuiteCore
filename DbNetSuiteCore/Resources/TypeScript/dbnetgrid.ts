type DbConnectionType = "Sqlite" | "SqlServer"
type ColumnPropertyType = "format" | "lookup" | "style" | "foreignKey" | "filter" | "filterMode" | "download" | "image"
type ToolbarPosition = "Top" | "Bottom" | "Hidden"

enum ToolbarButtonStyle {
    Image,
    Text,
    ImageAndText
}

enum BooleanDisplayMode {
    TrueFalse,
    YesNo,
    Checkbox
}

enum MultiRowSelectLocation {
    Left,
    Right
}
enum GridGenerationMode {
    Display,
    DataTable
}

interface Dictionary<T> {
    [Key: string]: T;
}

class DbNetGrid extends DbNetSuite {
    autoRowSelect = true;
    booleanDisplayMode: BooleanDisplayMode = BooleanDisplayMode.TrueFalse;
    cellIndexCache: Dictionary<number> = {};
    columnName: string | undefined = undefined;
    columns: GridColumn[];
    columnFilters: Dictionary<string> = {};
    connectionType: DbConnectionType = "SqlServer";
    connectionString = "";
    copy = true;
    culture = "";
    currentPage = 1;
    defaultColumn: GridColumn | undefined = undefined;
    dragAndDrop = true;
    dropIcon: JQuery<HTMLElement> | undefined;
    dropTarget: JQuery<HTMLElement> | undefined;
    export_ = true;
    fixedFilterParams: Dictionary<object> = {};
    fixedFilterSql = "";
    fromPart = "";
    frozenHeader = false;
    googleChartOptions: GoogleChartOptions | undefined = undefined;
    gridGenerationMode: GridGenerationMode = GridGenerationMode.Display;
    gridPanel: JQuery<HTMLElement> | undefined;
    groupBy = false;
    initialised = false;
    linkedGrids: Array<DbNetGrid> = [];
    lookupDialog: LookupDialog | undefined;
    multiRowSelect = false;
    multiRowSelectLocation: MultiRowSelectLocation = MultiRowSelectLocation.Left;
    navigation = true;
    nestedGrid = false;
    optimizeForLargeDataset = false;
    orderBy = "";
    orderByDirection = "asc";
    pageSize = 20;
    primaryKey: string | undefined = undefined;
    procedureName = "";
    procedureParams: Dictionary<object> = {};
    quickSearch = false;
    quickSearchDelay = 1000;
    quickSearchMinChars = 3;
    quickSearchTimerId: number | undefined;
    quickSearchToken = "";
    rowSelect = true;
    search = true;
    searchDialog: SearchDialog | undefined;
    searchFilterJoin = "";
    searchParams: Array<SearchParam> = [];
    toolbarButtonStyle: ToolbarButtonStyle = ToolbarButtonStyle.Image;
    toolbarPanel: JQuery<HTMLElement> | undefined;
    toolbarPosition: ToolbarPosition = "Top";
    totalPages = 0;
    view = false;
    viewDialog: ViewDialog | undefined;

    constructor(id: string) {
        super();
        this.id = id;
        this.columns = [];
        this.element = $(`#${this.id}`) as JQuery<HTMLElement>;
        this.element.addClass("dbnetsuite").addClass("cleanslate")

        this.checkStyleSheetLoaded();

        if (this.element.length == 0) {
            this.error(`DbNetGrid container element '${this.id}' not found`);
            return;
        }
    }

    initialize(): void {
        if (!this.element) {
            return;
        }

        this.element.empty();

        if (this.toolbarPosition == "Top") {
            this.toolbarPanel = this.addPanel("toolbar");
        }
        this.gridPanel = this.addPanel("grid");
        if (this.toolbarPosition == "Bottom") {
            this.toolbarPanel = this.addPanel("toolbar");
        }

        this.addLoadingPanel();
        this.dropIcon = this.addPanel("dropIcon", $("body"));
        this.dropIcon.addClass("drop-icon");

        this.post<DbNetGridResponse>("initialize", this.getRequest())
            .then((response) => {
                this.updateColumns(response);
                this.configureGrid(response);
            })
            .catch(() => {});
        this.initialised = true;
        this.fireEvent("onInitialized");
    }

    setColumnExpressions(...columnExpressions: string[]): void {
        columnExpressions.forEach(columnExpression => {
            const properties = { columnExpression: columnExpression } as GridColumnResponse;
            this.columns.push(new GridColumn(properties));
        });
    }

    setColumnKeys(...columnKeys: string[]): void {
        columnKeys.forEach((value,index) => {
            this.columns[index].columnKey = value;
        });
    }

    setColumnLabels(...labels: string[]): void {
        let idx = 0;
        labels.forEach(label => {
            if (this.columns.length > idx) {
                this.columns[idx].label = label;
                idx++;
            }
        });
    }

    setColumnProperty(columnName: string | Array<string>, property: ColumnPropertyType, propertyValue: any): void {
        if (columnName instanceof Array<string>) {
            columnName.forEach(c => this.setColumnProperty(c, property, propertyValue));
            return;
        }
        let matchingColumn = this.columns.find((col) => { return this.matchingColumn(col, columnName) });

        if (matchingColumn == undefined) {
            const properties = { columnExpression: columnName } as GridColumnResponse;
            matchingColumn = new GridColumn(properties, true)
            this.columns.push(matchingColumn);
        }

        (matchingColumn as any)[property] = propertyValue;
    }

    setColumnProperties(columnName: string, properties: GridColumnProperties) {
        Object.keys(properties).forEach((key: string) => {
            this.setColumnProperty(columnName, key as ColumnPropertyType, properties[key as ColumnPropertyType] as string);
        });
    }

    addNestedGrid(handler: Function) {
        this.bind("onNestedClick", handler);
        this.nestedGrid = true;
    }

    addLinkedGrid(grid: DbNetGrid) {
        this.linkedGrids.push(grid);
    }

    columnIndex(columnName: string) {
        let cellIndex = this.cellIndexCache[columnName];
        if (cellIndex) {
            return cellIndex;
        }

        cellIndex = -1;
        this.gridPanel?.find("th[data-columnname]").get().forEach((th) => {
            if ($(th).data("columnname")) {
                if ($(th).data('columnname').toString().toLowerCase() == columnName.toLowerCase()) {
                    cellIndex = (th as HTMLTableCellElement).cellIndex;
                    this.cellIndexCache[columnName] = cellIndex;
                }
            }
        });
        return cellIndex;
    }

    columnCell(columnName: string, row: HTMLTableRowElement | undefined) {
        const cellIndex = this.columnIndex(columnName);

        if (cellIndex == -1) {
            return null;
        }

        if (row === undefined) {
            row = this.selectedRow();
        }

        return row.cells[cellIndex];
    }

    selectedRow() {
        return this.gridPanel?.find("tr.data-row.selected")[0] as HTMLTableRowElement;
    }

    selectedRows() {
        return this.gridPanel?.find("tr.data-row.selected");
    } 

    columnValue(columnName: string, row: HTMLTableRowElement) {
        const value = $(row).data(columnName.toLowerCase());

        if (value !== undefined) {
            return value;     
        }

        const cell = this.columnCell(columnName, row);

        if (cell) {
            return $(cell).data("value");
        }

        return null;
    }

    reload() {
        this.currentPage = 1;
        this.getPage();
    }

    getDataArray(callback: Function) {
        this.post<any>("data-array", this.getRequest())
            .then((response) => {
                callback(response);
            })
    }
  
    private matchingColumn(gridColumn: GridColumn, columnName: string) {
        let match = false;
        if (gridColumn.columnKey?.includes(".")) {
            match = gridColumn.columnKey?.split('.').pop()?.toLowerCase() == columnName.toLowerCase();
        }
        if (gridColumn.columnKey?.split(' ').pop()?.toLowerCase() == columnName.toLowerCase()) {
            match = true;
        }
        if (!match) {
            match = gridColumn.columnKey?.toLowerCase() == columnName.toLowerCase();
        }
        return match;
    }

    private gridElement(name: string): JQuery<HTMLElement> {
        // eslint-disable-next-line @typescript-eslint/no-non-null-assertion
        return $(`#${this.gridElementId(name)}`)!;
    }

    private gridElementId(name: string): string {
        return `${this.id}_${name}`;
    }

    private setInputElement(name: string, value: number) {
        const el = this.gridElement(name);
        el.val(value.toString());
        el.width(`${value.toString().length}em`);
    }

    private configureToolbar(response: DbNetGridResponse) {
        if (response.toolbar) {
            const buttons = ["First", "Next", "Previous", "Last", "Download", "Copy", "View", "Search"];
            buttons.forEach(btn =>
                this.addEventListener(`${btn}Btn`)
            )
        }

        const $navigationElements = this.gridElement("dbnetgrid-toolbar").find(".navigation");
        const $noRecordsCell = this.gridElement("no-records-cell");
        this.setInputElement("Rows", response.totalRows);

        if (response.totalRows == 0) {
            $navigationElements.hide();
            $noRecordsCell.show();
        }
        else {
            $navigationElements.show();
            $noRecordsCell.hide();
            this.gridElement("dbnetgrid-toolbar").find(".navigation").show();
            this.setInputElement("PageNumber", response.currentPage);
            this.setInputElement("PageCount", response.totalPages);

            this.currentPage = response.currentPage;
            this.totalPages = response.totalPages;

            this.disable("FirstBtn", response.currentPage == 1);
            this.disable("PreviousBtn", response.currentPage == 1);
            this.disable("NextBtn", response.currentPage == response.totalPages);
            this.disable("LastBtn", response.currentPage == response.totalPages);
        }

        this.disable("ViewBtn", response.totalRows == 0);
        this.disable("DownloadBtn", response.totalRows == 0);
        this.disable("CopyBtn", response.totalRows == 0);

        this.gridElement("QuickSearch").on("keyup", (event) => this.quickSearchKeyPress(event));
    }

    private configureGrid(response: DbNetGridResponse) {
        if (this.toolbarPanel) {
            if (response.toolbar) {
                this.toolbarPanel?.html(response.toolbar);
            }
            this.configureToolbar(response);
        }

        if (this.gridGenerationMode.toString() == "DataTable") {
            this.configureDataTable(response.data);
            return;
        }

        this.gridPanel?.html(response.data);

        this.gridPanel?.find("tr.filter-row :input").get().forEach(input => {
            const $input = $(input);
            const width = $input.width();
            if (input.nodeName == "SELECT") {
                $input.width(width! + 20);
            }
            else if (width! < 100) {
                $input.width(100);
            }
            $input.on("keyup", (event) => this.columnFilterKeyPress(event));
        });

        this.gridPanel?.find("tr.data-row").get().forEach((tr) => {
            this.addRowEventHandlers($(tr));
            this.fireEvent("onRowTransform", { row: tr });
        }
        );

        if (this.dragAndDrop && this.procedureName == "")
        {
            this.gridPanel?.find("tr.header-row th")
                .draggable({ helper: "clone", cursor: "move" })
                .on("dragstart", (event: JQuery.TriggeredEvent<HTMLElement>, ui: JQueryUI.DraggableEventUIParams) => this.columnDragStarted(event, ui))
                .on("dragstop", (event: JQuery.TriggeredEvent<HTMLElement>, ui: JQueryUI.DraggableEventUIParams) => this.columnDragStopped(event, ui))
                .on("drag", (event: JQuery.TriggeredEvent<HTMLElement>, ui: JQueryUI.DraggableEventUIParams) => this.columnDrag(event, ui));

            this.gridPanel?.find("tr.header-row th")
                .droppable()
                .on("dropover", (event: JQuery.TriggeredEvent<HTMLElement>, ui: JQueryUI.DroppableEventUIParam) => this.dragDropOver(event, ui))
                .on("dropout", (event: JQuery.TriggeredEvent<HTMLElement>, ui: JQueryUI.DroppableEventUIParam) => this.dragDropOut(event, ui))
                .on("drop", (event: JQuery.TriggeredEvent<HTMLElement>, ui: JQueryUI.DroppableEventUIParam) => this.dragDropped(event, ui));
        }

        if (this.procedureName == "") {
            this.gridPanel?.find("tr.header-row th").get().forEach(th => {
                $(th).on("click", () => this.handleHeaderClick($(th)));
            });
        }

        this.gridPanel?.find("tr.filter-row select").get().forEach(select => {
            $(select).on("change", (event) => this.runColumnFilterSearch());
        });

        this.gridPanel?.find("input.multi-select-checkbox").get().forEach(e => {
            $(e).on("click", (e) => this.handleRowSelectClick(e));
        });

        this.gridPanel?.find("button.nested-grid-button").get().forEach(e => {
            $(e).on("click", (e) => this.openNestedGrid(e));
        });

        this.gridPanel?.find("button.download").get().forEach(e => {
            $(e).on("click", (e) => this.downloadCellData(e.currentTarget, false));
        });

        this.gridPanel?.find("img.image").get().forEach(e => {
            this.downloadCellData(e.parentElement!, true);
        });

        this.gridPanel?.find("th[data-columnname]").get().forEach((th) => {
            const columnName = $(th).data("columnname");
            const cellIdx = (th as HTMLTableCellElement).cellIndex;
            this.gridPanel?.find("tr.data-row").get().forEach((tr) => {
                const td = (tr as HTMLTableRowElement).cells[cellIdx];
                this.fireEvent("onCellTransform", { cell: td, row: tr, columnName: columnName });
            });
        });
        if (this.autoRowSelect) {
            this.gridPanel?.find("tr.data-row").first().trigger("click");
        }
        this.fireEvent("onPageLoaded", { table: this.gridPanel?.find("table.dbnetgrid-table")[0] });
        this.renderChart();

        if (this.frozenHeader) {
            const h: number = this.gridPanel?.find("tr.header-row").height() as number;
            const $filterRow = this.gridPanel?.find("tr.filter-row") as JQuery<HTMLTableRowElement>;
            $filterRow.find("th").css("top", h)
        }
    }

    private configureDataTable(_html: string) {
        this.element.removeClass("dbnetsuite");
        this.toolbarPanel?.addClass("dbnetsuite");
        this.gridPanel?.html(_html);
        this.gridPanel?.find("table").DataTable();
    }

    private renderChart() {
        if (!this.googleChartOptions) {
            return;
        }
        this.getDataArray((dataArray: any) => this.loadChart(dataArray));
    }

    private loadChart(dataArray:any) {
        google.charts.load('current', { 'packages': ['corechart'] });
        google.charts.setOnLoadCallback(() => this.drawChart(dataArray));
    }

    private drawChart(dataArray: any) {
        const dt = google.visualization.arrayToDataTable(dataArray);
        const chart = new (google.visualization as any)[this.googleChartOptions!.type](document.getElementById(this.googleChartOptions!.panelId));

        const options = {}
        if (this.googleChartOptions?.functionName) {
            (window as any)[this.googleChartOptions?.functionName](this, options);
        }

        chart.draw(dt, options);
    }
    private updateColumns(response: DbNetGridResponse) {
        this.columns = new Array<GridColumn>();
        response.columns.forEach((col) => {
            const properties = {
                columnExpression: col.columnExpression,
                columnName: col.columnName,
                label: col.label,
                format: col.format,
                foreignKey: col.foreignKey,
                foreignKeyValue: col.foreignKeyValue,
                lookup: col.lookup,
                style: col.style,
                display: col.display,
                filter: col.filter,
                filterMode: col.filterMode,
                groupHeader: col.groupHeader,
                download: col.download,
                image: col.image,
                view: col.view,
                dataType: col.dataType,
                aggregate: col.aggregate,
                totalBreak: col.totalBreak,
                clearDuplicateValue: col.clearDuplicateValue,
                primaryKey: col.primaryKey,
                index: col.index,
                dataOnly:col.dataOnly
            } as GridColumnResponse;
            this.columns.push(new GridColumn(properties));
        });
    }

    private columnDragStarted(event: JQuery.TriggeredEvent<HTMLElement>, ui: JQueryUI.DraggableEventUIParams) {
        const $el = $(event.currentTarget) as JQuery<HTMLElement>;
        $el.css("opacity", 0.5);
        ui.helper.css("opacity", 0.5);
        ui.helper.attr("dbnetgrid_id", this.id);
        ui.helper.width($el.width()! + 2).height($el.height()! + 2);
    }

    private columnDragStopped(event: JQuery.TriggeredEvent<HTMLElement>, ui: JQueryUI.DraggableEventUIParams) {
        const $el = $(event.currentTarget) as JQuery<HTMLElement>;
        $el.css("opacity", 1);
    }

    private columnDrag(event: JQuery.TriggeredEvent<HTMLElement>, ui: JQueryUI.DraggableEventUIParams) {
        if (!this.dropTarget || ui.helper.attr("dbnetgrid_id") != this.id) {
            return;
        }

        this.dropIcon?.show();

        const width = this.dropTarget.width() as number;
        const pos = this.dropTarget.offset() as JQueryCoordinates;

        const parentOffset = this.dropTarget.parent().offset() as JQueryCoordinates;
        const uiLeft = ui.position.left + parentOffset.left;

        let left = (pos.left - 9);
        const top = (pos.top - 14);

        ui.helper.attr("dropside", "left");
   
        if ((uiLeft + width) > pos.left ) {
            ui.helper.attr("dropside", "right");
            left += width + 4;
            console.log
        }

        this.dropIcon?.css({ "left": `${left}px`, "top": `${top}px` });	
    }

    private dragDropOver(event: JQuery.TriggeredEvent<HTMLElement>, ui: JQueryUI.DroppableEventUIParam) {
        if (ui.helper.attr("dbnetgrid_id") != this.id) {
            return;
        }

        this.dropTarget = $(event.currentTarget);
    }

    private dragDropOut(event: JQuery.TriggeredEvent<HTMLElement>, ui: JQueryUI.DroppableEventUIParam) {
        this.dropIcon?.hide();
        this.dropTarget = undefined;
    }

    private dragDropped(event: JQuery.TriggeredEvent<HTMLElement>, ui: JQueryUI.DroppableEventUIParam) {
        if (ui.helper.attr("dbnetgrid_id") != this.id) {
            return;
        }
        this.dropIcon?.hide();
        const cols: GridColumn[] = [];
        const sourceIdx = parseInt(ui.draggable.data("columnordinal")) -1;
        const targetIdx = parseInt($(event.currentTarget).data("columnordinal")) -1;
        const column: GridColumn = this.columns[sourceIdx];
        const dropSide = ui.helper.attr("dropside");
        for (let i = 0; i < this.columns.length; i++) {
            if (i == targetIdx && dropSide == "left") {
                cols.push(column);
            }
            if (i != sourceIdx) {
                cols.push(this.columns[i]);
            }
            if (i == targetIdx && dropSide == "right") {
                cols.push(column);
            }
        }

        this.columns = cols;
        this.orderBy = '';
        this.getPage();
    }

    private addRowEventHandlers(tr: JQuery<HTMLElement>) {
        tr.on("mouseover", () => tr.addClass("highlight"));
        tr.on("mouseout", () => tr.removeClass("highlight"));
        tr.on("click", () => this.handleRowClick(tr));
    }

    private handleRowClick(tr: JQuery<HTMLElement>) {
        if (this.rowSelect) {
            tr.parent().find("tr.data-row").removeClass("selected").find("input.multi-select-checkbox").prop('checked', false);
            tr.addClass("selected").find("input.multi-select-checkbox").prop('checked', true);
        }

        this.linkedGrids.forEach((grid) => {
            this.assignForeignKey(grid, tr.data("id"));
            if (grid.connectionString == "") {
                grid.connectionString = this.connectionString;
            }
            grid.currentPage = 1;
            grid.initialised ? grid.getPage() : grid.initialize();
        });

        if (this.viewDialog && this.viewDialog.isOpen()) {
            this.getViewContent();
        }

        this.fireEvent("onRowSelected", { row: tr[0] });
    }

    private handleHeaderClick(th: JQuery<HTMLElement>) {
        this.orderBy = th.data('columnordinal') as string;
        const dbDataType = th.data('dbdatatype') as string;

        switch (dbDataType) {
            case "text":
            case "ntext":
            case "image":
                return;
        }

        this.orderByDirection = "asc";
        if (th.attr("orderby")) {
            this.orderByDirection = th.attr("orderby") == "asc" ? "desc" : "asc";
        }
        this.getPage();
    }

    private handleRowSelectClick(event: JQuery.ClickEvent<HTMLElement>) {
        const checkbox = $(event.currentTarget);
        const checked = checkbox.is(':checked');
        let rows: JQuery<HTMLElement>;
        if (checkbox.parent().prop("tagName") == "TH") {
            rows = checkbox.closest("table").find("tbody").children();
            rows.get().forEach(tr => $(tr).find("input.multi-select-checkbox").prop('checked', checked));
        }
        else {
            rows = checkbox.parent().parent("tr");
        }
        if (checked) {
            rows.addClass("selected");
        }
        else {
            rows.removeClass("selected");
        }
        event.stopPropagation();
    }

    private handleClick(event: JQuery.TriggeredEvent): void {
        const id = (event.target as Element).id;

        switch (id) {
            case this.gridElementId("FirstBtn"):
                this.currentPage = 1;
                break;
            case this.gridElementId("NextBtn"):
                this.currentPage++;
                break;
            case this.gridElementId("PreviousBtn"):
                this.currentPage--;
                break;
            case this.gridElementId("LastBtn"):
                this.currentPage = this.totalPages;
                break;
            case this.gridElementId("DownloadBtn"):
            case this.gridElementId("CopyBtn"):
            case this.gridElementId("ViewBtn"):
            case this.gridElementId("SearchBtn"):
               break;
            default:
                return;
        }
        event.preventDefault();

        switch (id) {
            case this.gridElementId("DownloadBtn"):
                this.download();
                break;
            case this.gridElementId("CopyBtn"):
                this.copyGrid();
                break;
            case this.gridElementId("ViewBtn"):
                this.getViewContent();
                break;
            case this.gridElementId("SearchBtn"):
                this.openSearchDialog();
                break;
            default:
                this.getPage();
                break;
        }
    }

    private quickSearchKeyPress(event: JQuery.TriggeredEvent): void {
        const el = event.target as HTMLInputElement;
        window.clearTimeout(this.quickSearchTimerId);

        if (el.value.length >= this.quickSearchMinChars || el.value.length == 0 || event.key == 'Enter')
            this.quickSearchTimerId = window.setTimeout(() => { this.runQuickSearch(el.value) }, this.quickSearchDelay);
    }

    private columnFilterKeyPress(event: JQuery.TriggeredEvent): void {
        window.clearTimeout(this.quickSearchTimerId);
        this.quickSearchTimerId = window.setTimeout(() => { this.runColumnFilterSearch() }, this.quickSearchDelay);
    }

    private runQuickSearch(token: string) {
        this.quickSearchToken = token;
        this.currentPage = 1;
        this.getPage();
    }

    private runColumnFilterSearch() {
        this.columnFilters = {};

        this.gridPanel?.find("tr.filter-row input").get().forEach(e => {
            const $input = $(e as HTMLInputElement);
            if ($input.val() != '') {
                this.columnFilters[$input.data('columnname')] = $input.val()!.toString();
            }
        });

        this.gridPanel?.find("tr.filter-row select").get().forEach(e => {
            const $input = $(e as HTMLSelectElement);
            if ($input.val() != '') {
                this.columnFilters[$input.data('columnname')] = $input.val()!.toString();
            }
        });

        if (this.searchDialog) {
            this.searchDialog.clear();
            this.searchParams = [];
        }

        this.currentPage = 1;
        this.getPage();
    }

    public clearColumnFilters() {
        this.columnFilters = {};

        this.gridPanel?.find("tr.filter-row input").get().forEach(e => {
            const $input = $(e as HTMLInputElement);
            $input.val('');
        });

        this.gridPanel?.find("tr.filter-row select").get().forEach(e => {
            const $input = $(e as HTMLSelectElement);
            $input.val('');
        });
    }

    public getPage(callback?:Function) {
        const activeElementId: string | undefined = this.activeElementId();

        this.post<DbNetGridResponse>("page", this.getRequest())
            .then((response) => {
                if (response.error == false) {
                    this.configureGrid(response);
                    this.focusActiveElement(activeElementId);
                }
                if (callback) {
                    callback(response);
                }
            })
    }

    public lookup($input: JQuery<HTMLInputElement>) {
        const request = this.getRequest();
        request.lookupColumnIndex = parseInt($input.attr("columnIndex") as string);

        if (this.lookupDialog && request.lookupColumnIndex == this.lookupDialog.columnIndex) {
            this.lookupDialog.open();
            return;
        }
        this.post<DbNetGridResponse>("lookup", request)
            .then((response) => {
                if (!this.lookupDialog) {
                    this.element.append(response.toolbar);
                    this.lookupDialog = new LookupDialog(`${this.id}_lookup_dialog`, this);
                }
                this.lookupDialog.update(response, $input);
                this.lookupDialog.open();
            })
    }

    private activeElementId() {
        let activeElementId: string | undefined = undefined;

        if (document.activeElement?.nodeName == "INPUT") {
            activeElementId = document.activeElement?.id;
        }
        return activeElementId;
    }

    private focusActiveElement(activeElementId: string | undefined) {
        if (activeElementId) {
            const $input = $(`#${activeElementId}`) as JQuery<HTMLInputElement>;
            const txt = $input.val() as string;
            $input.trigger("focus").val(txt);
            $input[0].setSelectionRange(txt.length, txt.length);
        }
    }

    private download() {
        switch (this.gridElement("DownloadSelect").val())
        {
            case "html":
                this.htmlExport();
                break;
            case "excel":
                this.downloadSpreadsheet();
                break;
        }
    }

    private htmlExport() {
        this.post<Blob>("html-export", this.getRequest(), true)
            .then((response) => {
                const url = window.URL.createObjectURL(response);
                const tab = window.open() as Window;
                tab.location.href = url;
            });
    }

    private downloadSpreadsheet() {
        this.post<Blob>("generate-spreadsheet", this.getRequest(), true)
            .then((response) => {
                const link = document.createElement("a");
                link.href = window.URL.createObjectURL(response);
                link.download = `report_${new Date().getTime()}.xlsx`;
                link.click();
            });
    }

    private getViewContent() {
        const $row = $(this.selectedRow());
        this.primaryKey = $row.data("id");

        this.post<DbNetGridResponse>("view-content", this.getRequest())
            .then((response) => {
                if (!this.viewDialog) {
                    this.element.append(response.toolbar);
                    this.viewDialog = new ViewDialog(`${this.id}_view_dialog`, this);
                }
                this.viewDialog.update(response, $row)
            });
    }

    private openSearchDialog() {
        if (this.searchDialog) {
            this.searchDialog.open();
            return;
        }

        this.post<DbNetGridResponse>("search-dialog", this.getRequest())
            .then((response) => {
                this.element.append(response.data);
                this.searchDialog = new SearchDialog(`${this.id}_search_dialog`, this);
                this.searchDialog.open();
            });
    }

    private copyGrid() {
        const table = this.gridPanel?.[0].querySelector('table.dbnetgrid-table');

        this.gridPanel?.find("tr.data-row.selected").addClass("unselected").removeClass("selected")

        try {
            const range = document.createRange();
            range.selectNode(table as Node);
            window.getSelection()?.addRange(range);
            document.execCommand('copy');
            window.getSelection()?.removeRange(range);
        } catch (e) {
            try {
                const content = (table as Element).innerHTML;
                const blobInput = new Blob([content], { type: 'text/html' });
                const clipboardItemInput = new ClipboardItem({ 'text/html': blobInput });
                navigator.clipboard.write([clipboardItemInput]);
            }
            catch (e) {
                alert("Copy failed")
                return
            }
        }
        this.gridPanel?.find("tr.data-row.unselected").addClass("selected").removeClass("unselected")

        this.message("Copied")
    }

    private message(text: string) {
        const dialogId = `${this.id}_message_dialog`;
        const $dialog = $(`#${dialogId}`);
        $dialog.text(text)
        $dialog.attr("title", "Copy")

        $dialog.dialog({
            autoOpen: false
        });

        $dialog.dialog("open");
    }
    private getRequest(): DbNetGridRequest {
        this.defaultColumn = this.columns.find((col) => { return col.columnExpression! == "*" });
        if (this.defaultColumn) {
            this.columns = this.columns.filter(item => item !== this.defaultColumn)
        }
        const request: DbNetGridRequest = {
            componentId: this.id,
            connectionString: this.connectionString,
            fromPart: this.fromPart,
            currentPage: this.currentPage,
            pageSize: this.toolbarPosition == "Hidden" ? -1 : this.pageSize,
            orderBy: this.orderBy,
            orderByDirection: this.orderByDirection,
            toolbarButtonStyle: this.toolbarButtonStyle,
            columns: this.columns.map((column) => { return column }),
            multiRowSelect: this.multiRowSelect,
            multiRowSelectLocation: this.multiRowSelectLocation,
            nestedGrid: this.nestedGrid,
            quickSearch: this.quickSearch,
            quickSearchToken: this.quickSearchToken,
            booleanDisplayMode: this.booleanDisplayMode,
            columnFilters: this.columnFilters,
            frozenHeader: this.frozenHeader,
            optimizeForLargeDataset: this.optimizeForLargeDataset,
            defaultColumn: this.defaultColumn,
            primaryKey: this.primaryKey,
            columnName: this.columnName,
            view: this.view,
            groupBy: this.groupBy,
            search: this.search,
            searchFilterJoin:this.searchFilterJoin,
            searchParams: this.searchParams,
            copy: this.copy,
            export: this.export_,
            navigation: this.navigation,
            culture: this.culture,
            fixedFilterParams: this.fixedFilterParams,
            fixedFilterSql: this.fixedFilterSql,
            procedureParams: this.procedureParams,
            procedureName: this.procedureName,
            gridGenerationMode: this.gridGenerationMode,
        };

        return request;
    }

    private addEventListener(id: string, eventName = "click") {
        this.gridElement(id).on(eventName, (event) => this.handleClick(event));
    }

    private disable(id: string, disabled: boolean) {
        this.gridElement(id).prop("disabled", disabled);
    }

    private post<T>(action: string, request: any, blob = false): Promise<T> {
        this.showLoader();
        const options = {
            method: "POST",
            headers: {
                Accept: "application/json",
                "Content-Type": "application/json;charset=UTF-8",
            },
            body: JSON.stringify(request)
        };

        return fetch(`~/dbnetgrid.dbnetsuite?action=${action}`, options)
            .then(response => {
                this.hideLoader();

                if (!response.ok) {
                    throw response;
                }
                if (blob) {
                    return response.blob() as Promise<T>;
                }
                return response.json() as Promise<T>;
            })
            .catch(err => {
                err.text().then((errorMessage: string) => {
                    console.error(errorMessage);
                    this.error(errorMessage.split("\n").shift() as string)
                });

                return Promise.reject()
            })
    }

    public downloadCellData(element: HTMLElement, image: boolean) {
        const $viewContentRow = $(element).closest("tr.view-content-row");
        const $button = $(element);
        const $cell = $button.closest("td");
        const $row = $button.closest("tr");

        if ($viewContentRow.length) {
            this.columnName = $viewContentRow.data("columnname") as string;
        }
        else {
            this.primaryKey = $row.data("id")
            this.columnName = this.gridPanel?.find("th[data-columnname]").get($cell.prop("cellIndex"))?.getAttribute("data-columnname") as string;
        }

        const args: CellDataDownloadArgs = {
            row: $row.get(0) as HTMLTableRowElement,
            cell: $cell.get(0) as HTMLTableCellElement,
            extension: "xlxs",
            fileName: `${this.columnName}_${this.primaryKey}.xlsx`,
            columnName: this.columnName
        }

        if (image) {
            args.image = $cell.find("img").get(0) as HTMLImageElement;
        }

        this.fireEvent("onCellDataDownload", args);

        this.post<Blob>("download-column-data", this.getRequest(), true)
            .then((blob) => {
                if (image) {
                    const img = $cell.find("img") as unknown as JQuery<HTMLElement>;
                    img.attr("src", window.URL.createObjectURL(blob));
                }
                else {
                    const link = document.createElement("a");
                    link.href = window.URL.createObjectURL(blob);
                    link.download = args.fileName;
                    link.click();
                }
            });
    }

    private openNestedGrid(event: JQuery.ClickEvent<HTMLElement>) {
        const $button = $(event.currentTarget);
        const row = $button.closest("tr");

        if ($button.hasClass("open")) {
            $button.removeClass("open");
            row.next().hide();
            return;
        }

        const table = $button.closest("table");

        $button.addClass("open");

        if (row.next().hasClass("nested-grid-row")) {
            row.next().show();
            return;
        }

        const newRow = table[0].insertRow(row[0].rowIndex + 1);
        newRow.className = "nested-grid-row";
        const cell = newRow.insertCell(-1);
        cell.className = "nested-grid-cell";
        cell.colSpan = row[0].cells.length;

        const handlers = this.eventHandlers["onNestedClick"]
        for (let i = 0; i < handlers.length; i++) {
            if (handlers[i]) {
                this.configureNestedGrid(handlers[i], cell, row.data("id"))
            }
        }
    }

    private configureNestedGrid(handler: Function, cell: HTMLTableCellElement, pk: object) {
        const gridId = `dbnetgrid${new Date().valueOf()}`;
        jQuery(document.createElement("div")).attr("id", gridId).appendTo($(cell));
        const grid = new DbNetGrid(gridId);
        grid.connectionString = this.connectionString;

        const args = [grid, this];
        handler.apply(window, args);
        this.assignForeignKey(grid, pk);
        grid.initialize();
    }

    private assignForeignKey(grid: DbNetGrid, pk: object) {
        const col = grid.columns.find((col) => { return col.foreignKey == true });

        if (col == undefined) {
            alert('No foreign key defined for nested grid')
            return;
        }

        col.foreignKeyValue = pk;
    }
}