type DbNetEditResponseCallback = (response: DbNetEditResponse) => void;
type EditMode = "update" | "insert"
type ValidationMessageType = "Native" | "Application"

class DbNetEdit extends DbNetGridEdit {
    browseControl: DbNetGrid | undefined;
    browseDialog: BrowseDialog | undefined;
    browseDialogId = "";
    changes: Dictionary<object> = {};
    currentRow = 1;
    delete = false;
    editMode: EditMode = "update";
    editPanel: JQuery<HTMLElement> | undefined;
    formPanel: JQuery<HTMLElement> | undefined;
    formData: FormData;
    insert = false;
    layoutColumns = 1;
    messagePanel: JQuery<HTMLElement> | undefined;
    search = true;
    totalRows = 0;
    isEditDialog = false;
    uploadDialog: UploadDialog | undefined;
    validationMessageType: ValidationMessageType = "Application";

    constructor(id: string) {
        super(id);
        if (this.toolbarPosition == undefined) {
            this.toolbarPosition = "Bottom";
        }
        this.maxImageHeight = 100;
        this.formData = new FormData();
    }

    initialize(primaryKey: string | null = null): void {
        if (!this.element) {
            return;
        }
        this.element.empty();
        this.editPanel = this.addPanel("form");
        this.addLoadingPanel();
        if (primaryKey) {
            this.primaryKey = primaryKey;
        }
        this.post<DbNetEditResponse>("initialize", this.getRequest())
            .then((response) => {
                if (response.error == false) {
                    this.updateColumns(response);
                    this.configureEdit(response);
                    this.initialised = true;
                    this.fireEvent("onInitialized");
                    if (response.message) {
                        this.message(response.message);
                    }
                }
            })

        this.linkedControls.forEach((control) => {
            if ((control as DbNetGrid).isBrowseDialog) {
                this.browseControl = (control as DbNetGrid);
                this.browseControl.internalBind("onRowSelected", (sender, args) => this.browseDialogRowSelected(sender, args));
                this.browseControl.internalBind("onPageLoaded", () => this.browseControlReloaded());
                this.browseControl.initialize();
            }
        });
    }

    getRows(callback?: DbNetEditResponseCallback) {
        this.post<DbNetEditResponse>("search", this.getRequest())
            .then((response) => {
                if (response.error == false) {
                    this.configureToolbar(response);
                    this.updateForm(response);
                    this.updateBrowseControl();
                }

                if (callback) {
                    callback(response);
                }
            })
    }

    columnValue(columnName: string) {
        return this.formElements().filter(`:input[name='${columnName}']`).data("value");
    }

    setColumnValue(columnName: string, value: string) {
        this.formElements().filter(`:input[name='${columnName}']`).val(value);
    }

    private clearForm() {
        this.formElements().each(
            function () {
                const $input = $(this);
                if ($input.attr("type") == "checkbox") {
                    $input.prop("checked", false).data("value", false);
                }
                else {
                    $input.val('').data("value", '');
                }
            });
        this.binaryElements().each(
            function () {
                const $img = $(this);
                $img.hide();
            });

    }

    disableForm() {
        this.clearForm();
        this.formPanel?.find("button").prop("disabled", true);
        this.formElements().not("[primarykey='true']").not("[foreignkey='true']").not("[datatype='Guid']").prop("disabled", true);
        this.toolbarPanel?.find("button").prop("disabled", true);
        this.toolbarPanel?.find("input.toolbar-info").val("");

        this.disable("SearchBtn", this.parentControlType != '');
        this.disable("InsertBtn", this.parentControlType != '');
        this.disable("QuickSearch", this.parentControlType != '');

        if (this.initialised) {
            this.configureLinkedControls(null, DbNetSuite.DBNull);
        }
    }

