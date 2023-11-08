class DbNetFile extends DbNetSuite {
    rootFolder = "";
    folder = "";
    folderPanel: JQuery<HTMLElement> | undefined;
    columns: FileColumn[];

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
        this.folderPanel = this.addPanel("folder");
        this.addLoadingPanel();
        this.callServer("page");
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
  
    private configurePage(response: DbNetFileResponse) {
        this.folderPanel?.html(response.html);
        this.folderPanel?.find("a.folder-link").get().forEach(e => {
            $(e).on("click", (e) => this.selectFolder(e));
        });
        this.fireEvent("onPageLoaded", { });
    }

    private selectFolder(event: JQuery.ClickEvent<HTMLElement>) {
        const $anchor = $(event.currentTarget);
        this.folder = $anchor.data("folder");
        this.callServer("page");
    }

    public callServer(action:string) {
        this.post<DbNetFileResponse>(action, this.getRequest())
            .then((response) => {
                if (response.error == false) {
                    this.configurePage(response);
                }
            })
    }

    private getRequest(): DbNetFileRequest {
        const request = this._getRequest() as DbNetFileRequest;
        request.rootFolder = this.rootFolder;
        request.folder = this.folder;
        request.columns = this.columns.map((column) => { return column as FileColumnRequest });
        return request;
    }
 }