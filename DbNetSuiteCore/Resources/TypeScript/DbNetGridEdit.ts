type ParentChildRelationship = "OneToOne" | "OneToMany" | null

class DbNetGridEdit extends DbNetSuite {
    columnName: string | undefined = undefined;
    columns: DbColumn[];
    _delete = false;
    fromPart = "";
    insert = false;
    lookupDialog: LookupDialog | undefined;
    fixedFilterParams: Dictionary<object> = {};
    fixedFilterSql = "";
    maxImageHeight = 40;
    navigation = true;
    primaryKey: string | undefined = undefined;
    initialOrderBy = "";
    optimizeForLargeDataset = false;
    quickSearch = false;
    quickSearchDelay = 1000;
    quickSearchMinChars = 3;
    quickSearchTimerId: number | undefined;
    quickSearchToken = "";
    search = true;
    searchDialog: SearchDialog | undefined;
    searchFilterJoin = "";
    searchParams: Array<SearchParam> = [];
    toolbarButtonStyle: ToolbarButtonStyle = ToolbarButtonStyle.Image;
    toolbarPanel: JQuery<HTMLElement> | undefined;
    toolbarPosition: ToolbarPosition;
    constructor(id: string) {
        super(id);
        this.columns = [];
    }

    setColumnExpressions(...columnExpressions: string[]): void {
        columnExpressions.forEach(columnExpression => {
            this.columns.push(this.newColumn(columnExpression, false))
         });
    }

    setColumnKeys(...columnKeys: string[]): void {
        columnKeys.forEach((value, index) => {
            this.columns[index].columnKey = value;
        });
    }

    setColumnLabels(...labels: string[]): void {
        let idx = 0;
        labels.forEach(label => {
            if (this.columns.length > idx) {
                this.columns[idx].label = label;
                idx++;
            }
        });
    }

    setColumnProperty(columnName: string | Array<string>, property: string, propertyValue: object): void {
        if (columnName instanceof Array<string>) {
            columnName.forEach(c => this.setColumnProperty(c, property, propertyValue));
            return;
        }
        let matchingColumn = this.columns.find((col) => { return this.matchingColumn(col, columnName) });

        if (matchingColumn == undefined) {
            matchingColumn = this.newColumn(columnName, true);
            this.columns.push(matchingColumn);
        }

        (matchingColumn as any)[property] = propertyValue;
    }

    setColumnProperties(columnName: string, properties: object) {
        Object.keys(properties).forEach((key: string) => {
            this.setColumnProperty(columnName, key, properties[key]);
        });
    }

    protected openSearchDialog(request:DbNetGridEditRequest) {
        if (this.searchDialog) {
            this.searchDialog.open();
            return;
        }

        this.post<DbNetSuiteResponse>("search-dialog", request)
            .then((response) => {
                this.element?.append(response.dialog);
                this.searchDialog = new SearchDialog(`${this.id}_search_dialog`, this);
                this.searchDialog.open();
            });
    }

    protected quickSearchKeyPress(event: JQuery.TriggeredEvent): void {
        const el = event.target as HTMLInputElement;
        window.clearTimeout(this.quickSearchTimerId);

        if (el.value.length >= this.quickSearchMinChars || el.value.length == 0 || event.key == 'Enter')
            this.quickSearchTimerId = window.setTimeout(() => { this.runQuickSearch(el.value) }, this.quickSearchDelay);
    }

    private runQuickSearch(token: string) {
        this.quickSearchToken = token;
        if (this instanceof DbNetGrid) {
            const grid = this as DbNetGrid;
            grid.currentPage = 1;
            grid.getPage();
        }
        else if (this instanceof DbNetEdit) {
            const edit = this as DbNetEdit;
            edit.currentRow = 1;
            edit.getRows();
        }
    }

    public lookup($input: JQuery<HTMLInputElement>, request: DbNetGridEditRequest) {
        $input.uniqueId();
        request.lookupColumnIndex = parseInt($input.attr("columnIndex") as string);

        if (this.lookupDialog && $input.attr("id") == this.lookupDialog.$input?.attr("id")) {
            this.lookupDialog.open();
            return;
        }
        this.post<DbNetSuiteResponse>("lookup", request)
            .then((response) => {
                if (!this.lookupDialog) {
                    this.element?.append(response.dialog);
                    this.lookupDialog = new LookupDialog(`${this.id}_lookup_dialog`, this);
                }
                this.lookupDialog.update(response, $input);
                this.lookupDialog.open();
            })
    }

    private newColumn(columnExpr: string, unmatched:boolean) {
        if (this.constructor.name == "DbNetGrid") {
            return new GridColumn({ columnExpression: columnExpr } as GridColumnResponse, unmatched)
        }
        else {
            return new EditColumn({ columnExpression: columnExpr } as EditColumnResponse, unmatched)
        }
    }

    private matchingColumn(dbColumn: DbColumn, columnName: string) {
        let match = false;
        if (dbColumn.columnKey?.includes(".")) {
            match = dbColumn.columnKey?.split('.').pop()?.toLowerCase() == columnName.toLowerCase();
        }
        if (dbColumn.columnKey?.split(' ').pop()?.toLowerCase() == columnName.toLowerCase()) {
            match = true;
        }
        if (dbColumn.columnExpression?.split(' ').pop()?.toLowerCase() == columnName.toLowerCase()) {
            match = true;
        }
        if (!match) {
            match = dbColumn.columnKey?.toLowerCase() == columnName.toLowerCase();
        }
        return match;
    }

    public assignForeignKey(control: DbNetSuite, fk: string | object | null) {
        if (control instanceof DbNetGridEdit) {
            const gridEdit = control as DbNetGridEdit;
            const col = gridEdit.columns.find((col) => { return col.foreignKey == true });

            if (col == undefined) {
                if (control.parentChildRelationship == "OneToMany") {
                    control.highlight();
                    this.error("A foreign key column has not been specified for the linked control")
                }
                return;
            }

            if (control instanceof DbNetEdit) {
                const edit = control as DbNetEdit;
                if (edit.initialised) {
                    edit.updateForeignKeyValue(fk as object);
                }
            }

            col.foreignKeyValue = fk ? fk : DbNetSuite.DBNull;
        }
    }

    protected baseRequest(): DbNetGridEditRequest {
        const request = this._getRequest() as DbNetGridEditRequest;

        request.fromPart = this.fromPart;
        request.toolbarButtonStyle = this.toolbarButtonStyle;
        request.quickSearch = this.quickSearch;
        request.quickSearchToken = this.quickSearchToken;
        request.optimizeForLargeDataset = this.optimizeForLargeDataset;
        request.primaryKey = this.primaryKey as string;
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

        return request;
    }
}