    private configureEdit(response: DbNetEditResponse) {
        if (response.form) {
            this.editPanel?.html(response.form);
            this.formPanel = this.editPanel?.find("table.dbnetedit-form")
            this.configureForm();
            const toolbarContainer = this.editPanel?.find(".toolbar-container");
            this.toolbarPanel = this.addPanel("toolbar", toolbarContainer);
        }
        if (response.toolbar) {
            this.toolbarPanel?.html(response.toolbar);
            this.configureToolbar(response);
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
        this.formPanel?.find("[button-type='lookup']").on("click", (event) => this.editLookup(event));
        this.formPanel?.find("[button-type='delete']").on("click", (event) => this.deleteFile(event));
        this.formPanel?.find("[button-type='upload']").on("click", (event) => this.uploadFile(event));
        this.formPanel?.find("img.dbnetedit").on("load", (event) => this.imageLoaded(event));
        this.formPanel?.find("img.dbnetedit").on("click", (event) => this.viewImage(event));
        this.formPanel?.find("select[dependentLookup]").on("change", (event) => this.updateOptions(event));
        this.formPanel?.find("input[texttransform]").on("input", (event) => this.textTransform(event));

        this.formPanel?.find("input[datatype='DateTime'").get().forEach(e => {
            const $input = $(e as HTMLInputElement);
            this.addDatePicker($input, this.datePickerOptions);
        });

    }

    private imageLoaded(event: JQuery.TriggeredEvent) {
        const $img = $(event.currentTarget);
        $img.show();
        if (this.imageViewer?.isOpen()) {
            $img.trigger("click");
        }
    }

    private updateForm(response: DbNetEditResponse) {
        if (response.totalRows == 0) {
            this.disableForm();
            return;
        }
        this.formPanel?.find(':input').not("[primarykey='true']").not("[foreignkey='true']").not("[datatype='Guid']").prop("disabled", false);
        this.formPanel?.find("button").prop("disabled", false);
        const record = response.record;
        for (const columnName in record) {
            const $input = this.formPanel?.find(`:input[name='${columnName}']`) as JQuery<HTMLFormElement>;
            if ($input.length == 0) {
                continue;
            }

            const value = record[columnName];

            if ($input.attr("isDependentLookup")) {
                $input.empty();
                $input.data("value", value.toString());
                continue;
            }

            if ($input.attr("type") == "checkbox") {
                const checked = (value as unknown as boolean === true)
                $input.prop("checked", checked).data("value", checked);
            }
            else {
                $input.val(value.toString()).data("value", value.toString());
            }

            if ($input.attr("primarykey") || $input.attr("foreignkey") || $input.attr("datatype") == "Guid") {
                $input.prop("disabled", true);
            }
        }


        for (let i = 0; i < this.formElements().length; i++) {
            const $input = this.formElements().eq(i);
            if ($input.attr("dependentLookup") && !$input.attr("isDependentLookup")) {
                if ($input.val() != "") {
                    this.refreshOptions($input.attr("dependentLookup") as string, $input.val() as string);
                }
            }
        }

        const outputTypes = ['range', 'color'];
        for (let i = 0; i < outputTypes.length; i++) {
            this.formElements().filter(`:input[type='${outputTypes[i]}']`).trigger("change");
        }
        const $firstElement = this.formElements().filter(':not(:disabled):first') as JQuery<HTMLFormElement>;
        $firstElement.trigger("focus");

        this.editMode = "update";
        this.primaryKey = response.primaryKey as string;
        this.formData = new FormData();

        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const self = this;
        this.binaryElements().each(
            function () {
                const $element = $(this);
                self.configureBinaryData($element, record as Dictionary<object>);
            });

        if (this.browseDialog) {
            if (this.browseDialog.isOpen()) {
                this.browseDialog.selectRow(this.currentRow)
            }
        }

        if (response.message) {
            this.message(response.message);
        }
        this.configureLinkedControls(null, this.primaryKey);

        this.fireEvent("onRecordSelected", { formElements: this.formElements(), binaryElements: this.binaryElements() });
    }

    private updateOptions(event: JQuery.TriggeredEvent): void {
        const $select = $(event.target as HTMLSelectElement);
        const columnName = $select.attr("dependentLookup") as string;
        const $dependentLookup = this.formPanel?.find(`:input[name='${columnName.toLowerCase()}']`) as JQuery<HTMLFormElement>;
        $dependentLookup.data("value", "");
        this.refreshOptions(columnName, $select.val() as string);
    }

    private textTransform(event: JQuery.TriggeredEvent): void {
        const input = event.target;
        const p = input.selectionStart;
        switch ($(input).attr("texttransform")) {
            case "Uppercase":
                input.value = input.value.toUpperCase();
                break;
            case "Lowercase":
                input.value = input.value.toLowerCase();
                break;
            case "Capitalize":
                input.value = this.toTitleCase(input.value);
                break;
        }
        input.setSelectionRange(p, p);
    }

    private toTitleCase(phrase: string)
    {
        return phrase.toLowerCase().split(' ').map((word: string) => word.charAt(0).toUpperCase() + word.slice(1)).join(' ');
    }

    private refreshOptions(columnName: string, parameterValue: string) {
        const $dependentLookup = this.formPanel?.find(`:input[name='${columnName.toLowerCase()}']`) as JQuery<HTMLFormElement>;
        const request = this.getRequest();
        request.lookupColumnIndex = parseInt($dependentLookup.attr("columnIndex") as string);
        request.lookupParameterValue = parameterValue;

        this.post<DbNetEditResponse>("get-options", request)
            .then((response) => {
                $dependentLookup.html(response.html);
                $dependentLookup.val($dependentLookup.data("value"));

                if ($dependentLookup.attr("dependentLookup") != null) {
                    columnName = $dependentLookup.attr("dependentLookup") as string;
                    parameterValue = $dependentLookup.data("value");
                    this.refreshOptions(columnName, parameterValue);
                }
            });
    }

    private callServer(action: string, callback?: DbNetEditResponseCallback) {
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
        const request = this.baseRequest() as DbNetEditRequest;

        request.changes = this.changes;
        request.columns = this.columns.map((column) => { return column as EditColumnRequest });
        request.currentRow = this.currentRow;
        request.layoutColumns = this.layoutColumns;
        request.totalRows = this.totalRows;
        request.isEditDialog = this.isEditDialog;
        request.toolbarPosition = this.toolbarPosition;
        return request;
    }

    private configureToolbar(response: DbNetEditResponse) {
        const $navigationElements = this.controlElement("dbnetedit-toolbar").find(".navigation");
        const $noRecordsCell = this.controlElement("no-records-cell");
        const buttons = this.isEditDialog ? ["Cancel", "Apply"] : ["First", "Next", "Previous", "Last", "Cancel", "Apply", "Search", "Insert", "Delete", "Browse"];

        if (response.toolbar) {
            buttons.forEach(btn =>
                this.controlElement(`${btn}Btn`).on("click", (event) => this.handleClick(event))
            );
            this.controlElement("QuickSearch").on("keyup", (event) => this.quickSearchKeyPress(event));
        }

        this.setInputElement("Rows", response.totalRows);
        this.configureToolbarButtons(false, response.totalRows);

        if (this.parentControlType && this.parentChildRelationship == "OneToOne") {
            $navigationElements.show();
            $noRecordsCell.hide();
            return;
        }

        if (response.totalRows == 0) {
            $navigationElements.hide();
            $noRecordsCell.show();
        }
        else {
            $navigationElements.show();
            $noRecordsCell.hide();
        }

        this.controlElement("dbnetgrid-toolbar").find(".navigation").show();
        this.setInputElement("RowNumber", response.currentRow);
        this.setInputElement("RowCount", response.totalRows);

        this.currentRow = response.currentRow;
        this.totalRows = response.totalRows;

        this.disable("SearchBtn", false);
        this.disable("InsertBtn", false);
        this.disable("QuickSearch", false);
        this.disable("FirstBtn", response.currentRow == 1);
        this.disable("PreviousBtn", response.currentRow == 1);
        this.disable("NextBtn", response.currentRow == response.totalRows);
        this.disable("LastBtn", response.currentRow == response.totalRows);
        this.disable("BrowseBtn", response.totalRows < 2);

        if (this.linkedGridOrEdit() == false) {
            this.disable("DeleteBtn", response.totalRows == 0);
        }

        this.disable("DeleteBtn", response.totalRows == 0);
        this.disable("ApplyBtn", response.totalRows == 0);
        this.disable("CancelBtn", response.totalRows == 0);

        if (this.parentGridOrEdit()) {
            this.configureParentDeleteButton(response.totalRows > 0);
        }
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
                pattern: col.pattern,
                browse: col.browse,
                search: col.search,
                autoIncrement: col.autoIncrement,
                annotation: col.annotation,
                placeholder: col.placeholder,
                inputValidation: col.inputValidation,
                textTransform: col.textTransform
            } as unknown as EditColumnResponse;
            this.columns.push(new EditColumn(properties));
        });
    }

    private handleClick(event: JQuery.TriggeredEvent): void {
        const id = (event.target as Element).id;

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
        }
        event.preventDefault();

        switch (id) {
            case this.controlElementId("SearchBtn"):
                this.openSearchDialog(this.getRequest());
                break;
            case this.controlElementId("BrowseBtn"):
                this.openBrowseDialog();
                break;
            case this.controlElementId("ApplyBtn"):
                this.applyChanges();
                break;
            case this.controlElementId("CancelBtn"):
                this.cancelChanges();
                break;
            case this.controlElementId("InsertBtn"):
                this.insertRecord();
                break;
            case this.controlElementId("DeleteBtn"):
                this.deleteRecord();
                break;
            default:
                this.getRecord();
                break;
        }
    }

    public configureLinkedControl(control: DbNetSuite, pk: string | null) {
        if (control instanceof DbNetEdit) {
            const edit = control as DbNetEdit;
            if (pk == DbNetSuite.DBNull) {
                edit.disableForm();
                return;
            }
            if (edit.parentChildRelationship == "OneToMany") {
                this.assignForeignKey(control, pk);
                pk = null;
            }

            edit.initialised ? edit.getRows() : edit.initialize(pk);
        }

        if (control instanceof DbNetGrid) {
            const grid = control as DbNetGrid;
            if (grid.isBrowseDialog == false) {
                this.assignForeignKey(grid, pk);
                grid.currentPage = 1;
                grid.initialised ? grid.getPage() : grid.initialize();
            }
        }
    }

    public getRecord(primaryKey: string | null = null) {
        if (primaryKey) {
            this.primaryKey = primaryKey;
        }
        this.callServer("getrecord");
    }

    public insertRecord() {
        this.clearForm();
        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const self = this;
        this.formElements().each(
            function () {
                const $input = $(this);
                $input.prop("disabled", false);
                if ($input.attr("primarykey")) {
                    if ($input.attr("autoincrement")) {
                        $input.prop("disabled", true);
                    }
                }
                if ($input.attr("foreignkey")) {
                    $input.prop("disabled", true);
                }
                if ($input.attr("datatype") == "Guid" && $input.attr("required")) {
                    $input.prop("disabled", true);
                    $input.val(self.uuid());
                }

                if ($input.attr("initialvalue")) {
                    $input.val($input.attr("initialvalue") as string);
                }

                if ($input.attr("isDependentLookup")) {
                    $input.empty();
                }
            });

        this.formPanel?.find("button").prop("disabled", false);

        const $firstElement = this.formElements().filter(':not(:disabled):first') as JQuery<HTMLFormElement>;
        $firstElement.trigger("focus");
        this.editMode = "insert";
        this.configureToolbarButtons(true);
        this.configureLinkedControls(null, DbNetSuite.DBNull);
        this.fireEvent("onInsertInitalize", { formElements: this.formElements() });
    }

    updateForeignKeyValue(fk: object | string) {
        this.formElements().filter("[foreignkey='true']").attr("initialvalue", fk?.toString())
    }

    private configureToolbarButtons(insert: boolean, totalRows = 0) {
        const elements = this.controlElement("dbnetedit-toolbar").find(".navigation");
        const buttons = ["SearchBtn", "QuickSearch", "InsertBtn", "DeleteBtn", "BrowseBtn",]
        const noRecords = this.controlElement("no-records-cell");

        insert ? elements.hide() : elements.show();
        insert ? noRecords.hide() : noRecords.show();

        for (let i = 0; i < buttons.length; i++) {
            const $btn = this.controlElement(buttons[i]);
            insert ? $btn.hide() : $btn.show();
        }

        const disabled = !insert && totalRows == 0

        this.controlElement("ApplyBtn").prop("disabled", disabled);
        this.controlElement("CancelBtn").prop("disabled", disabled);
    }

    private formElements(): JQuery<HTMLFormElement> {
        return this.formPanel?.find(':input.dbnetedit,select.dbnetedit,textarea.dbnetedit') as JQuery<HTMLFormElement>;
    }

    private binaryElements(): JQuery<HTMLElement> {
        return this.formPanel?.find('img.dbnetedit,a.dbnetedit') as JQuery<HTMLElement>;
    }

    private primaryKeyCheck() {
        if (this.primaryKey == null) {
            this.error("A primary key has not been included in the edit columns");
            return false;
        }
        return true;
    }

    public deleteRecord() {
        if (!this.primaryKeyCheck()) {
            return;
        }

        this.confirm("Please confirm deletion of the current record", this.formPanel as JQuery<HTMLElement>, (buttonPressed: MessageBoxButtonType) => this.deletionConfirmed(buttonPressed));
    }

    public deletionConfirmed(buttonPressed: MessageBoxButtonType) {
        if (buttonPressed != MessageBoxButtonType.Confirm) {
            return;
        }

        this.post<DbNetSuiteResponse>("delete-record", this.getRequest())
            .then((response) => {
                if (response.error == false) {
                    this.recordDeleted();
                }
                else {
                    this.error(response.message);
                }
            })
    }

    private recordDeleted(): void {
        this.message("Record deleted");
        this.getRows();
        this.fireEvent("onRecordDeleted");
    }

    private applyChanges() {
        if (this.editMode == "update" && !this.primaryKeyCheck()) {
            return;
        }
        const changes: Dictionary<object> = {};
        let validationMessage: ValidationMessage | null = null;

        const $formElements = this.formElements();
        $formElements.filter('[required]').each(
            function () {
                const $input = $(this) as JQuery<HTMLFormElement>;
                const value = $input.val()?.toString().trim()
                if (value == '') {
                    $input.addClass("highlight");
                }
            });

        if ($formElements.filter('.highlight').length > 0) {
            this.message('An entry in the highlighted field(s) is required');
            setTimeout(() => this.clearHighlightedFields(), 3000);
            return;
        }

        $formElements.each(
            function () {
                const input = this as HTMLFormElement;
                const $input = $(input);
                const name = $input.attr("name") as string;
                if (!input.checkValidity()) {
                    validationMessage = { key: name, value: input.validationMessage } as ValidationMessage;
                    return false;
                }
            }
        );

        if (validationMessage != null) {
            this.fireEvent("onFormElementValidationFailed", validationMessage);
            const columnName = (validationMessage as ValidationMessage).key.toLowerCase();
            if (this.validationMessageType == "Native") {
                (this.formElement(columnName).get(0) as HTMLInputElement).reportValidity();
            }
            else {
                this.message((validationMessage as ValidationMessage).value);
                this.highlightField(columnName)
            }

            return;
        }

        $formElements.each(
            function () {
                const $input = $(this) as JQuery<HTMLFormElement>;
                const name = $input.attr("name") as string;

                if ($input.attr("type") == "checkbox") {
                    if ($input.prop("checked") != $input.data("value")) {
                        changes[name] = $input.prop("checked");
                    }
                }
                else {
                    if ($input.val() != $input.data("value")) {
                        changes[name] = $input.val() as object;
                    }
                }
            });

        if ($.isEmptyObject(changes) == true && this.hasFormData() == false) {
            return;
        }

        this.changes = changes;

        if (this.hasFormData()) {
            this.post<DbNetEditResponse>('save-files', this.formData)
                .then((response) => {
                    this.submitChanges(response);
                })
        }
        else {
            this.submitChanges(null);
        }
    }

    private submitChanges(response: DbNetEditResponse | null) {
        const request = this.getRequest();

        if (response) {
            request.formCacheKey = response.formCacheKey;
        }

        this.post<DbNetEditResponse>(`${this.editMode}-record`, request)
            .then((response) => {
                this.applyChangesCallback(response);
            })
    }

    private cancelChanges() {
        if (this.editMode == "insert") {
            this.getRows();
        }
        else {
            this.getRecord();
        }
    }

    private applyChangesCallback(response: DbNetEditResponse): void {
        if (response.validationMessage) {
            this.message(response.validationMessage.value);
            this.highlightField(response.validationMessage.key.toLowerCase())
            return;
        }

        if (response.error) {
            this.error(response.message);
            return;
        }
        this.message(response.message);

        if (this.editMode == "update") {
            this.updateForm(response);
            this.fireEvent("onRecordUpdated");
        }
        else {
            if (this.isEditDialog == false) {
                this.getRows();
            }
            this.fireEvent("onRecordInserted");
        }
    }

    private updateBrowseControl() {
        if (!this.browseControl || this.browseControl.initialised == false) {
            return;
        }

        this.browseControl.quickSearchToken = this.quickSearchToken;
        this.browseControl.searchParams = this.searchParams;
        this.browseControl.reload();
    }

    public browseControlReloaded() {
        if (this.browseDialog?.isOpen()) {
            this.browseDialog.selectRow(this.currentRow);
        }
    }

    private openBrowseDialog() {
        if (!this.browseDialog) {
            this.browseDialog = new BrowseDialog(this.browseDialogId as string, this.browseControl as DbNetGrid);
        }
        this.browseDialog?.show(this.currentRow);
    }

    private browseDialogRowSelected(sender: DbNetSuite, args: any) {
        if (args.row.rowIndex != this.currentRow) {
            this.currentRow = args.row.rowIndex;
            this.getRecord();
        }
    }

    private message(msg: string): void {
        this.editPanel?.find(".message").html(msg).addClass("highlight");
        setTimeout(() => this.clearMessage(), 3000);
    }

    private clearMessage(): void {
        this.editPanel?.find(".message").html("&nbsp;").removeClass("highlight");
    }

    private formElement(columnName: string): JQuery<HTMLElement> {
        return this.formElements().filter(`[name='${columnName}']`);
    }

    private highlightField(columnName: string): void {
        this.formElement(columnName).addClass("highlight");
        setTimeout(() => this.clearHighlightedFields(), 3000);
    }

    private clearHighlightedFields(): void {
        this.formElements().filter(".highlight").removeClass("highlight");
    }
    private selectDate(event: JQuery.TriggeredEvent): void {
        const $button = $(event.target as HTMLInputElement);
        const $input = $button.parent().find("input");

        if ($input.attr("readonly") || $input.prop("disabled") == true) {
            return;
        }
        $button.parent().find("input").datepicker("show");
    }

    private editLookup(event: JQuery.TriggeredEvent): void {
        const $button = $(event.target as HTMLInputElement);
        const $input = $button.parent().find("input");
        const request = this.getRequest();
        this.lookup($input, request);
    }

    protected uploadFile(event: JQuery.TriggeredEvent) {
        if (this.uploadDialog) {
            this.uploadDialog.show(event);
            return;
        }

        this.post<DbNetSuiteResponse>("upload-dialog", this.getRequest())
            .then((response) => {
                this.element?.append(response.dialog);
                this.uploadDialog = new UploadDialog(`${this.id}_upload_dialog`, this);
                this.uploadDialog.show(event);
            });
    }

    protected deleteFile(event: JQuery.TriggeredEvent) {
        const $img = ($(event.currentTarget) as JQuery<HTMLButtonElement>).closest("td").find("img");
        this.saveFile($img, null);
    }

    private uuid() {
        function getRandomSymbol(symbol: string) {
            let array;

            if (symbol === 'y') {
                array = ['8', '9', 'a', 'b'];
                return array[Math.floor(Math.random() * array.length)];
            }

            array = new Uint8Array(1);
            window.crypto.getRandomValues(array);
            return (array[0] % 16).toString(16);
        }

        return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, getRandomSymbol);
    }

    private configureBinaryData($element: JQuery<HTMLElement>, record: Dictionary<object>) {
        const columnName = $element.attr("name") as string;
        const size = record[columnName];

        if (size.toString().replace("0", "") == "") {
            $element.hide();
            return;
        }

        let fileName = this.getFileName(columnName) as string | null;

        if (!fileName) {
            fileName = "";
        }

        const args = {
            element: $element.get(0),
            fileName: fileName,
            columnName: this.columnName,
            record: record
        }

        this.fireEvent("onConfigureBinaryData", args);

        $element.data("filename", args.fileName);

        if ($element.attr("isimage") == "true") {
            this.columnName = columnName;
            this.post<Blob>("download-column-data", this.getRequest(), true)
                .then((blob) => {
                    if (blob.size) {
                        $element.attr("src", window.URL.createObjectURL(blob));
                        $element.show();
                    }

                });
        }
        else {
            $element.attr("href", "javascript:void(0)");
            $element.text("Download");
            $element.show();
            $element.off().on("click", (event) => this.downloadFile(event));
        }
    }

    private getFileName(columnName: string) {
        return this.formElements().filter(`[uploadmetadatacolumn='${columnName}'][uploadmetadata='FileName']`).val();
    }

    private downloadFile(event: JQuery.ClickEvent<HTMLElement>) {
        const $element = $(event.currentTarget);
        this.columnName = $element.attr("name") as string;
        let fileName = $element.data("filename");
        if (!fileName) {
            const ext = ($element.attr("extensions") as string).split(",")[0]
            fileName = `download${ext}`;
        }
        this.post<Blob>("download-column-data", this.getRequest(), true)
            .then((blob) => {
                if (blob.size) {
                    $element.off();
                    const link = $element.get(0);
                    link.href = window.URL.createObjectURL(blob);
                    link.download = fileName;
                    link.click();
                }
            });
    }

    public saveFile($element: JQuery<HTMLElement>, file: File | null, fileMetaData: FileMetaData | null = null) {
        if ($element.prop("tagName") == "A") {
            $element.text(fileMetaData?.fileName as string);
            $element.attr("href", null);
        }
        file ? $element.show() : $element.hide();

        const columnName = $element.attr("name") as string;
        if (this.formData.get(columnName) != null) {
            this.formData.delete(columnName);
        }
        if (file) {
            this.formData.append(columnName, file);
        }
        else {
            this.formData.append(columnName, new Blob());
        }

        const meteDataElements = this.formElements().filter(`:input[uploadmetadatacolumn='${columnName}']`);

        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const self = this;
        meteDataElements.each(
            function () {
                const $input = $(this);
                if (!file) {
                    $input.val('');
                    return;
                }
                switch ($input.attr("uploadmetadata")) {
                    case "FileName":
                        $input.val(fileMetaData?.fileName as string);
                        break;
                    case "Size":
                        $input.val(fileMetaData?.size as number);
                        break;
                    case "LastModified":
                        self.applyLastModified($input as JQuery<HTMLInputElement>, fileMetaData?.lastModified as Date);
                        break;
                    case "ContentType":
                        $input.val(fileMetaData?.contentType as string);
                        break;
                }
            });

        this.fireEvent("onFileSelected", { element: $element, fileMetaData: fileMetaData as FileMetaData });
    }

    private applyLastModified($input: JQuery<HTMLInputElement>, lastModified: Date) {
        const request = this.getRequest();
        request.javascriptDate = lastModified;
        request.columnName = $input.attr("name");

        this.post<DbNetEditResponse>("convert-date", request)
            .then((response) => {
                $input.val(response.convertedDate);
            })
    }

    private hasFormData(): boolean {
        let result = false;
        for (const key of this.formData.keys()) {
            result = true;
            break;
        }
        return result;
    }
}