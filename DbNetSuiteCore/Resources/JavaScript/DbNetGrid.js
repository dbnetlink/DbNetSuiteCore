"use strict";
var ToolbarButtonStyle;
(function (ToolbarButtonStyle) {
    ToolbarButtonStyle[ToolbarButtonStyle["Image"] = 0] = "Image";
    ToolbarButtonStyle[ToolbarButtonStyle["Text"] = 1] = "Text";
    ToolbarButtonStyle[ToolbarButtonStyle["ImageAndText"] = 2] = "ImageAndText";
    ToolbarButtonStyle[ToolbarButtonStyle["TextAndImage"] = 3] = "TextAndImage";
})(ToolbarButtonStyle || (ToolbarButtonStyle = {}));
var BooleanDisplayMode;
(function (BooleanDisplayMode) {
    BooleanDisplayMode[BooleanDisplayMode["TrueFalse"] = 0] = "TrueFalse";
    BooleanDisplayMode[BooleanDisplayMode["YesNo"] = 1] = "YesNo";
    BooleanDisplayMode[BooleanDisplayMode["Checkbox"] = 2] = "Checkbox";
})(BooleanDisplayMode || (BooleanDisplayMode = {}));
var MultiRowSelectLocation;
(function (MultiRowSelectLocation) {
    MultiRowSelectLocation[MultiRowSelectLocation["Left"] = 0] = "Left";
    MultiRowSelectLocation[MultiRowSelectLocation["Right"] = 1] = "Right";
})(MultiRowSelectLocation || (MultiRowSelectLocation = {}));
var GridGenerationMode;
(function (GridGenerationMode) {
    GridGenerationMode[GridGenerationMode["Display"] = 0] = "Display";
    GridGenerationMode[GridGenerationMode["DataTable"] = 1] = "DataTable";
})(GridGenerationMode || (GridGenerationMode = {}));
class DbNetGrid extends DbNetGridEdit {
    constructor(id) {
        super(id);
        this.autoRowSelect = true;
        this.booleanDisplayMode = BooleanDisplayMode.TrueFalse;
        this.cellIndexCache = {};
        this.columnFilters = {};
        this.copy = true;
        this.currentPage = 1;
        this.defaultColumn = undefined;
        this.dragAndDrop = true;
        this.editControl = undefined;
        this.editDialogId = "";
        this.export_ = true;
        this.frozenHeader = false;
        this.googleChartOptions = undefined;
        this.gridGenerationMode = GridGenerationMode.Display;
        this.groupBy = false;
        this.height = 0;
        this.isBrowseDialog = false;
        this.multiRowSelect = false;
        this.multiRowSelectLocation = MultiRowSelectLocation.Left;
        this.nestedGrid = false;
        this.orderBy = "";
        this.orderByDirection = "asc";
        this.pageSize = 20;
        this.procedureName = "";
        this.procedureParams = {};
        this.rowSelect = true;
        this.totalPages = 0;
        this.totalRows = 0;
        this.update = false;
        this.view = false;
        this.viewLayoutColumns = 1;
        if (this.toolbarPosition === undefined) {
            this.toolbarPosition = "Top";
        }
    }
    initialize() {
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
        this.post("initialize", this.getRequest())
            .then((response) => {
            this.updateColumns(response);
            this.configureGrid(response);
            this.initialised = true;
            this.fireEvent("onInitialized");
        });
    }
    addNestedGrid(handler) {
        this.bind("onNestedClick", handler);
        this.nestedGrid = true;
    }
    addLinkedGrid(grid) {
        this.linkedControls.push(grid);
    }
    columnIndex(columnName) {
        var _a;
        let cellIndex = this.cellIndexCache[columnName];
        if (cellIndex) {
            return cellIndex;
        }
        cellIndex = -1;
        (_a = this.gridPanel) === null || _a === void 0 ? void 0 : _a.find("th[data-columnname]").get().forEach((th) => {
            if ($(th).data("columnname")) {
                if ($(th).data('columnname').toString().toLowerCase() == columnName.toLowerCase()) {
                    cellIndex = th.cellIndex;
                    this.cellIndexCache[columnName] = cellIndex;
                }
            }
        });
        return cellIndex;
    }
    columnCell(columnName, row) {
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
        var _a;
        return (_a = this.gridPanel) === null || _a === void 0 ? void 0 : _a.find("tr.data-row.selected")[0];
    }
    selectedRows() {
        var _a;
        return (_a = this.gridPanel) === null || _a === void 0 ? void 0 : _a.find("tr.data-row.selected");
    }
    columnValue(columnName, row) {
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
    getDataArray(callback) {
        this.post("data-array", this.getRequest())
            .then((response) => {
            callback(response);
        });
    }
    configureToolbar(response) {
        if (response.toolbar) {
            const buttons = ["First", "Next", "Previous", "Last", "Export", "Copy", "View", "Search", "Insert", "Update", "Delete"];
            buttons.forEach(btn => this.addEventListener(`${btn}Btn`));
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
    configureGrid(response) {
        var _a, _b, _c, _d, _e, _f, _g, _h, _j, _k, _l, _m, _o, _p;
        if (this.toolbarPanel) {
            if (response.toolbar) {
                (_a = this.toolbarPanel) === null || _a === void 0 ? void 0 : _a.html(response.toolbar);
            }
            this.configureToolbar(response);
        }
        if (this.gridGenerationMode.toString() == "DataTable") {
            this.configureDataTable(response.data);
            return;
        }
        (_b = this.gridPanel) === null || _b === void 0 ? void 0 : _b.html(response.data);
        (_c = this.gridPanel) === null || _c === void 0 ? void 0 : _c.find("tr.filter-row :input").get().forEach(input => {
            const $input = $(input);
            const width = $input.width();
            if (input.nodeName == "SELECT") {
                $input.width(width + 20);
            }
            else if (width < 100) {
                $input.width(100);
            }
            $input.on("keyup", (event) => this.columnFilterKeyPress(event));
        });
        (_d = this.gridPanel) === null || _d === void 0 ? void 0 : _d.find("tr.data-row").get().forEach((tr) => {
            this.addRowEventHandlers($(tr));
            this.fireEvent("onRowTransform", { row: tr });
        });
        if (this.dragAndDrop && this.procedureName == "" && this.isBrowseDialog == false) {
            (_e = this.gridPanel) === null || _e === void 0 ? void 0 : _e.find("tr.header-row th").draggable({ helper: "clone", cursor: "move" }).on("dragstart", (event, ui) => this.columnDragStarted(event, ui)).on("dragstop", (event, ui) => this.columnDragStopped(event, ui)).on("drag", (event, ui) => this.columnDrag(event, ui));
            (_f = this.gridPanel) === null || _f === void 0 ? void 0 : _f.find("tr.header-row th").droppable().on("dropover", (event, ui) => this.dragDropOver(event, ui)).on("dropout", (event, ui) => this.dragDropOut(event, ui)).on("drop", (event, ui) => this.dragDropped(event, ui));
        }
        if (this.procedureName == "" && this.isBrowseDialog == false) {
            (_g = this.gridPanel) === null || _g === void 0 ? void 0 : _g.find("tr.header-row th").get().forEach(th => {
                $(th).on("click", () => this.handleHeaderClick($(th)));
            });
        }
        (_h = this.gridPanel) === null || _h === void 0 ? void 0 : _h.find("tr.filter-row select").get().forEach(select => {
            $(select).on("change", (event) => this.runColumnFilterSearch());
        });
        // this.configureDataRows(this.gridPanel?.find("tr.data-row") as JQuery<HTMLTableRowElement>);
        (_j = this.gridPanel) === null || _j === void 0 ? void 0 : _j.find("input.multi-select-checkbox").get().forEach(e => {
            $(e).on("click", (e) => this.handleRowSelectClick(e));
        });
        (_k = this.gridPanel) === null || _k === void 0 ? void 0 : _k.find("th[data-columnname]").get().forEach((th) => {
            var _a;
            const columnName = $(th).data("columnname");
            const cellIdx = th.cellIndex;
            (_a = this.gridPanel) === null || _a === void 0 ? void 0 : _a.find("tr.data-row").get().forEach((tr) => {
                const td = tr.cells[cellIdx];
                this.fireEvent("onCellTransform", { cell: td, row: tr, columnName: columnName });
            });
        });
        if (this.autoRowSelect) {
            (_l = this.gridPanel) === null || _l === void 0 ? void 0 : _l.find("tr.data-row").first().trigger("click");
        }
        const rowCount = (_m = this.gridPanel) === null || _m === void 0 ? void 0 : _m.find("tr.data-row").length;
        this.fireEvent("onPageLoaded", { table: this.table(), rowCount: rowCount });
        this.renderChart();
        if (this.frozenHeader) {
            const h = (_o = this.gridPanel) === null || _o === void 0 ? void 0 : _o.find("tr.header-row").height();
            const $filterRow = (_p = this.gridPanel) === null || _p === void 0 ? void 0 : _p.find("tr.filter-row");
            $filterRow.find("th").css("top", h);
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
    configureDataTable(_html) {
        var _a, _b, _c, _d;
        (_a = this.element) === null || _a === void 0 ? void 0 : _a.removeClass("dbnetsuite");
        (_b = this.toolbarPanel) === null || _b === void 0 ? void 0 : _b.addClass("dbnetsuite");
        (_c = this.gridPanel) === null || _c === void 0 ? void 0 : _c.html(_html);
        (_d = this.gridPanel) === null || _d === void 0 ? void 0 : _d.find("table").DataTable();
    }
    renderChart() {
        if (!this.googleChartOptions) {
            return;
        }
        this.getDataArray((dataArray) => this.loadChart(dataArray));
    }
    loadChart(dataArray) {
        google.charts.load('current', { 'packages': ['corechart'] });
        google.charts.setOnLoadCallback(() => this.drawChart(dataArray));
    }
    drawChart(dataArray) {
        var _a, _b;
        const dt = google.visualization.arrayToDataTable(dataArray);
        const chart = new google.visualization[this.googleChartOptions.type](document.getElementById(this.googleChartOptions.panelId));
        const options = {};
        if ((_a = this.googleChartOptions) === null || _a === void 0 ? void 0 : _a.functionName) {
            window[(_b = this.googleChartOptions) === null || _b === void 0 ? void 0 : _b.functionName](this, options);
        }
        chart.draw(dt, options);
    }
    updateColumns(response) {
        var _a;
        this.columns = new Array();
        (_a = response.columns) === null || _a === void 0 ? void 0 : _a.forEach((col) => {
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
            };
            this.columns.push(new GridColumn(properties));
        });
    }
    columnDragStarted(event, ui) {
        const $el = $(event.currentTarget);
        $el.css("opacity", 0.5);
        ui.helper.css("opacity", 0.5);
        ui.helper.attr("dbnetgrid_id", this.id);
        ui.helper.width($el.width() + 2).height($el.height() + 2);
    }
    columnDragStopped(event, ui) {
        const $el = $(event.currentTarget);
        $el.css("opacity", 1);
    }
    columnDrag(event, ui) {
        var _a, _b;
        if (!this.dropTarget || ui.helper.attr("dbnetgrid_id") != this.id) {
            return;
        }
        (_a = this.dropIcon) === null || _a === void 0 ? void 0 : _a.show();
        const width = this.dropTarget.width();
        const pos = this.dropTarget.offset();
        const parentOffset = this.dropTarget.parent().offset();
        const uiLeft = ui.position.left + parentOffset.left;
        let left = (pos.left - 9);
        const top = (pos.top - 14);
        ui.helper.attr("dropside", "left");
        if ((uiLeft + width) > pos.left) {
            ui.helper.attr("dropside", "right");
            left += width + 4;
            console.log;
        }
        (_b = this.dropIcon) === null || _b === void 0 ? void 0 : _b.css({ "left": `${left}px`, "top": `${top}px` });
    }
    dragDropOver(event, ui) {
        if (ui.helper.attr("dbnetgrid_id") != this.id) {
            return;
        }
        this.dropTarget = $(event.currentTarget);
    }
    dragDropOut(event, ui) {
        var _a;
        (_a = this.dropIcon) === null || _a === void 0 ? void 0 : _a.hide();
        this.dropTarget = undefined;
    }
    dragDropped(event, ui) {
        var _a;
        if (ui.helper.attr("dbnetgrid_id") != this.id) {
            return;
        }
        (_a = this.dropIcon) === null || _a === void 0 ? void 0 : _a.hide();
        const cols = [];
        const sourceIdx = parseInt(ui.draggable.data("columnordinal")) - 1;
        const targetIdx = parseInt($(event.currentTarget).data("columnordinal")) - 1;
        const column = this.columns[sourceIdx];
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
    addRowEventHandlers($tr) {
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
    selectRow(tr) {
        tr.parent().find("tr.data-row").removeClass("selected").find("input.multi-select-checkbox").prop('checked', false);
        tr.addClass("selected").find("input.multi-select-checkbox").prop('checked', true);
    }
    handleRowClick(tr) {
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
    handleHeaderClick(th) {
        this.orderBy = th.data('columnordinal');
        const dbDataType = th.data('dbdatatype');
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
    handleRowSelectClick(event) {
        const checkbox = $(event.currentTarget);
        const checked = checkbox.is(':checked');
        let rows;
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
    handleClick(event) {
        const id = event.target.id;
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
    columnFilterKeyPress(event) {
        window.clearTimeout(this.quickSearchTimerId);
        this.quickSearchTimerId = window.setTimeout(() => { this.runColumnFilterSearch(); }, this.quickSearchDelay);
    }
    runColumnFilterSearch() {
        var _a, _b;
        this.columnFilters = {};
        (_a = this.gridPanel) === null || _a === void 0 ? void 0 : _a.find("tr.filter-row input").get().forEach(e => {
            const $input = $(e);
            if ($input.val() != '') {
                this.columnFilters[$input.data('columnname')] = $input.val().toString();
            }
        });
        (_b = this.gridPanel) === null || _b === void 0 ? void 0 : _b.find("tr.filter-row select").get().forEach(e => {
            const $input = $(e);
            if ($input.val() != '') {
                this.columnFilters[$input.data('columnname')] = $input.val().toString();
            }
        });
        if (this.searchDialog) {
            this.searchDialog.clear();
            this.searchParams = [];
        }
        this.currentPage = 1;
        this.getPage();
    }
    clearColumnFilters() {
        var _a, _b;
        this.columnFilters = {};
        (_a = this.gridPanel) === null || _a === void 0 ? void 0 : _a.find("tr.filter-row input").get().forEach(e => {
            const $input = $(e);
            $input.val('');
        });
        (_b = this.gridPanel) === null || _b === void 0 ? void 0 : _b.find("tr.filter-row select").get().forEach(e => {
            const $input = $(e);
            $input.val('');
        });
    }
    getPage(callback) {
        const activeElementId = this.activeElementId();
        this.post("page", this.getRequest())
            .then((response) => {
            if (response.error == false) {
                this.configureGrid(response);
                this.focusActiveElement(activeElementId);
            }
            if (callback) {
                callback(response);
            }
        });
    }
    activeElementId() {
        var _a, _b;
        let activeElementId = undefined;
        if (((_a = document.activeElement) === null || _a === void 0 ? void 0 : _a.nodeName) == "INPUT") {
            activeElementId = (_b = document.activeElement) === null || _b === void 0 ? void 0 : _b.id;
        }
        return activeElementId;
    }
    focusActiveElement(activeElementId) {
        if (activeElementId) {
            const $input = $(`#${activeElementId}`);
            const txt = $input.val();
            $input.trigger("focus").val(txt);
            $input[0].setSelectionRange(txt.length, txt.length);
        }
    }
    download() {
        switch (this.controlElement("ExportSelect").val()) {
            case "html":
                this.htmlExport();
                break;
            case "excel":
                this.downloadSpreadsheet();
                break;
        }
    }
    htmlExport() {
        this.post("html-export", this.getRequest(), true)
            .then((response) => {
            const url = window.URL.createObjectURL(response);
            const tab = window.open();
            tab.location.href = url;
        });
    }
    downloadSpreadsheet() {
        this.post("generate-spreadsheet", this.getRequest(), true)
            .then((response) => {
            const link = document.createElement("a");
            link.href = window.URL.createObjectURL(response);
            link.download = `report_${new Date().getTime()}.xlsx`;
            link.click();
        });
    }
    getViewContent() {
        const $row = this.assignPrimaryKey();
        this.post("view-content", this.getRequest())
            .then((response) => {
            this.configureViewDialog(response, $row);
        });
    }
    configureViewDialog(response, $row) {
        var _a;
        if (!this.viewDialog) {
            (_a = this.element) === null || _a === void 0 ? void 0 : _a.append(response.dialog);
            this.viewDialog = new ViewDialog(`${this.id}_view_dialog`, this);
        }
        this.viewDialog.update(response, $row);
    }
    assignPrimaryKey() {
        const $row = $(this.selectedRow());
        this.primaryKey = $row.data("pk");
        return $row;
    }
    refreshRow() {
        const $row = this.assignPrimaryKey();
        this.post("grid-row", this.getRequest())
            .then((response) => {
            const $table = $(response.html);
            const $newRow = $table.find("tr.data-row");
            $newRow.removeClass("odd").removeClass("even");
            $row.hasClass("odd") ? $newRow.addClass("odd") : $newRow.addClass("even");
            $row.replaceWith($newRow);
            this.addRowEventHandlers($newRow);
            this.fireEvent("onRowTransform", { row: $newRow.get(0) });
            this.handleRowClick($newRow);
        });
    }
    openEditDialog(insert) {
        var _a, _b, _c;
        if (!this.editDialog) {
            this.editDialog = new EditDialog(this.editDialogId);
        }
        (_a = this.editDialog) === null || _a === void 0 ? void 0 : _a.open();
        if (insert) {
            (_b = this.editControl) === null || _b === void 0 ? void 0 : _b.insertRecord();
        }
        else {
            (_c = this.editControl) === null || _c === void 0 ? void 0 : _c.getRecord($(this.selectedRow()).data('pk'));
        }
    }
    updateRow() {
        if (!this.primaryKeyCheck()) {
            return;
        }
        this.openEditDialog(false);
    }
    primaryKeyCheck($row = undefined) {
        if (!$row) {
            $row = $(this.selectedRow());
        }
        if ($row.data('pk') == null) {
            this.error("A primary key has not been included in the grid columns");
            return false;
        }
        return true;
    }
    insertRow() {
        var _a;
        if (this.editDialogId) {
            this.openEditDialog(true);
        }
        else {
            (_a = this.editControl) === null || _a === void 0 ? void 0 : _a.insertRecord();
        }
    }
    deleteRow() {
        if (!this.primaryKeyCheck()) {
            return;
        }
        this.confirm("Please confirm deletion of the selected row", this.gridPanel, (buttonPressed) => this.deletionConfirmed(buttonPressed));
    }
    deletionConfirmed(buttonPressed) {
        if (buttonPressed != MessageBoxButtonType.Confirm) {
            return;
        }
        this.assignPrimaryKey();
        this.post("delete-record", this.getRequest())
            .then((response) => {
            if (response.error == false) {
                this.recordDeleted();
            }
            else {
                this.error(response.message);
            }
        });
    }
    recordDeleted() {
        this.reload();
        this.fireEvent("onRecordDeleted");
    }
    table() {
        var _a;
        return (_a = this.gridPanel) === null || _a === void 0 ? void 0 : _a[0].querySelector('table.dbnetgrid-table');
    }
    copyGrid() {
        var _a, _b, _c, _d;
        const table = this.table();
        (_a = this.gridPanel) === null || _a === void 0 ? void 0 : _a.find("tr.data-row.selected").addClass("unselected").removeClass("selected");
        try {
            const range = document.createRange();
            range.selectNode(table);
            (_b = window.getSelection()) === null || _b === void 0 ? void 0 : _b.addRange(range);
            document.execCommand('copy');
            (_c = window.getSelection()) === null || _c === void 0 ? void 0 : _c.removeRange(range);
        }
        catch (e) {
            try {
                const content = table.innerHTML;
                const blobInput = new Blob([content], { type: 'text/html' });
                const clipboardItemInput = new ClipboardItem({ 'text/html': blobInput });
                navigator.clipboard.write([clipboardItemInput]);
            }
            catch (e) {
                this.error("Copy failed");
                return;
            }
        }
        (_d = this.gridPanel) === null || _d === void 0 ? void 0 : _d.find("tr.data-row.unselected").addClass("selected").removeClass("unselected");
        this.info("Grid copied to clipboard", this.gridPanel);
    }
    getRequest() {
        this.defaultColumn = this.columns.find((col) => { return col.columnExpression == "*"; });
        if (this.defaultColumn) {
            this.columns = this.columns.filter(item => item !== this.defaultColumn);
        }
        const request = this.baseRequest();
        request.pageSize = this.toolbarPosition == "Hidden" ? -1 : this.pageSize;
        request.currentPage = this.currentPage;
        request.orderBy = this.orderBy;
        request.orderByDirection = this.orderByDirection;
        request.columns = this.columns.map((column) => { return column; });
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
    addEventListener(id, eventName = "click") {
        this.controlElement(id).on(eventName, (event) => this.handleClick(event));
    }
    openNestedGrid(event) {
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
        if (row.next().hasClass("nested-grid-row")) {
            row.next().show();
            return;
        }
        const newRow = table[0].insertRow(row[0].rowIndex + 1);
        newRow.className = "nested-grid-row";
        newRow.insertCell(-1);
        const gridCell = newRow.insertCell(-1);
        gridCell.className = "nested-grid-cell";
        gridCell.colSpan = row[0].cells.length - 1;
        const handlers = this.eventHandlers["onNestedClick"];
        for (let i = 0; i < handlers.length; i++) {
            if (handlers[i]) {
                this.configureNestedGrid(handlers[i], gridCell, row.data("id"));
            }
        }
    }
    configureNestedGrid(handler, cell, pk) {
        const gridId = `dbnetgrid${new Date().valueOf()}`;
        jQuery(document.createElement("div")).attr("id", gridId).appendTo($(cell));
        const grid = new DbNetGrid(gridId);
        grid.connectionString = this.connectionString;
        const args = [grid, this];
        handler.apply(window, args);
        this.assignForeignKey(grid, pk);
        grid.initialize();
    }
    configureLinkedControl(control, id, pk, fk) {
        if (control instanceof DbNetGrid) {
            const grid = control;
            this.assignForeignKey(grid, id);
            grid.currentPage = 1;
            grid.initialised ? grid.getPage() : grid.initialize();
        }
        if (control instanceof DbNetEdit) {
            const edit = control;
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
                this.configureEditButtons(edit);
            }
            else {
                this.initialiseEdit(edit, pk);
            }
        }
    }
    initialiseEdit(editControl, pk) {
        if (editControl.parentChildRelationship == "OneToOne") {
            editControl.internalBind("onInitialized", (sender) => this.configureEdit(sender));
            editControl.internalBind("onRecordUpdated", () => this.refreshRow());
            editControl.internalBind("onRecordInserted", () => this.reload());
        }
        editControl.initialize(pk);
    }
    downloadBinaryData(element, image) {
        var _a, _b;
        const $viewContentContainer = $(element).closest(".view-dialog-value");
        const $cell = $(element).closest("td");
        let $row = $(element).closest("tr");
        if ($viewContentContainer.length) {
            this.columnName = $viewContentContainer.data("columnname");
            $row = $(this.selectedRow());
        }
        else {
            this.primaryKey = $row.data("pk");
            this.columnName = (_b = (_a = this.gridPanel) === null || _a === void 0 ? void 0 : _a.find("th[data-columnname]").get($cell.prop("cellIndex"))) === null || _b === void 0 ? void 0 : _b.getAttribute("data-columnname");
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
        const args = {
            row: $row.get(0),
            cell: $cell.get(0),
            fileName: fileName,
            columnName: this.columnName
        };
        this.fireEvent("onConfigureBinaryData", args);
        if (args.fileName != fileName) {
            $(element).data("filename", args.fileName);
            fileName = args.fileName;
        }
        this.post("download-column-data", this.getRequest(), true)
            .then((blob) => {
            if (blob.size) {
                if (image) {
                    const $img = $cell.find("img");
                    $img.attr("src", window.URL.createObjectURL(blob));
                    $img.on("click", (event) => this.viewImage(event));
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
    configureEditButtons(edit) {
        const $row = $(this.selectedRow());
        edit.controlElement("NextBtn").prop("disabled", $row.next('.data-row').length == 0);
        edit.controlElement("PreviousBtn").prop("disabled", $row.prev('.data-row').length == 0);
    }
    configureEdit(sender) {
        sender.controlElement("NextBtn").off().on("click", () => this.nextRecord());
        sender.controlElement("PreviousBtn").off().on("click", () => this.previousRecord());
        if (this.editDialogId) {
            sender.controlElement("CancelBtn").off().on("click", () => { var _a; return (_a = this.editDialog) === null || _a === void 0 ? void 0 : _a.close(); });
        }
        this.configureEditButtons(sender);
    }
    viewElement(columnName) {
        var _a;
        return (_a = this.viewDialog) === null || _a === void 0 ? void 0 : _a.viewElement(columnName);
    }
    nextRecord() {
        $(this.selectedRow()).next().trigger("click");
    }
    previousRecord() {
        $(this.selectedRow()).prev().trigger("click");
    }
}
