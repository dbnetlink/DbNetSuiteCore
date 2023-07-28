class DbNetCombo extends DbNetSuite {
    addEmptyOption = false;
    addFilter = false;
    autoRowSelect = false;
    comboPanel: JQuery<HTMLElement> | undefined;
    currentValue = "";
    dataOnlyColumns: Array<string> = [];
    distinct = false;
    emptyOptionText = "";
    fromPart = "";
    filterDelay = 1000;
    filterMinChars = 3;
    filterTimerId: number | undefined;
    filterToken = "";
    foreignKeyColumn = "";
    foreignKeyValue: Array<string> | undefined;
    multipleSelect = false;
    procedureParams: Dictionary<object> = {};
    procedureName = "";
    size = 1;
    textColumn = "";
    valueColumn = "";

    $select: JQuery<HTMLSelectElement> | undefined;
    $filter: JQuery<HTMLInputElement> | undefined;

    constructor(id: string) {
        super(id);
    }

    initialize(): void {
        this.comboPanel = this.addPanel("combo");
        this.addLoadingPanel();
        this.callServer("page");
        this.initialised = true;
        this.fireEvent("onInitialized");
    }

    reload() {
        this.callServer("page");
    }

    selectedValues() {
        return this.getSelectedValues();
    }

    selectedOptions() {
        return this.getSelectedOptions();
    }

    private configureCombo(response: DbNetComboResponse) {
        if (response.select) {
            this.comboPanel?.html(response.select);
            this.$select = this.comboPanel?.find("select") as JQuery<HTMLSelectElement>;
            this.$select?.html(response.options);
            const selectWidth = this.$select?.width() as number;
            this.$filter = this.comboPanel?.find("input");
            this.$filter?.width(selectWidth);
            this.$filter?.on("keyup", (event) => this.filterKeyPress(event));
            this.$select.on("change", () => this.optionSelected())
        }
        else {
            this.$select?.html(response.options);
        }

        if (this.autoRowSelect && this.size > 1 && this.$select?.children().length as number > 0) {
            this.$select?.prop("selectedIndex", 0);
        }

        this.fireEvent("onOptionsLoaded", { options: this.getOptions() });
        this.optionSelected();
    }

    private optionSelected(): void {
        const selectedValues = this.getSelectedValues();
        const selectedOptions = this.getSelectedOptions();
        const pk = selectedOptions.length ? selectedOptions[0].dataset["pk"] : null;

        this.fireEvent("onOptionSelected", { selectedValues: selectedValues, selectedOptions: selectedOptions });
        this.configureLinkedControls(selectedValues, pk);
    }

    private getSelectedValues() {
        const selectedValues = new Array<string>();
        this.$select?.find("option:selected").each(function () {
            const s = $(this).val() as string;
            if (s.length > 0) {
                selectedValues.push(s)
            }
        });

        return selectedValues;
    }

    private getSelectedOptions() {
        const selectedOptions = new Array<HTMLOptionElement>();
        this.$select?.find("option:selected").each(function () {
            selectedOptions.push($(this)[0] as HTMLOptionElement)
        });

        return selectedOptions;
    }

    private getOptions() {
        const options = new Array<HTMLOptionElement>();
        this.$select?.find("option").each(function () {
            options.push($(this)[0] as HTMLOptionElement)
        });

        return options;
    }

    public configureLinkedControl(control: DbNetSuite, fk: Array<string>, pk: string | null) {
        if (control instanceof DbNetCombo) {
            const combo = control as DbNetCombo
            if (control.connectionString == "") {
                control.connectionString = this.connectionString;
            }
            combo.foreignKeyValue = fk;
            combo.initialised ? combo.reload() : combo.initialize();
        }
        if (control instanceof DbNetGrid) {
            const grid = control as DbNetGrid
            grid.assignForeignKey(grid, fk);
            grid.currentPage = 1;
            grid.initialised ? grid.getPage() : grid.initialize();
        }
        if (control instanceof DbNetEdit) {
            const edit = control as DbNetEdit
            edit.assignForeignKey(edit, fk);
            edit.currentRow = 1;
            edit.initialised ? edit.getRecord(pk) : edit.initialize(pk);
        }
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
            culture: this.culture,
            dataOnlyColumns: this.dataOnlyColumns,
            parentControlType: this.parentControlType,
            fromPart: this.fromPart,
            valueColumn: this.valueColumn,
            textColumn: this.textColumn,
            procedureParams: this.procedureParams,
            addEmptyOption: this.addEmptyOption,
            emptyOptionText: this.emptyOptionText,
            addFilter: this.addFilter,
            filterToken: this.filterToken,
            foreignKeyColumn: this.foreignKeyColumn,
            foreignKeyValue: this.foreignKeyValue,
            size: this.size,
            multipleSelect: this.multipleSelect,
            procedureName: this.procedureName,
            distinct:this.distinct
        };

        return request;
    }
  

 }