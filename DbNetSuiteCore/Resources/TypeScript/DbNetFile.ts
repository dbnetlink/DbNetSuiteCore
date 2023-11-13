class DbNetFile extends DbNetSuite {
    rootFolder = "";
    folder = "";
    folderPanel: JQuery<HTMLElement> | undefined;
    toolbarPanel: JQuery<HTMLElement> | undefined;
    columns: FileColumn[];
    quickSearch = true;
    quickSearchToken = "";
    toolbarButtonStyle: ToolbarButtonStyle = ToolbarButtonStyle.Image;
    search = true;
    export = true;
    copy = true;
    upload = false;
    navigation = true;
    totalRows = 0;
    totalPages = 0;
    currentPage = 1;
    pageSize = 20;
    caption = "";
    nested = false;

    constructor(id: string) {
        super(id);
        this.columns = [];
    }

    initialize(): void {
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
    }

    setColumnTypes(...types: string[]): void {
        types.forEach(type => {
            this.columns.push(new FileColumn(type))
        });
    }

    reload() {
        this.callServer("page");
    }

    private configureToolbar(response: DbNetFileResponse) {
        if (response.toolbar) {
            const buttons = ["First", "Next", "Previous", "Last", "Export", "Copy", "Search", "Upload"];
            buttons.forEach(btn =>
                this.addEventListener(`${btn}Btn`)
            )
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
  
    private configurePage(response: DbNetFileResponse) {
        if (this.toolbarPanel) {
            if (response.toolbar) {
                this.toolbarPanel?.html(response.toolbar);
            }
            this.configureToolbar(response);
        }
        this.folderPanel?.html(response.html);
        this.folderPanel?.find("a.folder-link").get().forEach(e => {
            $(e).on("click", (e) => this.selectFolder(e));
        });
        this.folderPanel?.find("td.folder").get().forEach(e => {
            $(e).on("click", (e) => this.openNestedFolder(e));
        });
        this.fireEvent("onPageLoaded", { });
    }

    private selectFolder(event: JQuery.ClickEvent<HTMLElement>) {
        const $anchor = $(event.currentTarget);
        this.folder = $anchor.data("folder");
        this.callServer("page");
    }

    private openNestedFolder(event: JQuery.ClickEvent<HTMLElement>) {
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

    public callServer(action:string) {
        this.post<DbNetFileResponse>(action, this.getRequest())
            .then((response) => {
                if (response.error == false) {
                    this.configurePage(response);
                }
            })
    }

    private addEventListener(id: string, eventName = "click") {
        this.controlElement(id).on(eventName, (event) => this.handleClick(event));
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
        //        this.download();
                break;
            case this.controlElementId("CopyBtn"):
         //       this.copyGrid();
                break;
            case this.controlElementId("SearchBtn"):
         //       this.openSearchDialog(this.getRequest());
                break;
            case this.controlElementId("UploadBtn"):
     //           this.uploadFile();
                break;
            default:
                this.reload();
                break;
        }
    }

    private getRequest(): DbNetFileRequest {
        const request = this._getRequest() as DbNetFileRequest;
        request.rootFolder = this.rootFolder;
        request.folder = this.folder;
        request.columns = this.columns.map((column) => { return column as FileColumnRequest });
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

        return request;
    }
 }