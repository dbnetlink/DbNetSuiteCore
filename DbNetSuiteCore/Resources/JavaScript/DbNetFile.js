"use strict";
class DbNetFile extends DbNetSuite {
    constructor(id) {
        super(id);
        this.rootFolder = "";
        this.fileName = "";
        this.folder = "";
        this.quickSearch = true;
        this.quickSearchToken = "";
        this.toolbarButtonStyle = ToolbarButtonStyle.Image;
        this.search = true;
        this.searchFilterJoin = "";
        this.searchParams = [];
        this.export = true;
        this.copy = true;
        this.upload = false;
        this.navigation = true;
        this.totalRows = 0;
        this.totalPages = 0;
        this.currentPage = 1;
        this.pageSize = 20;
        this.previewHeight = 30;
        this.caption = "";
        this.nested = false;
        this.orderBy = "";
        this.orderByDirection = "asc";
        this.searchResultsDialogId = "";
        this.isSearchResults = false;
        this.includeSubfolders = false;
        this.columns = [];
    }
    initialize() {
        if (!this.element) {
            return;
        }
        this.rootFolder = this.folder;
        this.element.empty();
        this.toolbarPanel = this.addPanel("toolbar");
        this.folderPanel = this.addPanel("folder");
        this.addLoadingPanel();
        this.callServer("initialise");
        this.initialised = true;
        this.fireEvent("onInitialized");
        this.linkedControls.forEach((control) => {
            if (control.isSearchResults) {
                this.searchResultsControl = control;
                this.searchResultsControl.internalBind("onPageLoaded", () => this.openSearchResultsDialog());
                //this.searchResultsControl.initialize();
            }
        });
    }
    setColumnTypes(...types) {
        types.forEach(type => {
            this.columns.push(new FileColumn(type));
        });
    }
    setColumnProperty(columnType, property, propertyValue) {
        let matchingColumn = this.columns.find((col) => { return col.type == columnType; });
        if (matchingColumn == undefined) {
            matchingColumn = new FileColumn(columnType);
            this.columns.push(matchingColumn);
        }
        matchingColumn[property] = propertyValue;
    }
    reload() {
        if (this.initialised) {
            this.currentPage = 1;
            this.getPage();
        }
        else {
            this.initialize();
        }
    }
    getPage(callback) {
        this.callServer("page", callback);
    }
    configureToolbar(response) {
        if (response.toolbar) {
            const buttons = ["First", "Next", "Previous", "Last", "Export", "Copy", "Search", "Upload"];
            buttons.forEach(btn => this.addEventListener(`${btn}Btn`));
        }
        const $navigationElements = this.controlElement("dbnetfile-toolbar").find(".navigation");
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
            this.controlElement("dbnetfile-toolbar").find(".navigation").show();
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
        this.disable("ExportBtn", response.totalRows == 0);
        this.disable("CopyBtn", response.totalRows == 0);
        this.disable("UpdateBtn", response.totalRows == 0);
    }
    configurePage(response) {
        var _a, _b, _c, _d, _e, _f, _g, _h;
        if (this.toolbarPanel) {
            if (response.toolbar) {
                (_a = this.toolbarPanel) === null || _a === void 0 ? void 0 : _a.html(response.toolbar);
            }
            this.configureToolbar(response);
        }
        (_b = this.folderPanel) === null || _b === void 0 ? void 0 : _b.html(response.html);
        (_c = this.folderPanel) === null || _c === void 0 ? void 0 : _c.find("a.folder-link").get().forEach(e => {
            $(e).on("click", (e) => this.selectFolder(e));
        });
        (_d = this.folderPanel) === null || _d === void 0 ? void 0 : _d.find("td.folder").get().forEach(e => {
            $(e).on("click", (e) => this.openNestedFolder(e));
        });
        (_e = this.folderPanel) === null || _e === void 0 ? void 0 : _e.find("a.file-link").get().forEach(e => {
            $(e).on("click", (e) => this.selectFile(e));
        });
        (_f = this.folderPanel) === null || _f === void 0 ? void 0 : _f.find("tr.data-row").get().forEach((tr) => {
            this.addRowEventHandlers($(tr));
        });
        (_g = this.folderPanel) === null || _g === void 0 ? void 0 : _g.find("tr.header-row th").get().forEach(th => {
            $(th).on("click", () => this.handleHeaderClick($(th)));
        });
        (_h = this.folderPanel) === null || _h === void 0 ? void 0 : _h.find("td[column-type='Preview']").get().forEach(td => this.loadPreview($(td)));
        this.fireEvent("onPageLoaded", {});
    }
    selectFolder(event) {
        const $anchor = $(event.currentTarget);
        if (this.isSearchResults) {
            const parentControl = this.parentControl;
            parentControl.folder = $anchor.data("folder");
            parentControl.reload();
        }
        else {
            this.folder = $anchor.data("folder");
            this.currentPage = 1;
            this.callServer("page");
        }
    }
    loadPreview($td) {
        const $anchor = $td.parent().find("a[data-filetype='Image']");
        if ($anchor) {
            this.fileName = $anchor.data("file");
            this.post("download-file", this.getRequest(), true)
                .then((blob) => {
                if (blob.size) {
                    const $image = $(new Image());
                    $image.hide();
                    $image.attr("src", window.URL.createObjectURL(blob));
                    $image.on("load", (event) => this.setPreviewHeight(event));
                    $td.empty().append($image);
                }
            });
        }
    }
    setPreviewHeight(event) {
        const $img = $(event.currentTarget);
        const height = $img === null || $img === void 0 ? void 0 : $img.height();
        if (height > this.previewHeight) {
            $img.height(this.previewHeight);
        }
        $img.show();
    }
    selectFile(event) {
        const $anchor = $(event.currentTarget);
        this.fileName = $anchor.data("file");
        const fileType = $anchor.data("filetype");
        const fileName = $anchor.text();
        this.post("download-file", this.getRequest(), true)
            .then((blob) => {
            if (blob.size) {
                switch (fileType) {
                    case "Image":
                    case "Video":
                    case "Audio":
                        this.viewUrl(window.URL.createObjectURL(blob), fileName, fileType);
                        break;
                    case "Html":
                    case "Pdf":
                        this.viewInTab(blob);
                        break;
                    default:
                        this.downloadBlob(blob, fileName);
                        break;
                }
            }
        });
    }
    addRowEventHandlers($tr) {
        $tr.on("mouseover", (e) => $(e.currentTarget).addClass("highlight"));
        $tr.on("mouseout", (e) => $(e.currentTarget).removeClass("highlight"));
        $tr.on("click", (e) => this.handleRowClick($(e.currentTarget)));
    }
    handleRowClick(tr) {
        tr.parent().find("tr.data-row").removeClass("selected");
        tr.addClass("selected");
        this.fireEvent("onRowSelected", { row: tr[0] });
    }
    handleHeaderClick(th) {
        this.orderBy = th.data('type');
        this.orderByDirection = "asc";
        if (th.attr("orderby")) {
            this.orderByDirection = th.attr("orderby") == "asc" ? "desc" : "asc";
        }
        this.getPage();
    }
    downloadBlob(blob, fileName) {
        const link = document.createElement("a");
        link.href = window.URL.createObjectURL(blob);
        link.download = fileName;
        link.click();
    }
    viewInTab(blob) {
        const link = document.createElement("a");
        link.href = window.URL.createObjectURL(blob);
        link.target = "_blank";
        link.click();
    }
    openNestedFolder(event) {
        const $cell = $(event.currentTarget);
        const $row = $cell.closest("tr");
        if ($cell.hasClass("open")) {
            $cell.removeClass("open");
            $row.next().hide();
            return;
        }
        const $table = $cell.closest("table");
        $cell.addClass("open");
        if ($row.next().hasClass("nested-component-row")) {
            $row.next().show();
            return;
        }
        const newRow = $table[0].insertRow($row[0].rowIndex + 1);
        newRow.className = "nested-component-row";
        newRow.insertCell(-1);
        const newCell = newRow.insertCell(-1);
        newCell.className = "nested-component-cell";
        newCell.colSpan = $row[0].cells.length - 1;
        const id = `dbnetfile${new Date().valueOf()}`;
        jQuery(document.createElement("div")).attr("id", id).appendTo($(newCell));
        const dbnetfile = new DbNetFile(id);
        dbnetfile.folder = $cell.data("folder");
        dbnetfile.nested = true;
        dbnetfile.initialize();
    }
    callServer(action, callback) {
        this.post(action, this.getRequest())
            .then((response) => {
            if (callback) {
                callback(response);
            }
            else if (response.error == false) {
                this.configurePage(response);
            }
        });
    }
    addEventListener(id, eventName = "click") {
        this.controlElement(id).on(eventName, (event) => this.handleClick(event));
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
                //        this.download();
                break;
            case this.controlElementId("CopyBtn"):
                //       this.copyGrid();
                break;
            case this.controlElementId("SearchBtn"):
                this.openSearchDialog(this.getRequest());
                break;
            case this.controlElementId("UploadBtn"):
                //           this.uploadFile();
                break;
            default:
                this.getPage();
                break;
        }
    }
    applySearch(searchFilterJoin, includeSubfolders) {
        if (!this.searchResultsControl) {
            return;
        }
        this.searchResultsControl.searchFilterJoin = searchFilterJoin;
        this.searchResultsControl.includeSubfolders = includeSubfolders;
        this.searchResultsControl.folder = this.folder;
        this.searchResultsControl.searchParams = this.searchParams;
        this.searchResultsControl.reload();
    }
    openSearchResultsDialog() {
        var _a;
        if (!this.searchResultsDialog) {
            this.searchResultsDialog = new SearchResultsDialog(this.searchResultsDialogId, this.searchResultsControl);
        }
        (_a = this.searchResultsDialog) === null || _a === void 0 ? void 0 : _a.show();
    }
    openSearchDialog(request) {
        if (this.searchDialog) {
            this.searchDialog.open();
            return;
        }
        this.post("search-dialog", request)
            .then((response) => {
            var _a;
            (_a = this.element) === null || _a === void 0 ? void 0 : _a.append(response.dialog);
            this.searchDialog = new FileSearchDialog(`${this.id}_search_dialog`, this);
            this.searchDialog.open();
        });
    }
    getRequest() {
        const request = this._getRequest();
        request.rootFolder = this.rootFolder;
        request.folder = this.folder;
        request.columns = this.columns.map((column) => { return column; });
        request.quickSearch = this.quickSearch;
        request.quickSearchToken = this.quickSearchToken;
        request.toolbarButtonStyle = this.toolbarButtonStyle;
        request.search = this.search;
        request.export = this.export;
        request.copy = this.copy;
        request.upload = this.upload;
        request.navigation = this.navigation;
        request.pageSize = this.pageSize;
        request.currentPage = this.currentPage;
        request.caption = this.caption;
        request.fileName = this.fileName;
        request.orderBy = this.orderBy;
        request.orderByDirection = this.orderByDirection;
        request.searchParams = this.searchParams;
        request.searchFilterJoin = this.searchFilterJoin;
        request.isSearchResults = this.isSearchResults;
        request.includeSubfolders = this.includeSubfolders;
        return request;
    }
}
class FileColumn {
    constructor(type) {
        this.type = type;
    }
}
