"use strict";
class DbNetGridEdit extends DbNetSuite {
    constructor(id) {
        super(id);
        this.columnName = undefined;
        this._delete = false;
        this.fromPart = "";
        this.insert = false;
        this.maxImageHeight = 40;
        this.navigation = true;
        this.primaryKey = undefined;
        this.optimizeForLargeDataset = false;
        this.quickSearch = false;
        this.quickSearchDelay = 1000;
        this.quickSearchMinChars = 3;
        this.quickSearchToken = "";
        this.search = true;
        this.searchFilterJoin = "";
        this.searchParams = [];
        this.toolbarButtonStyle = ToolbarButtonStyle.Image;
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
    quickSearchKeyPress(event) {
        const el = event.target;
        window.clearTimeout(this.quickSearchTimerId);
        if (el.value.length >= this.quickSearchMinChars || el.value.length == 0 || event.key == 'Enter')
            this.quickSearchTimerId = window.setTimeout(() => { this.runQuickSearch(el.value); }, this.quickSearchDelay);
    }
    runQuickSearch(token) {
        this.quickSearchToken = token;
        if (this instanceof DbNetGrid) {
            const grid = this;
            grid.currentPage = 1;
            grid.getPage();
        }
        else if (this instanceof DbNetEdit) {
            const edit = this;
            edit.currentRow = 1;
            edit.getRows();
        }
    }
    lookup($input, request) {
        request.lookupColumnIndex = parseInt($input.attr("columnIndex"));
        if (this.lookupDialog && request.lookupColumnIndex == this.lookupDialog.columnIndex) {
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
}
