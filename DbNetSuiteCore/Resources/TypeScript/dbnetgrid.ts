type DbConnectionType = "Sqlite" | "SqlServer"
type ColumnPropertyType = "format" | "lookup" | "style" | "foreignKey" | "filter" | "filterMode" | "download" | "image"
type ToolbarPosition = "Top" | "Bottom" | "Hidden" | undefined

enum ToolbarButtonStyle {
    Image,
    Text,
    ImageAndText,
    TextAndImage
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

type DbNetGridResponseCallback = (response: DbNetGridResponse) => void;

class DbNetGrid extends DbNetGridEdit {
    autoRowSelect = true;
    booleanDisplayMode: BooleanDisplayMode = BooleanDisplayMode.TrueFalse;
    cellIndexCache: Dictionary<number> = {};
    columnFilters: Dictionary<string> = {};
    copy = true;
    currentPage = 1;
    defaultColumn: GridColumn | undefined = undefined;
    dragAndDrop = true;
    dropIcon: JQuery<HTMLElement> | undefined;
    dropTarget: JQuery<HTMLElement> | undefined;
    editDialog: EditDialog | undefined;
    editControl: DbNetEdit | undefined = undefined;
    editDialogId = "";
    export_ = true;
    frozenHeader = false;
    googleChartOptions: GoogleChartOptions | undefined = undefined;
    gridGenerationMode: GridGenerationMode = GridGenerationMode.Display;
    gridPanel: JQuery<HTMLElement> | undefined;
    groupBy = false;
    height = 0;
    isBrowseDialog = false;
    multiRowSelect = false;
    multiRowSelectLocation: MultiRowSelectLocation = MultiRowSelectLocation.Left;
    nestedGrid = false;
    orderBy = "";
    orderByDirection = "asc";
    pageSize = 20;
    procedureName = "";
    procedureParams: Dictionary<object> = {};
    rowSelect = true;
    totalPages = 0;
    totalRows = 0;
    update = false;
    view = false;
    viewDialog: ViewDialog | undefined;
    viewLayoutColumns = 1;
    constructor(id: string) {
        super(id);
        if (this.toolbarPosition === undefined) {
            this.toolbarPosition = "Top";
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

        if (this.height) {
            this.gridPanel.css("max-height", this.height).css("overflow", "auto");
        }
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
                this.initialised = true;
                this.fireEvent("onInitialized");
            });
    }

    addNestedGrid(handler: EventHandler) {
        this.bind("onNestedClick", handler);
        this.nestedGrid = true;
    }

