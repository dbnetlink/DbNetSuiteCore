class DbNetCombo extends DbNetSuite {
    addEmptyOption = false;
    addFilter = false;
    autoRowSelect = false;
    currentValue = "";
    emptyOptionText = "";
    fromPart = "";
    filterDelay = 1000;
    filterMinChars = 3;
    filterTimerId: number | undefined;
    filterToken = "";
    foreignKeyColumn = "";
    foreignKeyValue: Array<string> | undefined;
    linkedControls: Array<DbNetSuite> = [];
    multipleSelect = false;
    params: Dictionary<object> = {};
    size = 1;
    textColumn = "";
    valueColumn = "";

    $select: JQuery<HTMLSelectElement> | undefined;

    constructor(id: string) {
        super(id);
    }

    initialize(): void {
        this.callServer("page");
        this.initialised = true;
        this.fireEvent("onInitialized");
    }

    addLinkedControl(control: DbNetSuite) {
        this.linkedControls.push(control);
    }

    reload() {
        this.callServer("page");
    }

    private configureCombo(response: DbNetComboResponse) {
        if (response.select) {
            this.element?.html(response.select);
            this.$select = this.element?.find("select") as JQuery<HTMLSelectElement>;
            this.$select?.html(response.options);
            const selectWidth = this.$select?.width() as number;
            const $input = this.element?.find("input");
            $input?.width(selectWidth);
            $input?.on("keyup", (event) => this.filterKeyPress(event));
            this.$select.on("change", () => this.optionSelected())
        }
        else {
            this.$select?.html(response.options);
        }

        if (this.autoRowSelect && this.size > 1 && this.$select?.children().length as number > 0) {
            this.$select?.prop("selectedIndex", 0);
        }

        this.fireEvent("onOptionsLoaded");
        this.optionSelected();
    }

    private optionSelected(): void {
        const selectedValue = new Array<string>();

        this.$select?.find("option:selected").each(function () {
            const s = $(this).val() as string;
            if (s.length > 0) {
                selectedValue.push(s)
            }
        });

        this.fireEvent("onOptionSelected");

        this.linkedControls.forEach((control) => {
            if (control instanceof DbNetGrid) {
                const grid = control as DbNetGrid;
                grid.configureLinkedGrid(grid, selectedValue);
            }
            if (control instanceof DbNetCombo) {
                const combo = control as DbNetCombo;
                combo.configureLinkedCombo(combo, selectedValue);
            }
        });
    }

    public configureLinkedCombo(combo: DbNetCombo, fk: Array<string>) {
        if (combo.connectionString == "") {
            combo.connectionString = this.connectionString;
        }
        combo.foreignKeyValue = fk;
        combo.initialised ? combo.reload() : combo.initialize();
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
            fromPart: this.fromPart,
            valueColumn: this.valueColumn,
            textColumn: this.textColumn,
            params: this.params,
            addEmptyOption: this.addEmptyOption,
            emptyOptionText: this.emptyOptionText,
            addFilter: this.addFilter,
            filterToken: this.filterToken,
            foreignKeyColumn: this.foreignKeyColumn,
            foreignKeyValue: this.foreignKeyValue,
            size: this.size,
            multipleSelect: this.multipleSelect
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
                    this.error(errorMessage.split("\n").shift() as string)
                });

                return Promise.reject()
            })
    }
 }