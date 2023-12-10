/// <reference types="jquery" />
/// <reference types="jquery" />
/// <reference types="jqueryui" />
/// <reference types="bootstrap" />
type ParentChildRelationship = "OneToOne" | "OneToMany" | null;
declare class DbNetGridEdit extends DbNetSuite {
    columnName: string | undefined;
    columns: DbColumn[];
    _delete: boolean;
    fromPart: string;
    insert: boolean;
    lookupDialog: LookupDialog | undefined;
    fixedFilterParams: Dictionary<object>;
    fixedFilterSql: string;
    maxImageHeight: number;
    navigation: boolean;
    primaryKey: string | undefined;
    initialOrderBy: string;
    optimizeForLargeDataset: boolean;
    search: boolean;
    searchDialog: SearchDialog | undefined;
    searchFilterJoin: string;
    searchParams: Array<SearchParam>;
    toolbarButtonStyle: ToolbarButtonStyle;
    toolbarPanel: JQuery<HTMLElement> | undefined;
    toolbarPosition: ToolbarPosition;
    jsonKey: string;
    json: object | null;
    dataSourceType: DataSourceType;
    constructor(id: string);
    setColumnExpressions(...columnExpressions: string[]): void;
    setColumnKeys(...columnKeys: string[]): void;
    setColumnLabels(...labels: string[]): void;
    setColumnProperty(columnName: string | Array<string>, property: string, propertyValue: object): void;
    setColumnProperties(columnName: string, properties: object): void;
    protected openSearchDialog(request: DbNetGridEditRequest): void;
    lookup($input: JQuery<HTMLInputElement>, request: DbNetGridEditRequest): void;
    private newColumn;
    private matchingColumn;
    assignForeignKey(control: DbNetSuite, fk: string | object | null): void;
    protected baseRequest(): DbNetGridEditRequest;
    protected invokeOnJsonUpdated(editMode: EditMode): void;
    processJsonUpdateResponse(response: JsonUpdateResponse): void;
}