    addLinkedGrid(grid: DbNetGrid) {
        this.linkedControls.push(grid);
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

    private configureToolbar(response: DbNetGridResponse) {
        if (response.toolbar) {
            const buttons = ["First", "Next", "Previous", "Last", "Export", "Copy", "View", "Search", "Insert", "Update", "Delete"];
            buttons.forEach(btn =>
                this.addEventListener(`${btn}Btn`)
            )
        }

        const $navigationElements = this.controlElement("dbnetgrid-toolbar").find(".navigation");
        const $noRecordsCell = this.controlElement("no-records-cell");
        this.setInputElement("Rows", response.totalRows);
        this.totalRows = response.totalRows;
        if (response.totalRows == 0) {
            $navigationElements.hide();
            $noRecordsCell.show();
            this.configureLinkedControls(null);
        }
        else {
            $navigationElements.show();
            $noRecordsCell.hide();
            this.controlElement("dbnetgrid-toolbar").find(".navigation").show();
            this.setInputElement("PageNumber", response.currentPage);
            this.setInputElement("PageCount", response.totalPages);

            this.currentPage = response.currentPage;
            this.totalPages = response.totalPages;

            this.disable("FirstBtn", response.currentPage == 1);
            this.disable("PreviousBtn", response.currentPage == 1);
            this.disable("NextBtn", response.currentPage == response.totalPages);
            this.disable("LastBtn", response.currentPage == response.totalPages);
        }
        this.controlElement("QuickSearch").on("keyup", (event) => this.quickSearchKeyPress(event));

        this.disable("ViewBtn", response.totalRows == 0);
        this.disable("ExportBtn", response.totalRows == 0);
        this.disable("CopyBtn", response.totalRows == 0);
        this.disable("UpdateBtn", response.totalRows == 0);

        if (this.linkedGridOrEdit() == false) {
            this.disable("DeleteBtn", response.totalRows == 0);
        }

        if (this.parentGridOrEdit()) {
            this.configureParentDeleteButton(response.totalRows > 0);
        }
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

        if (this.dragAndDrop && this.procedureName == "" && this.isBrowseDialog == false) {
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

        if (this.procedureName == "" && this.isBrowseDialog == false) {
            this.gridPanel?.find("tr.header-row th").get().forEach(th => {
                $(th).on("click", () => this.handleHeaderClick($(th)));
            });
        }
        this.gridPanel?.find("tr.filter-row select").get().forEach(select => {
            $(select).on("change", (event) => this.runColumnFilterSearch());
        });

        // this.configureDataRows(this.gridPanel?.find("tr.data-row") as JQuery<HTMLTableRowElement>);


        this.gridPanel?.find("input.multi-select-checkbox").get().forEach(e => {
            $(e).on("click", (e) => this.handleRowSelectClick(e));
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

        const rowCount = this.gridPanel?.find("tr.data-row").length;

        this.fireEvent("onPageLoaded", { table: this.table(), rowCount: rowCount });
        this.renderChart();

        if (this.frozenHeader) {
            const h: number = this.gridPanel?.find("tr.header-row").height() as number;
            const $filterRow = this.gridPanel?.find("tr.filter-row") as JQuery<HTMLTableRowElement>;
            $filterRow.find("th").css("top", h)
        }

        if (rowCount === 0 && this.initialised) {
            this.configureLinkedControls(null);
        }
    }

    /*
    private configureDataRows($rows: JQuery<HTMLTableRowElement>) {
        $rows.find("button.nested-grid-button").get().forEach(e => {
            $(e).on("click", (e) => this.openNestedGrid(e));
        });

        $rows.find("button.download").get().forEach(e => {
            $(e).on("click", (e) => this.downloadBinaryData(e.currentTarget, false));
        });

        $rows.find("img.image").get().forEach(e => {
            this.downloadBinaryData(e.parentElement!, true);
        });
    }
    */

    private configureDataTable(_html: string) {
        this.element?.removeClass("dbnetsuite");
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

    private loadChart(dataArray: any) {
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
        response.columns?.forEach((col) => {
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
                dataOnly: col.dataOnly,
                search: col.search,
                uploadMetaData: col.uploadMetaData,
                uploadMetaDataColumn: col.uploadMetaDataColumn
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

        if ((uiLeft + width) > pos.left) {
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
        const sourceIdx = parseInt(ui.draggable.data("columnordinal")) - 1;
        const targetIdx = parseInt($(event.currentTarget).data("columnordinal")) - 1;
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

    private addRowEventHandlers($tr: JQuery<HTMLElement>) {
        $tr.on("mouseover", (e) => $(e.currentTarget).addClass("highlight"));
        $tr.on("mouseout", (e) => $(e.currentTarget).removeClass("highlight"));
        $tr.on("click", (e) => this.handleRowClick($(e.currentTarget)));

        $tr.find("button.nested-grid-button").get().forEach(e => {
            $(e).on("click", (e) => this.openNestedGrid(e));
        });

        $tr.find("button.download, a.download").get().forEach(e => {
            $(e).on("click", (e) => this.downloadBinaryData(e.currentTarget, false));
        });

        $tr.find("img.image").get().forEach(e => {
            this.downloadBinaryData(e, true);
        });
    }

    public selectRow(tr: JQuery<HTMLElement>) {
        tr.parent().find("tr.data-row").removeClass("selected").find("input.multi-select-checkbox").prop('checked', false);
        tr.addClass("selected").find("input.multi-select-checkbox").prop('checked', true);
    }

    private handleRowClick(tr: JQuery<HTMLElement>) {
        if (this.rowSelect) {
            this.selectRow(tr);
        }

        if (this.linkedControls.length) {
            const pk = tr.data("pk");
            const fk = tr.data("fk");

            if (pk == null) {
                if (this.linkedControls.length > 1 || this.editDialogId == '') {
                    this.error("A primary key column must be specfied when linking child controls");
                }
                return;
            }
            else {
                this.configureLinkedControls(tr.data("id"), pk, fk ? fk : pk);
            }
        }

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
            case this.controlElementId("FirstBtn"):
                this.currentPage = 1;
                break;
            case this.controlElementId("NextBtn"):
                this.currentPage++;
                break;
            case this.controlElementId("PreviousBtn"):
                this.currentPage--;
                break;
            case this.controlElementId("LastBtn"):
                this.currentPage = this.totalPages;
                break;
        }

        event.preventDefault();

        switch (id) {
            case this.controlElementId("ExportBtn"):
                this.download();
                break;
            case this.controlElementId("CopyBtn"):
                this.copyGrid();
                break;
            case this.controlElementId("ViewBtn"):
                this.getViewContent();
                break;
            case this.controlElementId("SearchBtn"):
                this.openSearchDialog(this.getRequest());
                break;
            case this.controlElementId("UpdateBtn"):
                this.updateRow();
                break;
            case this.controlElementId("InsertBtn"):
                this.insertRow();
                break;
            case this.controlElementId("DeleteBtn"):
                this.deleteRow();
                break;
            default:
                this.getPage();
                break;
        }
    }

    private columnFilterKeyPress(event: JQuery.TriggeredEvent): void {
        window.clearTimeout(this.quickSearchTimerId);
        this.quickSearchTimerId = window.setTimeout(() => { this.runColumnFilterSearch() }, this.quickSearchDelay);
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

    public getPage(callback?: DbNetGridResponseCallback) {
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
        switch (this.controlElement("ExportSelect").val()) {
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
        const $row = this.assignPrimaryKey();
        this.post<DbNetGridResponse>("view-content", this.getRequest())
            .then((response) => {
                this.configureViewDialog(response, $row);
            });
    }

    private configureViewDialog(response: DbNetGridResponse, $row: JQuery<HTMLTableRowElement>) {
        if (!this.viewDialog) {
            this.element?.append(response.dialog);
            this.viewDialog = new ViewDialog(`${this.id}_view_dialog`, this);
        }
        this.viewDialog.update(response, $row)
    }

    private assignPrimaryKey(): JQuery<HTMLTableRowElement> {
        const $row = $(this.selectedRow());
        this.primaryKey = $row.data("pk");
        return $row;
    }

    private refreshRow() {
        const $row = this.assignPrimaryKey();

        this.post<DbNetGridResponse>("grid-row", this.getRequest())
            .then((response) => {
                const $table = $(response.html);
                const $newRow = $table.find("tr.data-row")
                $newRow.removeClass("odd").removeClass("even");
                $row.hasClass("odd") ? $newRow.addClass("odd") : $newRow.addClass("even");
                $row.replaceWith($newRow);
                this.addRowEventHandlers($newRow);
                this.fireEvent("onRowTransform", { row: $newRow.get(0) });
                this.handleRowClick($newRow);
            });
    }

    private openEditDialog(insert: boolean) {
        if (!this.editDialog) {
            this.editDialog = new EditDialog(this.editDialogId as string);
        }
        this.editDialog?.open();
        if (insert) {
            this.editControl?.insertRecord();
        }
        else {
            this.editControl?.getRecord($(this.selectedRow()).data('pk') as string);
        }
    }

    private updateRow() {
        if (!this.primaryKeyCheck()) {
            return
        }
        this.openEditDialog(false);
    }

    private primaryKeyCheck($row: JQuery<HTMLTableRowElement> | undefined = undefined) {
        if (!$row) {
            $row = $(this.selectedRow());
        }
        if ($row.data('pk') == null) {
            this.error("A primary key has not been included in the grid columns");
            return false;
        }
        return true;
    }

    private insertRow() {
        if (this.editDialogId) {
            this.openEditDialog(true);
        }
        else {
            this.editControl?.insertRecord();
        }
    }

    public deleteRow() {
        if (!this.primaryKeyCheck()) {
            return
        }

        this.confirm("Please confirm deletion of the selected row", this.gridPanel as JQuery<HTMLElement>, (buttonPressed: MessageBoxButtonType) => this.deletionConfirmed(buttonPressed));
    }

    public deletionConfirmed(buttonPressed: MessageBoxButtonType) {
        if (buttonPressed != MessageBoxButtonType.Confirm) {
            return;
        }

        this.assignPrimaryKey();

        this.post<DbNetSuiteResponse>("delete-record", this.getRequest())
            .then((response) => {
                if (response.error == false) {
                    this.recordDeleted();
                }
                else {
                    this.error(response.message);
                }
            })
    }

    private recordDeleted(): void {
        this.reload();
        this.fireEvent("onRecordDeleted");
    }

    public table(): HTMLTableElement {
        return this.gridPanel?.[0].querySelector('table.dbnetgrid-table') as HTMLTableElement;
    }

    private copyGrid() {
        const table = this.table();

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
                this.error("Copy failed")
                return
            }
        }
        this.gridPanel?.find("tr.data-row.unselected").addClass("selected").removeClass("unselected")
        this.info("Grid copied to clipboard", this.gridPanel as JQuery<HTMLElement>)
    }

    public getRequest(): DbNetGridRequest {
        this.defaultColumn = this.columns.find((col) => { return col.columnExpression == "*" });
        if (this.defaultColumn) {
            this.columns = this.columns.filter(item => item !== this.defaultColumn)
        }

        const request = this.baseRequest() as DbNetGridRequest;
        request.pageSize = this.toolbarPosition == "Hidden" ? -1 : this.pageSize;
        request.currentPage = this.currentPage;
        request.orderBy = this.orderBy;
        request.orderByDirection = this.orderByDirection;
        request.columns = this.columns.map((column) => { return column as GridColumnRequest });
        request.multiRowSelect = this.multiRowSelect;
        request.multiRowSelectLocation = this.multiRowSelectLocation;
        request.nestedGrid = this.nestedGrid;
        request.booleanDisplayMode = this.booleanDisplayMode;
        request.columnFilters = this.columnFilters;
        request.frozenHeader = this.frozenHeader;
        request.defaultColumn = this.defaultColumn;
        request.view = this.view;
        request.groupBy = this.groupBy;
        request.copy = this.copy;
        request.export = this.export_;
        request.procedureParams = this.procedureParams;
        request.procedureName = this.procedureName;
        request.gridGenerationMode = this.gridGenerationMode;
        request.update = this.update;
        request.viewLayoutColumns = this.viewLayoutColumns;

        return request;
    }

    private addEventListener(id: string, eventName = "click") {
        this.controlElement(id).on(eventName, (event) => this.handleClick(event));
    }

    private openNestedGrid(event: JQuery.ClickEvent<HTMLElement>) {
        const $button = $(event.currentTarget);
        const row = $button.closest("tr");

        if ($button.hasClass("open")) {
            $button.removeClass("open");
            row.next().hide();
            return;
        }

        if (!this.primaryKeyCheck(row)) {
            return;
        }

        const table = $button.closest("table");

        $button.addClass("open");

        if (row.next().hasClass("nested-component-row")) {
            row.next().show();
            return;
        }

        const newRow = table[0].insertRow(row[0].rowIndex + 1);
        newRow.className = "nested-component-row";
        newRow.insertCell(-1);
        const gridCell = newRow.insertCell(-1);
        gridCell.className = "nested-component-cell";
        gridCell.colSpan = row[0].cells.length - 1;

        const handlers = this.eventHandlers["onNestedClick"]
        for (let i = 0; i < handlers.length; i++) {
            if (handlers[i]) {
                this.configureNestedGrid(handlers[i], gridCell, row.data("id"))
            }
        }
    }

    private configureNestedGrid(handler: EventHandler, cell: HTMLTableCellElement, pk: object) {
        const gridId = `dbnetgrid${new Date().valueOf()}`;
        jQuery(document.createElement("div")).attr("id", gridId).appendTo($(cell));
        const grid = new DbNetGrid(gridId);
        grid.connectionString = this.connectionString;

        const args = [grid, this];
        handler.apply(window, args);
        this.assignForeignKey(grid, pk);
        grid.initialize();
    }

    public configureLinkedControl(control: DbNetSuite, id: object | null, pk: string | null, fk: object | null) {
        if (control instanceof DbNetGrid) {
            const grid = control as DbNetGrid;
            this.assignForeignKey(grid, id);
            grid.currentPage = 1;
            grid.initialised ? grid.getPage() : grid.initialize();
        }

        if (control instanceof DbNetEdit) {
            const edit = control as DbNetEdit;
            edit.currentRow = 1;
            this.assignForeignKey(edit, pk);
            if (this.editControl == undefined) {
                this.editControl = edit;
            }
            if (id == null) {
                if (edit.initialised) {
                    edit.disableForm();
                    if (this.editDialog) {
                        this.editDialog.close();
                    }
                }
                else {
                    this.initialiseEdit(edit, null);
                }
                return;
            }

            if (edit.initialised) {
                if (edit.isEditDialog) {
                    if (!this.editDialog || this.editDialog.isOpen() === false) {
                        return;
                    }
                }
                edit.getRecord(pk);
                this.configureEditButtons(edit)
            }
            else {
                this.initialiseEdit(edit, pk);
            }
        }
    }

    private initialiseEdit(editControl: DbNetEdit, pk: string | null) {
        if (editControl.parentChildRelationship == "OneToOne") {
            editControl.internalBind("onInitialized", (sender: DbNetSuite) => this.configureEdit(sender as DbNetEdit));
            editControl.internalBind("onRecordUpdated", () => this.refreshRow());
            editControl.internalBind("onRecordInserted", () => this.reload());
        }
        editControl.initialize(pk);
    }

    public downloadBinaryData(element: HTMLElement, image: boolean) {
        const $viewContentContainer = $(element).closest(".view-dialog-value");
        const $cell = $(element).closest("td");
        let $row = $(element).closest("tr");

        if ($viewContentContainer.length) {
            this.columnName = $viewContentContainer.data("columnname") as string;
            $row = $(this.selectedRow());
        }
        else {
            this.primaryKey = $row.data("pk")
            this.columnName = this.gridPanel?.find("th[data-columnname]").get($cell.prop("cellIndex"))?.getAttribute("data-columnname") as string;
        }

        let fileName = '';

        if (image) {
            const $image = $(element);
            fileName = $image.data("filename").split('|')[0];
        }
        else {
            const $button = $(element);

            if ($button[0].tagName == 'a') {
                fileName = $button.text();
            }
            else {
                fileName = $button.data("filename").split('|')[0];
            }
        }

        const args: CellDataDownloadArgs = {
            row: $row.get(0) as HTMLTableRowElement,
            cell: $cell.get(0) as HTMLTableCellElement,
            fileName: fileName,
            columnName: this.columnName
        }

        this.fireEvent("onConfigureBinaryData", args);

        if (args.fileName != fileName) {
            $(element).data("filename", args.fileName);
            fileName = args.fileName;
        }

        this.post<Blob>("download-column-data", this.getRequest(), true)
            .then((blob) => {
                if (blob.size) {
                    if (image) {
                        const $img = $cell.find("img") as JQuery<HTMLImageElement>;
                        $img.attr("src", window.URL.createObjectURL(blob));
                        $img.on("click", (event) => this.viewImage(event))
                    }
                    else {
                        const link = document.createElement("a");
                        link.href = window.URL.createObjectURL(blob);
                        link.download = fileName;
                        link.click();
                    }
                }
            });
    }

    public configureEditButtons(edit: DbNetEdit) {
        const $row = $(this.selectedRow());
        edit.controlElement("NextBtn").prop("disabled", $row.next('.data-row').length == 0);
        edit.controlElement("PreviousBtn").prop("disabled", $row.prev('.data-row').length == 0);
    }

    public configureEdit(sender: DbNetEdit) {
        sender.controlElement("NextBtn").off().on("click", () => this.nextRecord());
        sender.controlElement("PreviousBtn").off().on("click", () => this.previousRecord());
        if (this.editDialogId) {
            sender.controlElement("CancelBtn").off().on("click", () => this.editDialog?.close());
        }
        this.configureEditButtons(sender);
    }

    public viewElement(columnName: string): HTMLElement | undefined {
        return this.viewDialog?.viewElement(columnName);
    }

    private nextRecord() {
        $(this.selectedRow()).next().trigger("click")
    }

    private previousRecord() {
        $(this.selectedRow()).prev().trigger("click")
    }

}