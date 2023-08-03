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
    maxImageHeight: number;
    navigation: boolean;
    primaryKey: string | undefined;
    optimizeForLargeDataset: boolean;
    quickSearch: boolean;
    quickSearchDelay: number;
    quickSearchMinChars: number;
    quickSearchTimerId: number | undefined;
    quickSearchToken: string;
    search: boolean;
    searchDialog: SearchDialog | undefined;
    searchFilterJoin: string;
    searchParams: Array<SearchParam>;
    toolbarButtonStyle: ToolbarButtonStyle;
    toolbarPanel: JQuery<HTMLElement> | undefined;
    toolbarPosition: ToolbarPosition;
    constructor(id: string);
    setColumnExpressions(...columnExpressions: string[]): void;
    setColumnKeys(...columnKeys: string[]): void;
    setColumnLabels(...labels: string[]): void;
    setColumnProperty(columnName: string | Array<string>, property: string, propertyValue: object): void;
    setColumnProperties(columnName: string, properties: object): void;
    protected openSearchDialog(request: DbNetGridEditRequest): void;
    protected quickSearchKeyPress(event: JQuery.TriggeredEvent): void;
    private runQuickSearch;
    lookup($input: JQuery<HTMLInputElement>, request: DbNetGridEditRequest): void;
    private newColumn;
    private matchingColumn;
    assignForeignKey(control: DbNetSuite, fk: string | object | null): void;
}
