class DbNetEdit extends DbNetSuite {
    columns: EditColumn[];
    currentRow = 1 ;
    editPanel: JQuery<HTMLElement> | undefined;
    toolbarPanel: JQuery<HTMLElement> | undefined;
    toolbarPosition: ToolbarPosition = "Top";
    linkedControls: Array<DbNetSuite> = [];
    fromPart = "";
    search = true;
    navigation = true;
    quickSearch = true;

    constructor(id: string) {
        super(id);
        this.columns = [];
    }

    initialize(): void {
        if(!this.element) {
            return;
        }
        this.element.empty();

        if (this.toolbarPosition == "Top") {
            this.toolbarPanel = this.addPanel("toolbar");
        }
        this.editPanel = this.addPanel("edit");
        if (this.toolbarPosition == "Bottom") {
            this.toolbarPanel = this.addPanel("toolbar");
        }

        this.addLoadingPanel();
        this.callServer("initialize");
        this.initialised = true;
        this.fireEvent("onInitialized");
    }

    addLinkedControl(control: DbNetSuite) {
        this.linkedControls.push(control);
    }

    reload() {
        this.callServer("page");
    }

    private configureEdit(response: DbNetEditResponse) {
        if (this.toolbarPanel) {
            if (response.toolbar) {
                this.toolbarPanel?.html(response.toolbar);
            }
        }
    }

    public callServer(action:string) {
        this.post<DbNetEditResponse>(action, this.getRequest())
            .then((response) => {
                if (response.error == false) {
                    this.configureEdit(response);
                }
            })
    }
    private getRequest(): DbNetEditRequest {
        const request: DbNetEditRequest = {
            componentId: this.id,
            connectionString: this.connectionString,
            fromPart: this.fromPart,
            columns: this.columns.map((column) => { return column as EditColumnRequest }),
            currentRow: this.currentRow,
            culture: this.culture,
            search: this.search,
            navigation: this.navigation,
            quickSearch: this.quickSearch
        };

        return request;
    }
  
 }