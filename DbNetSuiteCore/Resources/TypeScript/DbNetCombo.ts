class DbNetCombo extends DbNetSuite {
    addEmptyOption = false;
    addFilter = false;
    currentValue = "";
    emptyOptionText = "";
    linkedControls: Array<DbNetSuite> = [];
    sql = "";
    params: Dictionary<object> = {};
    filterDelay = 1000;
    filterMinChars = 3;
    filterTimerId: number | undefined;
    filterToken = "";

    constructor(id: string) {
        super();
        this.id = id;
        this.element = $(`#${this.id}`) as JQuery<HTMLElement>;
        this.element.addClass("dbnetsuite").addClass("cleanslate")

        this.checkStyleSheetLoaded();

        if (this.element.length == 0) {
         //   this.error(`DbNetCombo container element '${this.id}' not found`);
            return;
        }
    }

    initialize(): void {
        this.callServer("page");
        this.initialised = true;
        this.fireEvent("onInitialized");
    }

    addLinkedControl(control: DbNetGrid) {
        this.linkedControls.push(control);
    }

    reload() {
        this.callServer("page");
    }

    private configureCombo(response: DbNetComboResponse) {
        if (response.select) { 
            this.element?.html(response.select);
        }
        const $select = this.element?.find("select");
        $select?.html(response.options);
        const selectWidth = $select?.width() as number;
        const $input = this.element?.find("input");
        $input?.width(selectWidth - 4);
        $input?.on("keyup", (event) => this.filterKeyPress(event));
    }

    private filterKeyPress(event: JQuery.TriggeredEvent): void {
        const el = event.target as HTMLInputElement;
        window.clearTimeout(this.filterTimerId);

        if (el.value.length >= this.filterMinChars || el.value.length == 0 || event.key == 'Enter') {
            this.filterTimerId = window.setTimeout(() => { this.applyFilter(el.value) }, this.filterDelay);
        }
    }

    private applyFilter(filterToken:string): void {
        this.filterToken = filterToken;
        this.callServer("filter");
    }

    public callServer(action:string) {
        this.post<DbNetComboResponse>(action, this.getRequest())
            .then((response) => {
                if (response.error == false) {
                    this.configureCombo(response);
                }
            })
    }
    private getRequest(): DbNetComboRequest {
        const request: DbNetComboRequest = {
            componentId: this.id,
            connectionString: this.connectionString,
            sql: this.sql,
            params: this.params,
            addEmptyOption: this.addEmptyOption,
            emptyOptionText: this.emptyOptionText,
            addFilter: this.addFilter,
            filterToken: this.filterToken
        };

        return request;
    }
  
    private post<T>(action: string, request: any): Promise<T> {
        this.showLoader();
        const options = {
            method: "POST",
            headers: {
                Accept: "application/json",
                "Content-Type": "application/json;charset=UTF-8",
            },
            body: JSON.stringify(request)
        };

        return fetch(`~/dbnetcombo.dbnetsuite?action=${action}`, options)
            .then(response => {
                this.hideLoader();
                if (!response.ok) {
                    throw response;
                }
                return response.json() as Promise<T>;
            })
            .catch(err => {
                err.text().then((errorMessage: string) => {
                    console.error(errorMessage);
                });

                return Promise.reject()
            })
    }
 }