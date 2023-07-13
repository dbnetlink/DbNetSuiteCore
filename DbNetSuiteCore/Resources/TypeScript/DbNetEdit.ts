class DbNetEdit extends DbNetGridEdit {
    changes: Dictionary<object> = {};
    currentRow = 1;
    formPanel: JQuery<HTMLElement> | undefined;
    layoutColumns = 1;
    messagePanel: JQuery<HTMLElement> | undefined;
    primaryKey = "";
    search = true;
    totalRows = 0;

    constructor(id: string) {
        super(id);
        if (this.toolbarPosition == undefined) {
            this.toolbarPosition = "Bottom";
        }
    }

    initialize(): void {
        if (!this.element) {
            return;
        }
        this.element.empty();

        if (this.toolbarPosition == "Top") {
            this.toolbarPanel = this.addPanel("toolbar");
        }
        this.formPanel = this.addPanel("form");
        if (this.toolbarPosition == "Bottom") {
            this.toolbarPanel = this.addPanel("toolbar");
        }

        this.addLoadingPanel();
        this.post<DbNetEditResponse>("initialize", this.getRequest())
            .then((response) => {
                if (response.error == false) {
                    this.updateColumns(response);
                    this.configureEdit(response);
                }
            })
        this.initialised = true;
        this.fireEvent("onInitialized");
    }

    addLinkedControl(control: DbNetSuite) {
        this.linkedControls.push(control);
    }

    getRows(callback?: Function) {
        this.callServer("search", callback);
    }

    private configureEdit(response: DbNetEditResponse) {
        if (this.toolbarPanel) {
            if (response.toolbar) {
                this.toolbarPanel?.html(response.toolbar);
            }
            this.configureToolbar(response);
        }
        if (response.form) {
            this.formPanel?.html(response.form);
            this.configureForm();
        }

        this.updateForm(response);
    }

    private configureForm() {
        const $inputs = this.formPanel?.find(`:input[name]`) as JQuery<HTMLFormElement>;
        for (let i = 0; i < $inputs.length; i++) {
            const $input = $($inputs[i]);
            this.fireEvent("onFormElementCreated", { formElement: $input[0], columnName: $input.attr("name") });
        }

        this.formPanel?.find("[button-type='calendar']").on("click", (event) => this.selectDate(event));
        this.formPanel?.find("[button-type='clock']").on("click", (event) => this.selectTime(event));
        this.formPanel?.find("[button-type='lookup']").on("click", (event) => this.editLookup(event));

        this.formPanel?.find("input[datatype='DateTime'").get().forEach(e => {
            const $input = $(e as HTMLInputElement);
            this.addDatePicker($input, this.datePickerOptions);
        });
    }

    private updateForm(response: DbNetEditResponse) {
        if (response.totalRows == 0) {
            this.formPanel?.find(':input').val('').not("[primarykey='true']").prop('checked', false).prop('selected', false).prop("disabled", true);
            return;
        }
        this.formPanel?.find(':input').not("[primarykey='true']").prop("disabled", false);
        const record = response.record;
        for (const columnName in record) {
            const $input = this.formPanel?.find(`:input[name='${columnName}']`) as JQuery<HTMLFormElement>;
            const value = record[columnName];
            if ($input.attr("type") == "checkbox") {
                const checked = (value as unknown as boolean === true)
                $input.prop("checked", checked).data("value", checked);
            }
            else {
                $input.val(value.toString()).data("value", value.toString());
            }
        }

        this.primaryKey = response.primaryKey as string;

        if (response.message) {
            this.message(response.message);
        }
    }

    private callServer(action: string, callback?: Function) {
        this.post<DbNetEditResponse>(action, this.getRequest())
            .then((response) => {
                if (response.error == false) {
                    this.configureToolbar(response);
                    this.updateForm(response);
                }
                if (callback) {
                    callback(response);
                }
            })
    }
    public getRequest(): DbNetEditRequest {
        const request: DbNetEditRequest = {
            changes: this.changes,
            componentId: this.id,
            connectionString: this.connectionString,
            columns: this.columns.map((column) => { return column as EditColumnRequest }),
            currentRow: this.currentRow,
            culture: this.culture,
            fromPart: this.fromPart,
            layoutColumns: this.layoutColumns,
            navigation: this.navigation,
            quickSearch: this.quickSearch,
            quickSearchToken: this.quickSearchToken,
            search: this.search,
            searchFilterJoin: this.searchFilterJoin,
            searchParams: this.searchParams,
            totalRows: this.totalRows,
            primaryKey: this.primaryKey,
        };

        return request;
    }

    private configureToolbar(response: DbNetEditResponse) {
        if (response.toolbar) {
            const buttons = ["First", "Next", "Previous", "Last", "Cancel", "Apply", "Search"];
            buttons.forEach(btn =>
                this.addEventListener(`${btn}Btn`)
            )
        }

        const $navigationElements = this.controlElement("dbnetedit-toolbar").find(".navigation");
        const $noRecordsCell = this.controlElement("no-records-cell");
        this.setInputElement("Rows", response.totalRows);

        if (response.totalRows == 0) {
            $navigationElements.hide();
            $noRecordsCell.show();
        }
        else {
            $navigationElements.show();
            $noRecordsCell.hide();
            this.controlElement("dbnetgrid-toolbar").find(".navigation").show();
            this.setInputElement("RowNumber", response.currentRow);
            this.setInputElement("RowCount", response.totalRows);

            this.currentRow = response.currentRow;
            this.totalRows = response.totalRows;

            this.disable("FirstBtn", response.currentRow == 1);
            this.disable("PreviousBtn", response.currentRow == 1);
            this.disable("NextBtn", response.currentRow == response.totalRows);
            this.disable("LastBtn", response.currentRow == response.totalRows);
        }

        this.controlElement("QuickSearch").on("keyup", (event) => this.quickSearchKeyPress(event));
    }

    private updateColumns(response: DbNetEditResponse) {
        this.columns = new Array<EditColumn>();
        response.columns?.forEach((col) => {
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
                dataType: col.dataType,
                primaryKey: col.primaryKey,
                index: col.index,
                editControlType: col.editControlType,
                pattern: col.pattern
            } as unknown as EditColumnResponse;
            this.columns.push(new EditColumn(properties));
        });
    }

    private addEventListener(id: string, eventName = "click") {
        this.controlElement(id).on(eventName, (event) => this.handleClick(event));
    }

    private handleClick(event: JQuery.TriggeredEvent): void {
        const id = (event.target as Element).id;
        /*
        switch (id) {
            case this.controlElementId("FirstBtn"):
            case this.controlElementId("NextBtn"):
            case this.controlElementId("PreviousBtn"):
            case this.controlElementId("LastBtn"):
                if (this.hasUnappliedChanges()) {
                    this.messageBox("There are unapplied changes. Discard ?");

                }                    
                break;
            default:
                break;
        }
        */

        switch (id) {
            case this.controlElementId("FirstBtn"):
                this.currentRow = 1;
                break;
            case this.controlElementId("NextBtn"):
                this.currentRow++;
                break;
            case this.controlElementId("PreviousBtn"):
                this.currentRow--;
                break;
            case this.controlElementId("LastBtn"):
                this.currentRow = this.totalRows;
                break;
            case this.controlElementId("SearchBtn"):
            case this.controlElementId("CancelBtn"):
            case this.controlElementId("ApplyBtn"):
                break;
            default:
                return;
        }
        event.preventDefault();

        switch (id) {
            case this.controlElementId("SearchBtn"):
                this.openSearchDialog(this.getRequest());
                break;
            case this.controlElementId("ApplyBtn"):
                this.applyChanges();
                break;
            default:
                this.getRecord();
                break;
        }
    }

    private getRecord() {
        this.callServer("getrecord");
    }

    private applyChanges() {
        const changes: Dictionary<object> = {};
        let validForm = true;
        this.formPanel?.find(':input.dbnetedit,select.dbnetedit').each(
            function (index) {
                const $input = $(this);
                const name = $input.attr("name") as string;
                if ($input.attr("type") == "checkbox") {
                    if ($input.prop("checked") != $input.data("value")) {
                        changes[name] = $input.prop("checked");
                    }
                }
                else {
                    if ($input.attr("pattern")) {
                        const valid = ($input[0] as HTMLInputElement).reportValidity();
                        if (!valid) {
                            validForm = false;
                        }
                    }
                    if ($input.val() != $input.data("value")) {
                        changes[name] = $input.val() as object;
                    }
                }
            }
        );

        if ($.isEmptyObject(changes)) {
            return
        }
        if (validForm) {
            this.changes = changes;
            this.callServer("applychanges");
        }
    }

    private message(msg: string): void {
        this.formPanel?.find(".message").html(msg).addClass("highlight");
        setInterval(() => this.clearMessage(), 5000);
    }

    private clearMessage(): void {
        this.formPanel?.find(".message").html("&nbsp;").removeClass("highlight");
    }

    private selectDate(event: JQuery.TriggeredEvent): void {
        const $button = $(event.target as HTMLInputElement);
        $button.parent().find("input").datepicker("show");
    }

    private editLookup(event: JQuery.TriggeredEvent): void {
        const $button = $(event.target as HTMLInputElement);
        const $input = $button.parent().find("input");
        const request = this.getRequest();
        this.lookup($input, request);
    }

    private selectTime(event: JQuery.TriggeredEvent): void {
        const $button = $(event.target as HTMLInputElement);
        $button.parent().find("input").timepicker('open');
        event.stopPropagation();
    }
}