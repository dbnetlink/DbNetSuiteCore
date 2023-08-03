type DbNetEditResponseCallback = (response: DbNetEditResponse) => void;
type EditMode = "update" | "insert"

class DbNetEdit extends DbNetGridEdit {
    browseDialog: BrowseDialog | undefined;
    browseDialogControl: DbNetGrid | undefined;
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
                }
            })

        this.linkedControls.forEach((control) => {
            if ((control as DbNetGrid).isBrowseDialog) {
                this.browseDialogControl = (control as DbNetGrid);
            }
        });
    }

    addLinkedControl(control: DbNetSuite) {
        this.linkedControls.push(control);
    }

    getRows(callback?: DbNetEditResponseCallback) {
        this.callServer("search", callback);
    }

    columnValue(columnName: string) {
        this.formElements().filter(`:input[name='${columnName}']`).data("value");
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

    disableForm(disable: boolean) {
        if (!this.initialised) {
            return;
        }
        this.clearForm();
        this.formElements().not("[primarykey='true']").not("[foreignkey='true']").not("[datatype='Guid']").prop("disabled", disable);
        this.toolbarPanel?.find("button").prop("disabled", disable);
        this.toolbarPanel?.find("input.navigation").val("");

        this.disable("SearchBtn", this.parentControlType != '');
        this.disable("InsertBtn", this.parentControlType != '');
        this.disable("QuickSearch", this.parentControlType != '');

        if (disable) {
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
        this.formPanel?.find("[button-type='clock']").on("click", (event) => this.selectTime(event));
        this.formPanel?.find("[button-type='lookup']").on("click", (event) => this.editLookup(event));
        this.formPanel?.find("[button-type='delete']").on("click", (event) => this.deleteFile(event));
        this.formPanel?.find("[button-type='upload']").on("click", (event) => this.uploadFile(event));
        this.formPanel?.find("img.dbnetedit").on("load", () => { $(this).show(); });

        this.formPanel?.find("input[datatype='DateTime'").get().forEach(e => {
            const $input = $(e as HTMLInputElement);
            this.addDatePicker($input, this.datePickerOptions);
        });
    }

    private updateForm(response: DbNetEditResponse) {
        if (response.totalRows == 0) {
            this.disableForm(true);
            return;
        }
        this.formPanel?.find(':input').not("[primarykey='true']").not("[foreignkey='true']").not("[datatype='Guid']").prop("disabled", false);
        const record = response.record;
        for (const columnName in record) {
            const $input = this.formPanel?.find(`:input[name='${columnName}']`) as JQuery<HTMLFormElement>;
            if ($input.length == 0) {
                continue;
            }
            const value = record[columnName];
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
        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const self = this;
        this.binaryElements().each(
            function () {
                const $img = $(this);
                $img.hide();
                self.downloadBinaryData($img);
            });

        const $firstElement = this.formElements().filter(':not(:disabled):first') as JQuery<HTMLFormElement>;
        $firstElement.trigger("focus");

        this.editMode = "update";
        this.primaryKey = response.primaryKey as string;
        this.formData = new FormData();

        if (this.browseDialog) {
            if (this.browseDialog.isOpen()) {
                this.browseDialog.selectRow(this.currentRow)
            }
        }

        if (response.message) {
            this.message(response.message);
        }
        this.configureLinkedControls(null, this.primaryKey);

        this.fireEvent("onRecordSelected", { formElements: this.formElements() });
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
        const request: DbNetEditRequest = {
            changes: this.changes,
            componentId: this.id,
            connectionString: this.connectionString,
            columns: this.columns.map((column) => { return column as EditColumnRequest }),
            columnName: this.columnName,
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
            primaryKey: this.primaryKey as string,
            isEditDialog: this.isEditDialog,
            insert: this.insert,
            delete: this._delete,
            parentControlType: this.parentControlType,
            optimizeForLargeDataset: this.optimizeForLargeDataset,
            parentChildRelationship: this.parentChildRelationship,
            toolbarPosition: this.toolbarPosition,
            maxImageHeight: this.maxImageHeight
        };

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
        this.configureToolbarButtons(false);

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
        this.disable("DeleteBtn", response.totalRows == 0);
        this.disable("ApplyBtn", response.totalRows == 0);
        this.disable("CancelBtn", response.totalRows == 0);
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
                browse: col.browse
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
                this.initBrowseDialog();
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
                edit.disableForm(true);
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
            });

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

    private configureToolbarButtons(insert: boolean) {
        const elements = this.controlElement("dbnetedit-toolbar").find(".navigation");
        const buttons = ["SearchBtn", "QuickSearch", "InsertBtn", "DeleteBtn", "BrowseBtn",]
        const noRecords = this.controlElement("no-records-cell");

        insert ? elements.hide() : elements.show();
        insert ? noRecords.hide() : noRecords.show();

        for (let i = 0; i < buttons.length; i++) {
            const $btn = this.controlElement(buttons[i]);
            insert ? $btn.hide() : $btn.show();
        }

        if (insert) {
            this.controlElement("ApplyBtn").prop("disabled", false);
            this.controlElement("CancelBtn").prop("disabled", false);
        }
    }

    private formElements(): JQuery<HTMLFormElement> {
        return this.formPanel?.find(':input.dbnetedit,select.dbnetedit') as JQuery<HTMLFormElement>;
    }

    private binaryElements(): JQuery<HTMLElement> {
        return this.formPanel?.find('img,button.binary') as JQuery<HTMLElement>;
    }

    public deleteRecord() {
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

        $formElements.filter('[pattern]').each(
            function () {
                const $input = $(this) as JQuery<HTMLFormElement>;
                const name = $input.attr("name") as string;
                if (!(this as HTMLFormElement).reportValidity()) {
                    validationMessage = { key: name, value: `Entry does not match the input pattern (${$input.attr("pattern")})` } as ValidationMessage;
                    return false;
                }
            }
        );

        if (validationMessage != null) {
            this.message((validationMessage as ValidationMessage).value);
            this.highlightField((validationMessage as ValidationMessage).key.toLowerCase())
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

    private initBrowseDialog() {
        if (!this.browseDialogControl?.initialised) {
            this.browseDialogControl?.internalBind("onInitialized", () => this.openBrowseDialog());
            this.browseDialogControl?.internalBind("onRowSelected", (sender, args) => this.browseDialogRowSelected(sender, args));
            this.browseDialogControl?.initialize();
        }
        else {
            this.openBrowseDialog()
        }
    }

    private openBrowseDialog() {
        if (!this.browseDialog) {
            this.browseDialog = new BrowseDialog(this.browseDialogId as string, this.browseDialogControl as DbNetGrid);
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

    private highlightField(columnName: string): void {
        this.formElements().filter(`[name='${columnName}']`).addClass("highlight");
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

    private selectTime(event: JQuery.TriggeredEvent): void {
        const $button = $(event.target as HTMLInputElement);
        $button.parent().find("input").timepicker('open');
        event.stopPropagation();
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

    private downloadBinaryData($image: JQuery<HTMLElement>) {

        this.columnName = $image.attr("name") as string;

        this.post<Blob>("download-column-data", this.getRequest(), true)
            .then((blob) => {
                if ($image) {
                    console.log(`blob size => ${blob.size}`)
                    if (blob.size) {
                        $image.attr("src", window.URL.createObjectURL(blob));
                        $image.show();
                    }
                }
                else {
                    const link = document.createElement("a");
                    link.href = window.URL.createObjectURL(blob);
                    link.click();
                }
            });
    }

    public saveFile($img: JQuery<HTMLImageElement>, file: File | null, fileMetaData: FileMetaData | null = null) {
        file ? $img.show() : $img.hide();
        const columnName = $img.attr("name") as string;
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