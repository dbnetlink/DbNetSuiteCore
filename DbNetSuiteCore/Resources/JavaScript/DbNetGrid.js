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
class DbNetGrid extends DbNetSuite {
    constructor(id) {
        super();
        this.autoRowSelect = true;
        this.booleanDisplayMode = BooleanDisplayMode.TrueFalse;
        this.cellIndexCache = {};
        this.columnName = undefined;
        this.columnFilters = {};
        this.connectionType = "SqlServer";
        this.connectionString = "";
        this.copy = true;
        this.culture = "";
        this.currentPage = 1;
        this.defaultColumn = undefined;
        this.dragAndDrop = true;
        this.export_ = true;
        this.fixedFilterParams = {};
        this.fixedFilterSql = "";
        this.fromPart = "";
        this.frozenHeader = false;
        this.googleChartOptions = undefined;
        this.groupBy = false;
        this.id = "";
        this.initialised = false;
        this.linkedGrids = [];
        this.multiRowSelect = false;
        this.multiRowSelectLocation = MultiRowSelectLocation.Left;
        this.navigation = true;
        this.nestedGrid = false;
        this.optimizeForLargeDataset = false;
        this.orderBy = "";
        this.orderByDirection = "asc";
        this.pageSize = 20;
        this.primaryKey = undefined;
        this.quickSearch = false;
        this.quickSearchDelay = 1000;
        this.quickSearchMinChars = 3;
        this.quickSearchToken = "";
        this.rowSelect = true;
        this.search = true;
        this.searchFilterJoin = "";
        this.searchParams = [];
        this.toolbarButtonStyle = ToolbarButtonStyle.Image;
        this.toolbarPosition = "Top";
        this.totalPages = 0;
        this.view = false;
        this.id = id;
        this.columns = new Array();
        this.element = $(`#${this.id}`);
        this.element.addClass("dbnetsuite").addClass("cleanslate");
        if (this.element.length == 0) {
            alert(`DbNetGrid containing element '${this.id}' not found`);
            return;
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
        this.loadingPanel = this.addPanel("loading");
        this.addPanel("loadingIcon", this.loadingPanel);
        this.loadingPanel.addClass("dbnetgrid-loading");
        this.loadingPanel.children().first().addClass("icon");
        this.dropIcon = this.addPanel("dropIcon", $("body"));
        this.dropIcon.addClass("drop-icon");
        this.post("initialize", this.getRequest())
            .then((response) => {
            this.updateColumns(response);
            this.configureGrid(response);
        })
            .catch(error => {
        });
        this.initialised = true;
        this.fireEvent("onInitialized");
    }
    setColumnExpressions(...columnExpressions) {
        columnExpressions.forEach(columnExpression => {
            const properties = { columnExpression: columnExpression };
            this.columns.push(new GridColumn(properties));
        });
    }
    setColumnLabels(...labels) {
        let idx = 0;
        labels.forEach(label => {
            if (this.columns.length > idx) {
                this.columns[idx].label = label;
                idx++;
            }
        });
    }
    setColumnProperty(columnName, property, propertyValue) {
        if (columnName instanceof (Array)) {
            columnName.forEach(c => this.setColumnProperty(c, property, propertyValue));
            return;
        }
        var matchingColumn = this.columns.find((col) => { return this.matchingColumn(col, columnName); });
        if (matchingColumn == undefined) {
            const properties = { columnExpression: columnName };
            matchingColumn = new GridColumn(properties, true);
            this.columns.push(matchingColumn);
        }
        matchingColumn[property] = propertyValue;
    }
    setColumnProperties(columnName, properties) {
        Object.keys(properties).forEach((key) => {
            this.setColumnProperty(columnName, key, properties[key]);
        });
    }
    addNestedGrid(handler) {
        this.bind("onNestedClick", handler);
        this.nestedGrid = true;
    }
    addLinkedGrid(grid) {
        this.linkedGrids.push(grid);
    }
    columnIndex(columnName) {
        let cellIndex = this.cellIndexCache[columnName];
        if (cellIndex) {
            return cellIndex;
        }
        cellIndex = -1;
        this.gridPanel.find("th[data-columnname]").get().forEach((th) => {
            if ($(th).data("columnname")) {
                if ($(th).data('columnname').toString().toLowerCase() == columnName.toLowerCase()) {
                    cellIndex = th.cellIndex;
                    this.cellIndexCache[columnName] = cellIndex;
                }
            }
        });
        return cellIndex;
    }
    ;
    columnCell(columnName, row) {
        let cellIndex = this.columnIndex(columnName);
        if (cellIndex == -1) {
            return null;
        }
        if (row === undefined) {
            row = this.selectedRow();
        }
        return row.cells[cellIndex];
    }
    ;
    selectedRow() {
        return this.gridPanel.find("tr.data-row.selected")[0];
    }
    ;
    selectedRows() {
        return this.gridPanel.find("tr.data-row.selected");
    }
    ;
    columnValue(columnName, row) {
        var value = $(row).data(columnName.toLowerCase());
        if (value !== undefined) {
            return value;
        }
        let cell = this.columnCell(columnName, row);
        if (cell) {
            return $(cell).data("value");
        }
        return null;
    }
    reload() {
        this.currentPage = 1;
        this.getPage();
    }
    ;
    getDataArray(callback) {
        this.post("data-array", this.getRequest())
            .then((response) => {
            callback(response);
        });
    }
    ;
    matchingColumn(gridColumn, columnName) {
        var _a;
        var match = false;
        if (gridColumn.columnExpression.includes(".")) {
            match = gridColumn.columnExpression.split('.').pop().toLowerCase() == columnName.toLowerCase();
        }
        if (((_a = gridColumn.columnExpression.split(' ').pop()) === null || _a === void 0 ? void 0 : _a.toLowerCase()) == columnName.toLowerCase()) {
            match = true;
        }
        if (!match) {
            match = gridColumn.columnExpression.toLowerCase() == columnName.toLowerCase();
        }
        return match;
    }
    addPanel(panelId, parent = null) {
        const id = `${this.id}_${panelId}Panel`;
        if (parent == null) {
            parent = this.element;
        }
        jQuery('<div>', {
            id: id
        }).appendTo(parent);
        return $(`#${id}`);
    }
    gridElement(name) {
        return $(`#${this.gridElementId(name)}`);
    }
    gridElementId(name) {
        return `${this.id}_${name}`;
    }
    setInputElement(name, value) {
        var el = this.gridElement(name);
        el.val(value.toString());
        el.width(`${value.toString().length}em`);
    }
    configureToolbar(response) {
        if (response.toolbar) {
            var buttons = ["First", "Next", "Previous", "Last", "Download", "Copy", "View", "Search"];
            buttons.forEach(btn => this.addEventListener(`${btn}Btn`));
        }
        ;
        var $navigationElements = this.gridElement("dbnetgrid-toolbar").find(".navigation");
        var $noRecordsCell = this.gridElement("no-records-cell");
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
            this.setInputElement("Rows", response.totalRows);
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
    configureGrid(response) {
        if (this.toolbarPanel) {
            if (response.toolbar) {
                this.toolbarPanel.html(response.toolbar);
            }
            this.configureToolbar(response);
        }
        this.gridPanel.html(response.data);
        this.gridPanel.find("tr.filter-row :input").get().forEach(input => {
            let $input = $(input);
            let width = $input.width();
            if (input.nodeName == "SELECT") {
                $input.width(width + 20);
            }
            else if (width < 100) {
                $input.width(100);
            }
            $input.on("keyup", (event) => this.columnFilterKeyPress(event));
        });
        this.gridPanel.find("tr.data-row").get().forEach((tr) => {
            this.addRowEventHandlers($(tr));
            this.fireEvent("onRowTransform", { row: tr });
        });
        if (this.dragAndDrop) {
            this.gridPanel.find("tr.header-row th")
                .draggable({ helper: "clone", cursor: "move" })
                .on("dragstart", (event, ui) => this.columnDragStarted(event, ui))
                .on("dragstop", (event, ui) => this.columnDragStopped(event, ui))
                .on("drag", (event, ui) => this.columnDrag(event, ui));
            this.gridPanel.find("tr.header-row th")
                .droppable()
                .on("dropover", (event, ui) => this.dragDropOver(event, ui))
                .on("dropout", (event, ui) => this.dragDropOut(event, ui))
                .on("drop", (event, ui) => this.dragDropped(event, ui));
        }
        this.gridPanel.find("tr.header-row th").get().forEach(th => {
            $(th).on("click", () => this.handleHeaderClick($(th)));
        });
        this.gridPanel.find("tr.filter-row select").get().forEach(select => {
            $(select).on("change", (event) => this.runColumnFilterSearch());
        });
        this.gridPanel.find("input.multi-select-checkbox").get().forEach(e => {
            $(e).on("click", (e) => this.handleRowSelectClick(e));
        });
        this.gridPanel.find("button.nested-grid-button").get().forEach(e => {
            $(e).on("click", (e) => this.openNestedGrid(e));
        });
        this.gridPanel.find("button.download").get().forEach(e => {
            $(e).on("click", (e) => this.downloadCellData(e.currentTarget, false));
        });
        this.gridPanel.find("img.image").get().forEach(e => {
            this.downloadCellData(e.parentElement, true);
        });
        this.gridPanel.find("th[data-columnname]").get().forEach((th) => {
            const columnName = $(th).data("columnname");
            const cellIdx = th.cellIndex;
            this.gridPanel.find("tr.data-row").get().forEach((tr) => {
                const td = tr.cells[cellIdx];
                this.fireEvent("onCellTransform", { cell: td, row: tr, columnName: columnName });
            });
        });
        if (this.autoRowSelect) {
            this.gridPanel.find("tr.data-row").first().trigger("click");
        }
        this.fireEvent("onPageLoaded", { table: this.gridPanel.find("table.dbnetgrid-table")[0] });
        this.renderChart();
        if (this.frozenHeader) {
            let h = $(this.gridPanel.find("tr.header-row")).height();
            var $filterRow = $(this.gridPanel.find("tr.filter-row"));
            $filterRow.find("th").css("top", h);
        }
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
        var dt = google.visualization.arrayToDataTable(dataArray);
        var chart = new google.visualization[this.googleChartOptions.type](document.getElementById(this.googleChartOptions.panelId));
        var options = {};
        if ((_a = this.googleChartOptions) === null || _a === void 0 ? void 0 : _a.functionName) {
            window[(_b = this.googleChartOptions) === null || _b === void 0 ? void 0 : _b.functionName](this, options);
        }
        chart.draw(dt, options);
    }
    updateColumns(response) {
        this.columns = new Array();
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
                dataOnly: col.dataOnly
            };
            this.columns.push(new GridColumn(properties));
        });
    }
    columnDragStarted(event, ui) {
        var $el = $(event.currentTarget);
        $el.css("opacity", 0.5);
        ui.helper.css("opacity", 0.5);
        ui.helper.attr("dbnetgrid_id", this.id);
        ui.helper.width($el.width() + 2).height($el.height() + 2);
    }
    columnDragStopped(event, ui) {
        var $el = $(event.currentTarget);
        $el.css("opacity", 1);
    }
    columnDrag(event, ui) {
        if (!this.dropTarget || ui.helper.attr("dbnetgrid_id") != this.id) {
            return;
        }
        this.dropIcon.show();
        var width = this.dropTarget.width();
        var pos = this.dropTarget.offset();
        var parentOffset = this.dropTarget.parent().offset();
        var uiLeft = ui.position.left + parentOffset.left;
        var left = (pos.left - 9);
        var top = (pos.top - 14);
        ui.helper.attr("dropside", "left");
        if ((uiLeft + width) > pos.left) {
            ui.helper.attr("dropside", "right");
            left += width + 4;
            console.log;
        }
        this.dropIcon.css({ "left": `${left}px`, "top": `${top}px` });
    }
    dragDropOver(event, ui) {
        if (ui.helper.attr("dbnetgrid_id") != this.id) {
            return;
        }
        this.dropTarget = $(event.currentTarget);
    }
    dragDropOut(event, ui) {
        this.dropIcon.hide();
        this.dropTarget = undefined;
    }
    dragDropped(event, ui) {
        if (ui.helper.attr("dbnetgrid_id") != this.id) {
            return;
        }
        this.dropIcon.hide();
        var cols = [];
        var sourceIdx = parseInt(ui.draggable.data("columnordinal")) - 1;
        var targetIdx = parseInt($(event.currentTarget).data("columnordinal")) - 1;
        var column = this.columns[sourceIdx];
        var dropSide = ui.helper.attr("dropside");
        for (var i = 0; i < this.columns.length; i++) {
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
    handleHeaderClick(th) {
        this.orderBy = th.data('columnordinal');
        let dbDataType = th.data('dbdatatype');
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
        var checkbox = $(event.currentTarget);
        const checked = checkbox.is(':checked');
        var rows;
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
        var id = event.target.id;
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
    quickSearchKeyPress(event) {
        var el = event.target;
        window.clearTimeout(this.quickSearchTimerId);
        if (el.value.length >= this.quickSearchMinChars || el.value.length == 0 || event.key == 'Enter')
            this.quickSearchTimerId = window.setTimeout(() => { this.runQuickSearch(el.value); }, this.quickSearchDelay);
    }
    columnFilterKeyPress(event) {
        window.clearTimeout(this.quickSearchTimerId);
        this.quickSearchTimerId = window.setTimeout(() => { this.runColumnFilterSearch(); }, this.quickSearchDelay);
    }
    runQuickSearch(token) {
        this.quickSearchToken = token;
        this.currentPage = 1;
        this.getPage();
    }
    runColumnFilterSearch() {
        this.columnFilters = {};
        this.gridPanel.find("tr.filter-row input").get().forEach(e => {
            var $input = $(e);
            if ($input.val() != '') {
                this.columnFilters[$input.data('columnname')] = $input.val().toString();
            }
        });
        this.gridPanel.find("tr.filter-row select").get().forEach(e => {
            var $input = $(e);
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
        this.columnFilters = {};
        this.gridPanel.find("tr.filter-row input").get().forEach(e => {
            var $input = $(e);
            $input.val('');
        });
        this.gridPanel.find("tr.filter-row select").get().forEach(e => {
            var $input = $(e);
            $input.val('');
        });
    }
    getPage(callback) {
        let activeElementId = this.activeElementId();
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
    lookup($input) {
        var request = this.getRequest();
        request.lookupColumnIndex = parseInt($input.attr("columnIndex"));
        if (this.lookupDialog && request.lookupColumnIndex == this.lookupDialog.columnIndex) {
            this.lookupDialog.open();
            return;
        }
        this.post("lookup", request)
            .then((response) => {
            if (!this.lookupDialog) {
                this.element.append(response.toolbar);
                this.lookupDialog = new LookupDialog(`${this.id}_lookup_dialog`, this);
            }
            this.lookupDialog.update(response, $input);
            this.lookupDialog.open();
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
            let $input = $(`#${activeElementId}`);
            let txt = $input.val();
            $input.trigger("focus").val(txt);
            $input[0].setSelectionRange(txt.length, txt.length);
        }
    }
    download() {
        switch (this.gridElement("DownloadSelect").val()) {
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
            let url = window.URL.createObjectURL(response);
            let tab = window.open();
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
        let $row = $(this.selectedRow());
        this.primaryKey = $row.data("id");
        this.post("view-content", this.getRequest())
            .then((response) => {
            if (!this.viewDialog) {
                this.element.append(response.toolbar);
                this.viewDialog = new ViewDialog(`${this.id}_view_dialog`, this);
            }
            this.viewDialog.update(response, $row);
        });
    }
    openSearchDialog() {
        if (this.searchDialog) {
            this.searchDialog.open();
            return;
        }
        this.post("search-dialog", this.getRequest())
            .then((response) => {
            this.element.append(response.data);
            this.searchDialog = new SearchDialog(`${this.id}_search_dialog`, this);
            this.searchDialog.open();
        });
    }
    copyGrid() {
        var table = this.gridPanel[0].querySelector('table.dbnetgrid-table');
        this.gridPanel.find("tr.data-row.selected").addClass("unselected").removeClass("selected");
        try {
            var range = document.createRange();
            range.selectNode(table);
            window.getSelection().addRange(range);
            document.execCommand('copy');
            window.getSelection().removeRange(range);
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
        this.gridPanel.find("tr.data-row.unselected").addClass("selected").removeClass("unselected");
        this.message("Copied");
    }
    message(text) {
        let dialogId = `${this.id}_message_dialog`;
        let $dialog = $(`#${dialogId}`);
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
        let request = {
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
            fixedFilterSql: this.fixedFilterSql
        };
        return request;
    }
    addEventListener(id, eventName = "click") {
        this.gridElement(id).on(eventName, (event) => this.handleClick(event));
    }
    disable(id, disabled) {
        this.gridElement(id).prop("disabled", disabled);
    }
    showLoader() {
        this.loadingPanel.addClass("display");
    }
    hideLoader() {
        this.loadingPanel.removeClass("display");
    }
    post(action, request, blob = false) {
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
                return response.blob();
            }
            return response.json();
        })
            .catch(err => {
            err.text().then((errorMessage) => {
                console.error(errorMessage);
                this.gridPanel.text(errorMessage.split("\n").shift());
            });
            return Promise.reject();
        });
    }
    downloadCellData(element, image) {
        var _a;
        var $viewContentRow = $(element).closest("tr.view-content-row");
        const $button = $(element);
        var $cell = $button.closest("td");
        var $row = $button.closest("tr");
        if ($viewContentRow.length) {
            this.columnName = $viewContentRow.data("columnname");
        }
        else {
            this.primaryKey = $row.data("id");
            this.columnName = (_a = this.gridPanel.find("th[data-columnname]").get($cell.prop("cellIndex"))) === null || _a === void 0 ? void 0 : _a.getAttribute("data-columnname");
        }
        let args = {
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
                let img = $cell.find("img");
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
        var row = $button.closest("tr");
        if ($button.hasClass("open")) {
            $button.removeClass("open");
            row.next().hide();
            return;
        }
        var table = $button.closest("table");
        $button.addClass("open");
        if (row.next().hasClass("nested-grid-row")) {
            row.next().show();
            return;
        }
        var newRow = table[0].insertRow(row[0].rowIndex + 1);
        newRow.className = "nested-grid-row";
        var cell = newRow.insertCell(-1);
        cell.className = "nested-grid-cell";
        cell.colSpan = row[0].cells.length;
        var handlers = this.eventHandlers["onNestedClick"];
        for (var i = 0; i < handlers.length; i++) {
            if (handlers[i]) {
                this.configureNestedGrid(handlers[i], cell, row.data("id"));
            }
        }
    }
    configureNestedGrid(handler, cell, pk) {
        var gridId = `dbnetgrid${new Date().valueOf()}`;
        jQuery(document.createElement("div")).attr("id", gridId).appendTo($(cell));
        var grid = new DbNetGrid(gridId);
        grid.connectionString = this.connectionString;
        var args = [grid, this];
        handler.apply(window, args);
        this.assignForeignKey(grid, pk);
        grid.initialize();
    }
    assignForeignKey(grid, pk) {
        var col = grid.columns.find((col) => { return col.foreignKey == true; });
        if (col == undefined) {
            alert('No foreign key defined for nested grid');
            return;
        }
        col.foreignKeyValue = pk;
    }
}
