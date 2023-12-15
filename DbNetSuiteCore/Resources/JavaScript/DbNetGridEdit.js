"use strict";
class DbNetGridEdit extends DbNetSuite {
    constructor(id) {
        super(id);
        this.columnName = undefined;
        this._delete = false;
        this.fromPart = "";
        this.insert = false;
        this.fixedFilterParams = {};
        this.fixedFilterSql = "";
        this.maxImageHeight = 40;
        this.navigation = true;
        this.primaryKey = undefined;
        this.initialOrderBy = "";
        this.optimizeForLargeDataset = false;
        this.search = true;
        this.searchFilterJoin = "";
        this.searchParams = [];
        this.toolbarButtonStyle = ToolbarButtonStyle.Image;
        this.jsonKey = "";
        this.json = null;
        this.dataSourceType = DataSourceType.TableOrView;
        this.columns = [];
    }
    setColumnExpressions(...columnExpressions) {
        columnExpressions.forEach(columnExpression => {
            this.columns.push(this.newColumn(columnExpression, false));
        });
    }
    setColumnKeys(...columnKeys) {
        columnKeys.forEach((value, index) => {
            this.columns[index].columnKey = value;
        });
    }
    setColumnLabels(...labels) {
        let idx = 0;
        labels.forEach(label => {
            if (this.columns.length > idx) {
                this.columns[idx].label = label;
                idx++;
            }
        });
    }
    setColumnProperty(columnName, property, propertyValue) {
        if (columnName instanceof (Array)) {
            columnName.forEach(c => this.setColumnProperty(c, property, propertyValue));
            return;
        }
        let matchingColumn = this.columns.find((col) => { return this.matchingColumn(col, columnName); });
        if (matchingColumn == undefined) {
            matchingColumn = this.newColumn(columnName, true);
            this.columns.push(matchingColumn);
        }
        matchingColumn[property] = propertyValue;
    }
    setColumnProperties(columnName, properties) {
        Object.keys(properties).forEach((key) => {
            this.setColumnProperty(columnName, key, properties[key]);
        });
    }
    openSearchDialog(request) {
        if (this.searchDialog) {
            this.searchDialog.open();
            return;
        }
        this.post("search-dialog", request)
            .then((response) => {
            var _a;
            (_a = this.element) === null || _a === void 0 ? void 0 : _a.append(response.dialog);
            this.searchDialog = new SearchDialog(`${this.id}_search_dialog`, this);
            this.searchDialog.open();
        });
    }
    lookup($input, request) {
        var _a;
        $input.uniqueId();
        request.lookupColumnIndex = parseInt($input.attr("columnIndex"));
        if (this.lookupDialog && $input.attr("id") == ((_a = this.lookupDialog.$input) === null || _a === void 0 ? void 0 : _a.attr("id"))) {
            this.lookupDialog.open();
            return;
        }
        this.post("lookup", request)
            .then((response) => {
            var _a;
            if (!this.lookupDialog) {
                (_a = this.element) === null || _a === void 0 ? void 0 : _a.append(response.dialog);
                this.lookupDialog = new LookupDialog(`${this.id}_lookup_dialog`, this);
            }
            this.lookupDialog.update(response, $input);
            this.lookupDialog.open();
        });
    }
    newColumn(columnExpr, unmatched) {
        if (this.constructor.name == "DbNetGrid") {
            return new GridColumn({ columnExpression: columnExpr }, unmatched);
        }
        else {
            return new EditColumn({ columnExpression: columnExpr }, unmatched);
        }
    }
    matchingColumn(dbColumn, columnName) {
        var _a, _b, _c, _d, _e, _f, _g, _h;
        let match = false;
        if ((_a = dbColumn.columnKey) === null || _a === void 0 ? void 0 : _a.includes(".")) {
            match = ((_c = (_b = dbColumn.columnKey) === null || _b === void 0 ? void 0 : _b.split('.').pop()) === null || _c === void 0 ? void 0 : _c.toLowerCase()) == columnName.toLowerCase();
        }
        if (((_e = (_d = dbColumn.columnKey) === null || _d === void 0 ? void 0 : _d.split(' ').pop()) === null || _e === void 0 ? void 0 : _e.toLowerCase()) == columnName.toLowerCase()) {
            match = true;
        }
        if (((_g = (_f = dbColumn.columnExpression) === null || _f === void 0 ? void 0 : _f.split(' ').pop()) === null || _g === void 0 ? void 0 : _g.toLowerCase()) == columnName.toLowerCase()) {
            match = true;
        }
        if (!match) {
            match = ((_h = dbColumn.columnKey) === null || _h === void 0 ? void 0 : _h.toLowerCase()) == columnName.toLowerCase();
        }
        return match;
    }
    assignForeignKey(control, fk) {
        if (control instanceof DbNetGridEdit) {
            const gridEdit = control;
            const col = gridEdit.columns.find((col) => { return col.foreignKey == true; });
            if (col == undefined) {
                if (control.parentChildRelationship == "OneToMany") {
                    control.highlight();
                    this.error("A foreign key column has not been specified for the linked control");
                }
                return;
            }
            if (control instanceof DbNetEdit) {
                const edit = control;
                if (edit.initialised) {
                    edit.updateForeignKeyValue(fk);
                }
            }
            col.foreignKeyValue = fk ? fk : DbNetSuite.DBNull;
        }
    }
    baseRequest() {
        const request = this._getRequest();
        request.fromPart = this.fromPart;
        request.toolbarButtonStyle = this.toolbarButtonStyle;
        request.quickSearch = this.quickSearch;
        request.quickSearchToken = this.quickSearchToken;
        request.optimizeForLargeDataset = this.optimizeForLargeDataset;
        request.primaryKey = this.primaryKey;
        request.columnName = this.columnName;
        request.search = this.search;
        request.searchFilterJoin = this.searchFilterJoin;
        request.searchParams = this.searchParams;
        request.navigation = this.navigation;
        request.fixedFilterParams = this.fixedFilterParams;
        request.fixedFilterSql = this.fixedFilterSql;
        request.insert = this.insert;
        request.delete = this._delete;
        request.initialOrderBy = this.initialOrderBy;
        request.parentChildRelationship = this.parentChildRelationship;
        request.maxImageHeight = this.maxImageHeight;
        request.jsonKey = this.jsonKey;
        request.json = this.json;
        return request;
    }
    invokeOnJsonUpdated(editMode) {
        let updateArgs = {};
        if (this instanceof DbNetEdit) {
            const editControl = this;
            updateArgs = {
                primaryKey: this.primaryKey,
                editMode: editMode,
                changes: (editMode == EditMode.Delete) ? undefined : editControl.changes,
                formData: (editMode == EditMode.Delete) ? undefined : editControl.formData,
                columns: (editMode == EditMode.Delete) ? undefined : editControl.columns
            };
        }
        else {
            updateArgs = {
                primaryKey: this.primaryKey,
                editMode: editMode
            };
        }
        const eventName = "onJsonUpdated";
        if (this.eventHandlers[eventName]) {
            this.fireEvent(eventName, updateArgs);
        }
        else {
            this.error(`The <b>${eventName}</b> event handler has not been implemented.`);
        }
    }
    processJsonUpdateResponse(response) {
        var _a;
        if (response.success == false) {
            this.error((_a = response.message) !== null && _a !== void 0 ? _a : "An error has occurred");
            return;
        }
        if (this instanceof DbNetEdit) {
            const editControl = this;
            editControl.message(response.message);
            if (response.dataSet) {
                editControl.json = response.dataSet;
                if (editControl.browseControl) {
                    editControl.browseControl.json = response.dataSet;
                }
                editControl.sleep(1);
                if (this.isEditDialog) {
                    const grid = this.parentControl;
                    grid.json = response.dataSet;
                    grid.getPage();
                }
                else {
                    editControl.getRows();
                }
            }
        }
        else if (this instanceof DbNetGrid) {
            const gridControl = this;
            gridControl.json = response.dataSet;
            gridControl.getPage();
        }
    }
}
