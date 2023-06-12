class DbNetCombo extends DbNetSuite {
    id = "";
    connectionString = "";
    initialised = false;
    element: JQuery<HTMLElement>;
    linkedControls: Array<DbNetSuite> = [];
    addEmptyOption = false;
    emptyOptionText = "";
    sql = "";
    params: Dictionary<object> = {};
    currentValue = "";

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
        this.getPage();
        this.initialised = true;
        this.fireEvent("onInitialized");
    }

    addLinkedControl(control: DbNetGrid) {
        this.linkedControls.push(control);
    }

    reload() {
        this.getPage();
    }

    private configureCombo(response: DbNetComboResponse) {
        this.element?.html(response.data);
    }

    public getPage() {
        this.post<DbNetComboResponse>("page", this.getRequest())
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
            emptyOptionText: this.emptyOptionText
        };

        return request;
    }
  
    private post<T>(action: string, request: any, blob = false): Promise<T> {
       // this.showLoader();
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
              //  this.hideLoader();

                if (!response.ok) {
                    throw response;
                }
                if (blob) {
                    return response.blob() as Promise<T>;
                }
                return response.json() as Promise<T>;
            })
            .catch(err => {
                err.text().then((errorMessage: string) => {
                    console.error(errorMessage);
                 //   this.error(errorMessage.split("\n").shift() as string)
                });

                return Promise.reject()
            })
    }
 
}