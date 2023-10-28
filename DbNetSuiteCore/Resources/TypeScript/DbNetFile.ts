class DbNetFile extends DbNetSuite {
    folder = "";
    folderPanel: JQuery<HTMLElement> | undefined;

    constructor(id: string) {
        super(id);
    }

    initialize(): void {
        if (!this.element) {
            return;
        }

        this.element.empty();
        this.folderPanel = this.addPanel("folder");
        this.addLoadingPanel();
        this.callServer("page");
        this.initialised = true;
        this.fireEvent("onInitialized");
    }

    reload() {
        this.callServer("page");
    }
  
    private configurePage(response: DbNetFileResponse) {
        this.folderPanel?.html(response.html);
        this.fireEvent("onPageLoaded", { });
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
        const request = {} as DbNetFileRequest;
        request.folder = this.folder;
        return request;
    }
 
 }