"use strict";
var ToolbarButtonStyle;
(function (ToolbarButtonStyle) {
    ToolbarButtonStyle[ToolbarButtonStyle["Image"] = 0] = "Image";
    ToolbarButtonStyle[ToolbarButtonStyle["Text"] = 1] = "Text";
    ToolbarButtonStyle[ToolbarButtonStyle["ImageAndText"] = 2] = "ImageAndText";
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
        this.columnName = undefined;
        this.columnFilters = {};
        this.copy = true;
        this.currentPage = 1;
        this.defaultColumn = undefined;
        this.delete = false;
        this.dragAndDrop = true;
        this.editDialogId = "";
        this.export_ = true;
        this.fixedFilterParams = {};
        this.fixedFilterSql = "";
        this.frozenHeader = false;
        this.googleChartOptions = undefined;
        this.gridGenerationMode = GridGenerationMode.Display;
        this.groupBy = false;
        this.insert = false;
        this.multiRowSelect = false;
        this.multiRowSelectLocation = MultiRowSelectLocation.Left;
        this.nestedGrid = false;
        this.optimizeForLargeDataset = false;
        this.orderBy = "";
        this.orderByDirection = "asc";
        this.pageSize = 20;
        this.primaryKey = undefined;
        this.procedureName = "";
        this.procedureParams = {};
        this.rowSelect = true;
        this.totalPages = 0;
        this.update = false;
        this.view = false;
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
        })
            .catch(() => { });
        this.initialised = true;
        this.linkedControls.forEach((control) => {
            if (control.isEditDialog) {
                this.editDialogControl = control;
                ;
            }
        });
        this.fireEvent("onInitialized");
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
            const buttons = ["First", "Next", "Previous", "Last", "Download", "Copy", "View", "Search", "Insert", "Update", "Delete"];
            buttons.forEach(btn => this.addEventListener(`${btn}Btn`));
        }
        const $navigationElements = this.controlElement("dbnetgrid-toolbar").find(".navigation");
        const $noRecordsCell = this.controlElement("no-records-cell");
        this.setInputElement("Rows", response.totalRows);
        if (response.totalRows == 0) {
            $navigationElements.hide();
            $noRecordsCell.show();
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
        this.disable("ViewBtn", response.totalRows == 0);
        this.disable("DownloadBtn", response.totalRows == 0);
        this.disable("CopyBtn", response.totalRows == 0);
        this.controlElement("QuickSearch").on("keyup", (event) => this.quickSearchKeyPress(event));
    }
    configureGrid(response) {
        var _a, _b, _c, _d, _e, _f, _g, _h, _j, _k, _l, _m, _o, _p, _q, _r, _s, _t;
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
        if (this.dragAndDrop && this.procedureName == "") {
            (_e = this.gridPanel) === null || _e === void 0 ? void 0 : _e.find("tr.header-row th").draggable({ helper: "clone", cursor: "move" }).on("dragstart", (event, ui) => this.columnDragStarted(event, ui)).on("dragstop", (event, ui) => this.columnDragStopped(event, ui)).on("drag", (event, ui) => this.columnDrag(event, ui));
            (_f = this.gridPanel) === null || _f === void 0 ? void 0 : _f.find("tr.header-row th").droppable().on("dropover", (event, ui) => this.dragDropOver(event, ui)).on("dropout", (event, ui) => this.dragDropOut(event, ui)).on("drop", (event, ui) => this.dragDropped(event, ui));
        }
        if (this.procedureName == "") {
            (_g = this.gridPanel) === null || _g === void 0 ? void 0 : _g.find("tr.header-row th").get().forEach(th => {
                $(th).on("click", () => this.handleHeaderClick($(th)));
            });
        }
        (_h = this.gridPanel) === null || _h === void 0 ? void 0 : _h.find("tr.filter-row select").get().forEach(select => {
            $(select).on("change", (event) => this.runColumnFilterSearch());
        });
        (_j = this.gridPanel) === null || _j === void 0 ? void 0 : _j.find("input.multi-select-checkbox").get().forEach(e => {
            $(e).on("click", (e) => this.handleRowSelectClick(e));
        });
        (_k = this.gridPanel) === null || _k === void 0 ? void 0 : _k.find("button.nested-grid-button").get().forEach(e => {
            $(e).on("click", (e) => this.openNestedGrid(e));
        });
        (_l = this.gridPanel) === null || _l === void 0 ? void 0 : _l.find("button.download").get().forEach(e => {
            $(e).on("click", (e) => this.downloadCellData(e.currentTarget, false));
        });
        (_m = this.gridPanel) === null || _m === void 0 ? void 0 : _m.find("img.image").get().forEach(e => {
            this.downloadCellData(e.parentElement, true);
        });
        (_o = this.gridPanel) === null || _o === void 0 ? void 0 : _o.find("th[data-columnname]").get().forEach((th) => {
            var _a;
            const columnName = $(th).data("columnname");
            const cellIdx = th.cellIndex;
            (_a = this.gridPanel) === null || _a === void 0 ? void 0 : _a.find("tr.data-row").get().forEach((tr) => {
                const td = tr.cells[cellIdx];
                this.fireEvent("onCellTransform", { cell: td, row: tr, columnName: columnName });
            });
        });
        if (this.autoRowSelect) {
            (_p = this.gridPanel) === null || _p === void 0 ? void 0 : _p.find("tr.data-row").first().trigger("click");
        }
        const rowCount = (_q = this.gridPanel) === null || _q === void 0 ? void 0 : _q.find("tr.data-row").length;
        this.fireEvent("onPageLoaded", { table: (_r = this.gridPanel) === null || _r === void 0 ? void 0 : _r.find("table.dbnetgrid-table")[0], rowCount: rowCount });
        this.renderChart();
        if (this.frozenHeader) {
            const h = (_s = this.gridPanel) === null || _s === void 0 ? void 0 : _s.find("tr.header-row").height();
            const $filterRow = (_t = this.gridPanel) === null || _t === void 0 ? void 0 : _t.find("tr.filter-row");
            $filterRow.find("th").css("top", h);
        }
        if (rowCount === 0) {
            this.configureLinkedControls(null);
        }
    }
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
                dataOnly: col.dataOnly
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
    addRowEventHandlers(tr) {
        tr.on("mouseover", () => tr.addClass("highlight"));
        tr.on("mouseout", () => tr.removeClass("highlight"));
        tr.on("click", () => this.handleRowClick(tr));
    }
    handleRowClick(tr) {
        if (this.rowSelect) {
            tr.parent().find("tr.data-row").removeClass("selected").find("input.multi-select-checkbox").prop('checked', false);
            tr.addClass("selected").find("input.multi-select-checkbox").prop('checked', true);
        }
        this.configureLinkedControls(tr.data("id"));
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
            case this.controlElementId("DownloadBtn"):
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
        switch (this.controlElement("DownloadSelect").val()) {
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
        const $row = $(this.selectedRow());
        this.primaryKey = $row.data("id");
        this.post("view-content", this.getRequest())
            .then((response) => {
            var _a;
            if (!this.viewDialog) {
                (_a = this.element) === null || _a === void 0 ? void 0 : _a.append(response.toolbar);
                this.viewDialog = new ViewDialog(`${this.id}_view_dialog`, this);
            }
            this.viewDialog.update(response, $row);
        });
    }
    updateRow() {
        var _a, _b;
        if (!this.editDialog) {
            this.editDialog = new EditDialog(this.editDialogId, this);
            (_a = this.editDialogControl) === null || _a === void 0 ? void 0 : _a.initialize();
        }
        (_b = this.editDialogControl) === null || _b === void 0 ? void 0 : _b.getRecord($(this.selectedRow()).data('pk'));
        this.editDialog.update();
    }
    insertRow() {
        return;
    }
    deleteRow() {
        return;
    }
    copyGrid() {
        var _a, _b, _c, _d, _e;
        const table = (_a = this.gridPanel) === null || _a === void 0 ? void 0 : _a[0].querySelector('table.dbnetgrid-table');
        (_b = this.gridPanel) === null || _b === void 0 ? void 0 : _b.find("tr.data-row.selected").addClass("unselected").removeClass("selected");
        try {
            const range = document.createRange();
            range.selectNode(table);
            (_c = window.getSelection()) === null || _c === void 0 ? void 0 : _c.addRange(range);
            document.execCommand('copy');
            (_d = window.getSelection()) === null || _d === void 0 ? void 0 : _d.removeRange(range);
        }
        catch (e) {
            try {
                const content = table.innerHTML;
                const blobInput = new Blob([content], { type: 'text/html' });
                const clipboardItemInput = new ClipboardItem({ 'text/html': blobInput });
                navigator.clipboard.write([clipboardItemInput]);
            }
            catch (e) {
                alert("Copy failed");
                return;
            }
        }
        (_e = this.gridPanel) === null || _e === void 0 ? void 0 : _e.find("tr.data-row.unselected").addClass("selected").removeClass("unselected");
        this.message("Copied");
    }
    message(text) {
        const dialogId = `${this.id}_message_dialog`;
        const $dialog = $(`#${dialogId}`);
        $dialog.text(text);
        $dialog.attr("title", "Copy");
        $dialog.dialog({
            autoOpen: false
        });
        $dialog.dialog("open");
    }
    getRequest() {
        this.defaultColumn = this.columns.find((col) => { return col.columnExpression == "*"; });
        if (this.defaultColumn) {
            this.columns = this.columns.filter(item => item !== this.defaultColumn);
        }
        const request = {
            componentId: this.id,
            connectionString: this.connectionString,
            fromPart: this.fromPart,
            currentPage: this.currentPage,
            pageSize: this.toolbarPosition == "Hidden" ? -1 : this.pageSize,
            orderBy: this.orderBy,
            orderByDirection: this.orderByDirection,
            toolbarButtonStyle: this.toolbarButtonStyle,
            columns: this.columns.map((column) => { return column; }),
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
            searchFilterJoin: this.searchFilterJoin,
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
            insert: this.insert,
            update: this.update,
            delete: this.delete
        };
        return request;
    }
    addEventListener(id, eventName = "click") {
        this.controlElement(id).on(eventName, (event) => this.handleClick(event));
    }
    downloadCellData(element, image) {
        var _a, _b;
        const $viewContentRow = $(element).closest("tr.view-content-row");
        const $button = $(element);
        const $cell = $button.closest("td");
        const $row = $button.closest("tr");
        if ($viewContentRow.length) {
            this.columnName = $viewContentRow.data("columnname");
        }
        else {
            this.primaryKey = $row.data("id");
            this.columnName = (_b = (_a = this.gridPanel) === null || _a === void 0 ? void 0 : _a.find("th[data-columnname]").get($cell.prop("cellIndex"))) === null || _b === void 0 ? void 0 : _b.getAttribute("data-columnname");
        }
        const args = {
            row: $row.get(0),
            cell: $cell.get(0),
            extension: "xlxs",
            fileName: `${this.columnName}_${this.primaryKey}.xlsx`,
            columnName: this.columnName
        };
        if (image) {
            args.image = $cell.find("img").get(0);
        }
        this.fireEvent("onCellDataDownload", args);
        this.post("download-column-data", this.getRequest(), true)
            .then((blob) => {
            if (image) {
                const img = $cell.find("img");
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
    openNestedGrid(event) {
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
        const handlers = this.eventHandlers["onNestedClick"];
        for (let i = 0; i < handlers.length; i++) {
            if (handlers[i]) {
                this.configureNestedGrid(handlers[i], cell, row.data("id"));
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
    configureLinkedGrid(control, fk) {
        const grid = control;
        this.assignForeignKey(grid, fk);
        if (grid.connectionString == "") {
            grid.connectionString = this.connectionString;
        }
        grid.currentPage = 1;
        grid.initialised ? grid.getPage() : grid.initialize();
    }
    assignForeignKey(grid, fk) {
        const col = grid.columns.find((col) => { return col.foreignKey == true; });
        if (col == undefined) {
            alert('No foreign key defined for nested grid');
            return;
        }
        col.foreignKeyValue = fk ? fk : DbNetSuite.DBNull;
    }
}
