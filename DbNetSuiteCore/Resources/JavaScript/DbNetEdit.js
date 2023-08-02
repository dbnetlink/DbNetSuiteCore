"use strict";
class DbNetEdit extends DbNetGridEdit {
    constructor(id) {
        super(id);
        this.browseDialogId = "";
        this.changes = {};
        this.currentRow = 1;
        this.delete = false;
        this.editMode = "update";
        this.insert = false;
        this.layoutColumns = 1;
        this.search = true;
        this.totalRows = 0;
        this.isEditDialog = false;
        if (this.toolbarPosition == undefined) {
            this.toolbarPosition = "Bottom";
        }
        this.formData = new FormData();
    }
    initialize(primaryKey = null) {
        if (!this.element) {
            return;
        }
        this.element.empty();
        this.editPanel = this.addPanel("form");
        this.addLoadingPanel();
        if (primaryKey) {
            this.primaryKey = primaryKey;
        }
        this.post("initialize", this.getRequest())
            .then((response) => {
            if (response.error == false) {
                this.updateColumns(response);
                this.configureEdit(response);
                this.initialised = true;
                this.fireEvent("onInitialized");
            }
        });
        this.linkedControls.forEach((control) => {
            if (control.isBrowseDialog) {
                this.browseDialogControl = control;
            }
        });
    }
    addLinkedControl(control) {
        this.linkedControls.push(control);
    }
    getRows(callback) {
        this.callServer("search", callback);
    }
    columnValue(columnName) {
        this.formElements().filter(`:input[name='${columnName}']`).data("value");
    }
    clearForm() {
        this.formElements().each(function () {
            const $input = $(this);
            if ($input.attr("type") == "checkbox") {
                $input.prop("checked", false).data("value", false);
            }
            else {
                $input.val('').data("value", '');
            }
        });
        this.binaryElements().each(function () {
            const $img = $(this);
            $img.hide();
        });
    }
    disableForm(disable) {
        var _a, _b;
        if (!this.initialised) {
            return;
        }
        this.clearForm();
        this.formElements().not("[primarykey='true']").not("[foreignkey='true']").not("[datatype='Guid']").prop("disabled", disable);
        (_a = this.toolbarPanel) === null || _a === void 0 ? void 0 : _a.find("button").prop("disabled", disable);
        (_b = this.toolbarPanel) === null || _b === void 0 ? void 0 : _b.find("input.navigation").val("");
        this.disable("SearchBtn", this.parentControlType != '');
        this.disable("InsertBtn", this.parentControlType != '');
        this.disable("QuickSearch", this.parentControlType != '');
        if (disable) {
            this.configureLinkedControls(null, DbNetSuite.DBNull);
        }
    }
    configureEdit(response) {
        var _a, _b, _c, _d;
        if (response.form) {
            (_a = this.editPanel) === null || _a === void 0 ? void 0 : _a.html(response.form);
            this.formPanel = (_b = this.editPanel) === null || _b === void 0 ? void 0 : _b.find("table.dbnetedit-form");
            this.configureForm();
            const toolbarContainer = (_c = this.editPanel) === null || _c === void 0 ? void 0 : _c.find(".toolbar-container");
            this.toolbarPanel = this.addPanel("toolbar", toolbarContainer);
        }
        if (response.toolbar) {
            (_d = this.toolbarPanel) === null || _d === void 0 ? void 0 : _d.html(response.toolbar);
            this.configureToolbar(response);
        }
        this.updateForm(response);
    }
    configureForm() {
        var _a, _b, _c, _d, _e, _f, _g, _h;
        const $inputs = (_a = this.formPanel) === null || _a === void 0 ? void 0 : _a.find(`:input[name]`);
        for (let i = 0; i < $inputs.length; i++) {
            const $input = $($inputs[i]);
            this.fireEvent("onFormElementCreated", { formElement: $input[0], columnName: $input.attr("name") });
        }
        (_b = this.formPanel) === null || _b === void 0 ? void 0 : _b.find("[button-type='calendar']").on("click", (event) => this.selectDate(event));
        (_c = this.formPanel) === null || _c === void 0 ? void 0 : _c.find("[button-type='clock']").on("click", (event) => this.selectTime(event));
        (_d = this.formPanel) === null || _d === void 0 ? void 0 : _d.find("[button-type='lookup']").on("click", (event) => this.editLookup(event));
        (_e = this.formPanel) === null || _e === void 0 ? void 0 : _e.find("[button-type='delete']").on("click", (event) => this.deleteFile(event));
        (_f = this.formPanel) === null || _f === void 0 ? void 0 : _f.find("[button-type='upload']").on("click", (event) => this.uploadFile(event));
        (_g = this.formPanel) === null || _g === void 0 ? void 0 : _g.find("img.dbnetedit").on("load", () => { $(this).show(); });
        (_h = this.formPanel) === null || _h === void 0 ? void 0 : _h.find("input[datatype='DateTime'").get().forEach(e => {
            const $input = $(e);
            this.addDatePicker($input, this.datePickerOptions);
        });
    }
    updateForm(response) {
        var _a, _b;
        if (response.totalRows == 0) {
            this.disableForm(true);
            return;
        }
        (_a = this.formPanel) === null || _a === void 0 ? void 0 : _a.find(':input').not("[primarykey='true']").not("[foreignkey='true']").not("[datatype='Guid']").prop("disabled", false);
        const record = response.record;
        for (const columnName in record) {
            const $input = (_b = this.formPanel) === null || _b === void 0 ? void 0 : _b.find(`:input[name='${columnName}']`);
            if ($input.length == 0) {
                continue;
            }
            const value = record[columnName];
            if ($input.attr("type") == "checkbox") {
                const checked = (value === true);
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
        this.binaryElements().each(function () {
            const $img = $(this);
            $img.hide();
            self.downloadBinaryData($img);
        });
        const $firstElement = this.formElements().filter(':not(:disabled):first');
        $firstElement.trigger("focus");
        this.editMode = "update";
        this.primaryKey = response.primaryKey;
        this.formData = new FormData();
        if (this.browseDialog) {
            if (this.browseDialog.isOpen()) {
                this.browseDialog.selectRow(this.currentRow);
            }
        }
        if (response.message) {
            this.message(response.message);
        }
        this.configureLinkedControls(null, this.primaryKey);
        this.fireEvent("onRecordSelected", { formElements: this.formElements() });
    }
    callServer(action, callback) {
        this.post(action, this.getRequest())
            .then((response) => {
            if (response.error == false) {
                this.configureToolbar(response);
                this.updateForm(response);
            }
            if (callback) {
                callback(response);
            }
        });
    }
    getRequest() {
        const request = {
            changes: this.changes,
            componentId: this.id,
            connectionString: this.connectionString,
            columns: this.columns.map((column) => { return column; }),
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
            primaryKey: this.primaryKey,
            isEditDialog: this.isEditDialog,
            insert: this.insert,
            delete: this._delete,
            parentControlType: this.parentControlType,
            optimizeForLargeDataset: this.optimizeForLargeDataset,
            parentChildRelationship: this.parentChildRelationship,
            toolbarPosition: this.toolbarPosition
        };
        return request;
    }
    configureToolbar(response) {
        const $navigationElements = this.controlElement("dbnetedit-toolbar").find(".navigation");
        const $noRecordsCell = this.controlElement("no-records-cell");
        const buttons = this.isEditDialog ? ["Cancel", "Apply"] : ["First", "Next", "Previous", "Last", "Cancel", "Apply", "Search", "Insert", "Delete", "Browse"];
        if (response.toolbar) {
            buttons.forEach(btn => this.controlElement(`${btn}Btn`).on("click", (event) => this.handleClick(event)));
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
    updateColumns(response) {
        var _a;
        this.columns = new Array();
        (_a = response.columns) === null || _a === void 0 ? void 0 : _a.forEach((col) => {
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
            };
            this.columns.push(new EditColumn(properties));
        });
    }
    handleClick(event) {
        const id = event.target.id;
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
    configureLinkedControl(control, pk) {
        if (control instanceof DbNetEdit) {
            const edit = control;
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
            const grid = control;
            if (grid.isBrowseDialog == false) {
                this.assignForeignKey(grid, pk);
                grid.currentPage = 1;
                grid.initialised ? grid.getPage() : grid.initialize();
            }
        }
    }
    getRecord(primaryKey = null) {
        if (primaryKey) {
            this.primaryKey = primaryKey;
        }
        this.callServer("getrecord");
    }
    insertRecord() {
        this.clearForm();
        // eslint-disable-next-line @typescript-eslint/no-this-alias
        const self = this;
        this.formElements().each(function () {
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
                $input.val($input.attr("initialvalue"));
            }
        });
        const $firstElement = this.formElements().filter(':not(:disabled):first');
        $firstElement.trigger("focus");
        this.editMode = "insert";
        this.configureToolbarButtons(true);
        this.configureLinkedControls(null, DbNetSuite.DBNull);
        this.fireEvent("onInsertInitalize", { formElements: this.formElements() });
    }
    updateForeignKeyValue(fk) {
        this.formElements().filter("[foreignkey='true']").attr("initialvalue", fk === null || fk === void 0 ? void 0 : fk.toString());
    }
    configureToolbarButtons(insert) {
        const elements = this.controlElement("dbnetedit-toolbar").find(".navigation");
        const buttons = ["SearchBtn", "QuickSearch", "InsertBtn", "DeleteBtn", "BrowseBtn",];
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
    formElements() {
        var _a;
        return (_a = this.formPanel) === null || _a === void 0 ? void 0 : _a.find(':input.dbnetedit,select.dbnetedit');
    }
    binaryElements() {
        var _a;
        return (_a = this.formPanel) === null || _a === void 0 ? void 0 : _a.find('img,button.binary');
    }
    deleteRecord() {
        this.confirm("Please confirm deletion of the current record", this.formPanel, (buttonPressed) => this.deletionConfirmed(buttonPressed));
    }
    deletionConfirmed(buttonPressed) {
        if (buttonPressed != MessageBoxButtonType.Confirm) {
            return;
        }
        this.post("delete-record", this.getRequest())
            .then((response) => {
            if (response.error == false) {
                this.recordDeleted();
            }
            else {
                this.error(response.message);
            }
        });
    }
    recordDeleted() {
        this.message("Record deleted");
        this.getRows();
        this.fireEvent("onRecordDeleted");
    }
    applyChanges() {
        const changes = {};
        let validationMessage = null;
        const $formElements = this.formElements();
        $formElements.filter('[required]').each(function () {
            var _a;
            const $input = $(this);
            const value = (_a = $input.val()) === null || _a === void 0 ? void 0 : _a.toString().trim();
            if (value == '') {
                $input.addClass("highlight");
            }
        });
        if ($formElements.filter('.highlight').length > 0) {
            this.message('An entry in the highlighted field(s) is required');
            setTimeout(() => this.clearHighlightedFields(), 3000);
            return;
        }
        $formElements.filter('[pattern]').each(function () {
            const $input = $(this);
            const name = $input.attr("name");
            if (!this.reportValidity()) {
                validationMessage = { key: name, value: `Entry does not match the input pattern (${$input.attr("pattern")})` };
                return false;
            }
        });
        if (validationMessage != null) {
            this.message(validationMessage.value);
            this.highlightField(validationMessage.key.toLowerCase());
            return;
        }
        $formElements.each(function () {
            const $input = $(this);
            const name = $input.attr("name");
            if ($input.attr("type") == "checkbox") {
                if ($input.prop("checked") != $input.data("value")) {
                    changes[name] = $input.prop("checked");
                }
            }
            else {
                if ($input.val() != $input.data("value")) {
                    changes[name] = $input.val();
                }
            }
        });
        if ($.isEmptyObject(changes) == true && this.hasFormData() == false) {
            return;
        }
        this.changes = changes;
        if (this.hasFormData()) {
            this.post('save-files', this.formData)
                .then((response) => {
                this.submitChanges(response);
            });
        }
        else {
            this.submitChanges(null);
        }
    }
    submitChanges(response) {
        const request = this.getRequest();
        if (response) {
            request.formCacheKey = response.formCacheKey;
        }
        this.post(`${this.editMode}-record`, request)
            .then((response) => {
            this.applyChangesCallback(response);
        });
    }
    cancelChanges() {
        if (this.editMode == "insert") {
            this.getRows();
        }
        else {
            this.getRecord();
        }
    }
    applyChangesCallback(response) {
        if (response.validationMessage) {
            this.message(response.validationMessage.value);
            this.highlightField(response.validationMessage.key.toLowerCase());
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
    initBrowseDialog() {
        var _a, _b, _c, _d;
        if (!((_a = this.browseDialogControl) === null || _a === void 0 ? void 0 : _a.initialised)) {
            (_b = this.browseDialogControl) === null || _b === void 0 ? void 0 : _b.internalBind("onInitialized", () => this.openBrowseDialog());
            (_c = this.browseDialogControl) === null || _c === void 0 ? void 0 : _c.internalBind("onRowSelected", (sender, args) => this.browseDialogRowSelected(sender, args));
            (_d = this.browseDialogControl) === null || _d === void 0 ? void 0 : _d.initialize();
        }
        else {
            this.openBrowseDialog();
        }
    }
    openBrowseDialog() {
        var _a;
        if (!this.browseDialog) {
            this.browseDialog = new BrowseDialog(this.browseDialogId, this.browseDialogControl);
        }
        (_a = this.browseDialog) === null || _a === void 0 ? void 0 : _a.show(this.currentRow);
    }
    browseDialogRowSelected(sender, args) {
        if (args.row.rowIndex != this.currentRow) {
            this.currentRow = args.row.rowIndex;
            this.getRecord();
        }
    }
    message(msg) {
        var _a;
        (_a = this.editPanel) === null || _a === void 0 ? void 0 : _a.find(".message").html(msg).addClass("highlight");
        setTimeout(() => this.clearMessage(), 3000);
    }
    clearMessage() {
        var _a;
        (_a = this.editPanel) === null || _a === void 0 ? void 0 : _a.find(".message").html("&nbsp;").removeClass("highlight");
    }
    highlightField(columnName) {
        this.formElements().filter(`[name='${columnName}']`).addClass("highlight");
        setTimeout(() => this.clearHighlightedFields(), 3000);
    }
    clearHighlightedFields() {
        this.formElements().filter(".highlight").removeClass("highlight");
    }
    selectDate(event) {
        const $button = $(event.target);
        $button.parent().find("input").datepicker("show");
    }
    editLookup(event) {
        const $button = $(event.target);
        const $input = $button.parent().find("input");
        const request = this.getRequest();
        this.lookup($input, request);
    }
    uploadFile(event) {
        if (this.uploadDialog) {
            this.uploadDialog.show(event);
            return;
        }
        this.post("upload-dialog", this.getRequest())
            .then((response) => {
            var _a;
            (_a = this.element) === null || _a === void 0 ? void 0 : _a.append(response.dialog);
            this.uploadDialog = new UploadDialog(`${this.id}_upload_dialog`, this);
            this.uploadDialog.show(event);
        });
    }
    deleteFile(event) {
        const $img = $(event.currentTarget).closest("td").find("img");
        this.saveFile($img, null);
    }
    selectTime(event) {
        const $button = $(event.target);
        $button.parent().find("input").timepicker('open');
        event.stopPropagation();
    }
    uuid() {
        function getRandomSymbol(symbol) {
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
    downloadBinaryData($image) {
        this.columnName = $image.attr("name");
        this.post("download-column-data", this.getRequest(), true)
            .then((blob) => {
            if ($image) {
                console.log(`blob size => ${blob.size}`);
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
    saveFile($img, file) {
        file ? $img.show() : $img.hide();
        const columnName = $img.attr("name");
        if (this.formData.get(columnName) != null) {
            this.formData.delete(columnName);
        }
        if (file) {
            this.formData.append(columnName, file);
        }
        else {
            this.formData.append(columnName, new Blob());
        }
    }
    hasFormData() {
        let result = false;
        for (const key of this.formData.keys()) {
            result = true;
            break;
        }
        return result;
    }
}
